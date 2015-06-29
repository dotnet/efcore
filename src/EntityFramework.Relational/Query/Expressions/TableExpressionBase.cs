// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public abstract class TableExpressionBase : Expression
    {
        protected TableExpressionBase(
            [CanBeNull] IQuerySource querySource,
            [CanBeNull] string alias)
        {
            QuerySource = querySource;
            Alias = alias;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(object);

        public virtual IQuerySource QuerySource { get; [param: NotNull] set; }

        public virtual string Alias { get; [param: NotNull] set; }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
