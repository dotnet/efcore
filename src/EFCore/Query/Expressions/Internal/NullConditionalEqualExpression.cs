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
    public class NullConditionalEqualExpression : Expression, IPrintable
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NullConditionalEqualExpression(
            [NotNull] Expression outerNullProtection,
            [NotNull] Expression outerKey,
            [NotNull] Expression innerKey)
        {
            Check.NotNull(outerNullProtection, nameof(outerNullProtection));
            Check.NotNull(outerKey, nameof(outerKey));
            Check.NotNull(innerKey, nameof(innerKey));

            OuterNullProtection = outerNullProtection;
            OuterKey = outerKey;
            InnerKey = innerKey;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression OuterNullProtection { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression OuterKey { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression InnerKey { get; }

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
                OuterNullProtection,
                Equal(
                    OuterKey,
                    InnerKey));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newOuterCaller = visitor.Visit(OuterNullProtection);
            var newOuterKey = visitor.Visit(OuterKey);
            var newInnerKey = visitor.Visit(InnerKey);

            if (newOuterKey.Type != newInnerKey.Type
                && newOuterKey.Type.UnwrapNullableType() == newInnerKey.Type.UnwrapNullableType())
            {
                if (!newOuterKey.Type.IsNullableType())
                {
                    newOuterKey = Convert(newOuterKey, newInnerKey.Type);
                }
                else
                {
                    newInnerKey = Convert(newInnerKey, newOuterKey.Type);
                }
            }

            return newOuterCaller != OuterNullProtection || newOuterKey != OuterKey || newInnerKey != InnerKey
                ? new NullConditionalEqualExpression(newOuterCaller, newOuterKey, newInnerKey)
                : this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(OuterKey);
            expressionPrinter.StringBuilder.Append(" ?= ");
            expressionPrinter.Visit(InnerKey);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString()
            => OuterKey + " ?= " + InnerKey;
    }
}
