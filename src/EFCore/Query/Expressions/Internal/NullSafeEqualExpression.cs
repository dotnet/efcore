// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NullSafeEqualExpression : Expression, IPrintable
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NullSafeEqualExpression(
            [NotNull] Expression outerKeyNullCheck,
            [NotNull] BinaryExpression equalExpression)
        {
            Check.NotNull(outerKeyNullCheck, nameof(outerKeyNullCheck));
            Check.NotNull(equalExpression, nameof(equalExpression));

            OuterKeyNullCheck = outerKeyNullCheck;
            EqualExpression = equalExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression OuterKeyNullCheck { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual BinaryExpression EqualExpression { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type Type => typeof(bool);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CanReduce => true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Reduce()
            => AndAlso(
                OuterKeyNullCheck,
                EqualExpression);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newNullCheck = visitor.Visit(OuterKeyNullCheck);
            var newLeft = visitor.Visit(EqualExpression.Left);
            var newRight = visitor.Visit(EqualExpression.Right);

            if (newLeft.Type != newRight.Type
                && newLeft.Type.UnwrapNullableType() == newRight.Type.UnwrapNullableType())
            {
                if (!newLeft.Type.IsNullableType())
                {
                    newLeft = Convert(newLeft, newRight.Type);
                }
                else
                {
                    newRight = Convert(newRight, newLeft.Type);
                }
            }

            return newNullCheck != OuterKeyNullCheck
                   || EqualExpression.Left != newLeft
                   || EqualExpression.Right != newRight
                ? new NullSafeEqualExpression(newNullCheck, Equal(newLeft, newRight))
                : this;
        }

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString()
            => $" ?= {EqualExpression} =?";
    }
}
