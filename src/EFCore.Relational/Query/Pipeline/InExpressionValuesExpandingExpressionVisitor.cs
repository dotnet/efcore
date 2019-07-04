// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private class InExpressionValuesExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private IReadOnlyDictionary<string, object> _parametersValues;
            private ConstantExpressionIdentifier _constantExpressionIdentifier;
            private bool _innerExpressionChangedToConstant;

            public InExpressionValuesExpandingExpressionVisitor(
                ISqlExpressionFactory sqlExpressionFactory, IReadOnlyDictionary<string, object> parametersValues)
            {
                _sqlExpressionFactory = sqlExpressionFactory;
                _parametersValues = parametersValues;
                _constantExpressionIdentifier = new ConstantExpressionIdentifier();
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is InExpression inExpression
                    && inExpression.Values != null)
                {
                    var inValues = new List<object>();
                    var hasNullValue = false;
                    RelationalTypeMapping typeMapping = null;

                    switch (inExpression.Values)
                    {
                        // TODO: This shouldn't be here - should be part of compilation instead (#16375)
                        case SqlConstantExpression sqlConstant:
                            {
                                typeMapping = sqlConstant.TypeMapping;
                                var values = (IEnumerable)sqlConstant.Value;
                                foreach (var value in values)
                                {
                                    if (value == null)
                                    {
                                        hasNullValue = true;
                                        continue;
                                    }

                                    inValues.Add(value);
                                }
                            }
                            break;

                        case SqlParameterExpression sqlParameter:
                            {
                                typeMapping = sqlParameter.TypeMapping;
                                var values = (IEnumerable)_parametersValues[sqlParameter.Name];
                                foreach (var value in values)
                                {
                                    if (value == null)
                                    {
                                        hasNullValue = true;
                                        continue;
                                    }

                                    inValues.Add(value);
                                }
                            }
                            break;
                    }

                    var updatedInExpression = inValues.Count > 0
                        ? _sqlExpressionFactory.In(
                            (SqlExpression)Visit(inExpression.Item),
                            _sqlExpressionFactory.Constant(inValues, typeMapping),
                            inExpression.Negated)
                        : null;

                    var nullCheckExpression = hasNullValue
                        ? inExpression.Negated
                            ? _sqlExpressionFactory.IsNotNull(inExpression.Item)
                            : _sqlExpressionFactory.IsNull(inExpression.Item)
                        : null;

                    if (updatedInExpression != null && nullCheckExpression != null)
                    {
                        return inExpression.Negated
                            ? _sqlExpressionFactory.AndAlso(updatedInExpression, nullCheckExpression)
                            : _sqlExpressionFactory.OrElse(updatedInExpression, nullCheckExpression);
                    }

                    if (updatedInExpression == null && nullCheckExpression == null)
                    {
                        _innerExpressionChangedToConstant = true;
                        return _sqlExpressionFactory.Equal(_sqlExpressionFactory.Constant(true), _sqlExpressionFactory.Constant(inExpression.Negated));
                    }

                    return (SqlExpression)updatedInExpression ?? nullCheckExpression;
                }

                return base.Visit(expression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                var visited = base.VisitExtension(extensionExpression);
                if (_innerExpressionChangedToConstant && visited is OrderingExpression orderingExpression)
                {
                    // Our rewriting of InExpression above may have left an ordering which contains only constants -
                    // invalid in SqlServer. If so, rewrite the entire ordering to a single constant expression which
                    // QuerySqlGenerator will recognize and render as (SELECT 1).
                    _innerExpressionChangedToConstant = false;
                    return _constantExpressionIdentifier.IsExpressionConstant(orderingExpression)
                        ? new OrderingExpression(
                            new SqlConstantExpression(Expression.Constant(1), _sqlExpressionFactory.FindMapping(typeof(int))),
                            true)
                        : orderingExpression;
                }

                return visited;
            }
        }

        /// <summary>
        /// Visits an expression tree returns identifies whether it contains any non-constant nodes, e.g. columns.
        /// </summary>
        private class ConstantExpressionIdentifier : ExpressionVisitor
        {
            private bool _wasConstant;

            public bool IsExpressionConstant(Expression node)
            {
                _wasConstant = true;
                Visit(node);
                return _wasConstant;
            }

            public override Expression Visit(Expression node)
            {
                // TODO: can be optimized by immediately returning null when a matching node is found (but must be handled everywhere...)

                switch (node)
                {
                    case ColumnExpression _:
                    case SqlFragmentExpression _:  // May or may not be constant, but better to error than to give wrong results
                        _wasConstant = false;
                        break;
                }

                return base.Visit(node);
            }
        }
    }
}
