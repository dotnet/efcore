// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    public class ExplicitCastExpression : Expression
    {
        private readonly Type _type;

        public ExplicitCastExpression([NotNull] Expression operand, [NotNull] Type type)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(type, nameof(type));

            Operand = operand;
            _type = type;
        }

        public virtual Expression Operand { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _type;

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitExplicitCast(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newOperand = visitor.Visit(Operand);

            return newOperand != Operand
                ? new ExplicitCastExpression(newOperand, _type)
                : this;
        }

        public override string ToString() => "CAST(" + Operand + " AS " + _type.Name + ")";
    }
}
