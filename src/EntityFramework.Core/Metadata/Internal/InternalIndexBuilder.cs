// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalIndexBuilder : InternalMetadataItemBuilder<Index>
    {
        public InternalIndexBuilder([NotNull] Index index, [NotNull] InternalModelBuilder modelBuilder)
            : base(index, modelBuilder)
        {
        }

        public virtual bool IsUnique(bool isUnique, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource())
                || (Metadata.IsUnique == isUnique))
            {
                Metadata.SetIsUnique(isUnique, configurationSource);
                return true;
            }

            return false;
        }

        public virtual InternalIndexBuilder Attach(ConfigurationSource configurationSource)
        {
            var entityTypeBuilder = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);
            var newIndexBuilder = entityTypeBuilder.HasIndex(Metadata.Properties.Select(p => p.Name).ToList(), configurationSource);

            newIndexBuilder.MergeAnnotationsFrom(this);

            var isUniqueConfigurationSource = Metadata.GetIsUniqueConfigurationSource();
            if (isUniqueConfigurationSource.HasValue)
            {
                newIndexBuilder.IsUnique(Metadata.IsUnique, isUniqueConfigurationSource.Value);
            }

            return newIndexBuilder;
        }
    }
}
