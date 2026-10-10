# Source Context

The processor snapshot is a reduced pre-repair implementation. Constructor setup and unrelated annotation, sequence, and ownership compatibility helpers are omitted. This is a reading fixture, not a standalone build; it represents the released-package behavior in the report rather than every fix in the current checkout.

## MigrationsScaffolder.ScaffoldMigration

```csharp
var modelSnapshot = Dependencies.MigrationsAssembly.ModelSnapshot;
var lastModel = Dependencies.SnapshotModelProcessor.Process(modelSnapshot?.Model)?.GetRelationalModel();
var upOperations = Dependencies.MigrationsModelDiffer
    .GetDifferences(lastModel, Dependencies.Model.GetRelationalModel());
var downOperations = upOperations.Count > 0
    ? Dependencies.MigrationsModelDiffer.GetDifferences(Dependencies.Model.GetRelationalModel(), lastModel)
    : [];
```

## RelationalPropertyExtensions.IsColumnNullable

```csharp
public static bool IsColumnNullable(this IReadOnlyProperty property)
    => property.IsNullable
        || (property.DeclaringType.ContainingEntityType is { } entityType
            && entityType.BaseType != null
            && entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy)
        || (property.DeclaringType is IReadOnlyComplexType complexType
            && IsNullable(complexType.ComplexProperty));

private static bool IsNullable(IReadOnlyComplexProperty complexProperty)
    => complexProperty.IsNullable
        || (complexProperty.DeclaringType is IReadOnlyComplexType complexType
            && IsNullable(complexType.ComplexProperty));
```

## MigrationsModelDiffer.Diff

```csharp
var isNullableChanged = source.IsNullable != target.IsNullable;
var columnTypeChanged = source.StoreType != target.StoreType;

if (isNullableChanged
    || columnTypeChanged
    || !MultilineEquals(source.DefaultValueSql, target.DefaultValueSql)
    || !MultilineEquals(source.ComputedColumnSql, target.ComputedColumnSql)
    || source.IsStored != target.IsStored
    || sourceDefault?.GetType() != targetDefault?.GetType()
    || (sourceDefault != DBNull.Value && !target.ProviderValueComparer.Equals(sourceDefault, targetDefault))
    || !MultilineEquals(source.Comment, target.Comment)
    || source.Collation != target.Collation
    || source.Order != target.Order
    || HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
{
    var isDestructiveChange = (isNullableChanged && source.IsNullable)
        || columnTypeChanged;

    var alterColumnOperation = new AlterColumnOperation
    {
        Schema = table.Schema,
        Table = table.Name,
        Name = target.Name,
        IsDestructiveChange = isDestructiveChange
    };

    InitializeColumnHelper(alterColumnOperation, target, inline: !source.IsNullable);
    InitializeColumnHelper(alterColumnOperation.OldColumn, source, inline: true);
}
```

## Metadata Construction

`ModelSnapshot.Model` calls `BuildModel` with the parameterless `ModelBuilder`, which does not run the application's model conventions. A generic `ComplexProperty<TProperty>` uses `typeof(TProperty)` for the constructed metadata. `ClrType.IsNullableType()` returns true for reference types or `Nullable<T>`; C# nullable annotations do not change this runtime type check. The staged `ComplexProperty.cs` supplies the default and explicitly configured nullability behavior.