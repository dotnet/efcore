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
    public class PropertiesSnapshot
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public PropertiesSnapshot(
            [CanBeNull] List<InternalPropertyBuilder> properties,
            [CanBeNull] List<InternalIndexBuilder> indexes,
            [CanBeNull] List<(InternalKeyBuilder, ConfigurationSource?)> keys,
            [CanBeNull] List<RelationshipSnapshot> relationships)
        {
            Properties = properties;
            Indexes = indexes;
            Keys = keys;
            Relationships = relationships;
        }

        private List<InternalPropertyBuilder> Properties { [DebuggerStepThrough] get; }
        private List<RelationshipSnapshot> Relationships { [DebuggerStepThrough] get; set; }
        private List<InternalIndexBuilder> Indexes { [DebuggerStepThrough] get; set; }
        private List<(InternalKeyBuilder, ConfigurationSource?)> Keys { [DebuggerStepThrough] get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Add([NotNull] List<RelationshipSnapshot> relationships)
        {
            if (Relationships == null)
            {
                Relationships = relationships;
            }
            else
            {
                Relationships.AddRange(relationships);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Add([NotNull] List<InternalIndexBuilder> indexes)
        {
            if (Indexes == null)
            {
                Indexes = indexes;
            }
            else
            {
                Indexes.AddRange(indexes);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Add([NotNull] List<(InternalKeyBuilder, ConfigurationSource?)> keys)
        {
            if (Keys == null)
            {
                Keys = keys;
            }
            else
            {
                Keys.AddRange(keys);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (Properties != null)
            {
                foreach (var propertyBuilder in Properties)
                {
                    propertyBuilder.Attach(entityTypeBuilder);
                }
            }

            if (Keys != null)
            {
                foreach (var detachedKeyTuple in Keys)
                {
                    detachedKeyTuple.Item1.Attach(entityTypeBuilder.Metadata.RootType().Builder, detachedKeyTuple.Item2);
                }
            }

            if (Indexes != null)
            {
                foreach (var indexBuilder in Indexes)
                {
                    var originalEntityType = indexBuilder.Metadata.DeclaringEntityType;
                    var targetEntityTypeBuilder = originalEntityType.Name == entityTypeBuilder.Metadata.Name
                        ? entityTypeBuilder
                        : originalEntityType.Builder;
                    indexBuilder.Attach(targetEntityTypeBuilder);
                }
            }

            if (Relationships != null)
            {
                foreach (var detachedRelationshipTuple in Relationships)
                {
                    detachedRelationshipTuple.Attach(entityTypeBuilder);
                }
            }
        }
    }
}
