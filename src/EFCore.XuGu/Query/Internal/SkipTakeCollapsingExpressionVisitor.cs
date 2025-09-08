// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class SkipTakeCollapsingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private IReadOnlyDictionary<string, object?> _parameterValues;
        private bool _canCache;

        public SkipTakeCollapsingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
        {
            Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));

            _sqlExpressionFactory = sqlExpressionFactory;
            _parameterValues = null!;
        }

        public virtual Expression Process(
            Expression selectExpression,
            IReadOnlyDictionary<string, object?> parametersValues,
            out bool canCache)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            _parameterValues = parametersValues;
            _canCache = true;

            var result = Visit(selectExpression);

            canCache = _canCache;

            return result;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is SelectExpression selectExpression)
            {
                if (IsZero(selectExpression.Limit)
                    && IsZero(selectExpression.Offset))
                {
                    return selectExpression.Update(
                        selectExpression.Tables,
                        selectExpression.GroupBy.Count > 0
                            ? selectExpression.Predicate
                            : _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(false)),
                        selectExpression.GroupBy,
                        selectExpression.GroupBy.Count > 0
                            ? _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(false))
                            : null,
                        selectExpression.Projection,
                        new List<OrderingExpression>(0),
                        offset: null,
                        limit: null);
                }

                bool IsZero(SqlExpression? sqlExpression)
                {
                    switch (sqlExpression)
                    {
                        case SqlConstantExpression constant
                        when constant.Value is int intValue:
                            return intValue == 0;
                        case SqlParameterExpression parameter:
                            _canCache = false;
                            return _parameterValues[parameter.Name] is int value && value == 0;

                        default:
                            return false;
                    }
                }
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
