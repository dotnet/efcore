// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    public class ExpressionTransformingQueryModelVisitor : QueryModelVisitorBase
    {
        protected virtual RelinqExpressionVisitor TransformingVisitor { get; }

        public ExpressionTransformingQueryModelVisitor([NotNull] RelinqExpressionVisitor transformingVisitor)
        {
            Check.NotNull(transformingVisitor, nameof(transformingVisitor));

            TransformingVisitor = transformingVisitor;
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
            => fromClause.TransformExpressions(TransformingVisitor.Visit);

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
            => fromClause.TransformExpressions(TransformingVisitor.Visit);

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
            => joinClause.TransformExpressions(TransformingVisitor.Visit);

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
            => joinClause.TransformExpressions(TransformingVisitor.Visit);

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
            => whereClause.TransformExpressions(TransformingVisitor.Visit);

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
            => orderByClause.TransformExpressions(TransformingVisitor.Visit);

        public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
            => ordering.TransformExpressions(TransformingVisitor.Visit);

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
            => selectClause.TransformExpressions(TransformingVisitor.Visit);

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
            => resultOperator.TransformExpressions(TransformingVisitor.Visit);
    }
}
