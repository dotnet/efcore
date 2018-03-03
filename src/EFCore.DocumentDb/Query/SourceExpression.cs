// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class SourceExpression : QueryExpressionBase
    {
        public SourceExpression(IQuerySource querySource, string alias)
        {
            QuerySource = querySource;
            Alias = alias;
        }

        public IQuerySource QuerySource { get; }

        public string Alias { get; }

        public override Type Type => typeof(object);

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public virtual bool HandlesQuerySource(IQuerySource querySource)
        {
            return QuerySource == querySource;
        }
    }
}
