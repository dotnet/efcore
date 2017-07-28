// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using JetBrains.Annotations;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     An In-memory query compilation context. The primary data structure representing the state/components
    ///     used during query compilation for In-memory provider.
    /// </summary>
    public class InMemoryQueryCompilationContext : QueryCompilationContext
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryQueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            bool trackQueryResults)
            : base(dependencies, linqOperatorProvider, trackQueryResults)
        {
        }

        /// <summary>
        ///     Determines all query sources that require materialization.
        /// </summary>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="queryModel"> The query model. </param>
        public override void FindQuerySourcesRequiringMaterialization(
            EntityQueryModelVisitor queryModelVisitor, 
            QueryModel queryModel)
        {
            base.FindQuerySourcesRequiringMaterialization(queryModelVisitor, queryModel);

            var visitor = new InMemoryIncludeTypeOperatorsCompensatingVisitor(this);
            queryModel.TransformExpressions(visitor.Visit);
        }

        private class InMemoryIncludeTypeOperatorsCompensatingVisitor : ExpressionVisitorBase
        {
            private readonly QueryCompilationContext _queryCompilationContext;

            private bool _insideIncludeMethod = false;

            public InMemoryIncludeTypeOperatorsCompensatingVisitor(QueryCompilationContext queryCompilationContext)
            {
                _queryCompilationContext = queryCompilationContext;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (IncludeCompiler.IsIncludeMethod(node))
                {
                    _insideIncludeMethod = true;
                }

                return base.VisitMethodCall(node);
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (_insideIncludeMethod
                    && node.NodeType == ExpressionType.TypeAs
                    && node.Operand is QuerySourceReferenceExpression qsre)
                {
                    _queryCompilationContext.AddQuerySourceRequiringMaterialization(qsre.ReferencedQuerySource);
                }

                return base.VisitUnary(node);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is NullConditionalExpression nullConditional)
                {
                    Visit(nullConditional.Caller);
                    Visit(nullConditional.AccessOperation);
                }

                return base.VisitExtension(extensionExpression);
            }
        }
    }
}
