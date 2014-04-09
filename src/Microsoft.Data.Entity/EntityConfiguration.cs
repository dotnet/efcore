// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
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
        private IServiceProvider _serviceProvider;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected internal EntityConfiguration()
        {
        }

        public virtual EntityConfiguration Initialize([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            _serviceProvider = serviceProvider;

            return this;
        }

        public virtual IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public virtual IModelSource ModelSource
        {
            get { return _serviceProvider.GetRequiredService<IModelSource>(); }
        }

        public virtual EntitySetInitializer EntitySetInitializer
        {
            get { return _serviceProvider.GetRequiredService<EntitySetInitializer>(); }
        }

        public virtual EntitySetSource EntitySetSource
        {
            get { return _serviceProvider.GetRequiredService<EntitySetSource>(); }
        }

        public virtual DataStore DataStore
        {
            get { return _serviceProvider.GetRequiredService<DataStore>(); }
        }

        public virtual IdentityGeneratorFactory IdentityGeneratorFactory
        {
            get { return _serviceProvider.GetRequiredService<IdentityGeneratorFactory>(); }
        }

        public virtual ActiveIdentityGenerators ActiveIdentityGenerators
        {
            get { return _serviceProvider.GetRequiredService<ActiveIdentityGenerators>(); }
        }

        public virtual EntitySetFinder EntitySetFinder
        {
            get { return _serviceProvider.GetRequiredService<EntitySetFinder>(); }
        }

        public virtual EntityKeyFactorySource EntityKeyFactorySource
        {
            get { return _serviceProvider.GetRequiredService<EntityKeyFactorySource>(); }
        }

        public virtual ClrCollectionAccessorSource ClrCollectionAccessorSource
        {
            get { return _serviceProvider.GetRequiredService<ClrCollectionAccessorSource>(); }
        }

        public virtual ClrPropertyGetterSource ClrPropertyGetterSource
        {
            get { return _serviceProvider.GetRequiredService<ClrPropertyGetterSource>(); }
        }

        public virtual ClrPropertySetterSource ClrPropertySetterSource
        {
            get { return _serviceProvider.GetRequiredService<ClrPropertySetterSource>(); }
        }

        public virtual EntityMaterializerSource EntityMaterializerSource
        {
            get { return _serviceProvider.GetRequiredService<EntityMaterializerSource>(); }
        }

        public virtual StoreGeneratedValuesFactory StoreGeneratedValuesFactory
        {
            get { return _serviceProvider.GetRequiredService<StoreGeneratedValuesFactory>(); }
        }

        public virtual OriginalValuesFactory OriginalValuesFactory
        {
            get { return _serviceProvider.GetRequiredService<OriginalValuesFactory>(); }
        }

        public virtual ILoggerFactory LoggerFactory
        {
            get { return _serviceProvider.GetRequiredService<ILoggerFactory>(); }
        }
    }
}
