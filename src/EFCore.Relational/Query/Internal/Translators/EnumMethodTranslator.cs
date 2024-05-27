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
        = typeof(Enum).GetRuntimeMethod(nameof(Enum.HasFlag), new[] { typeof(Enum) })!;

    private static readonly MethodInfo ToStringMethodInfo
        = typeof(object).GetRuntimeMethod(nameof(ToString), new Type[] { })!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EnumMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
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
            && instance != null
            && instance.Type.IsEnum)
        {
            var converterType = instance.TypeMapping?.Converter?.GetType();

            if (converterType is not null
                && converterType.IsGenericType)
            {
                if (converterType.GetGenericTypeDefinition() == typeof(EnumToNumberConverter<,>)
                    && converterType.GetGenericArguments().Length == 2
                    && converterType.GetGenericArguments()[1] == typeof(int)
                    && (instance is SqlParameterExpression || instance is ColumnExpression))
                {
                    var cases = Enum.GetValues(instance.Type)
                        .Cast<object>()
                        .Select(value => new CaseWhenClause(
                            _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(value)),
                            _sqlExpressionFactory.Constant(value.ToString(), typeof(string))))
                        .ToArray();

                    return _sqlExpressionFactory.Case(cases, _sqlExpressionFactory.Constant(string.Empty, typeof(string)));
                }
                else if (converterType.GetGenericTypeDefinition() == typeof(EnumToStringConverter<>))
                {
                    // TODO: Unnecessary cast to string, #33733
                    return _sqlExpressionFactory.MakeUnary(ExpressionType.Convert, instance, typeof(string));
                }
            }
        }

        return null;
    }
}
