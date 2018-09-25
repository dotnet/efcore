// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Reflection;
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [UsedImplicitly]
        public SqlServerGeometryTypeMapping(SqlServerSpatialReader reader, string storeType)
            : base(new GeometryValueConverter<TGeometry>(reader), storeType)
        {
        }

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

            // TODO: This won't emit M (see NetTopologySuite/NetTopologySuite#156)
            var text = "'" + geometry.AsText() + "'";

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
            => (value is IGeometry geometry) ? $"SRID={geometry.SRID.ToString(CultureInfo.InvariantCulture)}; {geometry.AsText()}" : null;
    }
}
