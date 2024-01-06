// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Base class for relation type mappings to NTS Geometry and derived types.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
/// <typeparam name="TGeometry">The geometry type.</typeparam>
/// <typeparam name="TProvider">The native type of the database provider.</typeparam>
public abstract class RelationalGeometryTypeMapping<TGeometry, TProvider> : RelationalTypeMapping
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalGeometryTypeMapping{TGeometry,TProvider}" /> class.
    /// </summary>
    /// <param name="converter">The converter to use when converting to and from database types.</param>
    /// <param name="storeType">The store type name.</param>
    /// <param name="jsonValueReaderWriter">Handles reading and writing JSON values for instances of the mapped type.</param>
    protected RelationalGeometryTypeMapping(
        ValueConverter<TGeometry, TProvider>? converter,
        string storeType,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        : base(CreateRelationalTypeMappingParameters(storeType, jsonValueReaderWriter))
    {
        SpatialConverter = converter;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <param name="converter">The converter to use when converting to and from database types.</param>
    protected RelationalGeometryTypeMapping(
        RelationalTypeMappingParameters parameters,
        ValueConverter<TGeometry, TProvider>? converter)
        : base(
            parameters.WithCoreParameters(
                parameters.CoreParameters with
                {
                    ProviderValueComparer = parameters.CoreParameters.ProviderValueComparer
                    ?? (RuntimeFeature.IsDynamicCodeSupported
                        ? CreateProviderValueComparer(
                            parameters.CoreParameters.Converter?.ProviderClrType ?? parameters.CoreParameters.ClrType)
                        : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel))
                }))
    {
        SpatialConverter = converter;
    }

    private static ValueComparer? CreateProviderValueComparer(Type providerType)
        => providerType.IsAssignableTo(typeof(TGeometry))
            ? (ValueComparer)Activator.CreateInstance(typeof(GeometryValueComparer<>).MakeGenericType(providerType))!
            : null;

    private static RelationalTypeMappingParameters CreateRelationalTypeMappingParameters(
        string storeType,
        JsonValueReaderWriter? jsonValueReaderWriter)
    {
        var comparer = new GeometryValueComparer<TGeometry>();

        return new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(
                typeof(TGeometry),
                null,
                comparer,
                comparer,
                CreateProviderValueComparer(typeof(TGeometry)),
                jsonValueReaderWriter: jsonValueReaderWriter),
            storeType);
    }

    /// <summary>
    ///     The underlying Geometry converter.
    /// </summary>
    protected virtual ValueConverter<TGeometry, TProvider>? SpatialConverter { get; }

    /// <inheritdoc />
    public override DbParameter CreateParameter(
        DbCommand command,
        string name,
        object? value,
        bool? nullable = null,
        ParameterDirection direction = ParameterDirection.Input)
    {
        var parameter = command.CreateParameter();
        parameter.Direction = ParameterDirection.Input;
        parameter.ParameterName = name;

        if (Converter != null)
        {
            value = Converter.ConvertToProvider(value);
        }

        parameter.Value = value is null
            ? DBNull.Value
            : SpatialConverter is null
                ? value
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
    /// <param name="expression">The input expression, containing the database value.</param>
    /// <returns>The expression with conversion added.</returns>
    public override Expression CustomizeDataReaderExpression(Expression expression)
    {
        if (SpatialConverter is null)
        {
            return expression;
        }

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
    /// <param name="value">The value for which a literal is needed.</param>
    /// <returns>An expression tree that can be used to generate code for the literal value.</returns>
    public override Expression GenerateCodeLiteral(object value)
        => Expression.Convert(
            Expression.Call(
                Expression.New(WktReaderType),
                WktReaderType.GetMethod("Read", [typeof(string)])!,
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
    // ReSharper disable once InconsistentNaming
    protected abstract Type WktReaderType { get; }

    /// <summary>
    ///     Returns the Well-Known-Text (WKT) representation of the given object.
    /// </summary>
    /// <param name="value">The 'Geometry' value.</param>
    /// <returns>The WKT.</returns>
    protected abstract string AsText(object value);

    /// <summary>
    ///     Returns the SRID representation of the given object.
    /// </summary>
    /// <param name="value">The 'Geometry' value.</param>
    /// <returns>The SRID.</returns>
    protected abstract int GetSrid(object value);
}
