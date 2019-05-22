// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class PropertiesSnapshot
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
