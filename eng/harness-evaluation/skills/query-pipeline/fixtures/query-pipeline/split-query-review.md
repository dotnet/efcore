# Completion Proposal

The C# file contains reduced proposed excerpts from the split-query coordinator
and both enumerables. Setup, logging, and existing helper definitions are omitted;
this is a review input, not a standalone compilable program.

`VerifyNoOrphanedChildRows()` rejects child rows left over after consuming the
outer result. It may throw. Loader calls may leave a child row pending for a later outer row.
`CallerBuffersResults` is supplied by a wrapper that uses `ToList` or `ToListAsync`;
it is false when the wrapper exposes an enumerable directly.

The proposal introduces a shared completion helper so validation can accompany
returning a completed result or releasing its reader. It bypasses the check for
callers that materialize a list. The helper has been added to the shown consumers;
the remaining wiring follows the existing split-query implementation.

## Rollout requirements

The workload has 50,000 roots and ten children per root, with sizable string
projections. The deployment permits only one active reader per connection and
has a retrying execution strategy enabled. Some callers materialize a list;
others retain only the current root or group. The rollout's memory target allows
only the current root/group and its children beyond caller-owned retention.

Root and child commands run without an explicit transaction. Writers update
root and child values in a single transaction while readers are active. The
service contract requires a graph corresponding to one database state, not just
valid parent keys. Assess whether the proposal meets both rollout requirements.

## Test attachment points

The complete project supplies reader, shaper, loader, and context declarations
omitted here. Tests may add minimal declarations and local reader/loader doubles
to these reduced types, or use the existing provider test infrastructure with
precise attachment points. Exercise the changed consumers, not a parallel copy
of their algorithm. Supply bodies for any new helper and document integration
bindings omitted from the excerpts.

Reader doubles can expose read and dispose counters and inject exceptions.
Verification instrumentation can count calls and throw a fixed exception.
`HasNext` is tri-state: null asks the reader, true holds a prefetched row, and
false records EOF. A grouped move can physically read EOF while returning a
nonempty final group. Existing tests permit repeated terminal moves and disposal;
a reader already observed at EOF must not be read again.

Existing smoke coverage enumerates a single group synchronously, materializes a
normal result with `ToListAsync`, and checks successful reader disposal.