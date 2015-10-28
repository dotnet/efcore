// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalIndexBuilder : InternalMetadataItemBuilder<Index>
    {
        private ConfigurationSource? _isUniqueConfigurationSource;

        public InternalIndexBuilder([NotNull] Index index, [NotNull] InternalModelBuilder modelBuilder)
            : base(index, modelBuilder)
        {
        }

        public virtual bool IsUnique(bool? isUnique, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isUniqueConfigurationSource, Metadata.IsUnique.HasValue)
                || Metadata.IsUnique.Value == isUnique)
            {
                if (_isUniqueConfigurationSource == null
                    && Metadata.IsUnique != null)
                {
                    _isUniqueConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);
                }

                Metadata.IsUnique = isUnique;
                return true;
            }

            return false;
        }

        public virtual InternalIndexBuilder Attach(ConfigurationSource configurationSource)
        {
            var entityTypeBuilder = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);
            var newIndexBuilder = entityTypeBuilder.HasIndex(Metadata.Properties.Select(p => p.Name).ToList(), configurationSource);

            newIndexBuilder.MergeAnnotationsFrom(this);

            if (_isUniqueConfigurationSource.HasValue)
            {
                newIndexBuilder.IsUnique(Metadata.IsUnique, _isUniqueConfigurationSource.Value);
            }

            return newIndexBuilder;
        }
    }
}
