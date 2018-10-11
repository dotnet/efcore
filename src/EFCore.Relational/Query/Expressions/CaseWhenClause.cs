// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents one when clause of a <see cref="CaseExpression"/>.
    /// </summary>
    public class CaseWhenClause
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CaseWhenClause"/> class.
        /// </summary>
        /// <param name="test"> The when operand expression. </param>
        /// <param name="result"> The result expression. </param>
        public CaseWhenClause([NotNull] Expression test, [NotNull] Expression result)
        {
            Check.NotNull(test, nameof(test));
            Check.NotNull(result, nameof(result));

            Test = test;
            Result = result;
        }

        /// <summary>
        ///     Gets the when operand expression.
        /// </summary>
        public virtual Expression Test { get; }

        /// <summary>
        ///     Gets the result expression.
        /// </summary>
        public virtual Expression Result { get; }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                ? true
                : obj is CaseWhenClause whenThen && Equals(whenThen);
        }

        private bool Equals(CaseWhenClause other)
            => (Test == null
                    ? other.Test == null
                    : Test.Equals(other.Test))
                && (Result == null
                    ? other.Result == null
                    : Result.Equals(other.Result));

        /// <summary>
        ///     Gets a hash code for the current object.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode()
            => (Test.GetHashCode() * 397) ^ Result.GetHashCode();

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> The string. </returns>
        public override string ToString()
            => "WHEN " + Test + " THEN " + Result;
    }
}
