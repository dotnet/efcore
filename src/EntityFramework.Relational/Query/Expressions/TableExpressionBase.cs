// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public abstract class TableExpressionBase : ExtensionExpression
    {
        protected TableExpressionBase(
            [CanBeNull] IQuerySource querySource,
            [CanBeNull] string alias)
            : base(typeof(object))
        {
            QuerySource = querySource;
            Alias = alias;
        }

        public virtual IQuerySource QuerySource { get; }

        public virtual string Alias { get; [param: NotNull] set; }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
