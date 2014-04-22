// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
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
        private EntityConfiguration _entityConfiguration;
        private EntityContext _context;
        private LazyRef<IModel> _modelFromSource;
        private LazyRef<DataStore> _dataStore;
        private ServiceProviderSource _serviceProviderSource;

        public virtual ContextConfiguration Initialize(
            [NotNull] IServiceProvider scopedProvider,
            [NotNull] EntityConfiguration entityConfiguration,
            [NotNull] EntityContext context,
            ServiceProviderSource serviceProviderSource)
        {
            Check.NotNull(entityConfiguration, "entityConfiguration");
            Check.NotNull(context, "context");
            Check.IsDefined(serviceProviderSource, "serviceProviderSource");

            _services = new ContextServices(scopedProvider);
            _serviceProviderSource = serviceProviderSource;
            _entityConfiguration = entityConfiguration;
            _context = context;
            _modelFromSource = new LazyRef<IModel>(() => _services.ModelSource.GetModel(_context));
            _dataStore = new LazyRef<DataStore>(() => _services.DataStoreSelector.SelectDataStore(this));

            return this;
        }

        public virtual EntityContext Context
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
    }
}
