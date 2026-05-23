# Cosmos Linux emulator: EF Core tests skipped via `[CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]` for [issue 292](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292)

This directory captures the result of removing the `IsNotLinuxEmulator` skip from every
EF Core test that pointed at
[azure-cosmos-db-emulator-docker#292](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292)
and running the tests against the Linux Cosmos DB emulator
(`mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`, gateway mode,
`https://localhost:8081`).

## Tests that were investigated

| File | Test |
| ---- | ---- |
| `EndToEndCosmosTest.cs`              | `Can_add_update_delete_end_to_end(bool transactionalBatch)` |
| `CosmosTransactionalBatchTest.cs`    | *class-level skip* — every `[ConditionalFact]`/`[ConditionalTheory]` in the class |
| `Update/CosmosBulkExecutionTest.cs`  | `DoesNotBatchSingleBatchableWrite` |
| `Update/CosmosBulkExecutionTest.cs`  | `AutoTransactionBehaviorWhenNeeded_Throws` |
| `Update/CosmosBulkExecutionTest.cs`  | `AutoTransactionBehaviorAlways_Throws` |
| `Update/CosmosBulkConcurrencyTest.cs`| `Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException` |
| `Update/CosmosBulkConcurrencyTest.cs`| `Updating_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException` |

## Result of running them

23 tests ran in total (1 from `EndToEndCosmosTest`, 14 from `CosmosTransactionalBatchTest`,
5 from `CosmosBulkExecutionTest`, 3 from `CosmosBulkConcurrencyTest`). 13 failed.

For every failure I built a tiny stand-alone `Microsoft.Azure.Cosmos` repro to determine
whether the underlying behaviour comes from the emulator (a Cosmos issue) or from the EF
Core provider/test fixture (an EF issue). The split is:

### Confirmed Linux-emulator bugs (Cosmos team) — one `.md` each

| Group | EF tests it explains | Issue description |
| ----- | -------------------- | ----------------- |
| 1. `IfMatch`/ETag is ignored when `AllowBulkExecution = true` | `CosmosBulkConcurrencyTest.Updating_then_deleting_…`, `CosmosBulkConcurrencyTest.Updating_then_updating_…` | [`01-etag-not-enforced-in-bulk-mode.md`](./01-etag-not-enforced-in-bulk-mode.md) |
| 2. Server-side scripts (triggers / sprocs / UDFs) are not supported | `CosmosTransactionalBatchTest.SaveChanges_transaction_behavior_always_succeeds_for_single_entity_with_trigger` | [`02-server-side-scripts-not-supported.md`](./02-server-side-scripts-not-supported.md) |
| 3. Multi-byte UTF-8 in `id` / partition key causes `500 InternalServerError` (Postgres `E22P05`) | `CosmosTransactionalBatchTest.SaveChanges_update_id_contains_special_chars_…` (both variants) | [`03-multibyte-utf8-id-postgres-error.md`](./03-multibyte-utf8-id-postgres-error.md) |

For each group above the stand-alone repro reproduces the bug against the Linux emulator
*and* succeeds against the Windows emulator / real Cosmos service.

### Failures whose root cause is NOT the Linux emulator

These tests fail when run against the Linux emulator, but a focused stand-alone Cosmos
repro shows the emulator behaves *correctly* in the relevant scenario. The failures
therefore look like EF-side issues that happen to be uncovered by running on the Linux
emulator. They are listed here only for completeness — no Cosmos issue should be filed
for them.

| EF test | EF-side observation |
| ------- | ------------------- |
| `EndToEndCosmosTest.Can_add_update_delete_end_to_end(transactionalBatch: True)` | `Single` over the log fails looking for `ExecutedTransactionalBatch` — EF appears to fall back to `CreateItem` for a 1-op `Always` save, contrary to `CosmosDatabaseWrapper.cs:111-121` which is supposed to keep batching when `AutoTransactionBehavior == Always`. |
| `CosmosBulkExecutionTest.DoesNotBatchSingleBatchableWrite` | Expects the first two logged ops to be `ExecutedCreateItem` and the next two to be `ExecutedTransactionalBatch` — got `ExecutedCreateItem` where a batch was expected. Looks like bulk grouping behaves differently than the test assumes. |
| `CosmosBulkExecutionTest.AutoTransactionBehaviorAlways_Throws`, `AutoTransactionBehaviorWhenNeeded_Throws` | Expect `InvalidOperationException` from the `BulkExecutionWithTransactionalBatch` warning-as-error path; no exception is thrown. The `BulkFixture` ignores that warning, but these two methods build their own `contextFactory` — yet they still don't throw. |
| `CosmosTransactionalBatchTest.SaveChanges_transaction_behavior_always_fails_for_multiple_partitionkeys` | Expects `CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysAtomicity` from the *client-side* check in `CosmosDatabaseWrapper.cs:244-249`. The check is unconditional but does not fire. The stand-alone repro confirms the server *does* reject cross-partition batches with `400 BadRequest` — so this is not a server bug. |
| `CosmosTransactionalBatchTest.SaveChanges_transaction_behavior_always_fails_for_multiple_entities_with_triggers` | Same family — expects the client-side `…TriggerAtomicity` check from `CosmosDatabaseWrapper.cs:225-232`; does not fire. |
| `CosmosTransactionalBatchTest.SaveChanges_fails_for_duplicate_key_in_same_partition_prevents_other_inserts_in_same_partition_even_if_staged_before_add` | Expects only the seeded item to remain after a batched insert containing a duplicate id (i.e. atomic rollback). Got 2 items. The stand-alone repro shows that a single transactional batch with a duplicate id *is* rolled back atomically by the Linux emulator — so the EF save is not being sent as one batch in this scenario. |
| `CosmosTransactionalBatchTest.SaveChanges_transaction_behavior_always_fails_for_single_entity_with_trigger_and_entity_without_trigger` | Expects `InvalidOperationException` (`…TriggerAtomicity`); instead the second save throws a `DbUpdateException` wrapping `409 Conflict`. Same root cause as the previous row — client-side atomicity check is not firing, so the request reaches the server. |

## How the data was collected

```bash
docker run --rm -d -p 8081:8081 \
    mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview

# In the repo
. ./activate.sh
dotnet build test/EFCore.Cosmos.FunctionalTests/EFCore.Cosmos.FunctionalTests.csproj

export Test__Cosmos__DefaultConnection=https://localhost:8081
export Test__Cosmos__EmulatorType=linux

dotnet test test/EFCore.Cosmos.FunctionalTests/EFCore.Cosmos.FunctionalTests.csproj \
    --no-build \
    --filter "FullyQualifiedName~CosmosTransactionalBatchTest|FullyQualifiedName~CosmosBulkExecutionTest|FullyQualifiedName~CosmosBulkConcurrencyTest|FullyQualifiedName=Microsoft.EntityFrameworkCore.EndToEndCosmosTest.Can_add_update_delete_end_to_end"
```

The skip attributes `[CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]` were removed
from each of the seven `// .../issues/292` sites for the run; once the data was collected
they were restored so the test suite continues to behave as before in CI. Only this
`docs/CosmosLinuxEmulatorIssues/` folder is committed.
