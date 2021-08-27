// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Represents a result from an <see cref="IInterceptor" /> such as an <see cref="ISaveChangesInterceptor" /> to allow
    ///         suppression of the normal operation being intercepted.
    ///     </para>
    ///     <para>
    ///         A value of this type is passed to all interceptor methods that are called before the operation
    ///         being intercepted is executed.
    ///         Typically the interceptor should return the value passed in.
    ///         However, creating a result with <see cref="Suppress" /> causes the operation being
    ///         intercepted to be suppressed; that is, the operation is not executed.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information.
    /// </remarks>
    public readonly struct InterceptionResult
    {
        /// <summary>
        ///     Creates a new <see cref="InterceptionResult" /> instance indicating that
        ///     execution should be suppressed.
        /// </summary>
        public static InterceptionResult Suppress()
            => new(true);

        private InterceptionResult(bool suppress)
            => IsSuppressed = suppress;

        /// <summary>
        ///     If true, then interception is suppressed.
        /// </summary>
        public bool IsSuppressed { get; }
    }
}
