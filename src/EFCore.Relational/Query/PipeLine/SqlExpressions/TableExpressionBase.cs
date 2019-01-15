// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public abstract class TableExpressionBase : Expression
    {
        protected TableExpressionBase(string alias)
        {
            Alias = alias;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public string Alias { get; }

        public override Type Type => typeof(object);
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is TableExpressionBase tableExpressionBase
                    && Equals(tableExpressionBase));

        private bool Equals(TableExpressionBase tableExpressionBase)
            => string.Equals(Alias, tableExpressionBase.Alias);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Alias.GetHashCode();

                return hashCode;
            }
        }
    }
}
