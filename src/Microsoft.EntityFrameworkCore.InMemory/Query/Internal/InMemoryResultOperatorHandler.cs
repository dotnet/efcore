// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    using ResultHandler = Func<EntityQueryModelVisitor, ResultOperatorBase, QueryModel, Expression>;

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryResultOperatorHandler : IInMemoryResultOperatorHandler
    {
        private static readonly Dictionary<Type, ResultHandler> _handlers
            = new Dictionary<Type, ResultHandler>
            {
                { typeof(OfTypeResultOperator), (v, r, q) => HandleOfType(v, (OfTypeResultOperator)r) },
            };

        private readonly IModel _model;
        private readonly IResultOperatorHandler _resultOperatorHandler;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryResultOperatorHandler(
            [NotNull] IModel model,
            [NotNull] IResultOperatorHandler resultOperatorHandler)
        {
            _model = model;
            _resultOperatorHandler = resultOperatorHandler;
        }

        /// <summary>
        ///     Handles the result operator.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The entity query model visitor. </param>
        /// <param name="resultOperator"> The result operator. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An compiled query expression fragment representing the result operator.
        /// </returns>
        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            ResultHandler handler;
            if (!_handlers.TryGetValue(resultOperator.GetType(), out handler))
            {
                return _resultOperatorHandler.HandleResultOperator(
                    entityQueryModelVisitor,
                    resultOperator,
                    queryModel);
            }

            return handler(entityQueryModelVisitor, resultOperator, queryModel);
        }

        private static Expression HandleOfType(
            EntityQueryModelVisitor entityQueryModelVisitor,
            OfTypeResultOperator ofTypeResultOperator)
        {
            var entityType 
                = entityQueryModelVisitor.QueryCompilationContext.Model
                    .FindEntityType(ofTypeResultOperator.SearchedItemType);

            if (entityType != null)
            {
                var currentExpression = entityQueryModelVisitor.Expression as MethodCallExpression;

                if (currentExpression?.Method == InMemoryQueryModelVisitor.ProjectionQueryMethodInfo)
                {
                    return currentExpression.Update(
                        null,
                        new []
                        {
                            currentExpression.Arguments[0],
                            Expression.Constant(entityType)
                        });
                }
            }

            return Expression.Call(
                entityQueryModelVisitor.LinqOperatorProvider.OfType
                    .MakeGenericMethod(ofTypeResultOperator.SearchedItemType),
                entityQueryModelVisitor.Expression);
        }
    }
}
