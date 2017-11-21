// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public sealed class KeyValueIndex<TKey> : IKeyValueIndex
    {
        private readonly IForeignKey _foreignKey;
        private readonly TKey _keyValue;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly bool _fromOriginalValues;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public KeyValueIndex(
            [NotNull] IForeignKey foreignKey,
            [NotNull] TKey keyValue,
            [NotNull] IEqualityComparer<TKey> keyComparer,
            bool fromOriginalValues)
        {
            _foreignKey = foreignKey;
            _keyValue = keyValue;
            _fromOriginalValues = fromOriginalValues;
            _keyComparer = keyComparer;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IKeyValueIndex WithOriginalValuesFlag()
            => new KeyValueIndex<TKey>(_foreignKey, _keyValue, _keyComparer, fromOriginalValues: true);

        private bool Equals(KeyValueIndex<TKey> other)
            => other._fromOriginalValues == _fromOriginalValues
               && other._foreignKey == _foreignKey
               && _keyComparer.Equals(_keyValue, other._keyValue);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool Equals(object obj)
            => !(obj is null)
               && (ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((KeyValueIndex<TKey>)obj));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override int GetHashCode()
            => (((((typeof(TKey).GetHashCode() * 397)
                   ^ _fromOriginalValues.GetHashCode()) * 397)
                 ^ _foreignKey.GetHashCode()) * 397)
               ^ _keyComparer.GetHashCode(_keyValue);
    }
}
