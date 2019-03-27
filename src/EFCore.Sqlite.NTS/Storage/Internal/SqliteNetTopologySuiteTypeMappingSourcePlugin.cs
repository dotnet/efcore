// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using GeoAPI;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteNetTopologySuiteTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        private static readonly Dictionary<string, Type> _storeTypeMappings
            = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "GEOMETRY", typeof(IGeometry) },
                { "GEOMETRYCOLLECTION", typeof(IGeometryCollection) },
                { "LINESTRING", typeof(ILineString) },
                { "MULTILINESTRING", typeof(IMultiLineString) },
                { "MULTIPOINT", typeof(IMultiPoint) },
                { "MULTIPOLYGON", typeof(IMultiPolygon) },
                { "POINT", typeof(IPoint) },
                { "POLYGON", typeof(IPolygon) }
            };

        private readonly IGeometryServices _geometryServices;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteNetTopologySuiteTypeMappingSourcePlugin([NotNull] IGeometryServices geometryServices)
        {
            Check.NotNull(geometryServices, nameof(geometryServices));

            _geometryServices = geometryServices;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            string defaultStoreType = null;
            Type defaultClrType = null;

            return (clrType != null
                    && TryGetDefaultStoreType(clrType, out defaultStoreType))
                   || (storeTypeName != null
                       && _storeTypeMappings.TryGetValue(storeTypeName, out defaultClrType))
                ? (RelationalTypeMapping)Activator.CreateInstance(
                    typeof(SqliteGeometryTypeMapping<>).MakeGenericType(clrType ?? defaultClrType ?? typeof(IGeometry)),
                    _geometryServices,
                    storeTypeName ?? defaultStoreType ?? "GEOMETRY")
                : null;
        }

        private static bool TryGetDefaultStoreType(Type clrType, out string defaultStoreType)
        {
            if (typeof(ILineString).IsAssignableFrom(clrType))
            {
                defaultStoreType = "LINESTRING";
            }
            else if (typeof(IMultiLineString).IsAssignableFrom(clrType))
            {
                defaultStoreType = "MULTILINESTRING";
            }
            else if (typeof(IMultiPoint).IsAssignableFrom(clrType))
            {
                defaultStoreType = "MULTIPOINT";
            }
            else if (typeof(IMultiPolygon).IsAssignableFrom(clrType))
            {
                defaultStoreType = "MULTIPOLYGON";
            }
            else if (typeof(IPoint).IsAssignableFrom(clrType))
            {
                defaultStoreType = "POINT";
            }
            else if (typeof(IPolygon).IsAssignableFrom(clrType))
            {
                defaultStoreType = "POLYGON";
            }
            else if (typeof(IGeometryCollection).IsAssignableFrom(clrType))
            {
                defaultStoreType = "GEOMETRYCOLLECTION";
            }
            else if (typeof(IGeometry).IsAssignableFrom(clrType))
            {
                defaultStoreType = "GEOMETRY";
            }
            else
            {
                defaultStoreType = null;
            }

            return defaultStoreType != null;
        }
    }
}
