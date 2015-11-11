// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionVisitors.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public abstract class EntityQueryableExpressionVisitor : DefaultQueryExpressionVisitor
    {
        protected EntityQueryableExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
            : base(Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)))
        {
        }

        protected override Expression VisitConstant(ConstantExpression node)
            => node.Type.GetTypeInfo().IsGenericType
               && (node.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>))
                ? VisitEntityQueryable(((IQueryable)node.Value).ElementType)
                : node;

        protected abstract Expression VisitEntityQueryable([NotNull] Type elementType);
    }
}
