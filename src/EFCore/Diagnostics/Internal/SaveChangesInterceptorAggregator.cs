// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SaveChangesInterceptorAggregator : InterceptorAggregator<ISaveChangesInterceptor>
    {
        /// <summary>
        ///     Must be implemented by the inheriting type to create a single interceptor from the given list.
        /// </summary>
        /// <param name="interceptors"> The interceptors to combine. </param>
        /// <returns> The combined interceptor. </returns>
        protected override ISaveChangesInterceptor CreateChain(IEnumerable<ISaveChangesInterceptor> interceptors)
            => new CompositeSaveChangesInterceptor(interceptors);

        private sealed class CompositeSaveChangesInterceptor : ISaveChangesInterceptor
        {
            private readonly ISaveChangesInterceptor[] _interceptors;

            public CompositeSaveChangesInterceptor([NotNull] IEnumerable<ISaveChangesInterceptor> interceptors)
            {
                _interceptors = interceptors.ToArray();
            }

            public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = _interceptors[i].SavingChanges(eventData, result);
                }

                return result;
            }

            public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = _interceptors[i].SavedChanges(eventData, result);
                }

                return result;
            }

            public void SaveChangesFailed(DbContextErrorEventData eventData)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    _interceptors[i].SaveChangesFailed(eventData);
                }
            }

            public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
                DbContextEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
                }

                return result;
            }

            public async ValueTask<int> SavedChangesAsync(
                SaveChangesCompletedEventData eventData,
                int result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
                }

                return result;
            }

            public async Task SaveChangesFailedAsync(
                DbContextErrorEventData eventData,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    await _interceptors[i].SaveChangesFailedAsync(eventData, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
