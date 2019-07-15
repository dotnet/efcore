// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class RootReferenceExpression : Expression, IAccessExpression
    {
        public RootReferenceExpression(IEntityType entityType, string alias)
        {
            EntityType = entityType;
            Alias = alias;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => EntityType.ClrType;
        public IEntityType EntityType { get; }
        public string Alias { get; }
        string IAccessExpression.Name => Alias;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override string ToString() => Alias;

        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj is RootReferenceExpression rootReferenceExpression
                   && Equals(rootReferenceExpression));

        private bool Equals(RootReferenceExpression rootReferenceExpression)
            => string.Equals(Alias, rootReferenceExpression.Alias)
               && EntityType.Equals(rootReferenceExpression.EntityType);

        public override int GetHashCode() => HashCode.Combine(Alias, EntityType);
    }
}
