// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            [CanBeNull] EntityType entityType,
            [CanBeNull] PropertiesSnapshot properties,
            [CanBeNull] List<InternalIndexBuilder> indexes,
            [CanBeNull] List<(InternalKeyBuilder, ConfigurationSource?)> keys,
            [CanBeNull] List<(InternalRelationshipBuilder, EntityTypeSnapshot)> relationships)
        {
            EntityType = entityType;
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

        private EntityType EntityType { [DebuggerStepThrough] get; }
        private PropertiesSnapshot Properties { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            entityTypeBuilder.MergeAnnotationsFrom(EntityType);

            foreach (var ignoredMember in EntityType.GetIgnoredMembers())
            {
                entityTypeBuilder.Ignore(ignoredMember, EntityType.FindDeclaredIgnoredMemberConfigurationSource(ignoredMember).Value);
            }

            Properties.Attach(entityTypeBuilder);

            var seedData = EntityType.GetRawSeedData();
            if (seedData != null)
            {
                entityTypeBuilder.Metadata.AddSeedData(seedData.ToArray());
            }
        }
    }
}
