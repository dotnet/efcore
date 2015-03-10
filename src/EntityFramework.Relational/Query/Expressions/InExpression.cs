// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class InExpression : ExtensionExpression
    {
        public InExpression(
            [NotNull] ColumnExpression column,
            [NotNull] IReadOnlyList<Expression> values)
            : base(typeof(bool))
        {
            Check.NotNull(column, nameof(column));
            Check.NotNull(values, nameof(values));

            Column = column;
            Values = values;
        }

        public virtual ColumnExpression Column { get; }
        public virtual IReadOnlyList<Expression> Values { get; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitInExpression(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }

        public override string ToString()
        {
            return Column + " IN (" + string.Join(", ", Values) + ")";
        }
    }
}