# Source Excerpts

These frozen excerpts supply supporting code outside the C# snapshots.

## Snapshot Provenance

The full-file snapshots were copied unchanged from the EF Core working tree on
2026-10-09. The `before/` files retain those copies; the `proposed/` files derive
from the corresponding `before/` snapshots. The `context/` snapshot is shared by
both versions. SHA-256 values below identify the unchanged source bytes.

| Snapshot | Repository Source | SHA-256 |
| --- | --- | --- |
| `before/ColumnAccessorsFactory.cs` | `src/EFCore.Relational/Update/Internal/ColumnAccessorsFactory.cs` | `663551ab34aed2594e5feced47c0afa3f6a76f9dbc26d77fb87715943962e8dc` |
| `before/NonSharedModelUpdatesTestBase.cs` | `test/EFCore.Relational.Specification.Tests/Update/NonSharedModelUpdatesTestBase.cs` | `f8519dee3f0a6149cf8111e903a7b3cb07e99b85073639ab18e44aaa59539b32` |
| `context/CommandBatchPreparer.cs` | `src/EFCore.Relational/Update/Internal/CommandBatchPreparer.cs` | `65368acbd5bae0ce4b6459b77c3c03ab65f8a1f964446bf10d8a6a0188604312` |

## src/EFCore.Relational/Update/ModificationCommand.cs

From the tracked-command constructor:

```csharp
EntityState = EntityState.Modified;
```

From `AddEntry(IUpdateEntry entry, bool mainEntry)`:

```csharp
if (mainEntry)
{
    Check.DebugAssert(!_mainEntryAdded, "Only expected a single main entry");

    for (var i = 0; i < _entries.Count; i++)
    {
        ValidateState(entry, _entries[i]);
    }

    _mainEntryAdded = true;
    _entries.Insert(0, entry);

    _entityState = entry.SharedIdentityEntry == null
        ? entry.EntityState
        : entry.SharedIdentityEntry.EntityType == entry.EntityType
        || entry.SharedIdentityEntry.EntityType.GetTableMappings()
            .Any(m => m.Table.Name == TableName && m.Table.Schema == Schema)
            ? EntityState.Modified
            : entry.EntityState;

    if (_entityState == EntityState.Modified
        && IsOptionalSplitFragmentRowAssumedAbsent)
    {
        _entityState = EntityState.Added;
    }
    else if (_entityState == EntityState.Modified
        && IsOptionalSplitFragmentPayloadAllNull)
    {
        _entityState = EntityState.Deleted;
    }
}
else
{
    if (_mainEntryAdded)
    {
        ValidateState(_entries[0], entry);
    }

    _entries.Add(entry);
}
```

From `ValidateState(IUpdateEntry mainEntry, IUpdateEntry entry)`:

```csharp
var mainEntryState = mainEntry.SharedIdentityEntry == null
    ? mainEntry.EntityState
    : EntityState.Modified;
if (mainEntryState == EntityState.Modified)
{
    return;
}

var entryState = entry.SharedIdentityEntry == null
    ? entry.EntityState
    : EntityState.Modified;
```

## src/EFCore.Relational/Update/Internal/RowForeignKeyValueFactoryFactory.cs

```csharp
public static IRowForeignKeyValueFactory CreateSimpleNonNullableFactory<TKey, TForeignKey>(
    IForeignKeyConstraint foreignKey)
    where TKey : struct
{
    var dependentColumn = foreignKey.Columns.First();
    var columnAccessors = ((Column)dependentColumn).Accessors;

    return dependentColumn.IsNullable
        ? new SimpleNullableRowForeignKeyValueFactory<TKey, TForeignKey>(foreignKey, dependentColumn, columnAccessors)
        : new SimpleNonNullableRowForeignKeyValueFactory<TKey, TForeignKey>(
            foreignKey, dependentColumn, columnAccessors);
}
```

## src/EFCore.Relational/Update/Internal/RowForeignKeyValueFactory.cs

```csharp
public virtual object? CreateDependentEquatableKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
    => TryCreateDependentKeyValue(command, fromOriginalValues, out var keyValue)
        ? new EquatableKeyValue<TKey>(_foreignKey, keyValue, EqualityComparer)
        : null;
```

## src/EFCore.Relational/Update/Internal/SimpleNullableRowForeignKeyValueFactory.cs

```csharp
public override bool TryCreateDependentKeyValue(
    IReadOnlyModificationCommand command,
    bool fromOriginalValues,
    [NotNullWhen(true)] out TKey key)
{
    var (keyValue, present) = fromOriginalValues
        ? ((Func<IReadOnlyModificationCommand, (TKey, bool)>)ColumnAccessors.OriginalValueGetter)(command)
        : ((Func<IReadOnlyModificationCommand, (TKey, bool)>)ColumnAccessors.CurrentValueGetter)(command);
    return HandleNullableValue(present ? keyValue : null, out key);
}
```

## src/EFCore.Relational/Update/Internal/SimpleNonNullableRowForeignKeyValueFactory.cs

```csharp
public override bool TryCreateDependentKeyValue(
    IReadOnlyModificationCommand command,
    bool fromOriginalValues,
    [NotNullWhen(true)] out TKey? key)
{
    (key, var present) = fromOriginalValues
        ? ((Func<IReadOnlyModificationCommand, (TKey, bool)>)ColumnAccessors.OriginalValueGetter)(command)
        : ((Func<IReadOnlyModificationCommand, (TKey, bool)>)ColumnAccessors.CurrentValueGetter)(command);
    return present;
}
```
