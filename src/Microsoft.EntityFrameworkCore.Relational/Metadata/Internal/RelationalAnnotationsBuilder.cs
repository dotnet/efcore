// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class RelationalAnnotationsBuilder : RelationalAnnotations
    {
        public RelationalAnnotationsBuilder(
            [NotNull] InternalMetadataBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(internalBuilder.Metadata)
        {
            Check.NotNull(internalBuilder, nameof(internalBuilder));

            MetadataBuilder = internalBuilder;
            ConfigurationSource = configurationSource;
        }

        public virtual ConfigurationSource ConfigurationSource { get; }

        public virtual InternalMetadataBuilder MetadataBuilder { get; }

        public override bool SetAnnotation(
            string relationalAnnotationName,
            string providerAnnotationName,
            object value)
            => MetadataBuilder.HasAnnotation(providerAnnotationName ?? relationalAnnotationName, value, ConfigurationSource);

        public override bool CanSetAnnotation(
            string relationalAnnotationName,
            string providerAnnotationName,
            object value)
            => MetadataBuilder.CanSetAnnotation(providerAnnotationName ?? relationalAnnotationName, value, ConfigurationSource);
    }
}
