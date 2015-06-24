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
    public class SimpleEntityKey<TKey> : EntityKey
    {
        private static readonly IEqualityComparer _equalityComparer = EqualityComparer<TKey>.Default;
        
        private readonly TKey _keyValue;

        public SimpleEntityKey([NotNull] IEntityType entityType, [CanBeNull] TKey keyValue)
        {
            Debug.Assert(entityType != null); // hot path

            EntityType = entityType;
            _keyValue = keyValue;
        }

        public new virtual TKey Value => _keyValue;

        protected override object GetValue() => _keyValue;

        public override bool Equals([CanBeNull] object obj)
            => !ReferenceEquals(null, obj)
               && (ReferenceEquals(this, obj)
                   || (obj.GetType() == GetType()
                       && Equals((SimpleEntityKey<TKey>)obj)));

        private bool Equals(SimpleEntityKey<TKey> other)
            => EntityType == other.EntityType
               && _equalityComparer.Equals(_keyValue, other._keyValue);

        public override int GetHashCode()
            => (EntityType.GetHashCode() * 397)
               ^ _equalityComparer.GetHashCode(_keyValue);

        [UsedImplicitly]
        private string DebuggerDisplay => $"{EntityType.Name}({string.Join(", ", _keyValue)})";
    }
}
