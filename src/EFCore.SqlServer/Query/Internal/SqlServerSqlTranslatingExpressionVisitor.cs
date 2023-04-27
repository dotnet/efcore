// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
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
            return TranslateByteArrayElementAccess(
                binaryExpression.Left,
                binaryExpression.Right,
                binaryExpression.Type);
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
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression is MethodCallExpression { Method: MethodInfo { IsGenericMethod: true } } genericMethodCall
            && genericMethodCall.Method.GetGenericMethodDefinition() == EnumerableMethods.ElementAt
            && genericMethodCall.Arguments[0].Type == typeof(byte[]))
        {
            return TranslateByteArrayElementAccess(
                genericMethodCall.Arguments[0],
                genericMethodCall.Arguments[1],
                methodCallExpression.Type);
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    private Expression TranslateByteArrayElementAccess(Expression array, Expression index, Type resultType)
    {
        var visitedArray = Visit(array);
        var visitedIndex = Visit(index);

        return visitedArray is SqlExpression sqlArray
            && visitedIndex is SqlExpression sqlIndex
            ? Dependencies.SqlExpressionFactory.Convert(
                    Dependencies.SqlExpressionFactory.Function(
                        "SUBSTRING",
                        new[]
                        {
                            sqlArray,
                            Dependencies.SqlExpressionFactory.Add(
                                Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(sqlIndex),
                                Dependencies.SqlExpressionFactory.Constant(1)),
                            Dependencies.SqlExpressionFactory.Constant(1)
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true, true },
                        typeof(byte[])),
                    resultType)
            : QueryCompilationContext.NotTranslatedExpression;
    }

    private static string? GetProviderType(SqlExpression expression)
        => expression.TypeMapping?.StoreType;
}
