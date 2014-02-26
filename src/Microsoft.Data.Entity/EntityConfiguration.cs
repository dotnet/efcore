// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
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
        private ChangeTrackerFactory _changeTrackerFactory;
        private IdentityGeneratorFactory _identityGeneratorFactory;
        private ActiveIdentityGenerators _activeIdentityGenerators;
        private IModel _model;
        private IModelSource _modelSource;

        public EntityConfiguration()
            : this(new ServiceProvider().Add(EntityServices.GetDefaultServices()))
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
            get
            {
                return _modelSource
                       ?? _serviceProvider.GetService<IModelSource>()
                       ?? ThrowNotConfigured<IModelSource>();
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _modelSource = value;
            }
        }

        public virtual DataStore DataStore
        {
            get
            {
                return _dataStore
                       ?? _serviceProvider.GetService<DataStore>()
                       ?? ThrowNotConfigured<DataStore>();
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _dataStore = value;
            }
        }

        public virtual IdentityGeneratorFactory IdentityGeneratorFactory
        {
            get
            {
                return _identityGeneratorFactory
                       ?? _serviceProvider.GetService<IdentityGeneratorFactory>()
                       ?? ThrowNotConfigured<IdentityGeneratorFactory>();
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _identityGeneratorFactory = value;
            }
        }

        public virtual ActiveIdentityGenerators ActiveIdentityGenerators
        {
            get
            {
                return _activeIdentityGenerators
                       ?? _serviceProvider.GetService<ActiveIdentityGenerators>()
                       ?? ThrowNotConfigured<ActiveIdentityGenerators>();
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _activeIdentityGenerators = value;
            }
        }

        public virtual ChangeTrackerFactory ChangeTrackerFactory
        {
            get
            {
                return _changeTrackerFactory
                       ?? _serviceProvider.GetService<ChangeTrackerFactory>()
                       ?? ThrowNotConfigured<ChangeTrackerFactory>();
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _changeTrackerFactory = value;
            }
        }

        public virtual EntityContext CreateContext()
        {
            return new EntityContext(this);
        }

        private static T ThrowNotConfigured<T>()
        {
            throw new InvalidOperationException(
                Strings.MissingConfigurationItem(typeof(T)));
        }
    }
}
