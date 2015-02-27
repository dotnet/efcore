// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public abstract class InExpressionBase : ExtensionExpression
    {
        protected InExpressionBase(
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

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
