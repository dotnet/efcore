// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    public sealed class KeyValueIndex<TKey> : IKeyValueIndex
    {
        private readonly IForeignKey _foreignKey;
        private readonly TKey _keyValue;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly bool _fromOriginalValues;

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

        public IKeyValueIndex WithOriginalValuesFlag()
            => new KeyValueIndex<TKey>(_foreignKey, _keyValue, _keyComparer, fromOriginalValues: true);

        private bool Equals(KeyValueIndex<TKey> other)
            => other._fromOriginalValues == _fromOriginalValues
               && other._foreignKey == _foreignKey
               && _keyComparer.Equals(_keyValue, other._keyValue);

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
               && (ReferenceEquals(this, obj)
                   || (obj.GetType() == GetType()
                       && Equals((KeyValueIndex<TKey>)obj)));

        public override int GetHashCode()
            => (((((typeof(TKey).GetHashCode() * 397)
                   ^ _fromOriginalValues.GetHashCode()) * 397)
                 ^ _foreignKey.GetHashCode()) * 397)
               ^ _keyComparer.GetHashCode(_keyValue);
    }
}
