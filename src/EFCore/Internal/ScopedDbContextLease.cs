// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <inheritdoc cref="IScopedDbContextLease{TContext}" />
    public sealed class ScopedDbContextLease<TContext> : IScopedDbContextLease<TContext>, IDisposable, IAsyncDisposable
        where TContext : DbContext
    {
        private DbContextLease _lease;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ScopedDbContextLease([NotNull] IDbContextPool<TContext> contextPool)
            => _lease = new DbContextLease(contextPool, standalone: false);

        /// <inheritdoc />
        public TContext Context
            => (TContext)_lease.Context;

        /// <inheritdoc />
        void IDisposable.Dispose()
            => _lease.Release();

        /// <inheritdoc />
        ValueTask IAsyncDisposable.DisposeAsync()
            => _lease.ReleaseAsync();
    }
}
