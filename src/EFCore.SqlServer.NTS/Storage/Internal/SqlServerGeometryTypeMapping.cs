// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Reflection;
using GeoAPI;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, SqlBytes>
        where TGeometry : IGeometry
    {
        private static readonly MethodInfo _getSqlBytes
            = typeof(SqlDataReader).GetTypeInfo().GetDeclaredMethod(nameof(SqlDataReader.GetSqlBytes));

        private readonly bool _isGeography;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected SqlServerGeometryTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqlServerGeometryTypeMapping<TGeometry>(parameters);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var geometry = (IGeometry)value;
            var srid = geometry.SRID;

            var text = "'" + geometry.AsText() + "'";
            if (srid != (_isGeography ? 4326 : 0))
            {
                text = $"{(_isGeography ? "geography" : "geometry")}::STGeomFromText({text}, {srid})";
            }

            return srid > 0
                ? $"geometry::STGeomFromText({text}, {srid.ToString(CultureInfo.InvariantCulture)})"
                : text;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override MethodInfo GetDataReaderMethod()
            => _getSqlBytes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string AsText(object value)
            => ((IGeometry)value).AsText();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override int GetSrid(object value)
            => ((IGeometry)value).SRID;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Type WKTReaderType
            => typeof(WKTReader);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            if (parameter.Value == DBNull.Value)
            {
                parameter.Value = SqlBytes.Null;
            }
        }

        private static SqlServerBytesReader CreateReader(IGeometryServices services, bool isGeography)
            => new SqlServerBytesReader(services) { IsGeography = isGeography };

        private static SqlServerBytesWriter CreateWriter(bool isGeography)
            => new SqlServerBytesWriter { IsGeography = isGeography };

        private static bool IsGeography(string storeType)
            => string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);
    }
}
