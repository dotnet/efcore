// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a primary or alternate key on an entity type.
    /// </summary>
    public class RuntimeKey : AnnotatableBase, IRuntimeKey
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private Func<bool, IIdentityMap>? _identityMapFactory;
        private object? _principalKeyValueFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public RuntimeKey(IReadOnlyList<RuntimeProperty> properties)
        {
            Properties = properties;
        }

        /// <summary>
        ///     Gets the properties that make up the key.
        /// </summary>
        public virtual IReadOnlyList<RuntimeProperty> Properties { get; }

        /// <summary>
        ///     Gets the entity type the key is defined on. This may be different from the type that <see cref="IKey.Properties" />
        ///     are defined on when the key is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        public virtual RuntimeEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => Properties[0].DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual ISet<RuntimeForeignKey>? ReferencingForeignKeys { get; set; }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
            => ((IReadOnlyKey)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual DebugView DebugView
            => new(
                () => ((IReadOnlyKey)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IReadOnlyKey)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <inheritdoc/>
        IReadOnlyList<IReadOnlyProperty> IReadOnlyKey.Properties
        {
            [DebuggerStepThrough]
            get => Properties;
        }

        /// <inheritdoc/>
        IReadOnlyList<IProperty> IKey.Properties
        {
            [DebuggerStepThrough]
            get => Properties;
        }

        /// <inheritdoc/>
        IReadOnlyEntityType IReadOnlyKey.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => DeclaringEntityType;
        }

        /// <inheritdoc/>
        IEntityType IKey.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => DeclaringEntityType;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyForeignKey> IReadOnlyKey.GetReferencingForeignKeys()
            => ReferencingForeignKeys ?? Enumerable.Empty<IReadOnlyForeignKey>();

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IPrincipalKeyValueFactory<TKey> IKey.GetPrincipalKeyValueFactory<TKey>()
            => (IPrincipalKeyValueFactory<TKey>)NonCapturingLazyInitializer.EnsureInitialized(
                ref _principalKeyValueFactory, this, static key =>
                {
                    key.EnsureReadOnly();
                    return new KeyValueFactoryFactory().Create<TKey>(key);
                });

        /// <inheritdoc/>
        [DebuggerStepThrough]
        Func<bool, IIdentityMap> IRuntimeKey.GetIdentityMapFactory()
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _identityMapFactory, this, static key =>
                {
                    key.EnsureReadOnly();
                    return new IdentityMapFactoryFactory().Create(key);
                });
    }
}
