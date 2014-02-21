// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeTrackerFactory
    {
        private readonly ActiveIdentityGenerators _identityGenerators;

        // Intended only for creation of test doubles
        internal ChangeTrackerFactory()
        {
        }

        public ChangeTrackerFactory([NotNull] ActiveIdentityGenerators identityGenerators)
        {
            Check.NotNull(identityGenerators, "identityGenerators");

            _identityGenerators = identityGenerators;
        }

        public virtual ChangeTracker Create([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return new ChangeTracker(model, _identityGenerators);
        }
    }
}
