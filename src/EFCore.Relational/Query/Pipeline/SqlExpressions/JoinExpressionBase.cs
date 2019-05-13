// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public abstract class JoinExpressionBase : TableExpressionBase
    {
        #region Fields & Constructors
        protected JoinExpressionBase(TableExpressionBase table)
            : base("")
        {
            Table = table;
        }
        #endregion

        #region Public Properties
        public TableExpressionBase Table { get; }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is JoinExpressionBase joinExpressionBase
                    && Equals(joinExpressionBase));

        private bool Equals(JoinExpressionBase joinExpressionBase)
            => base.Equals(joinExpressionBase)
            && Table.Equals(joinExpressionBase.Table);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Table.GetHashCode();

                return hashCode;
            }
        }
        #endregion
    }
}
