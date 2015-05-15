// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class NotNullableExpression : ExtensionExpression
    {
        private readonly Expression _operand;

        public NotNullableExpression([NotNull] Expression operand)
            : base(Check.NotNull(operand, nameof(operand)).Type)
        {
            _operand = operand;
        }

        public virtual Expression Operand => _operand;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newExpression = visitor.Visit(_operand);

            return newExpression != _operand
                ? new NotNullableExpression(newExpression)
                : this;
        }

        public override bool CanReduce => true;

        public override Expression Reduce() => _operand;

        public override Type Type => typeof(bool);
    }
}
