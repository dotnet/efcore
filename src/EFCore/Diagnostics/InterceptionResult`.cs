// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Represents a result from an <see cref="IInterceptor" /> such as an 'IDbCommandInterceptor' to allow
    ///         suppression of the normal operation being intercepted.
    ///     </para>
    ///     <para>
    ///         A value of this type is passed to all interceptor methods that are called before the operation
    ///         being intercepted is executed.
    ///         Typically the interceptor should return the value passed in.
    ///         However, creating a result with <see cref="SuppressWithResult"/> causes the operation being
    ///         intercepted to be suppressed; that is, the operation is not executed.
    ///         The value in the result is then used as a substitute return value for the operation that was suppressed.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResult"> The new result to use. </typeparam>
    public readonly struct InterceptionResult<TResult>
    {
        private readonly TResult _result;

        /// <summary>
        ///     Creates a new <see cref="InterceptionResult{TResult}" /> instance indicating that
        ///     execution should be suppressed and the given result should be used instead.
        /// </summary>
        /// <param name="result"> The result to use. </param>
        public static InterceptionResult<TResult> SuppressWithResult([CanBeNull] TResult result)
            => new InterceptionResult<TResult>(result);

        private InterceptionResult(TResult result)
        {
            _result = result;
            HasResult = true;
        }

        /// <summary>
        ///     <para>
        ///         The result to use.
        ///     </para>
        ///     <para>
        ///         The property can only be accessed if <see cref="HasResult" /> is true. The concept here
        ///         is the same as <see cref="Nullable{T}.Value"/> and <see cref="Nullable{T}.HasValue" />
        ///     </para>
        ///     <exception cref="InvalidOperationException"> when <see cref="Result" /> is false. </exception>
        /// </summary>
        public TResult Result
        {
            get
            {
                if (!HasResult)
                {
                    throw new InvalidOperationException(CoreStrings.NoInterceptionResult);
                }

                return _result;
            }
        }

        /// <summary>
        ///     If true, then interception is suppressed, and <see cref="Result"/> contains the result to use.
        /// </summary>
        public bool HasResult { get; }
    }
}
