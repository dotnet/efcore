// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class ConcurrencyDetector : IConcurrencyDetector, IDisposable
    {
        private readonly IDisposable _disposer;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private int _inCriticalSection;

        public ConcurrencyDetector()
        {
            _disposer = new Disposer(this);
        }

        public virtual IDisposable EnterCriticalSection()
        {
            if (Interlocked.CompareExchange(ref _inCriticalSection, 1, 0) == 1)
            {
                throw new InvalidOperationException(CoreStrings.ConcurrentMethodInvocation);
            }

            return _disposer;
        }

        private void ExitCriticalSection()
        {
            Debug.Assert(_inCriticalSection == 1, "Expected to be in a critical section");

            _inCriticalSection = 0;
        }

        public virtual async Task<IDisposable> EnterCriticalSectionAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            return new AsyncDisposer(EnterCriticalSection(), _semaphore);
        }

        private struct AsyncDisposer : IDisposable
        {
            private readonly IDisposable _disposable;
            private readonly SemaphoreSlim _semaphore;

            public AsyncDisposer(IDisposable disposable, SemaphoreSlim semaphore)
            {
                _disposable = disposable;
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                _disposable.Dispose();
                _semaphore.Release();
            }
        }

        private struct Disposer : IDisposable
        {
            private readonly ConcurrencyDetector _concurrencyDetector;

            public Disposer(ConcurrencyDetector concurrencyDetector)
            {
                _concurrencyDetector = concurrencyDetector;
            }

            public void Dispose() => _concurrencyDetector.ExitCriticalSection();
        }

        public virtual void Dispose() => _semaphore.Dispose();
    }
}
