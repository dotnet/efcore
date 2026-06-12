# Repro: Linux Cosmos DB emulator returns `0:0#0` session token on query responses

This document is a standalone (EF-Core-free) reproduction of the emulator defect that
makes the following test fail when run against the Linux Cosmos DB emulator:

```
Microsoft.EntityFrameworkCore.CosmosSessionTokensTest+CosmosNonSharedSessionTokenTests
    .Optimistic_concurrency_precondition_failure_updates_session_token
```

The test is normally skipped on the Linux emulator via
`[ConditionalTheory(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]`
and tracked by <https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/319>.

## Affected emulator image

```
mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview
Digest:  sha256:54d7bc334494c50cea867c270880671a7db080626a9732832b34c0d69342f9b0
Created: 2026-06-02T17:46:04Z
```

* SDK: `Microsoft.Azure.Cosmos` 3.60.0
* Endpoint: `https://localhost:8081` (default emulator key)

## The defect

For a single-partition document that has committed writes up to local sequence number
`N`, the `x-ms-session-token` response header behaves differently per operation type:

| Operation  | `x-ms-session-token` returned | Correct? |
|------------|-------------------------------|----------|
| Create     | `0:0#N`                       | yes      |
| Replace    | `0:0#N` (latest LSN)          | yes      |
| Point read | `0:0#N` (latest LSN)          | yes      |
| **Query**  | **`0:0#0`**                   | **no**   |

A query against a partition that is at LSN `N` returns `0:0#0` instead of a token that
reflects the current LSN (`>= 0:0#N`). On real Azure Cosmos DB (and the stable
`:latest` Linux emulator image) every response — including queries — returns the
partition's current session token, so a client that "catches up" by reading always
observes the latest LSN.

### Why this breaks the EF Core test

EF Core's session-token management accumulates the union of every distinct
`range:globalLsn#localLsn` part it observes per context (see
`SessionTokenStorage.CompositeSessionToken`). The test runs two contexts against the
same single-partition document and asserts that, once both contexts have caught up to
the same state, their accumulated session tokens are equal:

```csharp
Assert.Equal(removedSessionToken, afterRemoveExceptionSessionToken);
```

On real Cosmos this holds because every read/query response returns the partition's
current (maximum) LSN, so both contexts converge to the same set of tokens. On the
vnext-preview emulator, the second context "catches up" via a query, which returns
`0:0#0` instead of the latest LSN, so it never observes the write the first context
made. The two contexts' accumulated tokens diverge by exactly that one missing LSN, for
example:

```
Expected: 0:0#18,0:0#19,0:0#0,0:0#21
Actual:   0:0#18,0:0#19,0:0#0,0:0#20,0:0#21
                                ^^^^^^ LSN of the write the query failed to surface
```

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
    ConsistencyLevel = ConsistencyLevel.Session,
    HttpClientFactory = () => new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    }),
    LimitToEndpoint = true
};

using var client = new CosmosClient(endpoint, key, options);
var db = (await client.CreateDatabaseIfNotExistsAsync("ReproDb")).Database;
var name = "Repro_" + Guid.NewGuid().ToString("N");
var container = (await db.CreateContainerIfNotExistsAsync(name, "/pk")).Container;
var pk = new PartitionKey("1");
string T(string? s) => string.IsNullOrEmpty(s) ? "<null/empty>" : s;
var ok = true;

try
{
    var create = await container.CreateItemAsync(new { id = "1", pk = "1", v = 1 }, pk);
    Console.WriteLine($"create               -> {T(create.Headers.Session)}");

    var replace = await container.ReplaceItemAsync(new { id = "1", pk = "1", v = 2 }, "1", pk);
    var latest = replace.Headers.Session;
    Console.WriteLine($"replace (latest LSN) -> {T(latest)}");

    var read = await container.ReadItemAsync<dynamic>("1", pk);
    Console.WriteLine($"point read           -> {T(read.Headers.Session)}   (correct: matches latest write)");

    using var iterator = container.GetItemQueryIterator<dynamic>(
        new QueryDefinition("SELECT * FROM c WHERE c.id = '1'"),
        requestOptions: new QueryRequestOptions { PartitionKey = pk });
    var query = await iterator.ReadNextAsync();
    var queryToken = query.Headers.Session;
    Console.WriteLine($"query                -> {T(queryToken)}   (EXPECTED >= '{T(latest)}')");

    if (queryToken != latest)
    {
        ok = false;
        Console.WriteLine();
        Console.WriteLine($"BUG: query returned '{T(queryToken)}' but the partition is at '{T(latest)}'.");
    }
}
finally
{
    await container.DeleteContainerAsync();
}

Console.WriteLine(ok ? "PASS (behaves like real Cosmos)" : "FAIL (emulator bug reproduced)");
return ok ? 0 : 1;
```

## Observed output (vnext-preview emulator)

```
create               -> 0:0#24
replace (latest LSN) -> 0:0#25
point read           -> 0:0#25   (correct: matches latest write)
query                -> 0:0#0   (EXPECTED >= '0:0#25')

BUG: query returned '0:0#0' but the partition is at '0:0#25'.
FAIL (emulator bug reproduced)
```

## Expected output (real Azure Cosmos DB / stable `:latest` Linux emulator)

```
create               -> 0:0#24
replace (latest LSN) -> 0:0#25
point read           -> 0:0#25   (correct: matches latest write)
query                -> 0:0#25   (EXPECTED >= '0:0#25')
PASS (behaves like real Cosmos)
```
