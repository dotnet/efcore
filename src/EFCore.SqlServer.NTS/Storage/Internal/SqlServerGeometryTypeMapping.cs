// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using System.Text;
using GeoAPI;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, SqlBytes>
        where TGeometry : IGeometry
    {
        private static readonly MethodInfo _getSqlBytes
            = typeof(SqlDataReader).GetRuntimeMethod(nameof(SqlDataReader.GetSqlBytes), new[] { typeof(int) });

        private readonly bool _isGeography;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [UsedImplicitly]
        public SqlServerGeometryTypeMapping(IGeometryServices geometryServices, string storeType)
            : base(
                new GeometryValueConverter<TGeometry>(
                    CreateReader(geometryServices, IsGeography(storeType)),
                    CreateWriter(IsGeography(storeType))),
                storeType)
            => _isGeography = IsGeography(storeType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected SqlServerGeometryTypeMapping(
            RelationalTypeMappingParameters parameters,
            ValueConverter<TGeometry, SqlBytes> converter)
            : base(parameters, converter)
        {
            _isGeography = IsGeography(StoreType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqlServerGeometryTypeMapping<TGeometry>(parameters, SpatialConverter);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var builder = new StringBuilder();
            var geometry = (IGeometry)value;
            var defaultSrid = geometry.SRID == (_isGeography ? 4326 : 0);

            builder
                .Append(_isGeography ? "geography" : "geometry")
                .Append("::")
                .Append(defaultSrid ? "Parse" : "STGeomFromText")
                .Append("('")
                .Append(geometry.AsText())
                .Append("'");

            if (!defaultSrid)
            {
                builder
                    .Append(", ")
                    .Append(geometry.SRID);
            }

            builder.Append(")");

            return builder.ToString();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override MethodInfo GetDataReaderMethod()
            => _getSqlBytes;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string AsText(object value)
            => ((IGeometry)value).AsText();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override int GetSrid(object value)
            => ((IGeometry)value).SRID;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Type WKTReaderType
            => typeof(WKTReader);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            if (parameter.Value == DBNull.Value)
            {
                parameter.Value = SqlBytes.Null;
            }
        }

        private static SqlServerBytesReader CreateReader(IGeometryServices services, bool isGeography)
            => new SqlServerBytesReader(services)
            {
                IsGeography = isGeography
            };

        private static SqlServerBytesWriter CreateWriter(bool isGeography)
            => new SqlServerBytesWriter
            {
                IsGeography = isGeography
            };

        private static bool IsGeography(string storeType)
            => string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);
    }
}
