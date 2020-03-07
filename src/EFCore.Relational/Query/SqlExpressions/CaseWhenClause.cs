// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class CaseWhenClause
    {
        public CaseWhenClause(SqlExpression test, SqlExpression result)
        {
            Test = test;
            Result = result;
        }

        public virtual SqlExpression Test { get; }
        public virtual SqlExpression Result { get; }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is CaseWhenClause caseWhenClause
                    && Equals(caseWhenClause));

        private bool Equals(CaseWhenClause caseWhenClause)
            => Test.Equals(caseWhenClause.Test)
                && Result.Equals(caseWhenClause.Result);

        public override int GetHashCode() => HashCode.Combine(Test, Result);
    }
}
