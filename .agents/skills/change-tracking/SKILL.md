---
name: change-tracking
description: 'Implementation details for EF Core change tracking. Use when changing InternalEntityEntry, ChangeDetector, SnapshotFactoryFactory, or related entity state, snapshot, or property accessor code.'
user-invocable: false
---

# Change Tracking

Manages entity states and detects changes for `SaveChanges()`.

## Core Components

- `StateManager` — owns tracked entries, identity/reference maps, fixup, cascades, notifications, and changed counts
- `InternalEntryBase` and derived classes — own per-entry state, flags, values, snapshots, and ordered state transitions
- `ChangeDetector` — compares current values with baselines and reports changes through entry mutation APIs
- `SnapshotFactoryFactory` subclasses — create passive original- and relationship-value baselines
- `IdentityMap` — permits one active entry per key; shared identity pairs a replacement with the prior `Deleted` entry
- `PropertyAccessorsFactory`, `ClrPropertyGetterFactory` and `ClrPropertySetterFactory` compile property accessors for efficient snapshotting and change detection
  - Ordinals in `indices` parameter specify element at each complex collection depth

## Change Detection

- Snapshot and notification strategies both call `SetPropertyModified()` to keep property flags and entity state consistent.
- `SetEntityState()` validates values, updates flags and complex entries, changes state, then runs manager bookkeeping hooks.

## Testing

Unit tests: `test/EFCore.Tests/ChangeTracking/`. Functional tests: `test/EFCore.Specification.Tests/GraphUpdates/`.

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| There is a failure when there is shared identity entry (Added and Deleted) | Add code that checks `SharedIdentityEntry` |

## Validation

- `DetectChanges()` identifies modified properties via snapshot comparison
- Setting original values marks properties as modified or unchanged based on comparison with current values
