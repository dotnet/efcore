// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
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

        private DataStore _dataStore;
        private StateManagerFactory _stateManagerFactory;
        private EntitySetInitializer _entitySetInitializer;
        private EntitySetSource _entitySetSource;
        private EntityMaterializerSource _entityMaterializerSource;
        private IdentityGeneratorFactory _identityGeneratorFactory;
        private ActiveIdentityGenerators _activeIdentityGenerators;
        private IModel _model;
        private IModelSource _modelSource;

        public EntityConfiguration()
            : this(EntityServices.GetDefaultServices().BuildServiceProvider())
        {
        }

        public EntityConfiguration([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            _serviceProvider = serviceProvider;
        }

        public virtual IModel Model
        {
            get { return _model ?? _serviceProvider.GetService<IModel>(); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _model = value;
            }
        }

        public virtual IModelSource ModelSource
        {
            get { return _modelSource ?? GetRequiredService<IModelSource>(); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _modelSource = value;
            }
        }

        public virtual EntitySetInitializer EntitySetInitializer
        {
            // TODO: Remove this once the service provider correctly returns singleton instances
            get { return _entitySetInitializer ?? (_entitySetInitializer = GetRequiredService<EntitySetInitializer>()); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _entitySetInitializer = value;
            }
        }

        public virtual EntityMaterializerSource EntityMaterializerSource
        {
            // TODO: Remove this once the service provider correctly returns singleton instances
            get { return _entityMaterializerSource ?? (_entityMaterializerSource = GetRequiredService<EntityMaterializerSource>()); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _entityMaterializerSource = value;
            }
        }

        public virtual EntitySetSource EntitySetSource
        {
            // TODO: Remove this once the service provider correctly returns singleton instances
            get { return _entitySetSource ?? (_entitySetSource = GetRequiredService<EntitySetSource>()); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _entitySetSource = value;
            }
        }

        public virtual DataStore DataStore
        {
            get { return _dataStore ?? GetRequiredService<DataStore>(); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _dataStore = value;
            }
        }

        public virtual IdentityGeneratorFactory IdentityGeneratorFactory
        {
            get { return _identityGeneratorFactory ?? GetRequiredService<IdentityGeneratorFactory>(); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _identityGeneratorFactory = value;
            }
        }

        public virtual ActiveIdentityGenerators ActiveIdentityGenerators
        {
            get { return _activeIdentityGenerators ?? GetRequiredService<ActiveIdentityGenerators>(); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _activeIdentityGenerators = value;
            }
        }

        public virtual StateManagerFactory StateManagerFactory
        {
            get { return _stateManagerFactory ?? GetRequiredService<StateManagerFactory>(); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _stateManagerFactory = value;
            }
        }

        public virtual EntityContext CreateContext()
        {
            return new EntityContext(this);
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
    }
}
