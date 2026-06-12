# Repro: Linux Cosmos DB emulator does not enforce transactional-batch request-size limits

This document is a standalone (EF-Core-free) reproduction of the emulator defect that
makes the following tests fail when run against the Linux Cosmos DB emulator:

```
Microsoft.EntityFrameworkCore.EndToEndCosmosTest
    .Can_add_update_delete_end_to_end(transactionalBatch: true)
Microsoft.EntityFrameworkCore.Update.CosmosBulkExecutionTest
    .DoesNotBatchSingleBatchableWrite
    .AutoTransactionBehaviorWhenNeeded_Throws
    .AutoTransactionBehaviorAlways_Throws
```

These were previously skipped on the Linux emulator via
`[ConditionalFact/Theory(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]`
and tracked by <https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292>
("Transactional batch limits not enforced").

`SaveChanges_entity_too_large_throws` and `Can_add_update_delete_end_to_end(transactionalBatch: false)`
are listed in the same investigation, but they **pass** on the Linux emulator (see
[Per-test impact](#per-test-impact) below) because they do not depend on transactional batches.

## Affected emulator image

```
mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest
Digest:  sha256:54d7bc334494c50cea867c270880671a7db080626a9732832b34c0d69342f9b0
Created: 2026-06-02T17:46:04Z
```

* SDK: `Microsoft.Azure.Cosmos` 3.60.0
* Endpoint: `https://localhost:8081` (default emulator key)

## The defect

A Cosmos transactional batch is subject to two server-enforced limits on real Azure
Cosmos DB:

| Limit                            | Real Azure Cosmos DB            | vnext-latest Linux emulator     |
|----------------------------------|---------------------------------|---------------------------------|
| Max **operations** per batch (100) | rejected (`400 BadRequest`)     | rejected (`400 BadRequest`) ✔   |
| Max **request size** per batch (~2 MB) | `413 RequestEntityTooLarge` | **`200 OK` (accepted!)** ✘      |

The emulator silently **accepts a transactional batch whose payload exceeds the request
size limit** instead of rejecting it with `413 RequestEntityTooLarge`. For comparison, a
single (non-batched) point write that exceeds the document/request size limit *is*
correctly rejected with `413`.

### Why this breaks the EF Core tests

Because the emulator's transactional-batch limit behavior is unreliable, the Cosmos test
harness installs `LinuxEmulatorSaveChangesInterceptor`
(`test/EFCore.Cosmos.FunctionalTests/TestUtilities/LinuxEmulatorSaveChangesInterceptor.cs`),
which forces `DatabaseFacade.AutoTransactionBehavior = AutoTransactionBehavior.Never` on
every `SaveChanges` call when `CosmosTestEnvironment.IsLinuxEmulator` is true (see
`CosmosTestStore.AddProviderOptions`). With `Never` forced, EF never creates a
transactional batch on the Linux emulator — every write is sent individually.

That defeats every assertion that depends on a transactional batch being produced:

* Tests that assert an `ExecutedTransactionalBatch` log event instead observe individual
  `ExecutedCreateItem` / `ExecutedReplaceItem` / `ExecutedDeleteItem` events.
* Tests that assert the `BulkExecutionWithTransactionalBatch` warning-as-error never see
  it, because that warning is only raised when `AutoTransactionBehavior != Never`
  (`CosmosDatabaseWrapper.CreateSaveGroups`).

## Standalone repro (no EF Core)

`repro.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.60.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

`Program.cs`:

```csharp
using System.Net;
using Microsoft.Azure.Cosmos;

const string endpoint = "https://localhost:8081";
const string key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

var options = new CosmosClientOptions
{
    ConnectionMode = ConnectionMode.Gateway,
    HttpClientFactory = () => new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    }),
    LimitToEndpoint = true
};

using var client = new CosmosClient(endpoint, key, options);
var db = (await client.CreateDatabaseIfNotExistsAsync("BatchReproDb")).Database;
var name = "Batch_" + Guid.NewGuid().ToString("N");
var container = (await db.CreateContainerIfNotExistsAsync(name, "/pk")).Container;
var pk = new PartitionKey("1");
var ok = true;

async Task ShowBatch(string label, TransactionalBatch batch, bool expectRejected, string expectation)
{
    try
    {
        var resp = await batch.ExecuteAsync();
        Console.WriteLine($"{label,-28}-> {(int)resp.StatusCode} {resp.StatusCode}   [real Cosmos: {expectation}]");
        if (expectRejected && resp.IsSuccessStatusCode)
        {
            ok = false;
            Console.WriteLine($"  BUG: emulator accepted a batch that real Cosmos rejects ({expectation}).");
        }
    }
    catch (CosmosException ex)
    {
        Console.WriteLine($"{label,-28}-> threw {(int)ex.StatusCode} {ex.StatusCode}   [real Cosmos: {expectation}]");
    }
}

