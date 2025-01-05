// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that finds entity types which share a table
///     which has a concurrency token column where those entity types
///     do not have a property mapped to that column. It then
///     creates a shadow concurrency property mapped to that column
///     on the base-most entity type(s).
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class TableSharingConcurrencyTokenConvention : IModelFinalizingConvention
{
    private const string ConcurrencyPropertyPrefix = "_TableSharingConcurrencyTokenConvention_";

    /// <summary>
    ///     Creates a new instance of <see cref="TableSharingConcurrencyTokenConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public TableSharingConcurrencyTokenConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        var tableToEntityTypes = new Dictionary<StoreObjectIdentifier, List<IConventionEntityType>>();
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
            if (table == null)
            {
                continue;
            }

            if (!tableToEntityTypes.TryGetValue(table.Value, out var mappedTypes))
            {
                mappedTypes = [];
                tableToEntityTypes[table.Value] = mappedTypes;
            }

            mappedTypes.Add(entityType);
        }

        foreach (var (table, mappedTypes) in tableToEntityTypes)
        {
            var concurrencyColumns = GetConcurrencyTokensMap(table, mappedTypes);
            if (concurrencyColumns == null)
            {
                continue;
            }

            foreach (var (concurrencyColumnName, readOnlyProperties) in concurrencyColumns)
            {
                Dictionary<IConventionEntityType, IReadOnlyProperty>? entityTypesMissingConcurrencyColumn = null;
                foreach (var entityType in mappedTypes)
                {
                    Check.DebugAssert(readOnlyProperties.Count != 0, $"No properties mapped to column '{concurrencyColumnName}'");

                    var foundMappedProperty = !IsConcurrencyTokenMissing(readOnlyProperties, entityType, mappedTypes)
                        || entityType.GetProperties()
                            .Any(p => p.GetColumnName(table) == concurrencyColumnName);

                    if (!foundMappedProperty)
                    {
                        entityTypesMissingConcurrencyColumn ??= new Dictionary<IConventionEntityType, IReadOnlyProperty>();

                        // store the concurrency token property to be used later as a template
                        entityTypesMissingConcurrencyColumn.Add(entityType, readOnlyProperties.First());
                    }
                }

                if (entityTypesMissingConcurrencyColumn == null)
                {
                    continue;
                }

                RemoveDerivedEntityTypes(entityTypesMissingConcurrencyColumn, mappedTypes);

                foreach (var (conventionEntityType, exampleProperty) in entityTypesMissingConcurrencyColumn)
                {
                    var propertyBuilder = conventionEntityType.Builder.CreateUniqueProperty(
                            exampleProperty.ClrType,
                            ConcurrencyPropertyPrefix + exampleProperty.Name,
                            !exampleProperty.IsNullable)!
                        .HasColumnName(concurrencyColumnName)!
                        .HasColumnType(exampleProperty.GetColumnType())!
                        .IsConcurrencyToken(true)!
                        .ValueGenerated(exampleProperty.ValueGenerated)!;

                    var typeMapping = exampleProperty.FindTypeMapping();
                    if (typeMapping != null)
                    {
                        propertyBuilder = propertyBuilder.HasTypeMapping(typeMapping)!;
                    }

                    var converter = exampleProperty.GetValueConverter();
                    if (converter != null)
                    {
                        propertyBuilder = propertyBuilder.HasConversion(converter)!;
                    }

                    var providerType = exampleProperty.GetProviderClrType();
                    if (providerType != propertyBuilder.Metadata.GetProviderClrType())
                    {
                        propertyBuilder = propertyBuilder.HasConversion(providerType)!;
                    }

                    var comparer = exampleProperty.GetValueComparer();
                    if (comparer != null)
                    {
                        propertyBuilder.HasValueComparer(comparer);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static Dictionary<string, List<IReadOnlyProperty>>? GetConcurrencyTokensMap(
        in StoreObjectIdentifier storeObject,
        IReadOnlyList<IReadOnlyEntityType> mappedTypes)
    {
        if (mappedTypes.Count < 2)
        {
            return null;
        }

        Dictionary<string, List<IReadOnlyProperty>>? concurrencyColumns = null;
        var nonHierarchyTypesCount = 0;
        foreach (var entityType in mappedTypes)
        {
            if ((entityType.BaseType == null && !mappedTypes.Any(t => t.BaseType == entityType))
                || (entityType.BaseType != null && !mappedTypes.Contains(entityType.BaseType)))
            {
                nonHierarchyTypesCount++;
            }

            concurrencyColumns = FindConcurrencyColumns(entityType, storeObject, concurrencyColumns);
        }

        return nonHierarchyTypesCount < 1 ? null : concurrencyColumns;

        static Dictionary<string, List<IReadOnlyProperty>>? FindConcurrencyColumns(
            IReadOnlyTypeBase structuralType,
            StoreObjectIdentifier storeObject,
            Dictionary<string, List<IReadOnlyProperty>>? concurrencyColumns)
        {
            foreach (var property in structuralType.GetProperties())
            {
                if (!property.IsConcurrencyToken
                    || (property.ValueGenerated & ValueGenerated.OnUpdate) == 0)
                {
                    continue;
                }

                var columnName = property.GetColumnName(storeObject);
                if (columnName == null)
                {
                    continue;
                }

                concurrencyColumns ??= new Dictionary<string, List<IReadOnlyProperty>>();
                if (!concurrencyColumns.TryGetValue(columnName, out var properties))
                {
                    properties = [];
                    concurrencyColumns[columnName] = properties;
                }

                properties.Add(property);
            }

            foreach (var complexProperty in structuralType.GetComplexProperties())
            {
                concurrencyColumns = FindConcurrencyColumns(complexProperty.ComplexType, storeObject, concurrencyColumns);
            }

            return concurrencyColumns;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static bool IsConcurrencyTokenMissing(
        List<IReadOnlyProperty> propertiesMappedToConcurrencyColumn,
        IReadOnlyEntityType entityType,
        IReadOnlyList<IReadOnlyEntityType> mappedTypes)
    {
        if (entityType.FindPrimaryKey() == null)
        {
            return false;
        }

        var propertyMissing = true;
        foreach (var mappedProperty in propertiesMappedToConcurrencyColumn)
        {
            var containingEntityType = mappedProperty.DeclaringType.ContainingEntityType;
            if (containingEntityType.IsAssignableFrom(entityType)
                || entityType.IsAssignableFrom(containingEntityType)
                || containingEntityType.IsInOwnershipPath(entityType)
                || entityType.IsInOwnershipPath(containingEntityType))
            {
                // The concurrency token is on the base type, derived type, a contained complex type or in the same aggregate
                propertyMissing = false;
                continue;
            }

            var linkingFks = containingEntityType.FindForeignKeys(containingEntityType.FindPrimaryKey()!.Properties)
                .Where(
                    fk => fk.PrincipalKey.IsPrimaryKey()
                        && mappedTypes.Contains(fk.PrincipalEntityType)).ToList();
            if (linkingFks != null
                && linkingFks.Count > 0
                && linkingFks.All(fk => fk.PrincipalEntityType != entityType)
                && linkingFks.Any(
                    fk => fk.PrincipalEntityType.IsAssignableFrom(entityType)
                        || entityType.IsAssignableFrom(fk.PrincipalEntityType)))
            {
                // The concurrency token is on a type that shares the row with a base or derived type
                propertyMissing = false;
            }
        }

        return propertyMissing;
    }

    private static void RemoveDerivedEntityTypes(
        Dictionary<IConventionEntityType, IReadOnlyProperty> entityTypeDictionary,
        List<IConventionEntityType> mappedTypes)
    {
        foreach (var (entityType, property) in entityTypeDictionary)
        {
            var removed = false;
            var baseType = entityType.BaseType;
            while (baseType != null)
            {
                if (entityTypeDictionary.Remove(entityType))
                {
                    removed = true;
                    break;
                }

                baseType = baseType.BaseType;
            }

            if (!removed
                && entityType.IsAssignableFrom(property.DeclaringType))
            {
                entityTypeDictionary.Remove(entityType);
            }
        }
    }
}
