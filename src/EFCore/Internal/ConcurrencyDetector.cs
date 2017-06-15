// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ConcurrencyDetector : IConcurrencyDetector, IDisposable
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private int _inCriticalSection;

        private static readonly AsyncLocal<bool?> _suspended = new AsyncLocal<bool?>();

        private static bool Suspended
        {
            get => _suspended.Value ?? false;
            set => _suspended.Value = value;
        }

        private void EnterCriticalSection()
        {
            if (!Suspended)
            {
                if (Interlocked.CompareExchange(ref _inCriticalSection, 1, 0) == 1)
                {
                    throw new InvalidOperationException(CoreStrings.ConcurrentMethodInvocation);
                }
                Suspended = true;
            }
            else
            {
                _inCriticalSection++;
            }
        }

        private void ExitCriticalSection()
        {
            var inCriticalSection = --_inCriticalSection;
            Debug.Assert(inCriticalSection >= 0, "Expected to be in a critical section");
            if (inCriticalSection <= 0)
            {
                Debug.Assert(Suspended, "Expected to be suspended");
                Suspended = false;
                _inCriticalSection = 0;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TResult ExecuteInCriticalSection<TState, TResult>(TState state, Func<TState, TResult> operation)
        {
            EnterCriticalSection();
            try
            {
                return operation(state);
            }
            finally
            {
                ExitCriticalSection();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task<TResult> ExecuteInCriticalSectionAsync<TState, TResult>(
            TState state, Func<TState, CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
        {
            if (!Suspended)
            {
                await _semaphore.WaitAsync(cancellationToken);
            }
            EnterCriticalSection();
            try
            {
                return await operation(state, cancellationToken);
            }
            finally
            {
                ExitCriticalSection();

                if (_semaphore == null)
                {
                    throw new ObjectDisposedException(GetType().ShortDisplayName(), CoreStrings.ContextDisposed);
                }

                _semaphore.Release();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Dispose()
        {
            _semaphore?.Dispose();
            _semaphore = null;
        }
    }
}
