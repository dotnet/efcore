// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Reflection;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, byte[]>
        where TGeometry : IGeometry
    {
        private static readonly MethodInfo _getBytes
            = typeof(DbDataReader).GetTypeInfo()
                .GetDeclaredMethod(nameof(DbDataReader.GetFieldValue))
                .MakeGenericMethod(typeof(byte[]));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [UsedImplicitly]
        public SqliteGeometryTypeMapping(GaiaGeoReader reader, string storeType)
            : base(new GeometryValueConverter<TGeometry>(reader), storeType)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected SqliteGeometryTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqliteGeometryTypeMapping<TGeometry>(parameters);

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
                ? $"GeomFromText({text}, {srid})"
                : $"GeomFromText({text})";
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override MethodInfo GetDataReaderMethod()
            => _getBytes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string AsText(object value)
            => (value is IGeometry geometry) ? geometry.AsText() : null;
    }
}
