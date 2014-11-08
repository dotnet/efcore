// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DbContextConfiguration
    {
        public enum ServiceProviderSource
        {
            Explicit,
            Implicit,
        }

        // TODO: Remove this (Issue #641)
        private ContextServices _services;

        private IServiceProvider _scopedProvider;
        private DbContextOptions _contextOptions;
        private DbContext _context;
        private LazyRef<IModel> _modelFromSource;
        private LazyRef<DataStoreServices> _dataStoreServices;
        private ServiceProviderSource _serviceProviderSource;
        private bool _inOnModelCreating;

        public virtual DbContextConfiguration Initialize(
            [NotNull] IServiceProvider externalProvider,
            [NotNull] IServiceProvider scopedProvider,
            [NotNull] DbContextOptions contextOptions,
            [NotNull] DbContext context,
            ServiceProviderSource serviceProviderSource)
        {
            Check.NotNull(externalProvider, "externalProvider");
            Check.NotNull(scopedProvider, "scopedProvider");
            Check.NotNull(contextOptions, "contextOptions");
            Check.NotNull(context, "context");
            Check.IsDefined(serviceProviderSource, "serviceProviderSource");

            _services = new ContextServices(scopedProvider);

            _scopedProvider = scopedProvider;
            _serviceProviderSource = serviceProviderSource;
            _contextOptions = contextOptions;
            _context = context;

            _dataStoreServices = new LazyRef<DataStoreServices>(() =>
                _scopedProvider.GetRequiredServiceChecked<DataStoreSelector>().SelectDataStore(this));

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
                return _scopedProvider
                    .GetRequiredServiceChecked<IModelSource>()
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

        public static Func<IServiceProvider, LazyRef<DbContext>> ContextFactory
        {
            get { return p => new LazyRef<DbContext>(() => p.GetRequiredServiceChecked<DbContextConfiguration>().Context); }
        }

        public static Func<IServiceProvider, LazyRef<IModel>> ModelFactory
        {
            get { return p => new LazyRef<IModel>(() => p.GetRequiredServiceChecked<DbContextConfiguration>().Model); }
        }

        public static Func<IServiceProvider, LazyRef<IDbContextOptions>> ContextOptionsFactory
        {
            get { return p => new LazyRef<IDbContextOptions>(() => p.GetRequiredServiceChecked<DbContextConfiguration>().ContextOptions); }
        }

        public virtual ServiceProviderSource ProviderSource
        {
            get { return _serviceProviderSource; }
        }

        public virtual IServiceProvider ScopedServiceProvider
        {
            get { return _scopedProvider; }
        }

        // TODO: Remove this (Issue #641)
        public virtual DataStore DataStore
        {
            get { return _dataStoreServices.Value.Store; }
        }

        // TODO: Remove this (Issue #641)
        public virtual Database Database
        {
            get { return _dataStoreServices.Value.Database; }
        }
        
        // TODO: Remove this (Issue #641)
        public virtual DataStoreCreator DataStoreCreator
        {
            get { return _dataStoreServices.Value.Creator; }
        }

        // TODO: Remove this (Issue #641)
        public virtual ValueGeneratorCache ValueGeneratorCache
        {
            get { return _dataStoreServices.Value.ValueGeneratorCache; }
        }

        // TODO: Remove this (Issue #641)
        public virtual DataStoreConnection Connection
        {
            get { return _dataStoreServices.Value.Connection; }
        }

        // TODO: Remove this (Issue #641)
        public virtual StateManager StateManager
        {
            get { return Services.StateManager; }
        }

        public virtual ContextServices Services
        {
            get { return _services; }
        }
    }
}
