---
name: change-tracking
description: 'EF Core change tracking, entity states, StateManager, snapshot comparison, change detection, complex properties/collections, property values, property accessors, proxies. Use when working on InternalEntityEntry, ChangeDetector, or SnapshotFactoryFactory.'
user-invokable: false
---

# Change Tracking

Manages entity states and detects changes for `SaveChanges()`.

## When to Use

- Modifying how entity state transitions work
- Working on snapshot comparison or change detection
- Debugging property accessor expression trees or ordinal indexing
- Working on change tracking or lazy loading proxies

## Core Components

- `StateManager` — central engine, identity maps, tracks all entities
- `InternalEntityEntry` — per-entity state, property flags, snapshots
- `ChangeDetector` — calls `DetectChanges()` which compares snapshots
- `ChangeTracker` — public API wrapping StateManager

## Snapshots

Built by `SnapshotFactoryFactory` subclasses via compiled expression trees.

## Property Accessors

Compiled expression trees in `PropertyAccessorsFactory`:
- Ordinals in `indices` parameter specify element at each complex collection depth

Getters and setterscompiled lazily via `ClrPropertyGetterFactory` ands `ClrPropertySetterFactory`.

## Testing

Unit tests: `test/EFCore.Tests/ChangeTracking/`. Functional tests: `test/EFCore.Specification.Tests/GraphUpdates/`.

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| `SharedIdentityEntry` for deleted entities (table splitting) | These are skipped by `DetectChanges` to avoid double-processing |

## Validation

- `DetectChanges()` identifies modified properties via snapshot comparison
- Setting original values marks properties as modified or unchanged based on comparison with current values
