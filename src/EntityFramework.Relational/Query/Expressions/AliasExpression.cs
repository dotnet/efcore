// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class AliasExpression : Expression
    {
        private readonly Expression _expression;

        public AliasExpression([NotNull] Expression expression)
        {
            _expression = expression;
        }

        public AliasExpression([CanBeNull] string alias, [NotNull] Expression expression)
        {
            Alias = alias;
            _expression = expression;
        }

        public virtual string Alias { get; [param: NotNull] set; }

        public virtual Expression Expression => _expression;

        public virtual bool Projected { get; set; } = false;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _expression.Type;

        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitAlias(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newInnerExpression = visitor.Visit(_expression);

            return newInnerExpression != _expression ? new AliasExpression(Alias, newInnerExpression) : this;
        }

        public override string ToString()
            => this.TryGetColumnExpression()?.ToString() ?? Expression.NodeType + " " + Alias;
    }
}
