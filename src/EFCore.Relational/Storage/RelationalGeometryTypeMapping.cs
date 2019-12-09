// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Base class for relation type mappings to NTS Geometry and derived types.
    /// </summary>
    /// <typeparam name="TGeometry"> The geometry type. </typeparam>
    /// <typeparam name="TProvider"> The native type of the database provider. </typeparam>
    public abstract class RelationalGeometryTypeMapping<TGeometry, TProvider> : RelationalTypeMapping
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalGeometryTypeMapping{TGeometry,TProvider}" /> class.
        /// </summary>
        /// <param name="converter"> The converter to use when converting to and from database types. </param>
        /// <param name="storeType"> The store type name. </param>
        protected RelationalGeometryTypeMapping(
            [NotNull] ValueConverter<TGeometry, TProvider> converter,
            [NotNull] string storeType)
            : base(CreateRelationalTypeMappingParameters(storeType))
        {
            SpatialConverter = converter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <param name="converter"> The converter to use when converting to and from database types. </param>
        protected RelationalGeometryTypeMapping(
            RelationalTypeMappingParameters parameters,
            [CanBeNull] ValueConverter<TGeometry, TProvider> converter)
            : base(parameters)
        {
            SpatialConverter = converter;
        }

        /// <summary>
        ///     The underlying Geometry converter.
        /// </summary>
        protected virtual ValueConverter<TGeometry, TProvider> SpatialConverter { get; }

        private static RelationalTypeMappingParameters CreateRelationalTypeMappingParameters(string storeType)
        {
            var comparer = new GeometryValueComparer<TGeometry>();

            return new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(TGeometry),
                    null,
                    comparer,
                    comparer),
                storeType);
        }

        /// <summary>
        ///     Creates a <see cref="DbParameter" /> with the appropriate type information configured.
        /// </summary>
        /// <param name="command"> The command the parameter should be created on. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="value"> The value to be assigned to the parameter. </param>
        /// <param name="nullable"> A value indicating whether the parameter should be a nullable type. </param>
        /// <returns> The newly created parameter. </returns>
        public override DbParameter CreateParameter(DbCommand command, string name, object value, bool? nullable = null)
        {
            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;

            if (Converter != null)
            {
                value = Converter.ConvertToProvider(value);
            }

            parameter.Value = value == null
                ? DBNull.Value
                : SpatialConverter.ConvertToProvider(value);

            if (nullable.HasValue)
            {
                parameter.IsNullable = nullable.Value;
            }

            ConfigureParameter(parameter);

            return parameter;
        }

        /// <summary>
        ///     Gets a custom expression tree for the code to convert from the database value
        ///     to the model value.
        /// </summary>
        /// <param name="expression"> The input expression, containing the database value. </param>
        /// <returns> The expression with conversion added. </returns>
        public override Expression CustomizeDataReaderExpression(Expression expression)
        {
            if (expression.Type != SpatialConverter.ProviderClrType)
            {
                expression = Expression.Convert(expression, SpatialConverter.ProviderClrType);
            }

            return ReplacingExpressionVisitor.Replace(
                SpatialConverter.ConvertFromProviderExpression.Parameters.Single(),
                expression,
                SpatialConverter.ConvertFromProviderExpression.Body);
        }

        /// <summary>
        ///     Creates a an expression tree that can be used to generate code for the literal value.
        ///     Currently, only very basic expressions such as constructor calls and factory methods taking
        ///     simple constants are supported.
        /// </summary>
        /// <param name="value"> The value for which a literal is needed. </param>
        /// <returns> An expression tree that can be used to generate code for the literal value. </returns>
        public override Expression GenerateCodeLiteral(object value)
            => Expression.Convert(
                Expression.Call(
                    Expression.New(WKTReaderType),
                    WKTReaderType.GetMethod("Read", new[] { typeof(string) }),
                    Expression.Constant(CreateWktWithSrid(value), typeof(string))),
                value.GetType());

        private string CreateWktWithSrid(object value)
        {
            var srid = GetSrid(value);
            var text = AsText(value);
            if (srid != -1)
            {
                text = $"SRID={srid};" + text;
            }

            return text;
        }

        /// <summary>
        ///     The type of the NTS 'WKTReader'.
        /// </summary>
        protected abstract Type WKTReaderType { get; }

        /// <summary>
        ///     Returns the Well-Known-Text (WKT) representation of the given object.
        /// </summary>
        /// <param name="value"> The 'Geometry' value. </param>
        /// <returns> The WKT. </returns>
        protected abstract string AsText([NotNull] object value);

        /// <summary>
        ///     Returns the SRID representation of the given object.
        /// </summary>
        /// <param name="value"> The 'Geometry' value. </param>
        /// <returns> The SRID. </returns>
        protected abstract int GetSrid([NotNull] object value);
    }
}
