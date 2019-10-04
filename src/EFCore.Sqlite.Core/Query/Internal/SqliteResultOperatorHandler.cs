// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteResultOperatorHandler : RelationalResultOperatorHandler
    {
        private static readonly IDictionary<Type, IReadOnlyCollection<Type>> _restrictedOperators
            = new Dictionary<Type, IReadOnlyCollection<Type>>
            {
                [typeof(AverageResultOperator)] = new HashSet<Type>
                {
                    typeof(decimal)
                },
                [typeof(MaxResultOperator)] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [typeof(MinResultOperator)] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [typeof(SumResultOperator)] = new HashSet<Type>
                {
                    typeof(decimal)
                }
            };

        private readonly IResultOperatorHandler _resultOperatorHandler;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteResultOperatorHandler(
            [NotNull] IModel model,
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IResultOperatorHandler resultOperatorHandler)
            : base(model, sqlTranslatingExpressionVisitorFactory, selectExpressionFactory, resultOperatorHandler)
        {
            _resultOperatorHandler = resultOperatorHandler;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            var relationalQueryModelVisitor = (RelationalQueryModelVisitor)entityQueryModelVisitor;
            var selectExpression = relationalQueryModelVisitor.TryGetQuery(queryModel.MainFromClause);

            if (!relationalQueryModelVisitor.RequiresClientResultOperator
                && selectExpression?.Projection.Count == 1
                && _restrictedOperators.TryGetValue(resultOperator.GetType(), out var restrictedTypes))
            {
                PrepareSelectExpressionForAggregate(selectExpression, queryModel);

                if (!(selectExpression.Projection[0].RemoveConvert() is SelectExpression))
                {
                    if (restrictedTypes.Contains(queryModel.SelectClause.Selector.Type.UnwrapNullableType()))
                    {
                        relationalQueryModelVisitor.RequiresClientResultOperator = true;

                        return _resultOperatorHandler.HandleResultOperator(
                            entityQueryModelVisitor,
                            resultOperator,
                            queryModel);
                    }
                }
            }

            return base.HandleResultOperator(entityQueryModelVisitor, resultOperator, queryModel);
        }
    }
}
