// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> _releaserTask;
        private readonly IDisposable _releaser;

        public AsyncLock()
        {
            _releaser = new Releaser(this);
            _releaserTask = Task.FromResult(_releaser);
        }

        public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var waitTask = _semaphore.WaitAsync(cancellationToken);

            return waitTask.IsCompleted
                ? _releaserTask
                : waitTask.ContinueWith(
                    (_, state) => (IDisposable)state,
                    _releaserTask.Result,
                    cancellationToken,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
        }

        public IDisposable Lock()
        {
            _semaphore.Wait();

            return _releaser;
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _asyncLock;

            public Releaser(AsyncLock asyncLock)
            {
                _asyncLock = asyncLock;
            }

            public void Dispose() => _asyncLock._semaphore.Release();
        }
    }
}
