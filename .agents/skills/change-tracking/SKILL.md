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

Built by `SnapshotFactoryFactory` subclasses via compiled expression trees:
- `OriginalValuesFactoryFactory` — for detecting property changes
- `RelationshipSnapshotFactoryFactory` — for FK/navigation fix-up

## Property Accessors

Compiled expression trees in `PropertyAccessorsFactory`:
- `CreateMemberAccess()` — entity → complex property → nested property
- `CreateComplexCollectionElementAccess()` — entity → collection → element[ordinal] → property
- Ordinals in `indices` parameter specify element at each collection depth
- Guard expressions added when `DetailedErrorsEnabled` annotation is set on the model

Getters compiled lazily via `ClrPropertyGetterFactory` → `ClrPropertyGetter`. Two delegates:
- `GetClrValue(instance)` — from immediate declaring object
- `GetClrValueUsingContainingEntity(entity, indices)` — from root entity through complex chain

## Key Files

- `src/EFCore/ChangeTracking/Internal/StateManager.cs`
- `src/EFCore/ChangeTracking/Internal/InternalEntityEntry.cs`
- `src/EFCore/ChangeTracking/Internal/ChangeDetector.cs`
- `src/EFCore/Metadata/Internal/PropertyAccessorsFactory.cs`
- `src/EFCore/Metadata/Internal/ClrPropertyGetterFactory.cs`
- `src/EFCore/ChangeTracking/Internal/SnapshotFactoryFactory.cs`

## Testing

Unit tests: `test/EFCore.Tests/ChangeTracking/`. Functional tests: `test/EFCore.{Provider}.FunctionalTests/`.

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| `SharedIdentityEntry` for deleted entities (table splitting) | These are skipped by `DetectChanges` to avoid double-processing |

## Validation

- Entity states transition correctly (`Added` → `Unchanged` after save, etc.)
- `DetectChanges()` identifies modified properties via snapshot comparison
- Property accessor expression trees compile without errors at runtime
