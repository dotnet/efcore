// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DbContextServices : IDisposable
    {
        public enum ServiceProviderSource
        {
            Explicit,
            Implicit
        }

        private IServiceProvider _scopedProvider;
        private DbContextOptions _contextOptions;
        private DbContext _context;
        private LazyRef<IModel> _modelFromSource;
        private LazyRef<DataStoreServices> _dataStoreServices;
        private bool _inOnModelCreating;

        public virtual DbContextServices Initialize(
            [NotNull] IServiceProvider scopedProvider,
            [NotNull] DbContextOptions contextOptions,
            [NotNull] DbContext context,
            ServiceProviderSource serviceProviderSource)
        {
            Check.NotNull(scopedProvider, "scopedProvider");
            Check.NotNull(contextOptions, "contextOptions");
            Check.NotNull(context, "context");
            Check.IsDefined(serviceProviderSource, "serviceProviderSource");

            _scopedProvider = scopedProvider;
            _contextOptions = contextOptions;
            _context = context;

            _dataStoreServices = new LazyRef<DataStoreServices>(() =>
                _scopedProvider.GetRequiredServiceChecked<DataStoreSelector>().SelectDataStore(serviceProviderSource));

            _modelFromSource = new LazyRef<IModel>(CreateModel);

            return this;
        }

        private IModel CreateModel()
        {
            if (_inOnModelCreating)
            {
                throw new InvalidOperationException(Strings.RecursiveOnModelCreating);
            }

            try
            {
                _inOnModelCreating = true;
                return _dataStoreServices.Value.ModelSource
                    .GetModel(_context, _dataStoreServices.Value.ModelBuilderFactory);
            }
            finally
            {
                _inOnModelCreating = false;
            }
        }

        public virtual DbContext Context
        {
            get { return _context; }
        }

        public virtual IModel Model
        {
            get { return _contextOptions.Model ?? _modelFromSource.Value; }
        }

        public virtual IDbContextOptions ContextOptions
        {
            get { return _contextOptions; }
        }

        public virtual DataStoreServices DataStoreServices
        {
            get { return _dataStoreServices.Value; }
        }

        public static Func<IServiceProvider, DbContextService<DbContext>> ContextFactory
        {
            get { return p => new DbContextService<DbContext>(() => p.GetRequiredServiceChecked<DbContextServices>().Context); }
        }

        public static Func<IServiceProvider, DbContextService<IModel>> ModelFactory
        {
            get { return p => new DbContextService<IModel>(() => p.GetRequiredServiceChecked<DbContextServices>().Model); }
        }

        public static Func<IServiceProvider, DbContextService<IDbContextOptions>> ContextOptionsFactory
        {
            get { return p => new DbContextService<IDbContextOptions>(() => p.GetRequiredServiceChecked<DbContextServices>().ContextOptions); }
        }

        public virtual IServiceProvider ScopedServiceProvider
        {
            get { return _scopedProvider; }
        }

        public virtual DataStore DataStore
        {
            get { return _dataStoreServices.Value.Store; }
        }

        public virtual Database Database
        {
            get { return _dataStoreServices.Value.Database; }
        }

        public virtual DataStoreCreator DataStoreCreator
        {
            get { return _dataStoreServices.Value.Creator; }
        }

        public virtual ValueGeneratorCache ValueGeneratorCache
        {
            get { return _dataStoreServices.Value.ValueGeneratorCache; }
        }

        public virtual DataStoreConnection Connection
        {
            get { return _dataStoreServices.Value.Connection; }
        }

        public virtual void Dispose()
        {
            var disposableServiceProvider = _scopedProvider as IDisposable;

            if (disposableServiceProvider != null)
            {
                disposableServiceProvider.Dispose();
            }
        }
    }
}
