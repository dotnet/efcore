// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that gets values from <see cref="ShapedQueryExpression.QueryExpression" /> to be used in
    ///         <see cref="ShapedQueryExpression.ShaperExpression" /> while creating results.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ProjectionBindingExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ProjectionBindingExpression" /> class.
        /// </summary>
        /// <param name="queryExpression"> The query expression to get the value from. </param>
        /// <param name="projectionMember"> The projection member to bind with query expression. </param>
        /// <param name="type"> The clr type of value being read. </param>
        public ProjectionBindingExpression(
            [NotNull] Expression queryExpression,
            [NotNull] ProjectionMember projectionMember,
            [NotNull] Type type)
        {
            Check.NotNull(queryExpression, nameof(queryExpression));
            Check.NotNull(projectionMember, nameof(projectionMember));
            Check.NotNull(type, nameof(type));

            QueryExpression = queryExpression;
            ProjectionMember = projectionMember;
            Type = type;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ProjectionBindingExpression" /> class.
        /// </summary>
        /// <param name="queryExpression"> The query expression to get the value from. </param>
        /// <param name="index"> The index to bind with query expression projection. </param>
        /// <param name="type"> The clr type of value being read. </param>
        public ProjectionBindingExpression(
            [NotNull] Expression queryExpression,
            int index,
            [NotNull] Type type)
        {
            Check.NotNull(queryExpression, nameof(queryExpression));
            Check.NotNull(type, nameof(type));

            QueryExpression = queryExpression;
            Index = index;
            Type = type;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ProjectionBindingExpression" /> class.
        /// </summary>
        /// <param name="queryExpression"> The query expression to get the value from. </param>
        /// <param name="indexMap"> The index map to bind with query expression projection for ValueBuffer. </param>
        public ProjectionBindingExpression(
            [NotNull] Expression queryExpression,
            [NotNull] IDictionary<IProperty, int> indexMap)
        {
            Check.NotNull(queryExpression, nameof(queryExpression));
            Check.NotNull(indexMap, nameof(indexMap));

            QueryExpression = queryExpression;
            IndexMap = indexMap;
            Type = typeof(ValueBuffer);
        }

        /// <summary>
        ///     The query expression to bind with.
        /// </summary>
        public virtual Expression QueryExpression { get; }

        /// <summary>
        ///     The projection member to bind if binding is via projection member.
        /// </summary>
        public virtual ProjectionMember ProjectionMember { get; }

        /// <summary>
        ///     The projection member to bind if binding is via projection index.
        /// </summary>
        public virtual int? Index { get; }

        /// <summary>
        ///     The projection member to bind if binding is via index map for a value buffer.
        /// </summary>
        public virtual IDictionary<IProperty, int> IndexMap { get; }

        /// <inheritdoc />
        public override Type Type { get; }

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append(nameof(ProjectionBindingExpression) + ": ");
            if (ProjectionMember != null)
            {
                expressionPrinter.Append(ProjectionMember.ToString());
            }
            else if (Index != null)
            {
                expressionPrinter.Append(Index.ToString());
            }
            else
            {
                using (expressionPrinter.Indent())
                {
                    foreach (var kvp in IndexMap)
                    {
                        expressionPrinter.AppendLine($"{kvp.Key.Name}:{kvp.Value},");
                    }
                }
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ProjectionBindingExpression projectionBindingExpression
                    && Equals(projectionBindingExpression));

        private bool Equals(ProjectionBindingExpression projectionBindingExpression)
            => QueryExpression.Equals(projectionBindingExpression.QueryExpression)
                && Type == projectionBindingExpression.Type
                && (ProjectionMember?.Equals(projectionBindingExpression.ProjectionMember)
                    ?? projectionBindingExpression.ProjectionMember == null)
                && Index == projectionBindingExpression.Index
                // Using reference equality here since if we are this far, we don't need to compare this.
                && IndexMap == projectionBindingExpression.IndexMap;

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(QueryExpression, ProjectionMember, Index, IndexMap);
    }
}
