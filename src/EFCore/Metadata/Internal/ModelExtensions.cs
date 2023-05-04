// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void SetProductVersion(this IMutableModel model, string value)
        => model[CoreAnnotationNames.ProductVersion] = value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IEntityType> GetRootEntityTypes(this IModel model)
        => model.GetEntityTypes().Where(e => e.BaseType == null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IEntityType> GetEntityTypesInHierarchicalOrder(this IModel model)
    {
        var entityTypes = new Queue<IEntityType>();

        foreach (var root in model.GetRootEntityTypes())
        {
            entityTypes.Enqueue(root);
        }

        while (entityTypes.Count > 0)
        {
            var current = entityTypes.Dequeue();

            yield return current;

            foreach (var descendant in current.GetDirectlyDerivedTypes())
            {
                entityTypes.Enqueue(descendant);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? FindSameTypeNameWithDifferentNamespace(this IModel model, Type type)
        => model.GetEntityTypes()
            .Where(x => x.ClrType.DisplayName(false) == type.DisplayName(false))
            .Select(x => x.ClrType.DisplayName())
            .FirstOrDefault();
}
