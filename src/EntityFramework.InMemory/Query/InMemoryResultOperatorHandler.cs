// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ResultOperators;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.InMemory.Query
{
    using ResultHandler = Func<EntityQueryModelVisitor, ResultOperatorBase, QueryModel, Expression>;

    public class InMemoryResultOperatorHandler : IResultOperatorHandler
    {
        private static readonly Dictionary<Type, ResultHandler>
            _resultHandlers = new Dictionary<Type, ResultHandler>
                {
                    { typeof(IncludeResultOperator), (e, r, _) => HandleInclude(e, (IncludeResultOperator)r) }
                };

        private readonly IResultOperatorHandler _resultOperatorHandler = new ResultOperatorHandler();

        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            Check.NotNull(entityQueryModelVisitor, "entityQueryModelVisitor");
            Check.NotNull(resultOperator, "resultOperator");
            Check.NotNull(queryModel, "queryModel");

            ResultHandler resultHandler;
            if (!_resultHandlers.TryGetValue(resultOperator.GetType(), out resultHandler))
            {
                return _resultOperatorHandler
                    .HandleResultOperator(
                        entityQueryModelVisitor,
                        resultOperator,
                        queryModel);
            }

            return resultHandler(entityQueryModelVisitor, resultOperator, queryModel);
        }

        private static Expression HandleInclude(
            EntityQueryModelVisitor entityQueryModelVisitor, IncludeResultOperator includeResultOperator)
        {
            var navigation
                = entityQueryModelVisitor
                    .BindNavigationMemberExpression(
                        (MemberExpression)includeResultOperator.NavigationPropertyPath,
                        (n, _) => n);

            if (navigation == null)
            {
                return entityQueryModelVisitor.Expression;
            }

            return Expression.Call(
                _includeMethodInfo.MakeGenericMethod(
                    entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType),
                EntityQueryModelVisitor.QueryContextParameter,
                entityQueryModelVisitor.Expression,
                Expression.Constant(navigation));
        }

        private static readonly MethodInfo _includeMethodInfo
            = typeof(InMemoryResultOperatorHandler).GetTypeInfo()
                .GetDeclaredMethod("Include");

        [UsedImplicitly]
        private static IEnumerable<TEntity> Include<TEntity>(
            QueryContext queryContext, IEnumerable<TEntity> source, INavigation navigation)
        {
            var inMemoryQueryContext = ((InMemoryQueryContext)queryContext);
            var targetEntityType = navigation.GetTargetType();
            var targetTable = inMemoryQueryContext.Database.GetTable(targetEntityType);

            return
                source
                    .Select(entity =>
                        {
                            inMemoryQueryContext.QueryBuffer
                                .Include(
                                    entity,
                                    navigation,
                                    targetTable.Select(vs => new ObjectArrayValueReader(vs)));

                            return entity;
                        });
        }
    }
}
