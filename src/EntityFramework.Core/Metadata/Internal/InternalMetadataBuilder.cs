// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class InternalMetadataBuilder<TMetadata>
        where TMetadata : MetadataBase
    {
        private readonly TMetadata _metadata;

        private readonly LazyRef<Dictionary<string, ConfigurationSource>> _annotationSources =
            new LazyRef<Dictionary<string, ConfigurationSource>>(() => new Dictionary<string, ConfigurationSource>());

        protected InternalMetadataBuilder([NotNull] TMetadata metadata)
        {
            Check.NotNull(metadata, "metadata");

            _metadata = metadata;
        }

        public virtual bool Annotation([NotNull] string annotation, [NotNull] string value, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(annotation, "annotation");
            Check.NotEmpty(value, "value");

            var existingValue = Metadata[annotation];
            if (existingValue != null)
            {
                ConfigurationSource existingConfigurationSource;
                if (!_annotationSources.Value.TryGetValue(annotation, out existingConfigurationSource))
                {
                    existingConfigurationSource = ConfigurationSource.Explicit;
                }

                if (existingValue != value
                    && !configurationSource.Overrides(existingConfigurationSource))
                {
                    return false;
                }

                configurationSource = configurationSource.Max(existingConfigurationSource);
            }

            _annotationSources.Value[annotation] = configurationSource;
            _metadata[annotation] = value;

            return true;
        }

        public virtual TMetadata Metadata
        {
            get { return _metadata; }
        }

        public abstract InternalModelBuilder ModelBuilder { get; }
    }
}
