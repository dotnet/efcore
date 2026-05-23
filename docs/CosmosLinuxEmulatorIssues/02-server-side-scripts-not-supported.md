# Cosmos DB Linux emulator: Server-side scripts (triggers / UDFs / stored procedures) are not supported

## Summary

The Linux Cosmos DB emulator
(`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`) returns
`400 BadRequest — "Server-side scripts are not supported in this emulator"` for any
attempt to create a trigger, stored procedure, or user-defined function.

This is not part of
[azure-cosmos-db-emulator-docker#292](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292)
but is the same class of "feature missing on the Linux emulator" issue that prevents the
EF Core test suite from running on it.

## EF Core test that fails because of this

In `test/EFCore.Cosmos.FunctionalTests/CosmosTransactionalBatchTest.cs`:

* `SaveChanges_transaction_behavior_always_succeeds_for_single_entity_with_trigger`

The test calls `Container.Scripts.CreateTriggerAsync(…)` to register a pre-create trigger,
which fails immediately with:

```
Microsoft.Azure.Cosmos.CosmosException : Response status code does not indicate success:
BadRequest (400); Substatus: 0; ActivityId: ; Reason:
( code : BadRequest
  message : Server-side scripts are not supported in this emulator );
```

## Stand-alone repro (no EF)

```csharp
// dotnet add package Microsoft.Azure.Cosmos
// dotnet add package Newtonsoft.Json
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;

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
var db = (await client.CreateDatabaseIfNotExistsAsync("ScriptsRepro")).Database;
var c  = (await db.CreateContainerIfNotExistsAsync("items", "/pk")).Container;

try
{
    var resp = await c.Scripts.CreateTriggerAsync(new TriggerProperties
    {
        Id               = "preCreate",
        TriggerType      = TriggerType.Pre,
        TriggerOperation = TriggerOperation.All,
        Body             = "function trigger() { }"
    });
    Console.WriteLine($"Trigger created: {resp.StatusCode} (OK).");
}
catch (CosmosException ex)
{
    Console.WriteLine($"BUG: {ex.StatusCode} — {ex.Message.Split('\n')[0]}");
}

await db.DeleteAsync();
```

### Observed output against Linux emulator (vnext-preview)

```
BUG: BadRequest — Response status code does not indicate success: BadRequest (400);
     Substatus: 0; ActivityId: ; Reason: (
```

(The detailed reason in the response payload is `Server-side scripts are not supported in
this emulator`.)

### Expected output (and what is produced against the Windows emulator / real Cosmos service)

```
Trigger created: Created (OK).
```

## Notes

* The same `BadRequest` is returned for `Scripts.CreateStoredProcedureAsync` and
  `Scripts.CreateUserDefinedFunctionAsync`.
* EF Core's `HasTrigger(...)` modelling does not require server-side scripts unless the
  application actually issues `Scripts.*Async` calls — the corresponding model metadata
  is only used by EF to decide how to dispatch save operations.
