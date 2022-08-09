// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbTransactionInterceptorAggregator : InterceptorAggregator<IDbTransactionInterceptor>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IDbTransactionInterceptor CreateChain(IEnumerable<IDbTransactionInterceptor> interceptors)
        => new CompositeDbTransactionInterceptor(interceptors);

    private sealed class CompositeDbTransactionInterceptor : IDbTransactionInterceptor
    {
        private readonly IDbTransactionInterceptor[] _interceptors;

        public CompositeDbTransactionInterceptor(IEnumerable<IDbTransactionInterceptor> interceptors)
        {
            _interceptors = interceptors.ToArray();
        }

        public InterceptionResult<DbTransaction> TransactionStarting(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].TransactionStarting(connection, eventData, result);
            }

            return result;
        }

        public DbTransaction TransactionStarted(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].TransactionStarted(connection, eventData, result);
            }

            return result;
        }

        public async ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].TransactionStartingAsync(connection, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async ValueTask<DbTransaction> TransactionStartedAsync(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].TransactionStartedAsync(connection, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public DbTransaction TransactionUsed(
            DbConnection connection,
            TransactionEventData eventData,
            DbTransaction result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].TransactionUsed(connection, eventData, result);
            }

            return result;
        }

        public async ValueTask<DbTransaction> TransactionUsedAsync(
            DbConnection connection,
            TransactionEventData eventData,
            DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].TransactionUsedAsync(connection, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public InterceptionResult TransactionCommitting(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].TransactionCommitting(transaction, eventData, result);
            }

            return result;
        }

        public void TransactionCommitted(
            DbTransaction transaction,
            TransactionEndEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].TransactionCommitted(transaction, eventData);
            }
        }

        public async ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].TransactionCommittingAsync(transaction, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async Task TransactionCommittedAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].TransactionCommittedAsync(transaction, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public InterceptionResult TransactionRollingBack(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].TransactionRollingBack(transaction, eventData, result);
            }

            return result;
        }

        public void TransactionRolledBack(
            DbTransaction transaction,
            TransactionEndEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].TransactionRolledBack(transaction, eventData);
            }
        }

        public async ValueTask<InterceptionResult> TransactionRollingBackAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].TransactionRollingBackAsync(transaction, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async Task TransactionRolledBackAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].TransactionRolledBackAsync(transaction, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public InterceptionResult CreatingSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].CreatingSavepoint(transaction, eventData, result);
            }

            return result;
        }

        public void CreatedSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].CreatedSavepoint(transaction, eventData);
            }
        }

        public async ValueTask<InterceptionResult> CreatingSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].CreatingSavepointAsync(transaction, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async Task CreatedSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].CreatedSavepointAsync(transaction, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public InterceptionResult RollingBackToSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].RollingBackToSavepoint(transaction, eventData, result);
            }

            return result;
        }

        public void RolledBackToSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].RolledBackToSavepoint(transaction, eventData);
            }
        }

        public async ValueTask<InterceptionResult> RollingBackToSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].RollingBackToSavepointAsync(transaction, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async Task RolledBackToSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].RolledBackToSavepointAsync(transaction, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public InterceptionResult ReleasingSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ReleasingSavepoint(transaction, eventData, result);
            }

            return result;
        }

        public void ReleasedSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].ReleasedSavepoint(transaction, eventData);
            }
        }

        public async ValueTask<InterceptionResult> ReleasingSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].ReleasingSavepointAsync(transaction, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async Task ReleasedSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].ReleasedSavepointAsync(transaction, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].TransactionFailed(transaction, eventData);
            }
        }

        public async Task TransactionFailedAsync(
            DbTransaction transaction,
            TransactionErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].TransactionFailedAsync(transaction, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
