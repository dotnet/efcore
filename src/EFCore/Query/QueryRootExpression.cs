// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryRootExpression : Expression, IPrintableExpression
    {
        private readonly Type _type;

        public QueryRootExpression([NotNull] IAsyncQueryProvider asyncQueryProvider, [NotNull] IEntityType entityType)
        {
            Check.NotNull(asyncQueryProvider, nameof(asyncQueryProvider));
            Check.NotNull(entityType, nameof(entityType));

            QueryProvider = asyncQueryProvider;
            EntityType = entityType;
            _type = typeof(IQueryable<>).MakeGenericType(entityType.ClrType);
        }

        public QueryRootExpression([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            EntityType = entityType;
            QueryProvider = null;
            _type = typeof(IQueryable<>).MakeGenericType(entityType.ClrType);
        }

        public virtual IAsyncQueryProvider QueryProvider { get; }
        public virtual IEntityType EntityType { get; }

        public virtual Expression DetachQueryProvider() => new QueryRootExpression(EntityType);
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => _type;
        public override bool CanReduce => false;
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            if (EntityType.HasSharedClrType)
            {
                expressionPrinter.Append($"DbSet<{EntityType.ClrType.ShortDisplayName()}>(\"{EntityType.Name}\")");
            }
            else
            {
                expressionPrinter.Append($"DbSet<{EntityType.ClrType.ShortDisplayName()}>()");
            }
        }

        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                || obj is QueryRootExpression queryRootExpression
                    && EntityType == queryRootExpression.EntityType);

        public override int GetHashCode() => EntityType.GetHashCode();
    }
}
