// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DbContextPool<TContext> : IDbContextPool, IDisposable, IAsyncDisposable
        where TContext : DbContext
    {
        private const int DefaultPoolSize = 32;

        private readonly ConcurrentQueue<TContext> _pool = new ConcurrentQueue<TContext>();

        private readonly Func<TContext> _activator;

        private int _maxSize;
        private int _count;

        private DbContextPoolConfigurationSnapshot _configurationSnapshot;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public sealed class Lease : IDisposable, IAsyncDisposable
        {
            private DbContextPool<TContext> _contextPool;

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public Lease([NotNull] DbContextPool<TContext> contextPool)
            {
                _contextPool = contextPool;

                Context = _contextPool.Rent();
            }

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public TContext Context { get; private set; }

            void IDisposable.Dispose()
            {
                if (_contextPool != null)
                {
                    if (!_contextPool.Return(Context))
                    {
                        ((IDbContextPoolable)Context).SetPool(null);
                        Context.Dispose();
                    }

                    _contextPool = null;
                    Context = null;
                }
            }

            async ValueTask IAsyncDisposable.DisposeAsync()
            {
                if (_contextPool != null)
                {
                    if (!_contextPool.Return(Context))
                    {
                        ((IDbContextPoolable)Context).SetPool(null);
                        await Context.DisposeAsync();
                    }

                    _contextPool = null;
                    Context = null;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DbContextPool([NotNull] DbContextOptions options)
        {
            _maxSize = options.FindExtension<CoreOptionsExtension>()?.MaxPoolSize ?? DefaultPoolSize;

            options.Freeze();

            _activator = CreateActivator(options);

            if (_activator == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PoolingContextCtorError(typeof(TContext).ShortDisplayName()));
            }
        }

        private static Func<TContext> CreateActivator(DbContextOptions options)
        {
            var constructors
                = typeof(TContext).GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();

            if (constructors.Length == 1)
            {
                var parameters = constructors[0].GetParameters();

                if (parameters.Length == 1
                    && (parameters[0].ParameterType == typeof(DbContextOptions)
                        || parameters[0].ParameterType == typeof(DbContextOptions<TContext>)))
                {
                    return
                        Expression.Lambda<Func<TContext>>(
                                Expression.New(constructors[0], Expression.Constant(options)))
                            .Compile();
                }
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TContext Rent()
        {
            if (_pool.TryDequeue(out var context))
            {
                Interlocked.Decrement(ref _count);

                Debug.Assert(_count >= 0);

                ((IDbContextPoolable)context).Resurrect(_configurationSnapshot);

                return context;
            }

            context = _activator();

            NonCapturingLazyInitializer
                .EnsureInitialized(
                    ref _configurationSnapshot,
                    (IDbContextPoolable)context,
                    c => c.SnapshotConfiguration());

            ((IDbContextPoolable)context).SetPool(this);

            return context;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Return([NotNull] TContext context)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                ((IDbContextPoolable)context).ResetState();

                _pool.Enqueue(context);

                return true;
            }

            Interlocked.Decrement(ref _count);

            Debug.Assert(_maxSize == 0 || _pool.Count <= _maxSize);

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        DbContext IDbContextPool.Rent() => Rent();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IDbContextPool.Return(DbContext context) => Return((TContext)context);

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
                ((IDbContextPoolable)context).SetPool(null);
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
                ((IDbContextPoolable)context).SetPool(null);
                await context.DisposeAsync();
            }
        }
    }
}
