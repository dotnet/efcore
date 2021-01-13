// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
    ///     <para>
    ///         Instances should be registered on the internal service provider as multiple <see cref="IInterceptorAggregator" />
    ///         interfaces.
    ///     </para>
    /// </summary>
    public interface IInterceptorAggregator
    {
        /// <summary>
        ///     The interceptor type.
        /// </summary>
        Type InterceptorType { get; }

        /// <summary>
        ///     <para>
        ///         Resolves a single <see cref="IInterceptor" /> /> from all those registered on
        ///         the <see cref="DbContext" /> or in the internal service provider.
        ///     </para>
        /// </summary>
        /// <param name="interceptors"> The interceptors to combine. </param>
        /// <returns> The combined interceptor. </returns>
        IInterceptor AggregateInterceptors([NotNull] IReadOnlyList<IInterceptor> interceptors);
    }
}
