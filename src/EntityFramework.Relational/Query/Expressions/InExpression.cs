// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class InExpression : Expression
    {
        public InExpression(
            [NotNull] AliasExpression operand,
            [NotNull] IReadOnlyList<Expression> values)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(values, nameof(values));

            Operand = operand;
            Values = values;
        }

        public virtual AliasExpression Operand { get; }
        public virtual IReadOnlyList<Expression> Values { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(bool);

        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitIn(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override string ToString()
            => Operand.Expression + " IN (" + string.Join(", ", Values) + ")";
    }
}
