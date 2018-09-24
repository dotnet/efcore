// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Base class for relation type mappings to NTS IGeometry and implementing types.
    /// </summary>
    /// <typeparam name="TGeometry"> The geometry type. </typeparam>
    /// <typeparam name="TProvider"> The native type of the database provider. </typeparam>
    public abstract class RelationalGeometryTypeMapping<TGeometry, TProvider> : RelationalTypeMapping
    {
        private readonly ValueConverter<TGeometry, TProvider> _converter;

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
            _converter = converter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        protected RelationalGeometryTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

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
        ///     Gets a custom expression tree for the code to convert from the database value
        ///     to the model value.
        /// </summary>
        /// <param name="expression"> The input expression, containing the database value. </param>
        /// <returns> The expression with conversion added. </returns>
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

        /// <summary>
        ///     Attempts generation of a code (e.g. C#) literal for the given value.
        /// </summary>
        /// <param name="value"> The value for which a literal is needed. </param>
        /// <param name="languageCode"> The language code, which is typically the common file extension (e.g. ".cs") for the language. </param>
        /// <returns> The generated literal, or <c>null</c> if a literal could not be generated. </returns>
        public override string FindCodeLiteral(object value, string languageCode)
        {
            var geometryText = AsText(value);

            // TODO: Handle SRID
            // TODO: Consider constructing C# objects directly
            // TODO: Allow additional namespaces needed to be put in using directives
            return geometryText != null
                   && languageCode.Equals(".cs", StringComparison.OrdinalIgnoreCase)
                ? $"({value.GetType().ShortDisplayName()})new NetTopologySuite.IO.WKTReader().Read(\"{geometryText}\")"
                : null;
        }

        /// <summary>
        ///     Returns the Well-Known-Text (WKT) representation of the given object, or <c>null</c>
        ///     if the object is not an 'IGeometry'.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The WKT. </returns>
        protected abstract string AsText(object value);
    }
}
