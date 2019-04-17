// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
