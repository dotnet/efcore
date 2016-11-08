// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbContextPool<TContext> : IDbContextPool, IDisposable
        where TContext : DbContext
    {
        private const int DefaultPoolSize = 32;

        private readonly ConcurrentQueue<TContext> _pool = new ConcurrentQueue<TContext>();

        private int _maxSize;
        private int _count;

        private DbContextPoolConfigurationSnapshot _configurationSnapshot;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // ReSharper disable once SuggestBaseTypeForParameter
        public DbContextPool([NotNull] DbContextOptions<TContext> options)
        {
            _maxSize = options.FindExtension<CoreOptionsExtension>()?.MaxPoolSize ?? DefaultPoolSize;

            options.Freeze();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TContext Rent([NotNull] IServiceProvider serviceProvider)
        {
            TContext context;

            if (_pool.TryDequeue(out context))
            {
                Interlocked.Decrement(ref _count);

                Debug.Assert(_count >= 0);

                ((IDbContextPoolable)context).Resurrect(_configurationSnapshot);

                return context;
            }

            context = ActivatorUtilities.CreateInstance<TContext>(serviceProvider);

            NonCapturingLazyInitializer
                .EnsureInitialized(
                    ref _configurationSnapshot,
                    (IDbContextPoolable)context,
                    c => c.SnapshotConfiguration());

            ((IDbContextPoolable)context).SetPool(this);

            return context;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Return([NotNull] TContext context)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                _pool.Enqueue(context);

                return true;
            }

            Interlocked.Decrement(ref _count);

            Debug.Assert(_maxSize > 0 && _pool.Count <= _maxSize);

            return false;
        }

        DbContext IDbContextPool.Rent(IServiceProvider serviceProvider) => Rent(serviceProvider);

        bool IDbContextPool.Return(DbContext context) => Return((TContext)context);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Dispose()
        {
            _maxSize = 0;

            TContext context;
            while (_pool.TryDequeue(out context))
            {
                context.Dispose();
            }
        }
    }
}
