// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerObjectToStringTranslator : IMethodCallTranslator
{
    private const int DefaultLength = 100;

    private static readonly Dictionary<Type, string> TypeMapping
        = new()
        {
            { typeof(sbyte), "varchar(4)" },
            { typeof(byte), "varchar(3)" },
            { typeof(short), "varchar(6)" },
            { typeof(ushort), "varchar(5)" },
            { typeof(int), "varchar(11)" },
            { typeof(uint), "varchar(10)" },
            { typeof(long), "varchar(20)" },
            { typeof(ulong), "varchar(20)" },
            { typeof(float), $"varchar({DefaultLength})" },
            { typeof(double), $"varchar({DefaultLength})" },
            { typeof(decimal), $"varchar({DefaultLength})" },
            { typeof(char), "varchar(1)" },
            { typeof(DateTime), $"varchar({DefaultLength})" },
            { typeof(DateOnly), $"varchar({DefaultLength})" },
            { typeof(TimeOnly), $"varchar({DefaultLength})" },
            { typeof(DateTimeOffset), $"varchar({DefaultLength})" },
            { typeof(TimeSpan), $"varchar({DefaultLength})" },
            { typeof(Guid), "varchar(36)" },
            { typeof(byte[]), $"varchar({DefaultLength})" }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerObjectToStringTranslator(ISqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
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
        if (instance == null || method.Name != nameof(ToString) || arguments.Count != 0)
        {
            return null;
        }

        if (instance.TypeMapping?.ClrType == typeof(string))
        {
            return instance;
        }

        if (instance.Type == typeof(bool))
        {
            if (instance is ColumnExpression { IsNullable: true })
            {
                return _sqlExpressionFactory.Case(
                    new[]
                    {
                        new CaseWhenClause(
                            _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(false)),
                            _sqlExpressionFactory.Constant(false.ToString())),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(true)),
                            _sqlExpressionFactory.Constant(true.ToString()))
                    },
                    _sqlExpressionFactory.Constant(null, typeof(string)));
            }

            return _sqlExpressionFactory.Case(
                new[]
                {
                    new CaseWhenClause(
                        _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(false)),
                        _sqlExpressionFactory.Constant(false.ToString()))
                },
                _sqlExpressionFactory.Constant(true.ToString()));
        }

        return TypeMapping.TryGetValue(instance.Type, out var storeType)
            ? _sqlExpressionFactory.Function(
                "CONVERT",
                new[] { _sqlExpressionFactory.Fragment(storeType), instance },
                nullable: true,
                argumentsPropagateNullability: new[] { false, true },
                typeof(string),
                _typeMappingSource.GetMapping(storeType))
            : null;
    }
}
