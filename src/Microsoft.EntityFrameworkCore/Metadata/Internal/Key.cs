// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Key :
        ConventionalAnnotatable,
        IMutableKey,
        IIdentityMapFactorySource,
        IPrincipalKeyValueFactorySource,
        IReferencingForeignKeyMetadata
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

        public virtual InternalKeyBuilder Builder { get; [param: CanBeNull] set; }

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
            => ReferencingForeignKeys ?? Enumerable.Empty<ForeignKey>();

        public virtual Func<IIdentityMap> IdentityMapFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _identityMapFactory, this, k => new IdentityMapFactoryFactory().Create(k));

        public virtual Func<IWeakReferenceIdentityMap> WeakReferenceIdentityMapFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _weakReferenceIdentityMap, this, k => new WeakReferenceIdentityMapFactoryFactory().Create(k));

        public virtual IPrincipalKeyValueFactory<TKey> GetPrincipalKeyValueFactory<TKey>()
            => (IPrincipalKeyValueFactory<TKey>)NonCapturingLazyInitializer.EnsureInitialized(
                ref _principalKeyValueFactory, this, k => new KeyValueFactoryFactory().Create<TKey>(k));

        IReadOnlyList<IProperty> IKey.Properties => Properties;
        IReadOnlyList<IMutableProperty> IMutableKey.Properties => Properties;
        IEntityType IKey.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableKey.DeclaringEntityType => DeclaringEntityType;
        public virtual List<ForeignKey> ReferencingForeignKeys { get; [param: CanBeNull] set; }
        IEnumerable<IForeignKey> IReferencingForeignKeyMetadata.ReferencingForeignKeys => ReferencingForeignKeys ?? Enumerable.Empty<IForeignKey>();

        [UsedImplicitly]
        private string DebuggerDisplay => Property.Format(Properties);
    }
}
