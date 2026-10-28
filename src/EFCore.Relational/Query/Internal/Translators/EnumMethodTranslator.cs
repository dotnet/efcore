// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EnumMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo HasFlagMethodInfo
        = typeof(Enum).GetRuntimeMethod(nameof(Enum.HasFlag), [typeof(Enum)])!;

    private static readonly MethodInfo ToStringMethodInfo
        = typeof(object).GetRuntimeMethod(nameof(ToString), [])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EnumMethodTranslator(ISqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (Equals(method, HasFlagMethodInfo)
            && instance != null)
        {
            var argument = arguments[0];
            return instance.Type != argument.Type
                ? null
                : _sqlExpressionFactory.Equal(_sqlExpressionFactory.And(instance, argument), argument);
        }

        if (Equals(method, ToStringMethodInfo)
            && instance is { Type.IsEnum: true, TypeMapping.Converter: { } converter }
            && converter.GetType() is { IsGenericType: true } converterType)
        {
            switch (converterType)
            {
                case not null when converterType.GetGenericTypeDefinition() == typeof(EnumToNumberConverter<,>):
                    var whenClauses = Enum.GetValues(instance.Type)
                        .Cast<object>()
                        .Select(value => new CaseWhenClause(
                            _sqlExpressionFactory.Constant(value),
                            _sqlExpressionFactory.Constant(value.ToString(), typeof(string))))
                        .ToArray();

                    var elseResult = _sqlExpressionFactory.Coalesce(
                        _sqlExpressionFactory.Convert(instance, typeof(string)),
                        _sqlExpressionFactory.Constant(string.Empty));

                    return _sqlExpressionFactory.Case(instance, whenClauses, elseResult);

                case not null when converterType.GetGenericTypeDefinition() == typeof(EnumToStringConverter<>):
                    // The operand is emitted as is, so the result must keep describing the stored column (store type, size,
                    // Unicode-ness, etc.) rather than get a default string mapping. If no converter-free string mapping with the
                    // same store facets exists, fall back to an explicit conversion.
                    var operandMapping = instance.TypeMapping;
                    var stringMapping = _typeMappingSource.FindMapping(
                        typeof(string),
                        operandMapping.StoreType,
                        unicode: operandMapping.IsUnicode,
                        size: operandMapping.Size,
                        fixedLength: operandMapping.IsFixedLength,
                        precision: operandMapping.Precision,
                        scale: operandMapping.Scale);

                    return stringMapping is { Converter: null, ClrType: var clrType } && clrType == typeof(string)
                        ? new ClrTypeChangeExpression(instance, typeof(string), stringMapping)
                        : _sqlExpressionFactory.Convert(instance, typeof(string));

                default:
                    return null;
            }
        }

        return null;
    }
}
