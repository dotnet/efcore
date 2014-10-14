// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public virtual bool IsUnique(bool isUnique, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(Metadata.IsUnique.HasValue, _isUniqueConfigurationSource))
            {
                _isUniqueConfigurationSource = configurationSource;
                Metadata.IsUnique = isUnique;
                return true;
            }

            return false;
        }
    }
}
