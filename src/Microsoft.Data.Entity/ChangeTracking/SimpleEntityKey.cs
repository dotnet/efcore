// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // This is generic to avoid boxing key values to reduce heap memory usage for keys in the change tracker.
    public class SimpleEntityKey<TEntity, TKey> : EntityKey
    {
        private readonly TKey _keyValue;

        public SimpleEntityKey([CanBeNull] TKey keyValue)
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

        private bool Equals(SimpleEntityKey<TEntity, TKey> other)
        {
            return EqualityComparer<TKey>.Default.Equals(_keyValue, other._keyValue);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((SimpleEntityKey<TEntity, TKey>)obj);
        }

        public override int GetHashCode()
        {
            return (typeof(TEntity).GetHashCode() * 397)
                   ^ EqualityComparer<TKey>.Default.GetHashCode(_keyValue);
        }
    }
}
