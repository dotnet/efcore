// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityTypeSnapshot
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityTypeSnapshot(
            [CanBeNull] List<Tuple<string, ConfigurationSource>> ignoredMembers,
            [CanBeNull] IReadOnlyCollection<object> seedData,
            [CanBeNull] PropertiesSnapshot properties,
            [CanBeNull] List<Tuple<InternalIndexBuilder, ConfigurationSource>> indexes,
            [CanBeNull] List<Tuple<InternalKeyBuilder, ConfigurationSource?, ConfigurationSource>> keys,
            [CanBeNull] List<Tuple<InternalRelationshipBuilder, EntityTypeSnapshot, ConfigurationSource>> relationships)
        {
            IgnoredMembers = ignoredMembers;
            SeedData = seedData;
            Properties = properties ?? new PropertiesSnapshot(null, null, null, null);
            if (indexes != null)
            {
                Properties.Add(indexes);
            }
            if (keys != null)
            {
                Properties.Add(keys);
            }
            if (relationships != null)
            {
                Properties.Add(relationships);
            }
        }

        private IReadOnlyList<Tuple<string, ConfigurationSource>> IgnoredMembers { [DebuggerStepThrough] get; }
        private IReadOnlyCollection<object> SeedData { [DebuggerStepThrough] get; }
        private PropertiesSnapshot Properties { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (IgnoredMembers != null)
            {
                foreach (var ignoredTuple in IgnoredMembers)
                {
                    entityTypeBuilder.Ignore(ignoredTuple.Item1, ignoredTuple.Item2);
                }
            }

            Properties.Attach(entityTypeBuilder);

            if (SeedData != null)
            {
                entityTypeBuilder.Metadata.AddSeedData(SeedData.ToArray());
            }
        }
    }
}
