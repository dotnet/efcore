// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateManagerFactory
    {
        private readonly ActiveIdentityGenerators _identityGenerators;
        private readonly IEnumerable<IEntityStateListener> _entityStateListeners;
        private readonly EntityKeyFactorySource _keyFactorySource;
        private readonly StateEntryFactory _stateEntryFactory;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateManagerFactory()
        {
        }

        public StateManagerFactory(
            [NotNull] ActiveIdentityGenerators identityGenerators,
            [NotNull] IEnumerable<IEntityStateListener> entityStateListeners,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] StateEntryFactory stateEntryFactory)

        {
            Check.NotNull(identityGenerators, "identityGenerators");
            Check.NotNull(entityStateListeners, "entityStateListeners");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(stateEntryFactory, "stateEntryFactory");

            _identityGenerators = identityGenerators;
            _entityStateListeners = entityStateListeners;
            _keyFactorySource = entityKeyFactorySource;
            _stateEntryFactory = stateEntryFactory;
        }

        public virtual StateManager Create([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return new StateManager(model, _identityGenerators, _entityStateListeners, _keyFactorySource, _stateEntryFactory);
        }
    }
}
