---
name: cosmos-provider
description: 'Implementation details for the EF Core Azure Cosmos DB provider. Use when changing Cosmos-specific code.'
user-invocable: false
---

# Cosmos DB Provider

Non-relational provider with its own parallel query pipeline. Uses JSON for document materialization.

## When to Use

- Working on Cosmos SQL generation
- Working on document storage, partition key configuration, or `CosmosClientWrapper`

## Key Differences from Relational

- No migrations — use `EnsureCreated()`
- Documents as JSON — owned and complex types become embedded objects
- Partition key configuration required for performance
- `ETag` for optimistic concurrency
- No cross-container joins

## Azure Cosmos DB Emulator for Tests

### Automatic Testcontainer Startup

Cosmos functional tests automatically manage the emulator lifecycle via [Testcontainers](https://testcontainers.com/modules/cosmodb/?language=dotnet) (`Testcontainers.CosmosDb` NuGet package). The initialization logic in `TestEnvironment.cs` follows this order:

1. **Configured endpoint**: If `Test__Cosmos__DefaultConnection` env var (or `cosmosConfig.json` / `cosmosConfig.test.json`) is set, it is used directly — no container is started.
2. **Local emulator probe**: A quick HTTPS probe is sent to `https://localhost:8081`. If a running emulator responds, it is used.
3. **Testcontainer fallback**: If neither of the above succeeds, a `CosmosDbContainer` is started with the Linux emulator image (`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`). The container is disposed on process exit.
4. **Graceful skip**: If Docker is unavailable and no emulator is reachable, the default endpoint is used and `IsConnectionAvailableAsync()` returns `false`, causing tests to be skipped.

### Linux Emulator Detection

`TestEnvironment.IsLinuxEmulator` is `true` when:
- A testcontainer is running (always the Linux image), **or**
- The OS is not Windows (assumes the local emulator is the Linux Docker image), **or**
- `Test__Cosmos__EmulatorType` is explicitly set to `linux`.

The Linux (vnext) emulator does **not** support transactional batches, so `LinuxEmulatorSaveChangesInterceptor` forces `AutoTransactionBehavior.Never` on every `SaveChanges` call. Tests that require features absent from the Linux emulator are guarded with `[CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]`.

### HttpClient Handling

When a testcontainer is active, `CosmosDbContextOptionsBuilderExtensions.ApplyConfiguration` uses the container's `HttpMessageHandler` (a URI rewriter that routes requests to the mapped container port over HTTP). When connecting to a local HTTPS emulator, it uses `DangerousAcceptAnyServerCertificateValidator` instead.

### Manual Scripts (Legacy)

The shell scripts `eng/testing/run-cosmos-container.sh` and `eng/testing/run-cosmos-container.ps1` can still be used to manually start the emulator in Docker when needed, but they are no longer invoked by Helix or CI.

### Key Files

- `test/EFCore.Cosmos.FunctionalTests/TestUtilities/TestEnvironment.cs` — connection auto-detection and testcontainer lifecycle
- `test/EFCore.Cosmos.FunctionalTests/TestUtilities/CosmosTestStore.cs` — test store creation, seeding, cleanup
- `test/EFCore.Cosmos.FunctionalTests/TestUtilities/CosmosDbContextOptionsBuilderExtensions.cs` — shared Cosmos options (execution strategy, timeout, HttpClient, Gateway mode)
- `test/EFCore.Cosmos.FunctionalTests/TestUtilities/LinuxEmulatorSaveChangesInterceptor.cs` — disables transactional batches for the Linux emulator
- `test/EFCore.Cosmos.FunctionalTests/TestUtilities/CosmosConditionAttribute.cs` — conditional test execution based on emulator type
