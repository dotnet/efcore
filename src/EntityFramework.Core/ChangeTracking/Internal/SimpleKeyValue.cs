// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SimpleKeyValue<TKey> : KeyValue
    {
        private static readonly IEqualityComparer _equalityComparer = EqualityComparer<TKey>.Default;

        private readonly TKey _keyValue;

        public SimpleKeyValue([NotNull] IKey key, [CanBeNull] TKey keyValue)
            : base(key)
        {
            _keyValue = keyValue;
        }

        public new virtual TKey Value => _keyValue;

        protected override object GetValue() => _keyValue;

        public override bool Equals([CanBeNull] object obj)
            => !ReferenceEquals(null, obj)
               && (ReferenceEquals(this, obj)
                   || (obj.GetType() == GetType()
                       && Equals((SimpleKeyValue<TKey>)obj)));

        private bool Equals(SimpleKeyValue<TKey> other)
            => Key == other.Key
               && _equalityComparer.Equals(_keyValue, other._keyValue);

        public override int GetHashCode()
            => (Key.GetHashCode() * 397)
               ^ _equalityComparer.GetHashCode(_keyValue);

        [UsedImplicitly]
        private string DebuggerDisplay => $"{Key.DeclaringEntityType.Name}.{Key.Properties[0].Name}({string.Join(", ", _keyValue)})";
    }
}
