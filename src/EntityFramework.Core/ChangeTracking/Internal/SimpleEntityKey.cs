// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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

        private readonly IEntityType _entityType;
        private readonly TKey _keyValue;

        public SimpleEntityKey([NotNull] IEntityType entityType, [CanBeNull] TKey keyValue)
        {
            Debug.Assert(entityType != null); // hot path

            _entityType = entityType;
            _keyValue = keyValue;
        }

        public new virtual TKey Value => _keyValue;

        protected override object GetValue()
        {
            return _keyValue;
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                   || (obj.GetType() == GetType()
                       && Equals((SimpleEntityKey<TKey>)obj));
        }

        private bool Equals(SimpleEntityKey<TKey> other)
        {
            return _entityType == other._entityType
                   && _equalityComparer.Equals(_keyValue, other._keyValue);
        }

        public override int GetHashCode()
        {
            return (_entityType.GetHashCode() * 397)
                   ^ _equalityComparer.GetHashCode(_keyValue);
        }

        [UsedImplicitly]
        private string DebuggerDisplay
            => string.Format("{0}({1})", _entityType.Name, string.Join(", ", _keyValue));
    }
}
