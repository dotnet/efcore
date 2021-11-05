// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SkipTakeCollapsingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private IReadOnlyDictionary<string, object?> _parameterValues;
        private bool _canCache;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SkipTakeCollapsingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _parameterValues = null!;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SelectExpression Process(
            SelectExpression selectExpression,
            IReadOnlyDictionary<string, object?> parametersValues,
            out bool canCache)
        {
            _parameterValues = parametersValues;
            _canCache = true;

            var result = (SelectExpression)Visit(selectExpression);

            canCache = _canCache;

            return result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is SelectExpression selectExpression)
            {
                if (IsZero(selectExpression.Limit)
                    && IsZero(selectExpression.Offset))
                {
                    return selectExpression.Update(
                        selectExpression.Projection,
                        selectExpression.Tables,
                        selectExpression.GroupBy.Count > 0 ? selectExpression.Predicate : _sqlExpressionFactory.Constant(false),
                        selectExpression.GroupBy,
                        selectExpression.GroupBy.Count > 0 ? _sqlExpressionFactory.Constant(false) : null,
                        new List<OrderingExpression>(0),
                        limit: null,
                        offset: null);
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
