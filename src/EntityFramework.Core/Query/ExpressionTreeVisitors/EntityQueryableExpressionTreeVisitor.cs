// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public abstract class EntityQueryableExpressionTreeVisitor : DefaultQueryExpressionTreeVisitor
    {
        protected EntityQueryableExpressionTreeVisitor(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(
                Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)),
                Check.NotNull(querySource, nameof(querySource)))
        {
        }

        protected override Expression VisitConstantExpression(ConstantExpression constantExpression)
        {
            if (constantExpression.Type.GetTypeInfo().IsGenericType
                && constantExpression.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>))
            {
                return VisitEntityQueryable(((IQueryable)constantExpression.Value).ElementType);
            }

            return constantExpression;
        }

        protected abstract Expression VisitEntityQueryable([NotNull] Type elementType);
    }
}
