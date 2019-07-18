// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Represents a result from an <see cref="IInterceptor"/> such as an 'IDbConnectionInterceptor' to allow
    ///         suppression of the normal operation being intercepted.
    ///     </para>
    ///     <para>
    ///         A value of this type is passed to all interceptor methods that are called before the operation
    ///         being intercepted is executed.
    ///         Typically the interceptor should return the value passed in.
    ///         However, creating a result with <see cref="Suppress"/> causes the operation being
    ///         intercepted to be suppressed; that is, the operation is not executed.
    ///     </para>
    /// </summary>
    public readonly struct InterceptionResult
    {
        /// <summary>
        ///     Creates a new <see cref="InterceptionResult" /> instance indicating that
        ///     execution should be suppressed.
        /// </summary>
        public static InterceptionResult Suppress() => new InterceptionResult(true);

        private InterceptionResult(bool suppress) => IsSuppressed = suppress;

        /// <summary>
        ///     If true, then interception is suppressed.
        /// </summary>
        public bool IsSuppressed { get; }
    }
}
