// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Reflection;
using System.Text;
using GeoAPI;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        public SqliteGeometryTypeMapping(IGeometryServices geometryServices, string storeType)
            : base(new GeometryValueConverter<TGeometry>(CreateReader(geometryServices), CreateWriter()), storeType)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected SqliteGeometryTypeMapping(
            RelationalTypeMappingParameters parameters,
                ValueConverter<TGeometry, byte[]> converter)
            : base(parameters, converter)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqliteGeometryTypeMapping<TGeometry>(parameters, SpatialConverter);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var builder = new StringBuilder();
            var geometry = (IGeometry)value;

            builder
                .Append("GeomFromText('")
                .Append(geometry.AsText())
                .Append("'");

            if (geometry.SRID != 0)
            {
                builder
                    .Append(", ")
                    .Append(geometry.SRID);
            }

            builder.Append(")");

            return builder.ToString();
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

        private static GaiaGeoReader CreateReader(IGeometryServices geometryServices)
            => new GaiaGeoReader(
                geometryServices.DefaultCoordinateSequenceFactory,
                geometryServices.DefaultPrecisionModel);

        private static GaiaGeoWriter CreateWriter()
            => new GaiaGeoWriter();
    }
}
