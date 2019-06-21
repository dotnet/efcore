// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append(" ?= ");
            expressionPrinter.Visit(EqualExpression);
            expressionPrinter.StringBuilder.Append(" =? ");
        }
    }
}
