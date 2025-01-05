// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteAnnotationProvider : RelationalAnnotationProvider
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        if (model.Tables.SelectMany(t => t.Columns).Any(
                c => SqliteTypeMappingSource.IsSpatialiteType(c.StoreType)))
        {
            yield return new Annotation(SqliteAnnotationNames.InitSpatialMetaData, true);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        // JSON columns have no property mappings so all annotations that rely on property mappings should be skipped for them
        if (column is JsonColumn)
        {
            yield break;
        }

        // Model validation ensures that these facets are the same on all mapped properties
        var property = column.PropertyMappings.First().Property;
        // Only return auto increment for integer single column primary key
        var primaryKey = property.DeclaringType.ContainingEntityType.FindPrimaryKey();
        if (primaryKey is { Properties.Count: 1 }
            && primaryKey.Properties[0] == property
            && property.ValueGenerated == ValueGenerated.OnAdd
            && property.ClrType.UnwrapNullableType().IsInteger()
            && !HasConverter(property))
        {
            yield return new Annotation(SqliteAnnotationNames.Autoincrement, true);
        }

        var srid = property.GetSrid();
        if (srid != null)
        {
            yield return new Annotation(SqliteAnnotationNames.Srid, srid);
        }
    }

    private static bool HasConverter(IProperty property)
        => property.FindTypeMapping()?.Converter != null;
}
