// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbAnnotationsBuilder : DocumentDbAnnotations
    {
        public DocumentDbAnnotationsBuilder(
            InternalMetadataBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(internalBuilder.Metadata)
        {
            MetadataBuilder = internalBuilder;
            ConfigurationSource = configurationSource;
        }

        public virtual ConfigurationSource ConfigurationSource { get; }
        public virtual InternalMetadataBuilder MetadataBuilder { get; }
        public override bool SetAnnotation(
            string relationalAnnotationName,
            object value)
            => MetadataBuilder.HasAnnotation(relationalAnnotationName, value, ConfigurationSource);
        public override bool CanSetAnnotation(
            string relationalAnnotationName,
            object value)
            => MetadataBuilder.CanSetAnnotation(relationalAnnotationName, value, ConfigurationSource);
    }
}
