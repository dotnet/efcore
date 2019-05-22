// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using GeoAPI;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext"/>
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteNetTopologySuiteTypeMappingSourcePlugin([NotNull] IGeometryServices geometryServices)
        {
            Check.NotNull(geometryServices, nameof(geometryServices));

            _geometryServices = geometryServices;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
