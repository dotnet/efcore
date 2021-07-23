// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DbContextPool<TContext> : IDbContextPool<TContext>, IDisposable, IAsyncDisposable
        where TContext : DbContext
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public const int DefaultPoolSize = 1024;

        private readonly ConcurrentQueue<IDbContextPoolable> _pool = new();

        private readonly Func<DbContext> _activator;

        private int _maxSize;
        private int _count;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DbContextPool(DbContextOptions<TContext> options)
        {
            _maxSize = options.FindExtension<CoreOptionsExtension>()?.MaxPoolSize ?? DefaultPoolSize;

            if (_maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(CoreOptionsExtension.MaxPoolSize), CoreStrings.InvalidPoolSize);
            }

            options.Freeze();

            _activator = CreateActivator(options);
        }

        private static Func<DbContext> CreateActivator(DbContextOptions<TContext> options)
        {
            var constructors = typeof(TContext).GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && c.IsPublic).ToArray();

            if (constructors.Length == 1)
            {
                var parameters = constructors[0].GetParameters();
                if (parameters.Length == 1
                    && (parameters[0].ParameterType == typeof(DbContextOptions)
                        || parameters[0].ParameterType == typeof(DbContextOptions<TContext>)))
                {
                    return Expression.Lambda<Func<TContext>>(Expression.New(constructors[0], Expression.Constant(options))).Compile();
                }
            }

            throw new InvalidOperationException(CoreStrings.PoolingContextCtorError(typeof(TContext).ShortDisplayName()));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IDbContextPoolable Rent()
        {
            if (_pool.TryDequeue(out var context))
            {
                Interlocked.Decrement(ref _count);

                Check.DebugAssert(_count >= 0, $"_count is {_count}");

                return context;
            }

            context = _activator();

            context.SnapshotConfiguration();

            return context;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Return(IDbContextPoolable context)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                context.ResetState();

                _pool.Enqueue(context);
            }
            else
            {
                PooledReturn(context);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async ValueTask ReturnAsync(IDbContextPoolable context, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                await context.ResetStateAsync(cancellationToken).ConfigureAwait(false);

                _pool.Enqueue(context);
            }
            else
            {
                PooledReturn(context);
            }
        }

        private void PooledReturn(IDbContextPoolable context)
        {
            Interlocked.Decrement(ref _count);

            Check.DebugAssert(_maxSize == 0 || _pool.Count <= _maxSize, $"_maxSize is {_maxSize}");

            context.ClearLease();
            context.Dispose();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Dispose()
        {
            _maxSize = 0;

            while (_pool.TryDequeue(out var context))
            {
                context.ClearLease();
                context.Dispose();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async ValueTask DisposeAsync()
        {
            _maxSize = 0;

            while (_pool.TryDequeue(out var context))
            {
                context.ClearLease();
                await context.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
