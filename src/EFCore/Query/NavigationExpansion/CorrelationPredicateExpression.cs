// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class CorrelationPredicateExpression : Expression, IPrintable
    {
        public CorrelationPredicateExpression(
            [NotNull] Expression outerKeyNullCheck,
            [NotNull] BinaryExpression equalExpression)
        {
            Check.NotNull(outerKeyNullCheck, nameof(outerKeyNullCheck));
            Check.NotNull(equalExpression, nameof(equalExpression));

            OuterKeyNullCheck = outerKeyNullCheck;
            EqualExpression = equalExpression;
        }

        public virtual Expression OuterKeyNullCheck { get; }

        public virtual BinaryExpression EqualExpression { get; }

        public override Type Type => typeof(bool);

        public override bool CanReduce => true;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Expression Reduce()
            => AndAlso(
                OuterKeyNullCheck,
                EqualExpression);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newNullCheck = visitor.Visit(OuterKeyNullCheck);
            var newEqualExpression = (BinaryExpression)visitor.Visit(EqualExpression);

            return Update(newNullCheck, newEqualExpression);
        }

        public virtual CorrelationPredicateExpression Update(Expression outerKeyNullCheck, BinaryExpression equalExpression)
            => outerKeyNullCheck != OuterKeyNullCheck || equalExpression != EqualExpression
            ? new CorrelationPredicateExpression(outerKeyNullCheck, equalExpression)
            : this;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append(" ?= ");
            expressionPrinter.Visit(EqualExpression);
            expressionPrinter.StringBuilder.Append(" =? ");
        }
    }
}
