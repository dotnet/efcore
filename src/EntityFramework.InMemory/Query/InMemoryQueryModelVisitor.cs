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
            return new InMemoryQueryingExpressionTreeVisitor(this);
        }

        private static readonly MethodInfo _entityScanMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("EntityScan");

        [UsedImplicitly]
        private static IEnumerable<TEntity> EntityScan<TEntity>(QueryContext queryContext)
        {
            var entityType = queryContext.Model.GetEntityType(typeof(TEntity));

            return ((InMemoryQueryContext)queryContext).Database.GetTable(entityType)
                .Select(t => (TEntity)queryContext.StateManager
                    .GetOrMaterializeEntry(entityType, new ObjectArrayValueReader(t)).Entity);
        }

        private class InMemoryQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
        {
            public InMemoryQueryingExpressionTreeVisitor(EntityQueryModelVisitor entityQueryModelVisitor)
                : base(entityQueryModelVisitor)
            {
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                Check.NotNull(elementType, "elementType");

                return Expression.Call(
                    _entityScanMethodInfo.MakeGenericMethod(elementType),
                    QueryContextParameter);
            }
        }
    }
}
