// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, byte[]>
        where TGeometry : Geometry
    {
        private static readonly MethodInfo _getBytes
            = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFieldValue), new[] { typeof(int) })
                .MakeGenericMethod(typeof(byte[]));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [UsedImplicitly]
        public SqliteGeometryTypeMapping(NtsGeometryServices geometryServices, string storeType)
            : base(new GeometryValueConverter<TGeometry>(CreateReader(geometryServices), CreateWriter()), storeType)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected SqliteGeometryTypeMapping(
            RelationalTypeMappingParameters parameters,
            ValueConverter<TGeometry, byte[]> converter)
            : base(parameters, converter)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqliteGeometryTypeMapping<TGeometry>(parameters, SpatialConverter);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var builder = new StringBuilder();
            var geometry = (Geometry)value;

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override MethodInfo GetDataReaderMethod()
            => _getBytes;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string AsText(object value)
            => ((Geometry)value).AsText();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override int GetSrid(object value)
            => ((Geometry)value).SRID;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Type WKTReaderType
            => typeof(WKTReader);

        private static GaiaGeoReader CreateReader(NtsGeometryServices geometryServices)
            => new GaiaGeoReader(
                geometryServices.DefaultCoordinateSequenceFactory,
                geometryServices.DefaultPrecisionModel);

        private static GaiaGeoWriter CreateWriter()
            => new GaiaGeoWriter { HandleOrdinates = Ordinates.XY };
    }
}
