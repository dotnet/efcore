# Cosmos DB Linux emulator: `id` containing multi-byte UTF-8 characters returns `500 InternalServerError` (PostgresError E22P05)

## Summary

When a document `id` (or partition-key string) contains multi-byte UTF-8 characters such
as `€` or `Ω`, even a basic `CreateItemAsync` against the Linux Cosmos DB emulator
(`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`) fails with
`500 InternalServerError` and a Postgres error:

```
PGCosmosError(3, "Database query failed: PostgresError(SqlState(E22P05), None)", <disabled>)
```

`E22P05` is the PostgreSQL `untranslatable_character` SQL state, suggesting the emulator
is passing the raw multi-byte string through to the backing Postgres store without
encoding it correctly.

This is not part of
[azure-cosmos-db-emulator-docker#292](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292)
— it is a separate Linux-emulator-only bug.

## EF Core tests that fail because of this

In `test/EFCore.Cosmos.FunctionalTests/CosmosTransactionalBatchTest.cs`:

* `SaveChanges_update_id_contains_special_chars_which_makes_request_larger_than_2_mib_splits_into_2_batches(isIdSpecialChar: True)`
* `SaveChanges_update_id_contains_special_chars_which_makes_request_larger_than_2_mib_splits_into_2_batches(isIdSpecialChar: False)`

Both fail with `Microsoft.EntityFrameworkCore.DbUpdateException → InternalServerError (500)`
on the very first `SaveChanges` (the `isIdSpecialChar: False` variant only fails because
the partition key is also a multi-byte string).

## Stand-alone repro (no EF)

```csharp
// dotnet add package Microsoft.Azure.Cosmos
// dotnet add package Newtonsoft.Json
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
    ConnectionMode = ConnectionMode.Gateway
};

using var client = new CosmosClient(Endpoint, Key, options);
var db = (await client.CreateDatabaseIfNotExistsAsync("UnicodeIdRepro")).Database;
var c  = (await db.CreateContainerIfNotExistsAsync("items", "/pk")).Container;

// 340 * 3 = 1020 UTF-8 bytes, just under the documented 1023-byte id length limit.
var id = new string('€', 340);
var pk = new string('€', 340);

try
{
    var resp = await c.CreateItemAsync(
        new { id, pk, name = "x" },
        new PartitionKey(pk));
    Console.WriteLine($"OK: created with status {resp.StatusCode}.");
}
catch (CosmosException ex)
{
    Console.WriteLine($"BUG: {ex.StatusCode} — {ex.Message.Split('\n')[0]}");
}

await db.DeleteAsync();
```

### Observed output against Linux emulator (vnext-preview)

```
BUG: InternalServerError — Response status code does not indicate success: InternalServerError (500);
     Substatus: 0; ActivityId: ...; Reason:
     (PGCosmosError(3, "Database query failed: PostgresError(SqlState(E22P05), None)", <disabled>)
```

### Expected output (and what is produced against the Windows emulator / real Cosmos service)

```
OK: created with status Created.
```

## Notes

* Using ASCII-only `id` and `pk` values of identical *byte* length (e.g. `new string('x', 1020)`)
  succeeds. The failure is character-encoding related, not length related.
* The same failure happens for `ReplaceItemAsync` and inside a `TransactionalBatch` once
  the multi-byte payload is large enough.
