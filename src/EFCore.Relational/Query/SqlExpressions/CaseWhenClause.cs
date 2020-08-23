// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a WHEN...THEN... construct in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class CaseWhenClause
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="CaseWhenClause" /> class.
        /// </summary>
        /// <param name="test"> A value to compare with <see cref="CaseExpression.Operand" /> or condition to evaluate. </param>
        /// <param name="result"> A value to return if test succeeds. </param>
        public CaseWhenClause([NotNull] SqlExpression test, [NotNull] SqlExpression result)
        {
            Check.NotNull(test, nameof(test));
            Check.NotNull(result, nameof(result));

            Test = test;
            Result = result;
        }

        /// <summary>
        ///     The value to compare with <see cref="CaseExpression.Operand" /> or the condition to evaluate.
        /// </summary>
        public virtual SqlExpression Test { get; }

        /// <summary>
        ///     The value to return if <see cref="Test" /> succeeds.
        /// </summary>
        public virtual SqlExpression Result { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is CaseWhenClause caseWhenClause
                    && Equals(caseWhenClause));

        private bool Equals(CaseWhenClause caseWhenClause)
            => Test.Equals(caseWhenClause.Test)
                && Result.Equals(caseWhenClause.Result);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(Test, Result);
    }
}
