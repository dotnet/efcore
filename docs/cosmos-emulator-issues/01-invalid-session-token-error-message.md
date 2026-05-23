# Linux Cosmos DB emulator returns a different error message for an invalid session token

Tracking issue: [Azure/azure-cosmos-db-emulator-docker#291](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/291)

## Summary

When the SDK passes a syntactically-invalid value as the `SessionToken` request header, the **Linux** Cosmos DB emulator
(`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`) returns the error string

> `The session token provided 'invalidtoken' is not valid.`

The **Windows** emulator and the real Cosmos DB service instead return

> `The session token provided 'invalidtoken' is invalid.`

Both responses are `400 BadRequest`, but the wording is different (`is not valid` vs. `is invalid`). Code that
asserts on the exact message text (which is what we do in our EF Core tests) fails on the Linux emulator.

This is the failure observed in 21 of the 26 EF Core
`CosmosSessionTokensTest` failures (the `Query_uses_session_token`,
`Read_item_uses_session_token`, `Shaped_query_uses_session_token`,
`PagingQuery_uses_session_token`, and all `Add_uses_GetSessionToken` /
`Update_uses_session_token` / `Delete_uses_session_token` theory rows).

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
    new ContainerProperties("Repro", "/pk"))).Container;

await container.UpsertItemAsync(new { id = "1", pk = "1" }, new PartitionKey("1"));

try
{
    await container.ReadItemAsync<dynamic>(
        "1", new PartitionKey("1"),
        new ItemRequestOptions { SessionToken = "invalidtoken" });
}
catch (CosmosException ex)
{
    // Real Cosmos / Windows emulator:
    //   "The session token provided 'invalidtoken' is invalid."
    // Linux emulator:
    //   "The session token provided 'invalidtoken' is not valid."
    Console.WriteLine(ex.ResponseBody);
}
```

Expected output (real Cosmos / Windows emulator):

```
code : BadRequest
message : The session token provided 'invalidtoken' is invalid.
```

Actual output on the Linux emulator:

```
code : BadRequest
message : The session token provided 'invalidtoken' is not valid.
```

## Suggested fix

Align the Linux emulator's error message with the real service so that the substring `is invalid` is preserved.
