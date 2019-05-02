// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public abstract class TableExpressionBase : Expression
    {
        #region Fields & Constructors
        protected TableExpressionBase(string alias)
        {
            Alias = alias;
        }
        #endregion

        #region Public Properties
        public string Alias { get; internal set; }
        #endregion

        #region Expression-based methods/properties
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public override Type Type => typeof(object);
        public override ExpressionType NodeType => ExpressionType.Extension;
        #endregion

        #region Equality & HashCode
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
        #endregion
    }
}
