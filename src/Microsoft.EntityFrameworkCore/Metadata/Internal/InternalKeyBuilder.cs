// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    [DebuggerDisplay("{Metadata,nq}")]
    public class InternalKeyBuilder : InternalMetadataItemBuilder<Key>
    {
        public InternalKeyBuilder([NotNull] Key key, [NotNull] InternalModelBuilder modelBuilder)
            : base(key, modelBuilder)
        {
        }

        public virtual InternalKeyBuilder Attach(ConfigurationSource configurationSource)
        {
            // TODO: attach to same entity type
            // Issue #2611
            var entityTypeBuilder = Metadata.DeclaringEntityType.RootType().Builder;

            var propertyNames = Metadata.Properties.Select(p => p.Name).ToList();
            foreach (var propertyName in propertyNames)
            {
                if (entityTypeBuilder.Metadata.FindProperty(propertyName) == null)
                {
                    return null;
                }
            }

            var newKeyBuilder = entityTypeBuilder.HasKey(propertyNames, configurationSource);

            newKeyBuilder?.MergeAnnotationsFrom(this);

            return newKeyBuilder;
        }
    }
}
