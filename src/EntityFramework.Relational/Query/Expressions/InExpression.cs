// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class InExpression : InExpressionBase
    {
        public InExpression(
            [NotNull] ColumnExpression column,
            [NotNull] IReadOnlyList<Expression> values)
            : this(
                Check.NotNull(column, nameof(column)),
                Check.NotNull(values, nameof(values)),
                null)
        {
        }

        public InExpression(
            [NotNull] ColumnExpression column,
            [NotNull] ParameterExpression parameter)
            : this(
                  Check.NotNull(column, nameof(column)),
                  null,
                  Check.NotNull(parameter, nameof(parameter)))
        {
        }

        internal InExpression(
            ColumnExpression column, 
            IReadOnlyList<Expression> values, 
            ParameterExpression parameter)
            : base(column, values, parameter)
        {
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitInExpression(this)
                : base.Accept(visitor);
        }

        public override string ToString()
        {
            return Column + " IN (" + string.Join(", ", Values) + ")";
        }
    }
}
