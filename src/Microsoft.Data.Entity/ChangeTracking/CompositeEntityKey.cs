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

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class CompositeEntityKey : EntityKey
    {
        private readonly object[] _keyValueParts;

        public CompositeEntityKey([NotNull] IEntityType entityType, [NotNull] object[] keyValueParts)
            : base(entityType)
        {
            Check.NotNull(keyValueParts, "keyValueParts");

            _keyValueParts = keyValueParts;
        }

        public new virtual object[] Value
        {
            get { return _keyValueParts; }
        }

        protected override object GetValue()
        {
            return _keyValueParts;
        }

        private bool Equals(CompositeEntityKey other)
        {
            return EntityType == other.EntityType
                   && _keyValueParts.SequenceEqual(other._keyValueParts);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((CompositeEntityKey)obj);
        }

        public override int GetHashCode()
        {
            return _keyValueParts.Aggregate(
                EntityType.GetHashCode() * 397,
                (t, v) => (t * 397) ^ (v != null ? v.GetHashCode() : 0));
        }
    }
}
