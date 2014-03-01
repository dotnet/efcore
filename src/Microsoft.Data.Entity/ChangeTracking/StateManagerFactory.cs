// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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

        // Intended only for creation of test doubles
        internal StateManagerFactory()
        {
        }

        public StateManagerFactory([NotNull] ActiveIdentityGenerators identityGenerators, [NotNull] IEnumerable<IEntityStateListener> entityStateListeners)
        {
            Check.NotNull(identityGenerators, "identityGenerators");
            Check.NotNull(entityStateListeners, "entityStateListeners");

            _identityGenerators = identityGenerators;
            _entityStateListeners = entityStateListeners;
        }

        public virtual StateManager Create([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return new StateManager(model, _identityGenerators, _entityStateListeners);
        }
    }
}
