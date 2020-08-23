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
    /// <summary>
    ///     <para>
    ///         An expression that represents a query root in query expression.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class QueryRootExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="QueryRootExpression" /> class with associated query provider.
        /// </summary>
        /// <param name="asyncQueryProvider"> The query provider associated with this query root. </param>
        /// <param name="entityType"> The entity type this query root represents. </param>
        public QueryRootExpression([NotNull] IAsyncQueryProvider asyncQueryProvider, [NotNull] IEntityType entityType)
        {
            Check.NotNull(asyncQueryProvider, nameof(asyncQueryProvider));
            Check.NotNull(entityType, nameof(entityType));

            QueryProvider = asyncQueryProvider;
            EntityType = entityType;
            Type = typeof(IQueryable<>).MakeGenericType(entityType.ClrType);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="QueryRootExpression" /> class without any query provider.
        /// </summary>
        /// <param name="entityType"> The entity type this query root represents. </param>
        public QueryRootExpression([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            EntityType = entityType;
            QueryProvider = null;
            Type = typeof(IQueryable<>).MakeGenericType(entityType.ClrType);
        }

        /// <summary>
        ///     The query provider associated with this query root.
        /// </summary>
        public virtual IAsyncQueryProvider QueryProvider { get; }

        /// <summary>
        ///     The entity type reprenseted by this query root.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     Detaches the associated query provider from this query root expression.
        /// </summary>
        /// <returns> A new query root expression without query provider. </returns>
        public virtual Expression DetachQueryProvider()
            => new QueryRootExpression(EntityType);

        /// <inheritdoc />
        public override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <inheritdoc />
        public override bool CanReduce
            => false;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

        /// <summary>
        ///     Creates a printable string representation of the given expression using <see cref="ExpressionPrinter" />.
        /// </summary>
        /// <param name="expressionPrinter"> The expression printer to use. </param>
        protected virtual void Print([NotNull] ExpressionPrinter expressionPrinter)
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

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
            => Print(expressionPrinter);

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is QueryRootExpression queryRootExpression
                    && EntityType == queryRootExpression.EntityType);

        /// <inheritdoc />
        public override int GetHashCode()
            => EntityType.GetHashCode();
    }
}
