// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.SqlTypes;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerNetTopologySuiteTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    private readonly HashSet<string> _spatialStoreTypes = new(StringComparer.OrdinalIgnoreCase) { "geometry", "geography" };

    private readonly NtsGeometryServices _geometryServices;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerNetTopologySuiteTypeMappingSourcePlugin(NtsGeometryServices geometryServices)
    {
        _geometryServices = geometryServices;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;

        return typeof(Geometry).IsAssignableFrom(clrType)
            || (storeTypeName != null
                && _spatialStoreTypes.Contains(storeTypeName))
                ? (RelationalTypeMapping)Activator.CreateInstance(
                    typeof(SqlServerGeometryTypeMapping<>).MakeGenericType(
                        clrType is null || clrType == typeof(SqlBytes) ? typeof(Geometry) : clrType),
                    _geometryServices,
                    storeTypeName ?? "geography")!
                : null;
    }
}
