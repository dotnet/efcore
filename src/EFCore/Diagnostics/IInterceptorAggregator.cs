// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         A service to resolve a single <see cref="IInterceptor" /> /> from all those registered on
    ///         the <see cref="DbContext" /> or in the internal service provider.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Instances should be registered on the internal service provider as multiple <see cref="IInterceptorAggregator" />
    ///         interfaces.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information.
    ///     </para>
    /// </remarks>
    public interface IInterceptorAggregator
    {
        /// <summary>
        ///     The interceptor type.
        /// </summary>
        Type InterceptorType { get; }

        /// <summary>
        ///     Resolves a single <see cref="IInterceptor" /> /> from all those registered on
        ///     the <see cref="DbContext" /> or in the internal service provider.
        /// </summary>
        /// <param name="interceptors">The interceptors to combine.</param>
        /// <returns>The combined interceptor.</returns>
        IInterceptor? AggregateInterceptors(IReadOnlyList<IInterceptor> interceptors);
    }
}
