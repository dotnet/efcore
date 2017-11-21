// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class KeyValueIndexFactory<TKey> : IKeyValueIndexFactory
    {
        private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public KeyValueIndexFactory([NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
            => _principalKeyValueFactory = principalKeyValueFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IKeyValueIndex CreatePrincipalKeyValue(InternalEntityEntry entry, IForeignKey foreignKey)
            => new KeyValueIndex<TKey>(
                foreignKey,
                _principalKeyValueFactory.CreateFromCurrentValues(entry),
                _principalKeyValueFactory.EqualityComparer,
                fromOriginalValues: false);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IKeyValueIndex CreatePrincipalKeyValueFromOriginalValues(InternalEntityEntry entry, IForeignKey foreignKey)
            => new KeyValueIndex<TKey>(
                foreignKey,
                _principalKeyValueFactory.CreateFromOriginalValues(entry),
                _principalKeyValueFactory.EqualityComparer,
                fromOriginalValues: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IKeyValueIndex CreateDependentKeyValue(InternalEntityEntry entry, IForeignKey foreignKey)
            => foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromCurrentValues(entry, out var keyValue)
                ? new KeyValueIndex<TKey>(foreignKey, keyValue, _principalKeyValueFactory.EqualityComparer, fromOriginalValues: false)
                : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IKeyValueIndex CreateDependentKeyValueFromOriginalValues(InternalEntityEntry entry, IForeignKey foreignKey)
            => foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromOriginalValues(entry, out var keyValue)
                ? new KeyValueIndex<TKey>(foreignKey, keyValue, _principalKeyValueFactory.EqualityComparer, fromOriginalValues: true)
                : null;
    }
}
