// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.IO;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerGeometryTypeMapping : RelationalTypeMapping
    {
        private static readonly MethodInfo _getSqlBytes
            = typeof(SqlDataReader).GetTypeInfo().GetDeclaredMethod(nameof(SqlDataReader.GetSqlBytes));

        private readonly SqlServerSpatialReader _reader;
        private readonly GeometryValueConverter _converter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerGeometryTypeMapping(Type clrType, SqlServerSpatialReader reader, string storeType)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        clrType,
                        null,
                        new GeometryValueComparer(clrType)),
                    storeType))
        {
            _reader = reader;
            _converter = new GeometryValueConverter(clrType, reader);
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
            => new SqlServerGeometryTypeMapping(parameters);

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
                ? $"geometry::STGeomFromText({text}, {srid})"
                : text;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override DbParameter CreateParameter(DbCommand command, string name, object value, bool? nullable = null)
        {
            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;

            parameter.Value = value == null
                ? DBNull.Value
                : _converter.ConvertToProvider(value);

            if (nullable.HasValue)
            {
                parameter.IsNullable = nullable.Value;
            }

            ConfigureParameter(parameter);

            return parameter;
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
        public override Expression AddCustomConversion(Expression expression)
        {
            if (expression.Type != _converter.ProviderClrType)
            {
                expression = Expression.Convert(expression, _converter.ProviderClrType);
            }

            return ReplacingExpressionVisitor.Replace(
                _converter.ConvertFromProviderExpression.Parameters.Single(),
                expression,
                _converter.ConvertFromProviderExpression.Body);
        }
    }
}
