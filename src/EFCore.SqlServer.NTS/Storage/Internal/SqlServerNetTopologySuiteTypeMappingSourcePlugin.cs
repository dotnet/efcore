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

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext"/>
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class SqlServerNetTopologySuiteTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        private readonly HashSet<string> _spatialStoreTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "geometry",
            "geography"
        };

        private readonly IGeometryServices _geometryServices;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerNetTopologySuiteTypeMappingSourcePlugin([NotNull] IGeometryServices geometryServices)
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
            var clrType = mappingInfo.ClrType ?? typeof(IGeometry);
            var storeTypeName = mappingInfo.StoreTypeName;

            return typeof(IGeometry).IsAssignableFrom(clrType)
                   || (storeTypeName != null
                       && _spatialStoreTypes.Contains(storeTypeName))
                ? (RelationalTypeMapping)Activator.CreateInstance(
                    typeof(SqlServerGeometryTypeMapping<>).MakeGenericType(clrType),
                    _geometryServices,
                    storeTypeName ?? "geography")
                : null;
        }
    }
}
