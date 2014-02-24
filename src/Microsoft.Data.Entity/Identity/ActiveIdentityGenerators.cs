// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class ActiveIdentityGenerators
    {
        private readonly IdentityGeneratorFactory _factory;

        private readonly LazyRef<ImmutableDictionary<IProperty, IIdentityGenerator>> _identityGenerators
            = new LazyRef<ImmutableDictionary<IProperty, IIdentityGenerator>>(
                () => ImmutableDictionary<IProperty, IIdentityGenerator>.Empty);

        // Intended only for creation of test doubles
        internal ActiveIdentityGenerators()
        {
        }

        public ActiveIdentityGenerators([NotNull] IdentityGeneratorFactory factory)
        {
            Check.NotNull(factory, "factory");

            _factory = factory;
        }

        public virtual IIdentityGenerator GetOrAdd([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            IIdentityGenerator generator;
            if (_identityGenerators.Value.TryGetValue(property, out generator))
            {
                return generator;
            }

            _identityGenerators.ExchangeValue(
                d => !d.ContainsKey(property) ? d.Add(property, _factory.Create(property)) : d);

            return _identityGenerators.Value[property];
        }
    }
}
