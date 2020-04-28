// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class CollateExpression : SqlExpression
    {
        public CollateExpression([NotNull] SqlExpression operand, [NotNull] string collation)
            : base(operand.Type, operand.TypeMapping)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotEmpty(collation, nameof(collation));

            Operand = operand;
            Collation = collation;
        }

        public virtual SqlExpression Operand { get; }
        public virtual string Collation { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SqlExpression)visitor.Visit(Operand));
        }

        public virtual CollateExpression Update([NotNull] SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return operand != Operand
                ? new CollateExpression(operand, Collation)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Visit(Operand);
            expressionPrinter
                .Append(" COLLATE ")
                .Append(Collation);
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is CollateExpression collateExpression
                    && Equals(collateExpression));

        private bool Equals(CollateExpression collateExpression)
            => base.Equals(collateExpression)
                && Operand.Equals(collateExpression.Operand)
                && Collation.Equals(collateExpression.Collation, StringComparison.Ordinal);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Operand, Collation);
    }
}
