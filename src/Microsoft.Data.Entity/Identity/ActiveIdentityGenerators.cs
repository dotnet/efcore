// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class ActiveIdentityGenerators
    {
        private readonly IdentityGeneratorFactory _factory;

        private readonly ThreadSafeLazyRef<ImmutableDictionary<IProperty, IIdentityGenerator>> _identityGenerators
            = new ThreadSafeLazyRef<ImmutableDictionary<IProperty, IIdentityGenerator>>(
                () => ImmutableDictionary<IProperty, IIdentityGenerator>.Empty);

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ActiveIdentityGenerators()
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
