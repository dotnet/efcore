// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class ExistsExpression : ExtensionExpression
    {
        public ExistsExpression([NotNull] Expression expression)
            : base(typeof(bool))
        {
            Check.NotNull(expression, nameof(expression));

            Expression = expression;
        }

        public virtual Expression Expression { get; }

        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null 
                ? specificVisitor.VisitExistsExpression(this) 
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
