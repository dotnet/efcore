// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public abstract class TableExpressionBase : Expression
    {
        private string _alias;
        private IQuerySource _querySource;

        protected TableExpressionBase(
            [CanBeNull] IQuerySource querySource, [CanBeNull] string alias)
        {
            _querySource = querySource;
            _alias = alias;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(object);

        public virtual IQuerySource QuerySource
        {
            get { return _querySource; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _querySource = value;
            }
        }

        public virtual string Alias
        {
            get { return _alias; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _alias = value;
            }
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
