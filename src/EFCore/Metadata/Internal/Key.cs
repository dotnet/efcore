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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Key : ConventionalAnnotatable, IMutableKey
    {
        private ConfigurationSource _configurationSource;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private Func<bool, IIdentityMap> _identityMapFactory;

        private Func<IWeakReferenceIdentityMap> _weakReferenceIdentityMap;
        private object _principalKeyValueFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Key([NotNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            Properties = properties;
            _configurationSource = configurationSource;

            Builder = new InternalKeyBuilder(this, DeclaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> Properties { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType
        {
            [DebuggerStepThrough] get => Properties[0].DeclaringEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder Builder
        {
            [DebuggerStepThrough] get;
            [DebuggerStepThrough]
            [param: CanBeNull]
            set;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = _configurationSource.Max(configurationSource);
            foreach (var property in Properties)
            {
                property.UpdateConfigurationSource(configurationSource);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
            => ReferencingForeignKeys ?? Enumerable.Empty<ForeignKey>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<bool, IIdentityMap> IdentityMapFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _identityMapFactory, this, k => new IdentityMapFactoryFactory().Create(k));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<IWeakReferenceIdentityMap> WeakReferenceIdentityMapFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _weakReferenceIdentityMap, this, k => new WeakReferenceIdentityMapFactoryFactory().Create(k));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IPrincipalKeyValueFactory<TKey> GetPrincipalKeyValueFactory<TKey>()
            => (IPrincipalKeyValueFactory<TKey>)NonCapturingLazyInitializer.EnsureInitialized(
                ref _principalKeyValueFactory, this, k => new KeyValueFactoryFactory().Create<TKey>(k));

        IReadOnlyList<IProperty> IKey.Properties => Properties;
        IReadOnlyList<IMutableProperty> IMutableKey.Properties => Properties;
        IEntityType IKey.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableKey.DeclaringEntityType => DeclaringEntityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Note this is ISet because there is no suitable readonly interface in the profiles we are using
        public virtual ISet<ForeignKey> ReferencingForeignKeys { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString() => this.ToDebugString();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DebugView<Key> DebugView
            => new DebugView<Key>(this, m => m.ToDebugString(false));
    }
}
