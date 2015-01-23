// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class DefaultQueryExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private readonly EntityQueryModelVisitor _entityQueryModelVisitor;

        public DefaultQueryExpressionTreeVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
        {
            Check.NotNull(entityQueryModelVisitor, "entityQueryModelVisitor");

            _entityQueryModelVisitor = entityQueryModelVisitor;
        }

        public virtual EntityQueryModelVisitor QueryModelVisitor => _entityQueryModelVisitor;

        protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
        {
            Check.NotNull(subQueryExpression, "subQueryExpression");

            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

            return queryModelVisitor.Expression;
        }

        protected EntityQueryModelVisitor CreateQueryModelVisitor()
        {
            return QueryModelVisitor.QueryCompilationContext
                .CreateQueryModelVisitor(_entityQueryModelVisitor);
        }
    }
}
