// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class RelationshipsSnapshot : ArraySidecar
    {
        public RelationshipsSnapshot([NotNull] InternalEntityEntry entry)
            : base(entry, entry.EntityType.RelationshipPropertyCount())
        {
        }

        protected override int Index(IPropertyBase property) => property.GetRelationshipIndex();

        protected override void ThrowInvalidIndexException(IPropertyBase property)
        {
            throw new InvalidOperationException();
        }

        protected override object CopyValueFromEntry(IPropertyBase property)
        {
            var value = base.CopyValueFromEntry(property);

            var navigation = property as INavigation;
            if (value == null
                || navigation == null
                || !navigation.IsCollection())
            {
                return value;
            }

            // TODO: Perf: Consider updating the snapshot with what has changed rather than making a new snapshot every time.
            // TODO: This may need to be strongly typed to entity type--not just object
            var snapshot = new HashSet<object>(ReferenceEqualityComparer.Instance);

            foreach (var entity in (IEnumerable)value)
            {
                snapshot.Add(entity);
            }

            return snapshot;
        }

        public override string Name => WellKnownNames.RelationshipsSnapshot;

        public override bool TransparentRead => false;

        public override bool TransparentWrite => false;

        public override bool AutoCommit => false;
    }
}
