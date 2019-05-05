// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class CaseWhenClause
    {
        #region Fields & Constructors

        public CaseWhenClause(SqlExpression test, SqlExpression result)
        {
            Test = test;
            Result = result;
        }

        #endregion

        #region Public Properties

        public SqlExpression Test { get; }
        public SqlExpression Result { get; }

        #endregion

        #region Equality & HashCode

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is CaseWhenClause caseWhenClause
                    && Equals(caseWhenClause));

        private bool Equals(CaseWhenClause caseWhenClause)
            => Test.Equals(caseWhenClause.Test)
            && Result.Equals(caseWhenClause.Result);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Test.GetHashCode();
                hashCode = (hashCode * 397) ^ Result.GetHashCode();

                return hashCode;
            }
        }

        #endregion
    }
}
