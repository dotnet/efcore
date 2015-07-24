// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class RelationalAnnotationsBuilder : RelationalAnnotations
    {
        public RelationalAnnotationsBuilder(
            [NotNull] InternalMetadataBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] string providerPrefix)
            : base(internalBuilder.Metadata, providerPrefix)
        {
            Check.NotNull(internalBuilder, nameof(internalBuilder));

            EntityTypeBuilder = internalBuilder;
            ConfigurationSource = configurationSource;
        }

        public virtual ConfigurationSource ConfigurationSource { get; }

        public virtual InternalMetadataBuilder EntityTypeBuilder { get; }

        public override bool SetAnnotation(string annotationName, object value)
        {
            var fullName = (ProviderPrefix ?? RelationalAnnotationNames.Prefix) + annotationName;
            return EntityTypeBuilder.Annotation(fullName, value, ConfigurationSource);
        }
    }
}
