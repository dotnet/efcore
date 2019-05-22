// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly Releaser _releaser;
        private readonly Task<Releaser> _releaserTask;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public AsyncLock()
        {
            _releaser = new Releaser(this);
            _releaserTask = Task.FromResult(_releaser);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Task<Releaser> LockAsync(CancellationToken cancellationToken = default)
        {
            var wait = _semaphore.WaitAsync(cancellationToken);

            return wait.IsCompleted
                ? _releaserTask
                : wait.ContinueWith(
                    (_, state) => ((AsyncLock)state)._releaser,
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Releaser Lock()
        {
            _semaphore.Wait();

            return _releaser;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public readonly struct Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;

            internal Releaser([NotNull] AsyncLock toRelease)
            {
                _toRelease = toRelease;
            }

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public void Dispose()
                => _toRelease._semaphore.Release();
        }
    }
}
