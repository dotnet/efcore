// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalTypeBaseExtensions
{
    /// <summary>
    ///     Returns the storage mappings the type queries against, in priority order: default SQL query, default function,
    ///     view, then table. The first non-empty set wins; an empty enumerable means the type has no
    ///     real (non-default) storage and queries should fall back to default mappings.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Only the "default" function/SQL query mappings (those configured via <c>ToFunction</c>/<c>ToSqlQuery</c>
    ///         on the entity) participate in the priority; additional <c>HasDbFunction</c>-style mappings remain
    ///         invocation-only and never shadow the entity's view/table mapping for <c>Set&lt;T&gt;()</c> queries.
    ///     </para>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    /// </remarks>
    public static IEnumerable<ITableMappingBase> GetQueryMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        if (typeBase.FindRuntimeAnnotationValue(RelationalAnnotationNames.SqlQueryMappings) is List<SqlQueryMapping> sqlQueryMappings
            && GetDefaults(sqlQueryMappings, static m => m.IsDefaultSqlQueryMapping) is { } defaultSqlQueryMappings)
        {
            return defaultSqlQueryMappings;
        }

        if (typeBase.FindRuntimeAnnotationValue(RelationalAnnotationNames.FunctionMappings) is List<FunctionMapping> functionMappings
            && GetDefaults(functionMappings, static m => m.IsDefaultFunctionMapping) is { } defaultFunctionMappings)
        {
            return defaultFunctionMappings;
        }

        var viewMappings = typeBase.GetViewMappings();
        return viewMappings.Any() ? viewMappings : typeBase.GetTableMappings();

        static List<T>? GetDefaults<T>(List<T> mappings, Func<T, bool> isDefault)
        {
            var count = 0;
            for (var i = 0; i < mappings.Count; i++)
            {
                if (isDefault(mappings[i]))
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return null;
            }

            if (count == mappings.Count)
            {
                return mappings;
            }

            var defaults = new List<T>(count);
            for (var i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                if (isDefault(mapping))
                {
                    defaults.Add(mapping);
                }
            }

            return defaults;
        }
    }

    /// <summary>
    ///     Returns the entity type's query mappings scoped to the tables actually being projected (when known via
    ///     <paramref name="tableMap" />); falls back to all query mappings when no projected mapping qualifies. Used by
    ///     query translation sites that need to pick the principal split-entity table for an entity reference.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="tableMap">The tables being projected from in the containing query, or <see langword="null" /> when unknown.</param>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static List<ITableMappingBase> GetProjectedQueryMappings(
        this IEntityType entityType,
        IReadOnlyDictionary<ITableBase, string>? tableMap)
    {
        var allMappings = entityType.GetQueryMappings();
        if (tableMap is null)
        {
            return [.. allMappings];
        }

        var projected = new List<ITableMappingBase>();
        foreach (var mapping in allMappings)
        {
            if (tableMap.ContainsKey(mapping.Table))
            {
                projected.Add(mapping);
            }
        }

        return projected.Count > 0 ? projected : [.. allMappings];
    }
}
