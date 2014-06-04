// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
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
            if (EntityType != other.EntityType)
            {
                return false;
            }

            var parts = _keyValueParts;
            var otherParts = other._keyValueParts;

            var partCount = parts.Length;
            if (partCount != otherParts.Length)
            {
                return false;
            }

            for (var i = 0; i < partCount; i++)
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(parts[i], otherParts[i]))
                {
                    return false;
                }
            }

            return true;
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
                (t, v) => (t * 397) ^ (v != null ? StructuralComparisons.StructuralEqualityComparer.GetHashCode(v) : 0));
        }
    }
}
