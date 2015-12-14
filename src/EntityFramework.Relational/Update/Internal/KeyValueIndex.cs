// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Update.Internal
{
    public sealed class KeyValueIndex<TKey> : IKeyValueIndex
    {
        private readonly IForeignKey _foreignKey;
        private readonly TKey _keyValue;
        private readonly bool _fromOriginalValues;

        public KeyValueIndex([NotNull] IForeignKey foreignKey, [NotNull] TKey keyValue, bool fromOriginalValues)
        {
            _foreignKey = foreignKey;
            _keyValue = keyValue;
            _fromOriginalValues = fromOriginalValues;
        }

        public IKeyValueIndex WithOriginalValuesFlag()
            => new KeyValueIndex<TKey>(_foreignKey, _keyValue, fromOriginalValues: true);

        private bool Equals(KeyValueIndex<TKey> other)
            => other._fromOriginalValues == _fromOriginalValues
               && other._foreignKey == _foreignKey
               && other._keyValue.Equals(_keyValue);

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
               && (ReferenceEquals(this, obj)
                   || (obj.GetType() == GetType()
                       && Equals((KeyValueIndex<TKey>)obj)));

        public override int GetHashCode()
            => (((_fromOriginalValues.GetHashCode() * 397)
                 ^ _foreignKey.GetHashCode()) * 397)
               ^ _keyValue.GetHashCode();
    }
}
