// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

/// <summary>
/// Looks for aggregate functions (like SUM(), AVG() etc.) in an expression tree, but not in subqueries.
/// </summary>
public sealed class XGContainsAggregateFunctionExpressionVisitor : ExpressionVisitor
{
    // See https://dev.mysql.com/doc/refman/8.0/en/aggregate-functions.html
    private static readonly SortedSet<string> _aggregateFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "AVG",
        "BIT_AND",
        "BIT_OR",
        "BIT_XOR",
        "COUNT",
        "GROUP_CONCAT",
        "JSON_ARRAYAGG",
        "JSON_OBJECTAGG",
        "MAX",
        "MIN",
        "STD",
        "STDDEV",
        "STDDEV_POP",
        "STDDEV_SAMP",
        "SUM",
        "VAR_POP",
        "VAR_SAMP",
        "VARIANCE",
    };

    public bool AggregateFunctionFound { get; private set; }

    public bool ProcessUntilSelect(Expression node)
    {
        // Can be reused within the same thread.
        AggregateFunctionFound = false;

        Visit(node);

        return AggregateFunctionFound;
    }

    public bool ProcessSelect(SelectExpression selectExpression)
    {
        // Can be reused within the same thread.
        AggregateFunctionFound = false;

        foreach (var item in selectExpression.Projection)
        {
            Visit(item);
        }

        foreach (var table in selectExpression.Tables)
        {
            Visit(table);
        }

        Visit(selectExpression.Predicate);

        foreach (var groupingKey in selectExpression.GroupBy)
        {
            Visit(groupingKey);
        }

        Visit(selectExpression.Having);

        foreach (var ordering in selectExpression.Orderings)
        {
            Visit(ordering.Expression);
        }

        Visit(selectExpression.Offset);
        Visit(selectExpression.Limit);

        return AggregateFunctionFound;
    }

    public override Expression Visit(Expression node)
        => AggregateFunctionFound
            ? node
            : base.Visit(node);

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            SqlFunctionExpression sqlFunctionExpression => VisitSqlFunction(sqlFunctionExpression),
            SelectExpression selectExpression => selectExpression,
            ShapedQueryExpression shapedQueryExpression => shapedQueryExpression,
            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        if (_aggregateFunctions.Contains(sqlFunctionExpression.Name))
        {
            AggregateFunctionFound = true;
            return sqlFunctionExpression;
        }

        return base.VisitExtension(sqlFunctionExpression);
    }
}
