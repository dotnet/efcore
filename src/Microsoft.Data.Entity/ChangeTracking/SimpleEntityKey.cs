// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class SimpleEntityKey<TKey> : EntityKey
    {
        private readonly TKey _keyValue;

        public SimpleEntityKey([NotNull] IEntityType entityType, [CanBeNull] TKey keyValue)
            : base(entityType)
        {
            _keyValue = keyValue;
        }

        public new virtual TKey Value
        {
            get { return _keyValue; }
        }

        protected override object GetValue()
        {
            return _keyValue;
        }

        private bool Equals(SimpleEntityKey<TKey> other)
        {
            return EntityType == other.EntityType
                   && EqualityComparer<TKey>.Default.Equals(_keyValue, other._keyValue);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((SimpleEntityKey<TKey>)obj);
        }

        public override int GetHashCode()
        {
            return (EntityType.GetHashCode() * 397)
                   ^ EqualityComparer<TKey>.Default.GetHashCode(_keyValue);
        }
    }
}
