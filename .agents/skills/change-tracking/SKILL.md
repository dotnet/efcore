---
name: change-tracking
description: 'Implementation details for EF Core change tracking. Use when changing InternalEntityEntry, ChangeDetector, SnapshotFactoryFactory, or related entity state, snapshot, or property accessor code.'
user-invokable: false
---

# Change Tracking

Manages entity states and detects changes for `SaveChanges()`.

## Core Components

- `StateManager` — central engine, identity maps, tracks all entities
- `InternalEntityEntry` — per-entity state, property flags, snapshots
- `SnapshotFactoryFactory` subclasses build snapshot factories for change detection
- `PropertyAccessorsFactory`, `ClrPropertyGetterFactory` and `ClrPropertySetterFactory` compile property accessors for efficient snapshotting and change detection
  - Ordinals in `indices` parameter specify element at each complex collection depth

## Testing

Unit tests: `test/EFCore.Tests/ChangeTracking/`. Functional tests: `test/EFCore.Specification.Tests/GraphUpdates/`.

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| There is a failure when there is shared identity entry (Added and Deleted) | Add code that checks `SharedIdentityEntry` |

## Validation

- `DetectChanges()` identifies modified properties via snapshot comparison
- Setting original values marks properties as modified or unchanged based on comparison with current values
