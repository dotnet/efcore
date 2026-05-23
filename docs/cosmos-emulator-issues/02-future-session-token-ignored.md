# Linux Cosmos DB emulator silently accepts an unreachable (future) session token

Tracking issue: [Azure/azure-cosmos-db-emulator-docker#291](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/291)

## Summary

When a client passes a session token whose LSN is far in the future (one that the server can never satisfy because no
such write has occurred), the **real Cosmos DB service** and the **Windows** emulator block briefly waiting for the
session to become available and then return `404 NotFound` with sub-status `1002` and the message

> `The read session is not available for the input session token.`

The **Linux** emulator (`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`) instead **silently
returns the item with status `200 OK`**, completely ignoring the session token.

This causes the following EF Core `CosmosSessionTokensTest+CosmosNonSharedSessionTokenTests` tests to fail because the
expected `CosmosException` is never thrown:

- `UseSessionTokens_uses_session_tokens`
- `Read_item_session_not_found_throws_CosmosException`

## Stand-alone repro (no EF Core)

`Program.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.59.0" />
  </ItemGroup>
</Project>
```

`Program.cs`:

```csharp
using System.Net.Http;
using Microsoft.Azure.Cosmos;

const string Endpoint = "https://localhost:8081";
const string Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

var options = new CosmosClientOptions
{
    ConnectionMode = ConnectionMode.Gateway,
    ConsistencyLevel = ConsistencyLevel.Session,
    HttpClientFactory = () => new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
};

using var client = new CosmosClient(Endpoint, Key, options);
var db = (await client.CreateDatabaseIfNotExistsAsync("ReproDb")).Database;
var container = (await db.CreateContainerIfNotExistsAsync(
    new ContainerProperties("ReproFuture", "/pk"))).Container;

// Write an item to obtain a valid session token in the form "<rangeId>:<...>#<lsn>".
var write = await container.UpsertItemAsync(
    new { id = "1", pk = "1" }, new PartitionKey("1"));
var valid = write.Headers.Session;          // e.g. "0:0#5"

// Build a token in the same range but with LSN = int.MaxValue.
var hash = valid.IndexOf('#');
var future = valid.Substring(0, hash + 1) + int.MaxValue;
Console.WriteLine($"valid  = {valid}");
Console.WriteLine($"future = {future}");

try
{
    var result = await container.ReadItemAsync<dynamic>(
        "1", new PartitionKey("1"),
        new ItemRequestOptions { SessionToken = future });

    // Linux emulator: prints "200 OK" (ignored the unsatisfiable session token).
    Console.WriteLine($"Read succeeded with StatusCode={result.StatusCode}");
}
catch (CosmosException ex)
{
    // Real Cosmos / Windows emulator: 404 with the standard "read session is not available" message.
    Console.WriteLine($"Status={ex.StatusCode} SubStatus={ex.SubStatusCode}");
    Console.WriteLine(ex.ResponseBody);
}
```

Expected output (real Cosmos / Windows emulator):

```
Status=NotFound SubStatus=1002
... The read session is not available for the input session token. ...
```

Actual output on the Linux emulator:

```
Read succeeded with StatusCode=OK
```

## Suggested fix

The emulator should honor the session-token contract: when a client sends a session token whose LSN is greater than the
current max global LSN for the target partition, the request must either (a) wait for the session to become available
up to the configured timeout, or (b) return `404 NotFound` with sub-status `1002`
(`The read session is not available for the input session token.`), as the real service does. Returning `200 OK` while
ignoring the session token breaks session-consistency guarantees for clients that rely on causal reads.
