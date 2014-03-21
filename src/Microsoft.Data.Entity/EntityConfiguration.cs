// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

        public EntityConfiguration([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            _serviceProvider = serviceProvider;
        }

        // Required services

        public virtual IModelSource ModelSource
        {
            get { return GetRequiredService<IModelSource>(); }
        }

        public virtual EntitySetInitializer EntitySetInitializer
        {
            get { return GetRequiredService<EntitySetInitializer>(); }
        }

        public virtual EntitySetSource EntitySetSource
        {
            get { return GetRequiredService<EntitySetSource>(); }
        }

        public virtual DataStore DataStore
        {
            get { return GetRequiredService<DataStore>(); }
        }

        public virtual IdentityGeneratorFactory IdentityGeneratorFactory
        {
            get { return GetRequiredService<IdentityGeneratorFactory>(); }
        }

        public virtual ActiveIdentityGenerators ActiveIdentityGenerators
        {
            get { return GetRequiredService<ActiveIdentityGenerators>(); }
        }

        public virtual StateManagerFactory StateManagerFactory
        {
            get { return GetRequiredService<StateManagerFactory>(); }
        }

        public virtual EntitySetFinder EntitySetFinder
        {
            get { return GetRequiredService<EntitySetFinder>(); }
        }

        public virtual EntityKeyFactorySource EntityKeyFactorySource
        {
            get { return GetRequiredService<EntityKeyFactorySource>(); }
        }

        public virtual StateEntryFactory StateEntryFactory
        {
            get { return GetRequiredService<StateEntryFactory>(); }
        }

        public virtual ClrPropertyGetterSource ClrPropertyGetterSource
        {
            get { return GetRequiredService<ClrPropertyGetterSource>(); }
        }

        public virtual ClrPropertySetterSource ClrPropertySetterSource
        {
            get { return GetRequiredService<ClrPropertySetterSource>(); }
        }

        public virtual EntityMaterializerSource EntityMaterializerSource
        {
            get { return GetRequiredService<EntityMaterializerSource>(); }
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
            get { return _serviceProvider.GetService<IModel>(); }
        }

        public virtual ILoggerFactory LoggerFactory
        {
            get { return _serviceProvider.GetService<ILoggerFactory>(); }
        }

        public virtual IEnumerable<IEntityStateListener> EntityStateListeners
        {
            get { return _serviceProvider.GetService<IEnumerable<IEntityStateListener>>() 
                ?? Enumerable.Empty<IEntityStateListener>(); }
        }
    }
}
