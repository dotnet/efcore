// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class LiteralExpression : Expression
    {
        public LiteralExpression([NotNull] string literal)
        {
            Literal = literal;
        }

        public virtual string Literal { get; }

        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitLiteral(this)
                : base.Accept(visitor);
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(string);

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override string ToString() => Literal;
    }
}
