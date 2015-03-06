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
            [CanBeNull] IReadOnlyList<Expression> values,
            [CanBeNull] ParameterExpression parameter)
            : base(typeof(bool))
        {
            Check.NotNull(column, nameof(column));

            Column = column;
            Values = values;
            ParameterArgument = parameter;
        }

        public virtual ColumnExpression Column { get; }
        public virtual IReadOnlyList<Expression> Values { get; }
        public virtual ParameterExpression ParameterArgument { get; }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
