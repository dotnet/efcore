// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ContextConfiguration
    {
        public enum ServiceProviderSource
        {
            Explicit,
            Implicit,
        }

        private ContextServices _services;
        private IServiceProvider _externalProvider;
        private EntityConfiguration _entityConfiguration;
        private DbContext _context;
        private LazyRef<IModel> _modelFromSource;
        private LazyRef<DataStore> _dataStore;
        private ServiceProviderSource _serviceProviderSource;
        private LazyRef<ILoggerFactory> _loggerFactory;

        public virtual ContextConfiguration Initialize(
            [NotNull] IServiceProvider externalProvider,
            [NotNull] IServiceProvider scopedProvider,
            [NotNull] EntityConfiguration entityConfiguration,
            [NotNull] DbContext context,
            ServiceProviderSource serviceProviderSource)
        {
            Check.NotNull(externalProvider, "externalProvider");
            Check.NotNull(scopedProvider, "scopedProvider");
            Check.NotNull(entityConfiguration, "entityConfiguration");
            Check.NotNull(context, "context");
            Check.IsDefined(serviceProviderSource, "serviceProviderSource");

            _externalProvider = externalProvider;
            _services = new ContextServices(scopedProvider);
            _serviceProviderSource = serviceProviderSource;
            _entityConfiguration = entityConfiguration;
            _context = context;
            _modelFromSource = new LazyRef<IModel>(() => _services.ModelSource.GetModel(_context));
            _dataStore = new LazyRef<DataStore>(() => _services.DataStoreSelector.SelectDataStore(this));
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
            get { return _entityConfiguration.Model ?? _modelFromSource.Value; }
        }

        public virtual DataStore DataStore
        {
            get { return _dataStore.Value; }
        }

        public virtual ContextServices Services
        {
            get { return _services; }
        }

        public virtual EntityConfiguration EntityConfiguration
        {
            get { return _entityConfiguration; }
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
