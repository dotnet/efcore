// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

        private readonly Func<TContext> _activator;

        private int _maxSize;
        private int _count;

        private DbContextPoolConfigurationSnapshot _configurationSnapshot;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public sealed class Lease : IDisposable
        {
            private DbContextPool<TContext> _contextPool;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public Lease([NotNull] DbContextPool<TContext> contextPool)
            {
                _contextPool = contextPool;

                Context = _contextPool.Rent();
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
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
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
            var ctors
                = typeof(TContext).GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();

            if (ctors.Length == 1)
            {
                var parameters = ctors[0].GetParameters();

                if (parameters.Length == 1
                    && (parameters[0].ParameterType == typeof(DbContextOptions)
                        || parameters[0].ParameterType == typeof(DbContextOptions<TContext>)))
                {
                    return
                        Expression.Lambda<Func<TContext>>(
                                Expression.New(ctors[0], Expression.Constant(options)))
                            .Compile();
                }
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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

        DbContext IDbContextPool.Rent() => Rent();

        bool IDbContextPool.Return(DbContext context) => Return((TContext)context);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
    }
}
