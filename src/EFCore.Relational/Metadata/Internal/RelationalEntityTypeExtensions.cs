// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalEntityTypeExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public const int MaxEntityTypesSharingTable = 128;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IForeignKey> FindDeclaredReferencingRowInternalForeignKeys(
        this IEntityType entityType,
        StoreObjectIdentifier storeObject)
        => entityType.IsMappedToJson()
            ? Enumerable.Empty<IForeignKey>()
            : entityType.GetDeclaredReferencingForeignKeys().Where(fk => fk.IsRowInternal(storeObject));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsMainFragment(
        this IReadOnlyTypeBase type,
        StoreObjectIdentifier storeObject)
    {
        var storeObjectType = storeObject.StoreObjectType;
        var declaredStoreObject = StoreObjectIdentifier.Create(type, storeObjectType);
        if (declaredStoreObject != null)
        {
            return storeObject == declaredStoreObject;
        }

        if (storeObjectType is StoreObjectType.Function or StoreObjectType.SqlQuery)
        {
            return false;
        }

        if (type is not IReadOnlyEntityType entityType)
        {
            return false;
        }

        foreach (var derivedType in entityType.GetDirectlyDerivedTypes())
        {
            if (IsMainFragment(derivedType, storeObject))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ConfigurationSource? GetStoreObjectConfigurationSource(this IConventionEntityType entityType, StoreObjectType type)
        => type switch
        {
            StoreObjectType.Table => entityType.FindAnnotation(RelationalAnnotationNames.TableName)?.GetConfigurationSource(),
            StoreObjectType.View => entityType.FindAnnotation(RelationalAnnotationNames.ViewName)?.GetConfigurationSource(),
            StoreObjectType.SqlQuery => entityType.FindAnnotation(RelationalAnnotationNames.SqlQuery)?.GetConfigurationSource(),
            StoreObjectType.Function => entityType.FindAnnotation(RelationalAnnotationNames.FunctionName)?.GetConfigurationSource(),
            _ => StoredProcedure.GetStoredProcedureConfigurationSource(entityType, type),
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static List<IProperty> GetNonPrincipalSharedNonPkProperties(this IEntityType entityType, ITableBase table)
    {
        var principalEntityTypes = new HashSet<IEntityType>();
        PopulatePrincipalEntityTypes(table, entityType, principalEntityTypes);
        var properties = new List<IProperty>();
        foreach (var property in entityType.GetProperties())
        {
            if (property.IsPrimaryKey())
            {
                continue;
            }

            var column = table.FindColumn(property);
            if (column == null)
            {
                continue;
            }

            var propertyMappings = column.PropertyMappings;
            if (propertyMappings.Count() > 1
                && propertyMappings.Any(pm => principalEntityTypes.Contains(pm.TableMapping.TypeBase)))
            {
                continue;
            }

            properties.Add(property);
        }

        return properties;

        static void PopulatePrincipalEntityTypes(ITableBase table, IEntityType entityType, HashSet<IEntityType> entityTypes)
        {
            foreach (var linkingFk in table.GetRowInternalForeignKeys(entityType))
            {
                entityTypes.Add(linkingFk.PrincipalEntityType);
                PopulatePrincipalEntityTypes(table, linkingFk.PrincipalEntityType, entityTypes);
            }
        }
    }
}
