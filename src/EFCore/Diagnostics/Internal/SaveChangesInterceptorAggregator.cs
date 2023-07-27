// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

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
    /// <param name="interceptors">The interceptors to combine.</param>
    /// <returns>The combined interceptor.</returns>
    protected override ISaveChangesInterceptor CreateChain(IEnumerable<ISaveChangesInterceptor> interceptors)
        => new CompositeSaveChangesInterceptor(interceptors);

    private sealed class CompositeSaveChangesInterceptor : ISaveChangesInterceptor
    {
        private readonly ISaveChangesInterceptor[] _interceptors;

        public CompositeSaveChangesInterceptor(IEnumerable<ISaveChangesInterceptor> interceptors)
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

        public void SaveChangesCanceled(DbContextEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].SaveChangesCanceled(eventData);
            }
        }

        public InterceptionResult ThrowingConcurrencyException(
            ConcurrencyExceptionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ThrowingConcurrencyException(eventData, result);
            }

            return result;
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

        public async Task SaveChangesCanceledAsync(
            DbContextEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].SaveChangesCanceledAsync(eventData, cancellationToken).ConfigureAwait(false);
            }
        }

        public async ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
            ConcurrencyExceptionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i]
                    .ThrowingConcurrencyExceptionAsync(eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }
    }
}
