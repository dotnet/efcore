// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityConfiguration
    {
        private readonly IServiceProvider _serviceProvider;

        // TODO: Remove these once the service provider correctly returns singleton instances
        private IModelSource _modelSource;
        private EntitySetInitializer _entitySetInitializer;
        private EntitySetSource _entitySetSource;
        private DataStore _dataStore;
        private IdentityGeneratorFactory _identityGeneratorFactory;
        private ActiveIdentityGenerators _activeIdentityGenerators;
        private StateManagerFactory _stateManagerFactory;
        private EntitySetFinder _entitySetFinder;
        private EntityKeyFactorySource _entityKeyFactorySource;
        private StateEntryFactory _stateEntryFactory;
        private ClrPropertyGetterSource _clrPropertyGetterSource;
        private ClrPropertySetterSource _clrPropertySetterSource;
        private EntityMaterializerSource _entityMaterializerSource;
        private IModel _model;
        private ILoggerFactory _loggerFactory;
        private IEnumerable<IEntityStateListener> _entityStateListeners;

        public EntityConfiguration([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            _serviceProvider = serviceProvider;
        }

        // Required services

        public virtual IModelSource ModelSource
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _modelSource ?? (_modelSource = GetRequiredService<IModelSource>()); }
        }

        public virtual EntitySetInitializer EntitySetInitializer
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _entitySetInitializer ?? (_entitySetInitializer = GetRequiredService<EntitySetInitializer>()); }
        }

        public virtual EntitySetSource EntitySetSource
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _entitySetSource ?? (_entitySetSource = GetRequiredService<EntitySetSource>()); }
        }

        public virtual DataStore DataStore
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _dataStore ?? (_dataStore = GetRequiredService<DataStore>()); }
        }

        public virtual IdentityGeneratorFactory IdentityGeneratorFactory
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _identityGeneratorFactory ?? (_identityGeneratorFactory = GetRequiredService<IdentityGeneratorFactory>()); }
        }

        public virtual ActiveIdentityGenerators ActiveIdentityGenerators
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _activeIdentityGenerators ?? (_activeIdentityGenerators = GetRequiredService<ActiveIdentityGenerators>()); }
        }

        public virtual StateManagerFactory StateManagerFactory
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _stateManagerFactory ?? (_stateManagerFactory = GetRequiredService<StateManagerFactory>()); }
        }

        public virtual EntitySetFinder EntitySetFinder
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _entitySetFinder ?? (_entitySetFinder = GetRequiredService<EntitySetFinder>()); }
        }

        public virtual EntityKeyFactorySource EntityKeyFactorySource
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _entityKeyFactorySource ?? (_entityKeyFactorySource = GetRequiredService<EntityKeyFactorySource>()); }
        }

        public virtual StateEntryFactory StateEntryFactory
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _stateEntryFactory ?? (_stateEntryFactory = GetRequiredService<StateEntryFactory>()); }
        }

        public virtual ClrPropertyGetterSource ClrPropertyGetterSource
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _clrPropertyGetterSource ?? (_clrPropertyGetterSource = GetRequiredService<ClrPropertyGetterSource>()); }
        }

        public virtual ClrPropertySetterSource ClrPropertySetterSource
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _clrPropertySetterSource ?? (_clrPropertySetterSource = GetRequiredService<ClrPropertySetterSource>()); }
        }

        public virtual EntityMaterializerSource EntityMaterializerSource
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _entityMaterializerSource ?? (_entityMaterializerSource = GetRequiredService<EntityMaterializerSource>()); }
        }

        private TService GetRequiredService<TService>() where TService : class
        {
            var service = _serviceProvider.GetService<TService>();

            if (service != null)
            {
                return service;
            }

            throw new InvalidOperationException(Strings.FormatMissingConfigurationItem(typeof(TService)));
        }

        // Optional services

        public virtual IModel Model
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _model ?? (_model = _serviceProvider.GetService<IModel>()); }
        }

        public virtual ILoggerFactory LoggerFactory
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _loggerFactory ?? (_loggerFactory = _serviceProvider.GetService<ILoggerFactory>()); }
        }

        public virtual IEnumerable<IEntityStateListener> EntityStateListeners
        {
            // TODO: Remove the caching here once the service provider correctly returns singleton instances
            get { return _entityStateListeners ?? (_entityStateListeners = _serviceProvider.GetService<IEnumerable<IEntityStateListener>>()); }
        }
    }
}
