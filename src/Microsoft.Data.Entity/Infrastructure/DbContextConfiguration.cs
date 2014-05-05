// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

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
        private ImmutableDbContextOptions _contextOptions;
        private DbContext _context;
        private LazyRef<IModel> _modelFromSource;
        private LazyRef<DataStoreSource> _dataStoreSource;
        private LazyRef<DataStore> _dataStore;
        private LazyRef<DataStoreConnection> _connection;
        private ServiceProviderSource _serviceProviderSource;
        private LazyRef<ILoggerFactory> _loggerFactory;

        public virtual DbContextConfiguration Initialize(
            [NotNull] IServiceProvider externalProvider,
            [NotNull] IServiceProvider scopedProvider,
            [NotNull] ImmutableDbContextOptions contextOptions,
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
            _modelFromSource = new LazyRef<IModel>(() => _services.ModelSource.GetModel(_context));
            _dataStoreSource = new LazyRef<DataStoreSource>(() => _services.DataStoreSelector.SelectDataStore(this));
            _dataStore = new LazyRef<DataStore>(() => _dataStoreSource.Value.GetStore(this));
            _connection = new LazyRef<DataStoreConnection>(() => _dataStoreSource.Value.GetConnection(this));
            _loggerFactory = new LazyRef<ILoggerFactory>(() => GetLoggerFactory() ?? new NullLoggerFactory());

            return this;
        }

        private ILoggerFactory GetLoggerFactory()
        {
            try
            {
                return _externalProvider.GetService<ILoggerFactory>();
            }
            catch
            {
                // Work around issue where some DI containers will throw if service not registered
                return null;
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

        public virtual DataStore DataStore
        {
            get { return _dataStore.Value; }
        }

        public virtual DataStoreCreator DataStoreCreator
        {
            get { return _dataStoreSource.Value.GetCreator(this); }
        }

        public virtual DataStoreConnection Connection
        {
            get { return _connection.Value; }
        }

        public virtual ContextServices Services
        {
            get { return _services; }
        }

        public virtual ImmutableDbContextOptions ContextOptions
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
    }
}
