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
    public static IEnumerable<ITableMappingBase> GetViewOrTableMappings(this IEntityType entityType)
        => (IEnumerable<ITableMappingBase>?)(entityType.FindRuntimeAnnotationValue(
                    RelationalAnnotationNames.ViewMappings)
                ?? entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableMappings))
            ?? Enumerable.Empty<ITableMappingBase>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<string> GetTptDiscriminatorValues(this IReadOnlyEntityType entityType)
        => entityType.GetConcreteDerivedTypesInclusive().Select(et => et.ShortName()).ToList();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<IProperty> GetNonPrincipalSharedNonPkProperties(this IEntityType entityType, ITableBase table)
    {
        var nonPrincipalSharedProperties = new List<IProperty>();
        var principalEntityTypes = new HashSet<IEntityType>();
        PopulatePrincipalEntityTypes(table, entityType, principalEntityTypes);
        foreach (var property in entityType.GetProperties())
        {
            if (property.IsPrimaryKey())
            {
                continue;
            }

            var propertyMappings = table.FindColumn(property)!.PropertyMappings;
            if (propertyMappings.Count() > 1
                && propertyMappings.Any(pm => principalEntityTypes.Contains(pm.TableMapping.EntityType)))
            {
                continue;
            }

            nonPrincipalSharedProperties.Add(property);
        }

        return nonPrincipalSharedProperties;

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
