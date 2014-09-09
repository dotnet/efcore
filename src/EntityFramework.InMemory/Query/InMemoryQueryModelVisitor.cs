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
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryModelVisitor : EntityQueryModelVisitor
    {
        public InMemoryQueryModelVisitor([NotNull] QueryCompilationContext queryCompilationContext)
            : base(Check.NotNull(queryCompilationContext, "queryCompilationContext"))
        {
        }

        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(IQuerySource querySource)
        {
            return new InMemoryQueryingExpressionTreeVisitor(this, querySource);
        }

        private static readonly MethodInfo _entityQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("EntityQuery");

        [UsedImplicitly]
        private static IEnumerable<TEntity> EntityQuery<TEntity>(QueryContext queryContext)
        {
            var entityType = queryContext.Model.GetEntityType(typeof(TEntity));
            var inMemoryTable = ((InMemoryQueryContext)queryContext).Database.GetTable(entityType);

            return inMemoryTable
                .Select(t => (TEntity)queryContext.QueryBuffer
                    .GetEntity(entityType, new ObjectArrayValueReader(t)));
        }

        private static readonly MethodInfo _projectionQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("ProjectionQuery");

        [UsedImplicitly]
        private static IEnumerable<IValueReader> ProjectionQuery<TEntity>(QueryContext queryContext)
        {
            var entityType = queryContext.Model.GetEntityType(typeof(TEntity));

            return ((InMemoryQueryContext)queryContext).Database.GetTable(entityType)
                .Select(t => new ObjectArrayValueReader(t));
        }

        private class InMemoryQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
        {
            private readonly IQuerySource _querySource;

            public InMemoryQueryingExpressionTreeVisitor(
                InMemoryQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource)
                : base(entityQueryModelVisitor)
            {
                _querySource = querySource;
            }

            private new InMemoryQueryModelVisitor QueryModelVisitor
            {
                get { return (InMemoryQueryModelVisitor)base.QueryModelVisitor; }
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                Check.NotNull(elementType, "elementType");

                var queryMethodInfo = _projectionQueryMethodInfo;

                if (QueryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    queryMethodInfo = _entityQueryMethodInfo;
                }

                return Expression.Call(
                    queryMethodInfo.MakeGenericMethod(elementType),
                    QueryContextParameter);
            }
        }
    }
}
