// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Key : ConventionalAnnotatable, IMutableKey, IIdentityMapFactorySource, IPrincipalKeyValueFactorySource
    {
        private ConfigurationSource _configurationSource;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private Func<IIdentityMap> _identityMapFactory;
        private Func<IWeakReferenceIdentityMap> _weakReferenceIdentityMap;
        private object _principalKeyValueFactory;

        public Key([NotNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            Properties = properties;
            _configurationSource = configurationSource;

            Builder = new InternalKeyBuilder(this, DeclaringEntityType.Model.Builder);
        }

        public virtual IReadOnlyList<Property> Properties { get; }
        public virtual EntityType DeclaringEntityType => Properties[0].DeclaringEntityType;

        public virtual InternalKeyBuilder Builder { get; [param:CanBeNull] set; }

        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = _configurationSource.Max(configurationSource);
            foreach (var property in Properties)
            {
                property.UpdateConfigurationSource(configurationSource);
            }
        }

        public virtual IEnumerable<ForeignKey> FindReferencingForeignKeys()
            => ((IKey)this).FindReferencingForeignKeys().Cast<ForeignKey>();

        public virtual Func<IIdentityMap> IdentityMapFactory
            => LazyInitializer.EnsureInitialized(
                ref _identityMapFactory, () => new IdentityMapFactoryFactory().Create(this));

        public virtual Func<IWeakReferenceIdentityMap> WeakReferenceIdentityMapFactory
            => LazyInitializer.EnsureInitialized(
                ref _weakReferenceIdentityMap, () => new WeakReferenceIdentityMapFactoryFactory().Create(this));

        public virtual IPrincipalKeyValueFactory<TKey> GetPrincipalKeyValueFactory<TKey>()
            => (IPrincipalKeyValueFactory<TKey>)LazyInitializer.EnsureInitialized(
                ref _principalKeyValueFactory, () => new KeyValueFactoryFactory().Create<TKey>(this));

        IReadOnlyList<IProperty> IKey.Properties => Properties;
        IReadOnlyList<IMutableProperty> IMutableKey.Properties => Properties;
        IEntityType IKey.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableKey.DeclaringEntityType => DeclaringEntityType;

        [UsedImplicitly]
        private string DebuggerDisplay => Property.Format(Properties);
    }
}
