# Cosmos DB Linux emulator: ETag / `If-Match` precondition is silently ignored when `AllowBulkExecution = true`

## Summary

When `CosmosClientOptions.AllowBulkExecution = true`, the Linux Cosmos DB emulator
(`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`) does **not**
enforce the optimistic-concurrency precondition supplied via
`ItemRequestOptions.IfMatchEtag`. A `ReplaceItemAsync` (or `DeleteItemAsync`) call that
carries a *stale* ETag returns `200 OK` instead of `412 PreconditionFailed`.

When `AllowBulkExecution = false` the same emulator returns `412` correctly — so the bug
is specific to the bulk-execution code path.

This is the same family as
[azure-cosmos-db-emulator-docker#292](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292)
item 6 ("Optimistic concurrency (etag) not enforced"), but only manifests in bulk mode.

## EF Core tests that fail because of this

Both tests are in `test/EFCore.Cosmos.FunctionalTests/Update/CosmosBulkConcurrencyTest.cs`
(the fixture enables `UseCosmos(x => x.BulkExecutionAllowed())`):

* `Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException`
* `Updating_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException`

Both fail with `Assert.ThrowsAny<DbUpdateConcurrencyException>: No exception was thrown`.

## Stand-alone repro (no EF)

```csharp
// dotnet add package Microsoft.Azure.Cosmos
// dotnet add package Newtonsoft.Json
using System.Net;
using Microsoft.Azure.Cosmos;

const string Endpoint = "https://localhost:8081";
const string Key      = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

var options = new CosmosClientOptions
{
    HttpClientFactory = () => new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    }),
    ConnectionMode     = ConnectionMode.Gateway,
    AllowBulkExecution = true               // <-- only fails when bulk is on
};

using var client = new CosmosClient(Endpoint, Key, options);
var db = (await client.CreateDatabaseIfNotExistsAsync("EtagRepro")).Database;
var c  = (await db.CreateContainerIfNotExistsAsync("items", "/pk")).Container;
var pk = new PartitionKey("p");

// 1. Create.
var created    = await c.UpsertItemAsync(new { id = "1", pk = "p", v = 1 }, pk);
var staleEtag  = created.ETag;

// 2. Some other client updates the document, bumping its ETag.
await c.ReplaceItemAsync(new { id = "1", pk = "p", v = 2 }, "1", pk);

// 3. Try to update with the now-stale ETag. Should throw 412.
try
{
    var resp = await c.ReplaceItemAsync(
        new { id = "1", pk = "p", v = 3 }, "1", pk,
        new ItemRequestOptions { IfMatchEtag = staleEtag });

    Console.WriteLine($"BUG: stale-etag replace returned {resp.StatusCode} (expected 412 PreconditionFailed).");
}
catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
{
    Console.WriteLine("OK: 412 PreconditionFailed (correct behaviour).");
}

await db.DeleteAsync();
```

### Observed output against Linux emulator (vnext-preview)

```
BUG: stale-etag replace returned OK (expected 412 PreconditionFailed).
```

### Expected output (and what is produced against the Windows emulator / real Cosmos service)

```
OK: 412 PreconditionFailed (correct behaviour).
```

## Notes

* Removing `AllowBulkExecution = true` from `CosmosClientOptions` makes the same emulator
  return `412 PreconditionFailed` as expected — the bug is specific to the bulk-execution
  code path.
* The same behaviour is observed for `DeleteItemAsync` with a stale `IfMatchEtag`.
