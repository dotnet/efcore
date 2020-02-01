// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Abstract base class for implementations of the <see cref="IInterceptorAggregator" /> service.
    /// </summary>
    /// <typeparam name="TInterceptor"> The interceptor type. </typeparam>
    public abstract class InterceptorAggregator<TInterceptor> : IInterceptorAggregator
        where TInterceptor : class, IInterceptor
    {
        private TInterceptor _interceptor;
        private bool _resolved;

        /// <summary>
        ///     The interceptor type.
        /// </summary>
        public virtual Type InterceptorType => typeof(TInterceptor);

        /// <summary>
        ///     <para>
        ///         Resolves a single <see cref="IInterceptor" /> /> from all those registered on
        ///         the <see cref="DbContext" /> or in the internal service provider.
        ///     </para>
        /// </summary>
        /// <param name="interceptors"> The interceptors to combine. </param>
        /// <returns> The combined interceptor. </returns>
        public virtual IInterceptor AggregateInterceptors(IReadOnlyList<IInterceptor> interceptors)
        {
            Check.NotNull(interceptors, nameof(interceptors));

            if (!_resolved)
            {
                if (interceptors.Count == 1)
                {
                    _interceptor = interceptors[0] as TInterceptor;
                }
                else if (interceptors.Count > 1)
                {
                    var filtered = interceptors.OfType<TInterceptor>().ToList();

                    if (filtered.Count == 1)
                    {
                        _interceptor = filtered[0];
                    }
                    else if (filtered.Count > 1)
                    {
                        _interceptor = CreateChain(filtered);
                    }
                }

                _resolved = true;
            }

            return _interceptor;
        }

        /// <summary>
        ///     Must be implemented by the inheriting type to create a single interceptor from the given list.
        /// </summary>
        /// <param name="interceptors"> The interceptors to combine. </param>
        /// <returns> The combined interceptor. </returns>
        protected abstract TInterceptor CreateChain([NotNull] IEnumerable<TInterceptor> interceptors);
    }
}
