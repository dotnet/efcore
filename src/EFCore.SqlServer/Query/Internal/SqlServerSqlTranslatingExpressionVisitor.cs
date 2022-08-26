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
public class SqlServerSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private static readonly HashSet<string> DateTimeDataTypes
        = new()
        {
            "time",
            "date",
            "datetime",
            "datetime2",
            "datetimeoffset"
        };

    private static readonly HashSet<Type> DateTimeClrTypes
        = new()
        {
            typeof(TimeOnly),
            typeof(DateOnly),
            typeof(TimeSpan),
            typeof(DateTime),
            typeof(DateTimeOffset)
        };

    private static readonly HashSet<ExpressionType> ArithmeticOperatorTypes
        = new()
        {
            ExpressionType.Add,
            ExpressionType.Subtract,
            ExpressionType.Multiply,
            ExpressionType.Divide,
            ExpressionType.Modulo
        };

    private static readonly MethodInfo StringJoinMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.Join), new[] { typeof(string), typeof(string[]) })!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSqlTranslatingExpressionVisitor(
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
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (binaryExpression.NodeType == ExpressionType.ArrayIndex
            && binaryExpression.Left.Type == typeof(byte[]))
        {
            var left = Visit(binaryExpression.Left);
            var right = Visit(binaryExpression.Right);

            if (left is SqlExpression leftSql
                && right is SqlExpression rightSql)
            {
                return Dependencies.SqlExpressionFactory.Convert(
                    Dependencies.SqlExpressionFactory.Function(
                        "SUBSTRING",
                        new[]
                        {
                            leftSql,
                            Dependencies.SqlExpressionFactory.Add(
                                Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(rightSql),
                                Dependencies.SqlExpressionFactory.Constant(1)),
                            Dependencies.SqlExpressionFactory.Constant(1)
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true, true },
                        typeof(byte[])),
                    binaryExpression.Type);
            }
        }

        var visitedExpression = base.VisitBinary(binaryExpression);

        if (visitedExpression is SqlBinaryExpression sqlBinaryExpression
            && ArithmeticOperatorTypes.Contains(sqlBinaryExpression.OperatorType))
        {
            var inferredProviderType = GetProviderType(sqlBinaryExpression.Left) ?? GetProviderType(sqlBinaryExpression.Right);
            if (inferredProviderType != null)
            {
                if (DateTimeDataTypes.Contains(inferredProviderType))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }
            else
            {
                var leftType = sqlBinaryExpression.Left.Type;
                var rightType = sqlBinaryExpression.Right.Type;
                if (DateTimeClrTypes.Contains(leftType)
                    || DateTimeClrTypes.Contains(rightType))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
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
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        if (unaryExpression.NodeType == ExpressionType.ArrayLength
            && unaryExpression.Operand.Type == typeof(byte[]))
        {
            if (!(base.Visit(unaryExpression.Operand) is SqlExpression sqlExpression))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var isBinaryMaxDataType = GetProviderType(sqlExpression) == "varbinary(max)" || sqlExpression is SqlParameterExpression;
            var dataLengthSqlFunction = Dependencies.SqlExpressionFactory.Function(
                "DATALENGTH",
                new[] { sqlExpression },
                nullable: true,
                argumentsPropagateNullability: new[] { true },
                isBinaryMaxDataType ? typeof(long) : typeof(int));

            return isBinaryMaxDataType
                ? Dependencies.SqlExpressionFactory.Convert(dataLengthSqlFunction, typeof(int))
                : dataLengthSqlFunction;
        }

        return base.VisitUnary(unaryExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var translation = base.VisitMethodCall(methodCallExpression);

        if (translation != QueryCompilationContext.NotTranslatedExpression)
        {
            return translation;
        }

        if (methodCallExpression.Method == StringJoinMethodInfo)
        {
            if (methodCallExpression.Arguments[1] is not NewArrayExpression newArrayExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var sqlArguments = new SqlExpression[newArrayExpression.Expressions.Count + 1];

            if (TranslationFailed(methodCallExpression.Arguments[0], Visit(methodCallExpression.Arguments[0]), out var sqlDelimiter))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            sqlArguments[0] = sqlDelimiter!;

            var isUnicode = sqlDelimiter!.TypeMapping?.IsUnicode == true;

            for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
            {
                var argument = newArrayExpression.Expressions[i];
                if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                // CONCAT_WS returns a type with a length that varies based on actual inputs (i.e. the sum of all argument lengths, plus
                // the length needed for the delimiters). We don't know column values (or even parameter values, so we always return max.
                // We do vary return varchar(max) or nvarchar(max) based on whether we saw any nvarchar mapping.
                if (sqlArgument!.TypeMapping?.IsUnicode == true)
                {
                    isUnicode = true;
                }

                // CONCAT_WS filters out nulls, but string.Join treats them as empty strings; coalesce unless we know we have a non-nullable
                // argument.
                sqlArguments[i + 1] = sqlArgument switch
                {
                    ColumnExpression { IsNullable: false } => sqlArgument,
                    SqlConstantExpression constantExpression => constantExpression.Value is null
                        ? new SqlConstantExpression(Expression.Constant(string.Empty, typeof(string)), null)
                        : constantExpression,
                    _ => Dependencies.SqlExpressionFactory.Coalesce(
                        sqlArgument,
                        Dependencies.SqlExpressionFactory.Constant(string.Empty, typeof(string)))
                };
            }

            // CONCAT_WS never returns null; a null delimiter is interpreted as an empty string, and null arguments are skipped
            // (but we coalesce them above in any case).
            return Dependencies.SqlExpressionFactory.Function(
                "CONCAT_WS",
                sqlArguments,
                nullable: false,
                argumentsPropagateNullability: new bool[sqlArguments.Length],
                methodCallExpression.Method.ReturnType,
                Dependencies.TypeMappingSource.FindMapping(isUnicode ? "nvarchar(max)" : "varchar(max)"));
        }

        return QueryCompilationContext.NotTranslatedExpression;
    }

    private static string? GetProviderType(SqlExpression expression)
        => expression.TypeMapping?.StoreType;

    [DebuggerStepThrough]
    private static bool TranslationFailed(Expression? original, Expression? translation, out SqlExpression? castTranslation)
    {
        if (original != null
            && translation is not SqlExpression)
        {
            castTranslation = null;
            return true;
        }

        castTranslation = translation as SqlExpression;
        return false;
    }
}
