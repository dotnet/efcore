// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient; // Note: Hard reference to SqlClient here.
using Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite;
using NetTopologySuite.Geometries;
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
        where TGeometry : Geometry
    {
        private static readonly MethodInfo _getSqlBytes
            = typeof(SqlDataReader).GetRuntimeMethod(nameof(SqlDataReader.GetSqlBytes), new[] { typeof(int) });

        private static Action<DbParameter, SqlDbType> _sqlDbTypeSetter;
        private static Action<DbParameter, string> _udtTypeNameSetter;

        private readonly bool _isGeography;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [UsedImplicitly]
        public SqlServerGeometryTypeMapping(NtsGeometryServices geometryServices, string storeType)
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
            var geometry = (Geometry)value;
            var defaultSrid = geometry.SRID == (_isGeography ? 4326 : 0);
            if (geometry == Point.Empty)
            {
                defaultSrid = true;
            }

            builder
                .Append(_isGeography ? "geography" : "geometry")
                .Append("::")
                .Append(defaultSrid ? "Parse" : "STGeomFromText")
                .Append("('")
                .Append(WKTWriter.ForMicrosoftSqlServer().Write(geometry))
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            var type = parameter.GetType();
            LazyInitializer.EnsureInitialized(ref _sqlDbTypeSetter, () => CreateSqlDbTypeAccessor(type));
            LazyInitializer.EnsureInitialized(ref _udtTypeNameSetter, () => CreateUdtTypeNameAccessor(type));

            if (parameter.Value == DBNull.Value)
            {
                parameter.Value = SqlBytes.Null;
            }

            _sqlDbTypeSetter(parameter, SqlDbType.Udt);
            _udtTypeNameSetter(parameter, _isGeography ? "geography" : "geometry");
        }

        private static SqlServerBytesReader CreateReader(NtsGeometryServices services, bool isGeography)
            => new SqlServerBytesReader(services) { IsGeography = isGeography };

        private static SqlServerBytesWriter CreateWriter(bool isGeography)
            => new SqlServerBytesWriter { IsGeography = isGeography };

        private static bool IsGeography(string storeType)
            => string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

        private static Action<DbParameter, SqlDbType> CreateSqlDbTypeAccessor(Type paramType)
        {
            var paramParam = Expression.Parameter(typeof(DbParameter), "parameter");
            var valueParam = Expression.Parameter(typeof(SqlDbType), "value");

            return Expression.Lambda<Action<DbParameter, SqlDbType>>(
                Expression.Call(
                    Expression.Convert(paramParam, paramType),
                    paramType.GetProperty("SqlDbType").SetMethod,
                    valueParam),
                paramParam,
                valueParam).Compile();
        }

        private static Action<DbParameter, string> CreateUdtTypeNameAccessor(Type paramType)
        {
            var paramParam = Expression.Parameter(typeof(DbParameter), "parameter");
            var valueParam = Expression.Parameter(typeof(string), "value");

            return Expression.Lambda<Action<DbParameter, string>>(
                Expression.Call(
                    Expression.Convert(paramParam, paramType),
                    paramType.GetProperty("UdtTypeName").SetMethod,
                    valueParam),
                paramParam,
                valueParam).Compile();
        }
    }
}
