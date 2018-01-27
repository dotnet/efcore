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
    public class PropertiesSnapshot
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public PropertiesSnapshot(
            [CanBeNull] List<Tuple<InternalPropertyBuilder, ConfigurationSource>> properties,
            [CanBeNull] List<Tuple<InternalIndexBuilder, ConfigurationSource>> indexes,
            [CanBeNull] List<Tuple<InternalKeyBuilder, ConfigurationSource?, ConfigurationSource>> keys,
            [CanBeNull] List<Tuple<InternalRelationshipBuilder, EntityTypeSnapshot, ConfigurationSource>> relationships)
        {
            Properties = properties;
            Indexes = indexes;
            Keys = keys;
            Relationships = relationships;
        }

        private List<Tuple<InternalPropertyBuilder, ConfigurationSource>> Properties { [DebuggerStepThrough] get; }
        private List<Tuple<InternalRelationshipBuilder, EntityTypeSnapshot, ConfigurationSource>> Relationships { [DebuggerStepThrough] get; set; }
        private List<Tuple<InternalIndexBuilder, ConfigurationSource>> Indexes { [DebuggerStepThrough] get; set; }
        private List<Tuple<InternalKeyBuilder, ConfigurationSource?, ConfigurationSource>> Keys { [DebuggerStepThrough] get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Add([NotNull] List<Tuple<InternalRelationshipBuilder,EntityTypeSnapshot, ConfigurationSource>> relationships)
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
        public virtual void Add([NotNull] List<Tuple<InternalIndexBuilder, ConfigurationSource>> indexes)
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
        public virtual void Add([NotNull] List<Tuple<InternalKeyBuilder, ConfigurationSource?, ConfigurationSource>> keys)
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
                foreach (var propertyTuple in Properties)
                {
                    propertyTuple.Item1.Attach(entityTypeBuilder, propertyTuple.Item2);
                }
            }

            if (Keys != null)
            {
                foreach (var detachedKeyTuple in Keys)
                {
                    detachedKeyTuple.Item1.Attach(entityTypeBuilder.Metadata.RootType().Builder, detachedKeyTuple.Item2, detachedKeyTuple.Item3);
                }
            }

            if (Indexes != null)
            {
                foreach (var indexBuilderTuple in Indexes)
                {
                    var originalEntityType = indexBuilderTuple.Item1.Metadata.DeclaringEntityType;
                    var targetEntityTypeBuilder = originalEntityType.Name == entityTypeBuilder.Metadata.Name
                        ? entityTypeBuilder
                        : originalEntityType.Builder;
                    indexBuilderTuple.Item1.Attach(targetEntityTypeBuilder, indexBuilderTuple.Item2);
                }
            }

            if (Relationships != null)
            {
                foreach (var detachedRelationshipTuple in Relationships.Where(r => r.Item2 != null))
                {
                    var newRelationship = detachedRelationshipTuple.Item1.Attach(entityTypeBuilder, detachedRelationshipTuple.Item3);
                    if (newRelationship != null)
                    {
                        detachedRelationshipTuple.Item2.Attach(
                            newRelationship.Metadata.ResolveOtherEntityType(entityTypeBuilder.Metadata).Builder);
                    }
                }
                foreach (var detachedRelationshipTuple in Relationships.Where(r => r.Item2 == null))
                {
                    detachedRelationshipTuple.Item1.Attach(entityTypeBuilder, detachedRelationshipTuple.Item3);
                }
            }
        }
    }
}
