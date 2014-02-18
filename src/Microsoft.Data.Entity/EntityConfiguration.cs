// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityConfiguration
    {
        private readonly IServiceProvider _serviceProvider;

        private DataStore _dataStore;

        private readonly LazyRef<ImmutableDictionary<Type, IIdentityGenerator>> _identityGenerators
            = new LazyRef<ImmutableDictionary<Type, IIdentityGenerator>>(() => ImmutableDictionary<Type, IIdentityGenerator>.Empty);

        public EntityConfiguration()
            : this(new ServiceProvider().Add(EntityServices.GetDefaultServices()))
        {
        }

        public EntityConfiguration([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            _serviceProvider = serviceProvider;
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

        public virtual IIdentityGenerator<TIdentity> GetIdentityGenerator<TIdentity>()
        {
            IIdentityGenerator<TIdentity> identityGenerator = null;

            if (_identityGenerators.HasValue)
            {
                IIdentityGenerator untypedReference;
                if (_identityGenerators.Value.TryGetValue(typeof(TIdentity), out untypedReference))
                {
                    identityGenerator = untypedReference as IIdentityGenerator<TIdentity>;
                }
            }

            return identityGenerator
                   ?? _serviceProvider.GetService<IIdentityGenerator<TIdentity>>()
                   ?? ThrowNotConfigured<IIdentityGenerator<TIdentity>>();
        }

        public virtual EntityConfiguration SetIdentityGenerator<TIdentity>([NotNull] IIdentityGenerator<TIdentity> identityGenerator)
        {
            Check.NotNull(identityGenerator, "identityGenerator");

            _identityGenerators.ExchangeValue(igs => igs.SetItem(typeof(TIdentity), identityGenerator));

            return this;
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
