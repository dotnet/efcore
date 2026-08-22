// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Foreign key extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalForeignKeyExtensions
{
    /// <summary>
    ///     Returns the foreign key constraint name.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The foreign key constraint name.</returns>
    public static string? GetConstraintName(this IReadOnlyForeignKey foreignKey)
    {
        if (!foreignKey.IsConstrained)
        {
            return null;
        }

        var tableName = foreignKey.DeclaringEntityType.GetTableName();
        if (tableName == null)
        {
            return null;
        }

        var annotation = foreignKey.FindAnnotation(RelationalAnnotationNames.Name);
        return annotation != null
            ? (string?)annotation.Value
            : foreignKey.GetDefaultName();
    }

    /// <summary>
    ///     Returns the foreign key constraint name.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <param name="principalStoreObject">The identifier of the principal store object.</param>
    /// <returns>The foreign key constraint name.</returns>
    public static string? GetConstraintName(
        this IReadOnlyForeignKey foreignKey,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject)
        => foreignKey.GetConstraintName(storeObject, principalStoreObject, null);

    /// <summary>
    ///     Returns the default constraint name that would be used for this foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The default constraint name that would be used for this foreign key.</returns>
    public static string? GetDefaultName(this IReadOnlyForeignKey foreignKey)
    {
        var tableName = foreignKey.DeclaringEntityType.GetTableName();
        var principalTableName = foreignKey.PrincipalEntityType.GetTableName();
        if (tableName == null
            || principalTableName == null)
        {
            return null;
        }

        if (foreignKey.PrincipalEntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
            && foreignKey.PrincipalEntityType.GetDerivedTypes().Any(et => StoreObjectIdentifier.Create(et, StoreObjectType.Table) != null))
        {
            return null;
        }

        var name = new StringBuilder()
            .Append("FK_")
            .Append(tableName)
            .Append('_')
            .Append(principalTableName)
            .Append('_')
            .AppendJoin(foreignKey.Properties.Select(p => p.GetColumnName()), "_")
            .ToString();

        return Uniquifier.Truncate(name, foreignKey.DeclaringEntityType.Model.GetMaxIdentifierLength());
    }

    /// <summary>
    ///     Returns the default constraint name that would be used for this foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <param name="principalStoreObject">The identifier of the principal store object.</param>
    /// <returns>The default constraint name that would be used for this foreign key.</returns>
    public static string? GetDefaultName(
        this IReadOnlyForeignKey foreignKey,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject)
        => foreignKey.GetDefaultName(storeObject, principalStoreObject, null);

    /// <summary>
    ///     Sets the foreign key constraint name.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="value">The value to set.</param>
    public static void SetConstraintName(this IMutableForeignKey foreignKey, string? value)
        => foreignKey.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Name,
            Check.NullButNotEmpty(value));

    /// <summary>
    ///     Sets the foreign key constraint name.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured name.</returns>
    public static string? SetConstraintName(
        this IConventionForeignKey foreignKey,
        string? value,
        bool fromDataAnnotation = false)
        => (string?)foreignKey.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Name,
            Check.NullButNotEmpty(value),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the constraint name.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the constraint name.</returns>
    public static ConfigurationSource? GetConstraintNameConfigurationSource(this IConventionForeignKey foreignKey)
        => foreignKey.FindAnnotation(RelationalAnnotationNames.Name)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Sets the foreign key constraint name for a particular pair of dependent and principal store objects.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="name">The value to set. Use <see langword="null" /> to suppress a globally configured name for this pair.</param>
    /// <param name="storeObject">The identifier of the dependent store object.</param>
    /// <param name="principalStoreObject">The identifier of the principal store object.</param>
    public static void SetConstraintName(
        this IMutableForeignKey foreignKey,
        string? name,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject)
        => RelationalForeignKeyOverrides
            .GetOrCreate(foreignKey, new StoreObjectPair(storeObject, principalStoreObject), ConfigurationSource.Explicit)
            .SetName(Check.NullButNotEmpty(name), ConfigurationSource.Explicit);

    /// <summary>
    ///     Sets the foreign key constraint name for a particular pair of dependent and principal store objects.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="name">The value to set. Use <see langword="null" /> to suppress a globally configured name for this pair.</param>
    /// <param name="storeObject">The identifier of the dependent store object.</param>
    /// <param name="principalStoreObject">The identifier of the principal store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured name.</returns>
    public static string? SetConstraintName(
        this IConventionForeignKey foreignKey,
        string? name,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject,
        bool fromDataAnnotation = false)
    {
        var configurationSource = fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention;

        return RelationalForeignKeyOverrides
            .GetOrCreate(
                (IMutableForeignKey)foreignKey, new StoreObjectPair(storeObject, principalStoreObject), configurationSource)
            .SetName(Check.NullButNotEmpty(name), configurationSource);
    }

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the foreign key constraint name for a particular pair of dependent and
    ///     principal store objects.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the dependent store object.</param>
    /// <param name="principalStoreObject">The identifier of the principal store object.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the constraint name.</returns>
    public static ConfigurationSource? GetConstraintNameConfigurationSource(
        this IConventionForeignKey foreignKey,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject)
        => (RelationalForeignKeyOverrides.Find(foreignKey, new StoreObjectPair(storeObject, principalStoreObject))
            as IConventionRelationalForeignKeyOverrides)
            ?.GetNameConfigurationSource();

    /// <summary>
    ///     Returns all per-store-object-pair constraint name overrides configured for this foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The overrides.</returns>
    public static IEnumerable<IReadOnlyRelationalForeignKeyOverrides> GetOverrides(this IReadOnlyForeignKey foreignKey)
        => RelationalForeignKeyOverrides.Get(foreignKey) ?? [];

    /// <summary>
    ///     Returns all per-store-object-pair constraint name overrides configured for this foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The overrides.</returns>
    public static IEnumerable<IMutableRelationalForeignKeyOverrides> GetOverrides(this IMutableForeignKey foreignKey)
        => RelationalForeignKeyOverrides.Get(foreignKey)?.Cast<IMutableRelationalForeignKeyOverrides>() ?? [];

    /// <summary>
    ///     Returns all per-store-object-pair constraint name overrides configured for this foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The overrides.</returns>
    public static IEnumerable<IConventionRelationalForeignKeyOverrides> GetOverrides(this IConventionForeignKey foreignKey)
        => RelationalForeignKeyOverrides.Get(foreignKey)?.Cast<IConventionRelationalForeignKeyOverrides>() ?? [];

    /// <summary>
    ///     Returns all per-store-object-pair constraint name overrides configured for this foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The overrides.</returns>
    public static IEnumerable<IRelationalForeignKeyOverrides> GetOverrides(this IForeignKey foreignKey)
        => RelationalForeignKeyOverrides.Get(foreignKey)?.Cast<IRelationalForeignKeyOverrides>() ?? [];

    /// <summary>
    ///     Removes the per-store-object-pair constraint name override for the given pair of dependent and principal store objects.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the dependent store object.</param>
    /// <param name="principalStoreObject">The identifier of the principal store object.</param>
    /// <returns>The removed override, or <see langword="null" /> if none was configured.</returns>
    public static IMutableRelationalForeignKeyOverrides? RemoveOverrides(
        this IMutableForeignKey foreignKey,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject)
        => RelationalForeignKeyOverrides.Remove(foreignKey, new StoreObjectPair(storeObject, principalStoreObject));

    /// <summary>
    ///     Removes the per-store-object-pair constraint name override for the given pair of dependent and principal store objects.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the dependent store object.</param>
    /// <param name="principalStoreObject">The identifier of the principal store object.</param>
    /// <returns>The removed override, or <see langword="null" /> if none was configured.</returns>
    public static IConventionRelationalForeignKeyOverrides? RemoveOverrides(
        this IConventionForeignKey foreignKey,
        in StoreObjectIdentifier storeObject,
        in StoreObjectIdentifier principalStoreObject)
        => RelationalForeignKeyOverrides.Remove((IMutableForeignKey)foreignKey, new StoreObjectPair(storeObject, principalStoreObject));

    /// <summary>
    ///     Gets the foreign key constraints to which the foreign key is mapped.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The foreign key constraints to which the foreign key is mapped.</returns>
    public static IEnumerable<IForeignKeyConstraint> GetMappedConstraints(this IForeignKey foreignKey)
    {
        foreignKey.DeclaringEntityType.Model.EnsureRelationalModel();
        return (IEnumerable<IForeignKeyConstraint>?)foreignKey.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.ForeignKeyMappings)
            ?? [];
    }

    /// <summary>
    ///     <para>
    ///         Finds the first <see cref="IForeignKey" /> that is mapped to the same constraint in a shared table-like object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The foreign key if found, or <see langword="null" /> if none was found.</returns>
    public static IReadOnlyForeignKey? FindSharedObjectRootForeignKey(
        this IReadOnlyForeignKey foreignKey,
        in StoreObjectIdentifier storeObject)
    {
        if (foreignKey.PrincipalEntityType.GetTableName() is not { } principalTableName)
        {
            return null;
        }

        var foreignKeyName = foreignKey.GetConstraintName(
            storeObject,
            StoreObjectIdentifier.Table(principalTableName, foreignKey.PrincipalEntityType.GetSchema()));
        var rootForeignKey = foreignKey;

        // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
        // Using a hashset is detrimental to the perf when there are no cycles
        for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
        {
            IReadOnlyForeignKey? linkedForeignKey = null;
            foreach (var otherForeignKey in rootForeignKey.DeclaringEntityType
                         .FindRowInternalForeignKeys(storeObject)
                         .SelectMany(fk => fk.PrincipalEntityType.GetForeignKeys()))
            {
                principalTableName = otherForeignKey.PrincipalEntityType.GetTableName();

                if (principalTableName is null)
                {
                    return null;
                }

                if (otherForeignKey.GetConstraintName(
                        storeObject,
                        StoreObjectIdentifier.Table(principalTableName, otherForeignKey.PrincipalEntityType.GetSchema()))
                    == foreignKeyName)
                {
                    linkedForeignKey = otherForeignKey;
                    break;
                }
            }

            if (linkedForeignKey == null)
            {
                break;
            }

            rootForeignKey = linkedForeignKey;
        }

        return rootForeignKey == foreignKey ? null : rootForeignKey;
    }

    /// <summary>
    ///     Returns a value indicating whether this foreign key is between two entity types
    ///     sharing the same table-like store object.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    public static bool IsRowInternal(
        this IReadOnlyForeignKey foreignKey,
        StoreObjectIdentifier storeObject)
    {
        var entityType = foreignKey.DeclaringEntityType;
        var primaryKey = entityType.FindPrimaryKey();
        return primaryKey != null
            && !entityType.IsMappedToJson()
            && foreignKey.PrincipalKey.IsPrimaryKey()
            && !foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
            && foreignKey.Properties.SequenceEqual(primaryKey.Properties)
            && IsMapped(foreignKey, storeObject);

        static bool IsMapped(IReadOnlyForeignKey foreignKey, StoreObjectIdentifier storeObject)
            => (StoreObjectIdentifier.Create(foreignKey.DeclaringEntityType, storeObject.StoreObjectType) == storeObject
                    || foreignKey.DeclaringEntityType.GetMappingFragments(storeObject.StoreObjectType)
                        .Any(f => f.StoreObject == storeObject))
                && (StoreObjectIdentifier.Create(foreignKey.PrincipalEntityType, storeObject.StoreObjectType) == storeObject
                    || foreignKey.PrincipalEntityType.GetMappingFragments(storeObject.StoreObjectType)
                        .Any(f => f.StoreObject == storeObject));
    }

    /// <summary>
    ///     <para>
    ///         Finds the first <see cref="IMutableForeignKey" /> that is mapped to the same constraint in a shared table-like object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The foreign key if found, or <see langword="null" /> if none was found.</returns>
    public static IMutableForeignKey? FindSharedObjectRootForeignKey(
        this IMutableForeignKey foreignKey,
        in StoreObjectIdentifier storeObject)
        => (IMutableForeignKey?)((IReadOnlyForeignKey)foreignKey).FindSharedObjectRootForeignKey(storeObject);

    /// <summary>
    ///     <para>
    ///         Finds the first <see cref="IConventionForeignKey" /> that is mapped to the same constraint in a shared table-like object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The foreign key if found, or <see langword="null" /> if none was found.</returns>
    public static IConventionForeignKey? FindSharedObjectRootForeignKey(
        this IConventionForeignKey foreignKey,
        in StoreObjectIdentifier storeObject)
        => (IConventionForeignKey?)((IReadOnlyForeignKey)foreignKey).FindSharedObjectRootForeignKey(storeObject);

    /// <summary>
    ///     <para>
    ///         Finds the first <see cref="IForeignKey" /> that is mapped to the same constraint in a shared table-like object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="storeObject">The identifier of the containing store object.</param>
    /// <returns>The foreign key if found, or <see langword="null" /> if none was found.</returns>
    public static IForeignKey? FindSharedObjectRootForeignKey(
        this IForeignKey foreignKey,
        in StoreObjectIdentifier storeObject)
        => (IForeignKey?)((IReadOnlyForeignKey)foreignKey).FindSharedObjectRootForeignKey(storeObject);

    /// <summary>
    ///     Returns a value indicating whether the foreign key constraint is excluded from migrations.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns><see langword="true" /> if the foreign key constraint is excluded from migrations.</returns>
    public static bool IsExcludedFromMigrations(this IReadOnlyForeignKey foreignKey)
        => foreignKey is RuntimeForeignKey
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (bool?)foreignKey[RelationalAnnotationNames.IsForeignKeyExcludedFromMigrations] ?? false;

    /// <summary>
    ///     Sets a value indicating whether the foreign key constraint is excluded from migrations.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="excluded">The value to set.</param>
    public static void SetIsExcludedFromMigrations(this IMutableForeignKey foreignKey, bool? excluded)
        => foreignKey.SetOrRemoveAnnotation(RelationalAnnotationNames.IsForeignKeyExcludedFromMigrations, excluded);

    /// <summary>
    ///     Sets a value indicating whether the foreign key constraint is excluded from migrations.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="excluded">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsExcludedFromMigrations(
        this IConventionForeignKey foreignKey,
        bool? excluded,
        bool fromDataAnnotation = false)
        => (bool?)foreignKey.SetOrRemoveAnnotation(
            RelationalAnnotationNames.IsForeignKeyExcludedFromMigrations,
            excluded,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the foreign key exclusion from migrations.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the foreign key exclusion from migrations.</returns>
    public static ConfigurationSource? GetIsExcludedFromMigrationsConfigurationSource(this IConventionForeignKey foreignKey)
        => foreignKey.FindAnnotation(RelationalAnnotationNames.IsForeignKeyExcludedFromMigrations)
            ?.GetConfigurationSource();
}
