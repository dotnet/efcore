// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    /// <summary>
    ///     These are convenience methods for obtaining services directly from the DI container for situations
    ///     where using normal constructor injecttion for each service is not appropriate. For example, this is
    ///     used in <see cref="StateEntry" /> instances where we want to carry only one reference around rather than
    ///     carrying a reference for dependent service.
    /// </summary>
    public class ContextServices
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ContextServices()
        {
        }

        public ContextServices([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            _serviceProvider = serviceProvider;
        }

        public virtual IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public virtual ActiveIdentityGenerators ActiveIdentityGenerators
        {
            get { return _serviceProvider.GetRequiredService<ActiveIdentityGenerators>(); }
        }

        public virtual ClrPropertyGetterSource ClrPropertyGetterSource
        {
            get { return _serviceProvider.GetRequiredService<ClrPropertyGetterSource>(); }
        }

        public virtual ClrPropertySetterSource ClrPropertySetterSource
        {
            get { return _serviceProvider.GetRequiredService<ClrPropertySetterSource>(); }
        }

        public virtual ContextSets ContextSets
        {
            get { return ServiceProvider.GetRequiredService<ContextSets>(); }
        }

        public virtual DataStoreSelector DataStoreSelector
        {
            get { return _serviceProvider.GetRequiredService<DataStoreSelector>(); }
        }

        public virtual StateManager StateManager
        {
            get { return ServiceProvider.GetRequiredService<StateManager>(); }
        }

        public virtual EntityKeyFactorySource EntityKeyFactorySource
        {
            get { return _serviceProvider.GetRequiredService<EntityKeyFactorySource>(); }
        }

        public virtual IModelSource ModelSource
        {
            get { return _serviceProvider.GetRequiredService<IModelSource>(); }
        }

        public virtual OriginalValuesFactory OriginalValuesFactory
        {
            get { return _serviceProvider.GetRequiredService<OriginalValuesFactory>(); }
        }

        public virtual StateEntryNotifier StateEntryNotifier
        {
            get { return ServiceProvider.GetRequiredService<StateEntryNotifier>(); }
        }

        public virtual StoreGeneratedValuesFactory StoreGeneratedValuesFactory
        {
            get { return _serviceProvider.GetRequiredService<StoreGeneratedValuesFactory>(); }
        }

        public virtual StateEntryFactory StateEntryFactory
        {
            get { return ServiceProvider.GetRequiredService<StateEntryFactory>(); }
        }

        public virtual IEnumerable<IEntityStateListener> EntityStateListeners
        {
            get
            {
                return ServiceProvider.GetService<IEnumerable<IEntityStateListener>>()
                       ?? Enumerable.Empty<IEntityStateListener>();
            }
        }
    }
}
