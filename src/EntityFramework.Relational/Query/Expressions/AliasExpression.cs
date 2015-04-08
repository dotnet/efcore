// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class AliasExpression : ExtensionExpression
    {
        private readonly Expression _expression;

        public AliasExpression([NotNull] Expression expression)
            : base(Check.NotNull(expression, nameof(expression)).Type)
        {
            _expression = expression;
        }

        public AliasExpression([CanBeNull] string alias, [NotNull] Expression expression)
            : base(Check.NotNull(expression, nameof(expression)).Type)
        {
            Alias = alias;
            _expression = expression;
        }

        public virtual string Alias{ get; [param: NotNull] set; }

        public virtual Expression Expression
        {
            get { return _expression; }
        }

        public virtual bool Projected { get; set; } = false;

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitAliasExpression(this)
                : base.Accept(visitor);
        }


        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var newInnerExpression = visitor.VisitExpression(_expression);

            return newInnerExpression != _expression ? new AliasExpression(Alias, newInnerExpression) : this;
        }

        public override string ToString()
        {
            return this.ColumnExpression()?.ToString() ?? Expression.NodeType + " " + Alias;
        }
    }
}
