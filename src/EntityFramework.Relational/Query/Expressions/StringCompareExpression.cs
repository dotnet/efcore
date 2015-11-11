// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class StringCompareExpression : Expression
    {
        public StringCompareExpression(ExpressionType op, [NotNull] Expression left, [NotNull] Expression right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(bool);

        public virtual ExpressionType Operator { get; }

        public virtual Expression Left { get; }

        public virtual Expression Right { get; }

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

            return (newLeft != Left) || (newRight != Right)
                ? new StringCompareExpression(Operator, newLeft, newRight)
                : this;
        }
    }
}
