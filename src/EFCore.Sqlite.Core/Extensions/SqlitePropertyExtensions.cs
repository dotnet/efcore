// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IProperty" /> for SQLite metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqlitePropertyExtensions
{
    /// <summary>
    ///     Returns the <see cref="SqliteValueGenerationStrategy" /> to use for the property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The strategy to use for the property.</returns>
    public static SqliteValueGenerationStrategy GetValueGenerationStrategy(this IReadOnlyProperty property)
        => property[SqliteAnnotationNames.ValueGenerationStrategy] is SqliteValueGenerationStrategy strategy
            ? strategy
            : property.GetDefaultValueGenerationStrategy();

    /// <summary>
    ///     Returns the <see cref="SqliteValueGenerationStrategy" /> to use for the property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The strategy to use for the property.</returns>
    public static SqliteValueGenerationStrategy GetValueGenerationStrategy(
        this IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject)
    {
        var annotation = property.FindAnnotation(SqliteAnnotationNames.ValueGenerationStrategy);
        if (annotation != null)
        {
            return (SqliteValueGenerationStrategy)annotation.Value!;
        }

        var sharedProperty = property.FindSharedStoreObjectRootProperty(storeObject);
        return sharedProperty != null
            ? sharedProperty.GetValueGenerationStrategy(storeObject)
            : property.GetDefaultValueGenerationStrategy();
    }

    /// <summary>
    ///     Returns the default <see cref="SqliteValueGenerationStrategy" /> to use for the property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The default strategy for the property.</returns>
    public static SqliteValueGenerationStrategy GetDefaultValueGenerationStrategy(this IReadOnlyProperty property)
    {
        if (property.TryGetDefaultValue(out _)
            || property.GetDefaultValueSql() != null
            || property.GetComputedColumnSql() != null
            || property.IsForeignKey()
            || property.ValueGenerated == ValueGenerated.Never)
        {
            return SqliteValueGenerationStrategy.None;
        }

        var primaryKey = property.DeclaringType.ContainingEntityType.FindPrimaryKey();
        if (primaryKey is not { Properties.Count: 1 }
            || primaryKey.Properties[0] != property
            || !property.ClrType.UnwrapNullableType().IsInteger())
        {
            return SqliteValueGenerationStrategy.None;
        }

        // Check if provider type is also integer (important for value converters)
        var typeMapping = property.FindRelationalTypeMapping();
        var providerType = typeMapping?.Converter?.ProviderClrType ?? typeMapping?.ClrType ?? property.ClrType;
        
        // Only apply autoincrement by convention if this is a strongly-typed ID with a value converter
        // or other specific scenarios where autoincrement is expected
        return providerType.UnwrapNullableType().IsInteger() && typeMapping?.Converter != null
            ? SqliteValueGenerationStrategy.Autoincrement
            : SqliteValueGenerationStrategy.None;
    }

    internal static SqliteValueGenerationStrategy GetValueGenerationStrategy(
        this IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        var @override = property.FindOverrides(storeObject)?.FindAnnotation(SqliteAnnotationNames.ValueGenerationStrategy);
        if (@override != null)
        {
            return (SqliteValueGenerationStrategy?)@override.Value ?? SqliteValueGenerationStrategy.None;
        }

        var annotation = property.FindAnnotation(SqliteAnnotationNames.ValueGenerationStrategy);
        if (annotation?.Value != null
            && StoreObjectIdentifier.Create(property.DeclaringType, storeObject.StoreObjectType) == storeObject)
        {
            return (SqliteValueGenerationStrategy)annotation.Value;
        }

        var sharedProperty = property.FindSharedStoreObjectRootProperty(storeObject);
        return sharedProperty != null
            ? sharedProperty.GetValueGenerationStrategy(storeObject, typeMappingSource)
            : GetDefaultValueGenerationStrategy(property, storeObject, typeMappingSource);
    }

    private static SqliteValueGenerationStrategy GetDefaultValueGenerationStrategy(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table
            || property.IsForeignKey()
            || property.ValueGenerated == ValueGenerated.Never)
        {
            return SqliteValueGenerationStrategy.None;
        }

        var primaryKey = property.DeclaringType.ContainingEntityType.FindPrimaryKey();
        if (primaryKey is not { Properties.Count: 1 }
            || primaryKey.Properties[0] != property
            || !property.ClrType.UnwrapNullableType().IsInteger())
        {
            return SqliteValueGenerationStrategy.None;
        }

        // Check if provider type is also integer (important for value converters)
        var typeMapping = property.FindRelationalTypeMapping(storeObject) 
            ?? typeMappingSource?.FindMapping((IProperty)property);
        var providerType = typeMapping?.Converter?.ProviderClrType ?? typeMapping?.ClrType ?? property.ClrType;
        
        // Only apply autoincrement by convention if this is a strongly-typed ID with a value converter
        // or other specific scenarios where autoincrement is expected
        return providerType.UnwrapNullableType().IsInteger() && typeMapping?.Converter != null
            ? SqliteValueGenerationStrategy.Autoincrement
            : SqliteValueGenerationStrategy.None;
    }

    /// <summary>
    ///     Sets the <see cref="SqliteValueGenerationStrategy" /> to use for the property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The strategy to use.</param>
    public static void SetValueGenerationStrategy(
        this IMutableProperty property,
        SqliteValueGenerationStrategy? value)
        => property.SetOrRemoveAnnotation(SqliteAnnotationNames.ValueGenerationStrategy, value);

    /// <summary>
    ///     Sets the <see cref="SqliteValueGenerationStrategy" /> to use for the property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The strategy to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static SqliteValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionProperty property,
        SqliteValueGenerationStrategy? value,
        bool fromDataAnnotation = false)
        => (SqliteValueGenerationStrategy?)property.SetOrRemoveAnnotation(
            SqliteAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the value generation strategy.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the value generation strategy.</returns>
    public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(this IConventionProperty property)
        => property.FindAnnotation(SqliteAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();


    /// <summary>
    ///     Returns the SRID to use when creating a column for this property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The SRID to use when creating a column for this property.</returns>
    public static int? GetSrid(this IReadOnlyProperty property)
        => (int?)property[SqliteAnnotationNames.Srid];

    /// <summary>
    ///     Returns the SRID to use when creating a column for this property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The SRID to use when creating a column for this property.</returns>
    public static int? GetSrid(
        this IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject)
    {
        var annotation = property.FindAnnotation(SqliteAnnotationNames.Srid);
        if (annotation != null)
        {
            return (int?)annotation.Value;
        }

        return property.FindSharedStoreObjectRootProperty(storeObject)?.GetSrid(storeObject);
    }

    /// <summary>
    ///     Sets the SRID to use when creating a column for this property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The SRID.</param>
    public static void SetSrid(this IMutableProperty property, int? value)
        => property.SetOrRemoveAnnotation(SqliteAnnotationNames.Srid, value);

    /// <summary>
    ///     Sets the SRID to use when creating a column for this property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The SRID.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static int? SetSrid(this IConventionProperty property, int? value, bool fromDataAnnotation = false)
        => (int?)property.SetOrRemoveAnnotation(SqliteAnnotationNames.Srid, value, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the column SRID.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the column SRID.</returns>
    public static ConfigurationSource? GetSridConfigurationSource(this IConventionProperty property)
        => property.FindAnnotation(SqliteAnnotationNames.Srid)?.GetConfigurationSource();
}
