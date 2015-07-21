// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public abstract class AggregateExpression : Expression
    {
        private readonly Expression _expression;

        protected AggregateExpression([NotNull] Expression expression)
        {
            _expression = expression;
        }

        public virtual Expression Expression => _expression;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _expression.Type;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
