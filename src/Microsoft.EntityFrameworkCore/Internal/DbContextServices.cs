// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbContextServices : IDbContextServices
    {
        private IServiceProvider _scopedProvider;
        private IDbContextOptions _contextOptions;
        private ICurrentDbContext _currentContext;
        private LazyRef<IModel> _modelFromSource;
        private LazyRef<IDatabaseProviderServices> _providerServices;
        private bool _inOnModelCreating;
        private ILoggerFactory _loggerFactory;
        private IMemoryCache _memoryCache;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDbContextServices Initialize(
            IServiceProvider scopedProvider,
            IDbContextOptions contextOptions,
            DbContext context)
        {
            _scopedProvider = scopedProvider;
            _contextOptions = contextOptions;
            _currentContext = new CurrentDbContext(context);

            _providerServices = new LazyRef<IDatabaseProviderServices>(() =>
                _scopedProvider.GetRequiredService<IDatabaseProviderSelector>().SelectServices());

            _modelFromSource = new LazyRef<IModel>(CreateModel);

            return this;
        }

        private IModel CreateModel()
        {
            if (_inOnModelCreating)
            {
                throw new InvalidOperationException(CoreStrings.RecursiveOnModelCreating);
            }

            try
            {
                _inOnModelCreating = true;

                return _providerServices.Value.ModelSource.GetModel(
                    _currentContext.Context,
                    _providerServices.Value.ConventionSetBuilder,
                    _providerServices.Value.ModelValidator);
            }
            finally
            {
                _inOnModelCreating = false;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ICurrentDbContext CurrentContext => _currentContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IModel Model => CoreOptions?.Model ?? _modelFromSource.Value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ILoggerFactory LoggerFactory
            => _loggerFactory ?? (_loggerFactory = CoreOptions?.LoggerFactory ?? _scopedProvider?.GetRequiredService<ILoggerFactory>());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IMemoryCache MemoryCache
            => _memoryCache ?? (_memoryCache = CoreOptions?.MemoryCache ?? _scopedProvider?.GetRequiredService<IMemoryCache>());

        private CoreOptionsExtension CoreOptions
            => _contextOptions?.FindExtension<CoreOptionsExtension>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDbContextOptions ContextOptions => _contextOptions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDatabaseProviderServices DatabaseProviderServices
        {
            get
            {
                Debug.Assert(
                    _providerServices != null,
                    "DbContextServices not initialized. This may mean a service is registered as Singleton when it needs to be Scoped because it depends on other Scoped services.");

                return _providerServices.Value;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceProvider InternalServiceProvider => _scopedProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Reset()
        {
            if (_providerServices.HasValue)
            {
                _providerServices.Value.Reset();
            }
        }
    }
}
