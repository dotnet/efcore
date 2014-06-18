// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // TODO: Consider using ArraySidecar with pre-defined indexes
    public class RelationshipsSnapshot : DictionarySidecar
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RelationshipsSnapshot()
        {
        }

        public RelationshipsSnapshot([NotNull] StateEntry stateEntry)
            : base(stateEntry, GetProperties(Check.NotNull(stateEntry, "stateEntry")))
        {
        }

        private static IEnumerable<IPropertyBase> GetProperties(StateEntry stateEntry)
        {
            var entityType = stateEntry.EntityType;

            return entityType.ForeignKeys.SelectMany(fk => fk.Properties).Distinct()
                .Concat<IPropertyBase>(entityType.Navigations);
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

        public override string Name
        {
            get { return WellKnownNames.RelationshipsSnapshot; }
        }

        public override bool TransparentRead
        {
            get { return false; }
        }

        public override bool TransparentWrite
        {
            get { return false; }
        }

        public override bool AutoCommit
        {
            get { return false; }
        }
    }
}
