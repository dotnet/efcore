// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
