// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private static readonly IReadOnlyDictionary<ExpressionType, IReadOnlyCollection<Type>> RestrictedBinaryExpressions
        = new Dictionary<ExpressionType, IReadOnlyCollection<Type>>
        {
            [ExpressionType.Add] = new HashSet<Type>
            {
                typeof(DateOnly),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeOnly),
                typeof(TimeSpan)
            },
            [ExpressionType.Divide] = new HashSet<Type>
            {
                typeof(TimeOnly),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.GreaterThan] = new HashSet<Type>
            {
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.GreaterThanOrEqual] = new HashSet<Type>
            {
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.LessThan] = new HashSet<Type>
            {
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.LessThanOrEqual] = new HashSet<Type>
            {
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.Modulo] = new HashSet<Type> { typeof(ulong) },
            [ExpressionType.Multiply] = new HashSet<Type>
            {
                typeof(TimeOnly),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.Subtract] = new HashSet<Type>
            {
                typeof(DateOnly),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeOnly),
                typeof(TimeSpan)
            }
        };

    private static readonly IReadOnlyCollection<Type> FunctionModuloTypes = new HashSet<Type>
    {
        typeof(decimal),
        typeof(double),
        typeof(float)
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteSqlTranslatingExpressionVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        if (unaryExpression.NodeType == ExpressionType.ArrayLength
            && unaryExpression.Operand.Type == typeof(byte[]))
        {
            return Visit(unaryExpression.Operand) is SqlExpression sqlExpression
                ? Dependencies.SqlExpressionFactory.Function(
                    "length",
                    new[] { sqlExpression },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(int))
                : QueryCompilationContext.NotTranslatedExpression;
        }

        var visitedExpression = base.VisitUnary(unaryExpression);
        if (visitedExpression == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (visitedExpression is SqlUnaryExpression sqlUnary
            && sqlUnary.OperatorType == ExpressionType.Negate)
        {
            var operandType = GetProviderType(sqlUnary.Operand);
            if (operandType == typeof(decimal))
            {
                return Dependencies.SqlExpressionFactory.Function(
                    name: "ef_negate",
                    new[] { sqlUnary.Operand },
                    nullable: true,
                    new[] { true },
                    visitedExpression.Type);
            }

            if (operandType == typeof(TimeOnly)
                || operandType == typeof(TimeSpan))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        return visitedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        // See issue#16428
        //if (binaryExpression.NodeType == ExpressionType.ArrayIndex
        //    && binaryExpression.Left.Type == typeof(byte[]))
        //{
        //    var left = Visit(binaryExpression.Left);
        //    var right = Visit(binaryExpression.Right);

        //    if (left is SqlExpression leftSql
        //        && right is SqlExpression rightSql)
        //    {
        //        return Dependencies.SqlExpressionFactory.Function(
        //            "unicode",
        //            new SqlExpression[]
        //            {
        //                Dependencies.SqlExpressionFactory.Function(
        //                    "substr",
        //                    new SqlExpression[]
        //                    {
        //                        leftSql,
        //                        Dependencies.SqlExpressionFactory.Add(
        //                            Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(rightSql),
        //                            Dependencies.SqlExpressionFactory.Constant(1)),
        //                        Dependencies.SqlExpressionFactory.Constant(1)
        //                    },
        //                    nullable: true,
        //                    argumentsPropagateNullability: new[] { true, true, true },
        //                    typeof(byte[]))
        //            },
        //            nullable: true,
        //            argumentsPropagateNullability: new[] { true },
        //            binaryExpression.Type);
        //    }
        //}

        if (!(base.VisitBinary(binaryExpression) is SqlExpression visitedExpression))
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (visitedExpression is SqlBinaryExpression sqlBinary)
        {
            if (sqlBinary.OperatorType == ExpressionType.Modulo
                && (FunctionModuloTypes.Contains(GetProviderType(sqlBinary.Left))
                    || FunctionModuloTypes.Contains(GetProviderType(sqlBinary.Right))))
            {
                return Dependencies.SqlExpressionFactory.Function(
                    "ef_mod",
                    new[] { sqlBinary.Left, sqlBinary.Right },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    visitedExpression.Type,
                    visitedExpression.TypeMapping);
            }

            if (AttemptDecimalCompare(sqlBinary))
            {
                return DoDecimalCompare(visitedExpression, sqlBinary.OperatorType, sqlBinary.Left, sqlBinary.Right);
            }

            if (AttemptDecimalArithmetic(sqlBinary))
            {
                return DoDecimalArithmetics(visitedExpression, sqlBinary.OperatorType, sqlBinary.Left, sqlBinary.Right);
            }

            if (RestrictedBinaryExpressions.TryGetValue(sqlBinary.OperatorType, out var restrictedTypes)
                && (restrictedTypes.Contains(GetProviderType(sqlBinary.Left))
                    || restrictedTypes.Contains(GetProviderType(sqlBinary.Right))))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        return visitedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // EF.Default
        if (methodCallExpression.Method.IsEFDefaultMethod())
        {
            AddTranslationErrorDetails(SqliteStrings.DefaultNotSupported);
            return QueryCompilationContext.NotTranslatedExpression;
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    private static Type? GetProviderType(SqlExpression? expression)
        => expression == null
            ? null
            : (expression.TypeMapping?.Converter?.ProviderClrType
                ?? expression.TypeMapping?.ClrType
                ?? expression.Type);

    private static bool AreOperandsDecimals(SqlBinaryExpression sqlExpression)
        => GetProviderType(sqlExpression.Left) == typeof(decimal)
            && GetProviderType(sqlExpression.Right) == typeof(decimal);

    private static bool AttemptDecimalCompare(SqlBinaryExpression sqlBinary)
        => AreOperandsDecimals(sqlBinary)
            && new[]
            {
                ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan, ExpressionType.LessThanOrEqual
            }.Contains(sqlBinary.OperatorType);

    private Expression DoDecimalCompare(SqlExpression visitedExpression, ExpressionType op, SqlExpression left, SqlExpression right)
    {
        var actual = Dependencies.SqlExpressionFactory.Function(
            name: "ef_compare",
            new[] { left, right },
            nullable: true,
            new[] { true, true },
            typeof(int));
        var oracle = Dependencies.SqlExpressionFactory.Constant(value: 0);

        return op switch
        {
            ExpressionType.GreaterThan => Dependencies.SqlExpressionFactory.GreaterThan(left: actual, right: oracle),
            ExpressionType.GreaterThanOrEqual => Dependencies.SqlExpressionFactory.GreaterThanOrEqual(left: actual, right: oracle),
            ExpressionType.LessThan => Dependencies.SqlExpressionFactory.LessThan(left: actual, right: oracle),
            ExpressionType.LessThanOrEqual => Dependencies.SqlExpressionFactory.LessThanOrEqual(left: actual, right: oracle),
            _ => visitedExpression
        };
    }

    private static bool AttemptDecimalArithmetic(SqlBinaryExpression sqlBinary)
        => AreOperandsDecimals(sqlBinary)
            && new[] { ExpressionType.Add, ExpressionType.Subtract, ExpressionType.Multiply, ExpressionType.Divide }.Contains(
                sqlBinary.OperatorType);

    private Expression DoDecimalArithmetics(SqlExpression visitedExpression, ExpressionType op, SqlExpression left, SqlExpression right)
    {
        return op switch
        {
            ExpressionType.Add => DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, right),
            ExpressionType.Divide => DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, right),
            ExpressionType.Multiply => DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, right),
            ExpressionType.Subtract => DecimalSubtractExpressionFactoryMethod(left, right),
            _ => visitedExpression
        };

        static string ResolveFunctionNameFromExpressionType(ExpressionType expressionType)
            => expressionType switch
            {
                ExpressionType.Add => "ef_add",
                ExpressionType.Divide => "ef_divide",
                ExpressionType.Multiply => "ef_multiply",
                ExpressionType.Subtract => "ef_add",
                _ => throw new InvalidOperationException()
            };

        Expression DecimalArithmeticExpressionFactoryMethod(string name, SqlExpression left, SqlExpression right)
            => Dependencies.SqlExpressionFactory.Function(
                name,
                new[] { left, right },
                nullable: true,
                new[] { true, true },
                visitedExpression.Type);

        Expression DecimalSubtractExpressionFactoryMethod(SqlExpression left, SqlExpression right)
        {
            var subtrahend = Dependencies.SqlExpressionFactory.Function(
                "ef_negate",
                new[] { right },
                nullable: true,
                new[] { true },
                visitedExpression.Type);

            return DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, subtrahend);
        }
    }
}
