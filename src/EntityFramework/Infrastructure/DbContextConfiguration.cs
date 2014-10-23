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
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DbContextConfiguration
    {
        public enum ServiceProviderSource
        {
            Explicit,
            Implicit,
        }

        private ContextServices _services;
        private IServiceProvider _externalProvider;
        private DbContextOptions _contextOptions;
        private DbContext _context;
        private LazyRef<IModel> _modelFromSource;
        private LazyRef<DataStoreServices> _dataStoreServices;
        private LazyRef<DataStore> _dataStore;
        private LazyRef<DataStoreConnection> _connection;
        private LazyRef<StateManager> _stateManager;
        private ServiceProviderSource _serviceProviderSource;
        private LazyRef<ILoggerFactory> _loggerFactory;
        private LazyRef<Database> _database;

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

            _externalProvider = externalProvider;
            _services = new ContextServices(scopedProvider);
            _serviceProviderSource = serviceProviderSource;
            _contextOptions = contextOptions;
            _context = context;
            _dataStoreServices = new LazyRef<DataStoreServices>(() => _services.DataStoreSelector.SelectDataStore(this));
            _modelFromSource = new LazyRef<IModel>(() => _services.ModelSource.GetModel(_context, _dataStoreServices.Value.ModelBuilderFactory));
            _dataStore = new LazyRef<DataStore>(() => _dataStoreServices.Value.Store);
            _connection = new LazyRef<DataStoreConnection>(() => _dataStoreServices.Value.Connection);
            _loggerFactory = new LazyRef<ILoggerFactory>(() => _externalProvider.GetRequiredService<ILoggerFactory>());
            _database = new LazyRef<Database>(() => _dataStoreServices.Value.Database);
            _stateManager = new LazyRef<StateManager>(() => _services.StateManager);

            return this;
        }

        public virtual DbContext Context
        {
            get { return _context; }
        }

        public virtual IModel Model
        {
            get { return _contextOptions.Model ?? _modelFromSource.Value; }
        }

        public virtual DataStore DataStore
        {
            get { return _dataStore.Value; }
        }

        public virtual Database Database
        {
            get { return _database.Value; }
        }

        public virtual DataStoreServices DataStoreServices
        {
            get { return _dataStoreServices.Value; }
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
            get { return _connection.Value; }
        }

        public virtual ContextServices Services
        {
            get { return _services; }
        }

        public virtual IDbContextOptionsExtensions ContextOptions
        {
            get { return _contextOptions; }
        }

        public virtual ServiceProviderSource ProviderSource
        {
            get { return _serviceProviderSource; }
        }

        public virtual ILoggerFactory LoggerFactory
        {
            get { return _loggerFactory.Value; }
        }

        public virtual StateManager StateManager
        {
            get { return _stateManager.Value; }
        }
    }
}
