// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     Visitor for processing entity types roots.
    /// </summary>
    public abstract class EntityQueryableExpressionVisitor : DefaultQueryExpressionVisitor
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityQueryableExpressionVisitor" /> class.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The visitor for the query. </param>
        protected EntityQueryableExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
            : base(Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)))
        {
        }

        /// <summary>
        ///     Visits <see cref="ConstantExpression" /> nodes.
        /// </summary>
        /// <param name="constantExpression"> The node being visited. </param>
        /// <returns> An expression to use in place of the node. </returns>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => constantExpression.IsEntityQueryable()
                ? VisitEntityQueryable(((IQueryable)constantExpression.Value).ElementType)
                : constantExpression;

        /// <summary>
        ///     Visits entity type roots.
        /// </summary>
        /// <param name="elementType"> The entity type of the root. </param>
        /// <returns> An expression to use in place of the node. </returns>
        protected abstract Expression VisitEntityQueryable([NotNull] Type elementType);
    }
}
