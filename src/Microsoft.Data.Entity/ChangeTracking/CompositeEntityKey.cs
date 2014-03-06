// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class CompositeEntityKey : EntityKey
    {
        private readonly object[] _keyValueParts;

        public CompositeEntityKey([NotNull] IEntityType entityType, [CanBeNull] object[] keyValueParts)
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
