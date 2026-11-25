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
                    var names = Enum.GetNames(instance.Type);
                    var maxEnumNameLength = names.Length > 0 ? names.Max(name => name.Length) : 0;
                    var maxNumericLength = GetMaxNumericStringLength(converter.ProviderClrType);
                    var maxLength = Math.Max(maxEnumNameLength, maxNumericLength);
                    var stringTypeMapping = _typeMappingSource.FindMapping(typeof(string), storeTypeName: null, size: maxLength, unicode: true)
                        ?? _typeMappingSource.FindMapping(typeof(string));

                    var whenClauses = Enum.GetValues(instance.Type)
                        .Cast<object>()
                        .Select(value => new CaseWhenClause(
                            _sqlExpressionFactory.Constant(value),
                            _sqlExpressionFactory.Constant(value.ToString(), typeof(string), stringTypeMapping)))
                        .ToArray();

                    var elseResult = _sqlExpressionFactory.Coalesce(
                        _sqlExpressionFactory.Convert(instance, typeof(string), stringTypeMapping),
                        _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping));

                    return _sqlExpressionFactory.Case(instance, whenClauses, elseResult);

                case not null when converterType.GetGenericTypeDefinition() == typeof(EnumToStringConverter<>):
                    return _sqlExpressionFactory.Convert(instance, typeof(string));

                default:
                    return null;
            }
        }

        return null;
    }

    private static int GetMaxNumericStringLength(Type numericType)
        => Type.GetTypeCode(Nullable.GetUnderlyingType(numericType) ?? numericType) switch
        {
            TypeCode.Byte => 3,
            TypeCode.SByte => 4,
            TypeCode.UInt16 => 5,
            TypeCode.Int16 => 6,
            TypeCode.UInt32 => 10,
            TypeCode.Int32 => 11,
            TypeCode.UInt64 => 20,
            TypeCode.Int64 => 20,
            _ => 30
        };
}
