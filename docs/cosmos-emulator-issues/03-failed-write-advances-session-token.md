# Linux Cosmos DB emulator returns an extra session-token LSN across multi-context concurrency sequences

Tracking issue: [Azure/azure-cosmos-db-emulator-docker#291](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/291)

## Summary

While running EF Core's `CosmosSessionTokensTest+CosmosNonSharedSessionTokenTests.Optimistic_concurrency_precondition_failure_updates_session_token`
against the **Linux** emulator we observe that the set of session tokens EF Core sees across a multi-client
optimistic-concurrency sequence contains **one extra LSN value** that the same sequence does not produce against the
**Windows** emulator or the real Cosmos DB service.

Specifically, EF Core's `CompositeSessionToken` (which accumulates every distinct session-token response value the SDK
hands it during a logical sequence) collects:

| Environment | Tokens observed (joined) |
| --- | --- |
| Real Cosmos / Windows emulator | `0:0#51,0:0#52,0:0#0,0:0#54` |
| Linux emulator                 | `0:0#51,0:0#52,0:0#0,0:0#53,0:0#54` |

Note the extra `0:0#53` slipping in between `0:0#0` and `0:0#54`. That extra LSN was returned in a response (most likely
to a *failed* write — either the 412 from the stale-ETag `Replace`, or the 412/404 from the subsequent stale-ETag
`Delete`) that should not have advanced the partition's session LSN.

The failing assertions are:

- `Optimistic_concurrency_precondition_failure_updates_session_token(autoTransactionBehavior: Always)`
- `Optimistic_concurrency_precondition_failure_updates_session_token(autoTransactionBehavior: Never)`

## Stand-alone repro (no EF Core)

The simplest deterministic repro is a two-"client" sequence:
client&nbsp;A creates a document, client&nbsp;B reads then replaces it, then client&nbsp;A
tries to replace using its now-stale ETag (and again to delete with the stale ETag). Capture the session token
returned in each response and look for an LSN that only appears under the Linux emulator.

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
var c = (await db.CreateContainerIfNotExistsAsync(new ContainerProperties("ReproEtag", "/pk"))).Container;

void Log(string label, string? session) => Console.WriteLine($"{label,-12} session={session}");

// Client A creates
var a1 = await c.CreateItemAsync(new { id = "1", pk = "1", v = 1 }, new PartitionKey("1"));
var staleEtag = a1.ETag;
Log("A create",   a1.Headers.Session);

// Client B reads and updates
var b1 = await c.ReadItemAsync<dynamic>("1", new PartitionKey("1"));
Log("B read",     b1.Headers.Session);

var b2 = await c.ReplaceItemAsync(new { id = "1", pk = "1", v = 2 }, "1", new PartitionKey("1"),
    new ItemRequestOptions { IfMatchEtag = b1.ETag });
Log("B replace",  b2.Headers.Session);
var latestSuccessfulSession = b2.Headers.Session;

// Client A tries to replace using its stale ETag -> 412 (FAILED write)
try
{
    await c.ReplaceItemAsync(new { id = "1", pk = "1", v = 3 }, "1", new PartitionKey("1"),
        new ItemRequestOptions { IfMatchEtag = staleEtag });
}
catch (CosmosException ex)
{
    Log("A replace*", ex.Headers?.Session);
    // EXPECTED (real Cosmos / Windows emulator): same as 'B replace' (failed write does not advance LSN).
    // OBSERVED (Linux emulator): may differ by 1 LSN.
}

// Client A tries to delete using its stale ETag -> 412 (FAILED write)
try
{
    await c.DeleteItemAsync<dynamic>("1", new PartitionKey("1"),
        new ItemRequestOptions { IfMatchEtag = staleEtag });
}
catch (CosmosException ex)
{
    Log("A delete*", ex.Headers?.Session);
}
```

Expected on the real service: every session token observed after `B replace` equals `latestSuccessfulSession` until the
next successful write. Observed on the Linux emulator: at least one of the failed-write responses returns a session
token with a higher LSN than `latestSuccessfulSession`, which is then accumulated by EF Core's `CompositeSessionToken`
and causes the equality assertion to fail.

## Suggested fix

A failed write (412 PreconditionFailed, 404 NotFound on Delete/Replace, etc.) must not bump the partition's session
LSN, and the response header `x-ms-session-token` must reflect the last committed LSN — matching the behaviour of the
real Cosmos DB service and the Windows emulator. Otherwise, callers relying on session-token equality across
contexts to verify causality and detect concurrency outcomes (as the EF Core Cosmos provider does) see spurious
extra session tokens.
