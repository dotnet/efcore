// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Represents a result from an <see cref="IInterceptor"/> such as an 'IDbCommandInterceptor' to allow
    ///         suppression of the normal operation being intercepted.
    ///     </para>
    ///     <para>
    ///         A value of this type is passed to all interceptor methods that are called before the operation
    ///         being intercepted is executed.
    ///         Typically the interceptor should return the value passed in.
    ///         However, returning some other non-null value will cause the operation being intercepted to
    ///         be suppressed; that is, the operation is not executed.
    ///         The value is then used as a substitute return value for the operation that was suppressed.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResult"> The new result to use. </typeparam>
    public readonly struct InterceptionResult<TResult>
    {
        /// <summary>
        ///     Creates a new <see cref="InterceptionResult{TResult}" /> instance.
        /// </summary>
        /// <param name="result"> The result to use. </param>
        public InterceptionResult([CanBeNull] TResult result)
        {
            Result = result;
        }

        /// <summary>
        ///     The result.
        /// </summary>
        public TResult Result { get; }
    }
}