try
{
    // 1. A normal small batch works.
    var basic = container.CreateTransactionalBatch(pk);
    for (var i = 0; i < 3; i++) basic.CreateItem(new { id = $"basic-{i}", pk = "1", v = i });
    await ShowBatch("batch: 3 ops", basic, expectRejected: false, "200 OK");

    // 2. Operation-count limit IS enforced (rejected).
    var count = container.CreateTransactionalBatch(pk);
    for (var i = 0; i < 200; i++) count.CreateItem(new { id = $"count-{i}", pk = "1", v = i });
    await ShowBatch("batch: 200 ops", count, expectRejected: true, "rejected (>100 ops)");

    // 3. Request-size limit is NOT enforced (the defect).
    var big = new string('x', 2_000_000);
    var size = container.CreateTransactionalBatch(pk);
    size.CreateItem(new { id = "size-1", pk = "1", data = big });
    size.CreateItem(new { id = "size-2", pk = "1", data = big });
    await ShowBatch("batch: ~4MB payload", size, expectRejected: true, "413 RequestEntityTooLarge");

    // 4. For contrast: a single oversized point write IS correctly rejected with 413.
    try
    {
        var resp = await container.CreateItemAsync(new { id = "single-big", pk = "1", data = new string('x', 50_000_000) }, pk);
        ok = false;
        Console.WriteLine($"{"single point write: ~50MB",-28}-> {(int)resp.StatusCode} {resp.StatusCode}   [real Cosmos: 413]  BUG: accepted!");
    }
    catch (CosmosException ex)
    {
        Console.WriteLine($"{"single point write: ~50MB",-28}-> threw {(int)ex.StatusCode} {ex.StatusCode}   [real Cosmos: 413]");
    }
}
finally
{
    await container.DeleteContainerAsync();
}

Console.WriteLine(ok ? "PASS (behaves like real Cosmos)" : "FAIL (emulator bug reproduced: batch size limit not enforced)");
return ok ? 0 : 1;
```

## Observed output (vnext-latest emulator)

```
batch: 3 ops                -> 200 OK   [real Cosmos: 200 OK]
batch: 200 ops              -> 400 BadRequest   [real Cosmos: rejected (>100 ops)]
batch: ~4MB payload         -> 200 OK   [real Cosmos: 413 RequestEntityTooLarge]
  BUG: emulator accepted a batch that real Cosmos rejects (413 RequestEntityTooLarge).
single point write: ~50MB   -> threw 413 RequestEntityTooLarge   [real Cosmos: 413]
FAIL (emulator bug reproduced: batch size limit not enforced)
```

## Expected output (real Azure Cosmos DB)

```
batch: 3 ops                -> 200 OK   [real Cosmos: 200 OK]
batch: 200 ops              -> threw 400 BadRequest   [real Cosmos: rejected (>100 ops)]
batch: ~4MB payload         -> threw 413 RequestEntityTooLarge   [real Cosmos: 413 RequestEntityTooLarge]
single point write: ~50MB   -> threw 413 RequestEntityTooLarge   [real Cosmos: 413]
PASS (behaves like real Cosmos)
```

## Per-test impact

Status observed against `vnext-latest` after removing the `IsNotLinuxEmulator` skip:

| Test | Linux emulator | Reason |
|------|----------------|--------|
| `EndToEndCosmosTest.Can_add_update_delete_end_to_end(false)` | **pass** | No transactional batch involved. |
| `EndToEndCosmosTest.Can_add_update_delete_end_to_end(true)` | fail | Asserts `ExecutedTransactionalBatch`; harness forces `Never`, so individual writes are used. |
| `CosmosBulkExecutionTest.DoesNotBatchSingleBatchableWrite` | fail | Asserts `ExecutedTransactionalBatch`; harness forces `Never`. |
| `CosmosBulkExecutionTest.AutoTransactionBehaviorWhenNeeded_Throws` | fail | Expects the `BulkExecutionWithTransactionalBatch` warning-as-error, which is only raised when `AutoTransactionBehavior != Never`. |
| `CosmosBulkExecutionTest.AutoTransactionBehaviorAlways_Throws` | fail | Same as above. |
| `CosmosTransactionalBatchTest.SaveChanges_entity_too_large_throws` | **pass** (~17s) | Single oversized document → individual point write → emulator correctly returns `413`. Already `[Fact]`; no change needed. |

The failing tests are expected to pass on real Azure Cosmos DB and on the Windows
emulator, where transactional batches are fully supported. They are unskipped here so they
run on those environments; the Linux-emulator failures are caused by the emulator defect
documented above and are tracked by
<https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292>.
