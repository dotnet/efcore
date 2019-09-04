// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public abstract class TableExpressionBase : Expression, IPrintableExpression
    {
        protected TableExpressionBase([CanBeNull] string alias)
        {
            Check.NullButNotEmpty(alias, nameof(alias));

            Alias = alias;
        }

        public virtual string Alias { get; internal set; }

        /// <summary>
        /// Populated after this expression is first visited from its containing <see cref="SelectExpression"/>, to make sure that
        /// subsequent visits return the same instance, and to prevent needless multiple deep visits. Used only by visitors.
        /// </summary>
        /// <remarks>
        /// If you implement an expression visitor which contains specific logic for visiting <see cref="TableExpressionBase"/>,
        /// you should properly populate and check this field (<see cref="SelectExpression.VisitChildren"/> for an example).
        /// </remarks>
        public TableExpressionBase VisitedExpression { get; set; }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override Type Type => typeof(object);
        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        public abstract void Print(ExpressionPrinter expressionPrinter);

        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj is TableExpressionBase tableExpressionBase
                   && Equals(tableExpressionBase));

        private bool Equals(TableExpressionBase tableExpressionBase)
            => string.Equals(Alias, tableExpressionBase.Alias);

        public override int GetHashCode() => HashCode.Combine(Alias);
    }
}
