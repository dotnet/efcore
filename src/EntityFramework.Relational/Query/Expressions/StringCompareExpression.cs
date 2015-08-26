// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class StringCompareExpression : Expression
    {
        private ExpressionType _op;
        private Expression _left;
        private Expression _right;

        public StringCompareExpression(ExpressionType op, [NotNull] Expression left, [NotNull] Expression right)
        {
            _op = op;
            _left = left;
            _right = right;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public virtual ExpressionType Operator => _op;

        public virtual Expression Left => _left;

        public virtual Expression Right => _right;

        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitStringCompare(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newLeft = visitor.Visit(Left);
            var newRight = visitor.Visit(Right);

            return newLeft != Left || newRight != Right
                ? new StringCompareExpression(Operator, newLeft, newRight)
                : this;
        } 
    }
}
