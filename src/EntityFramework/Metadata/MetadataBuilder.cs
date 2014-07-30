// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class MetadataBuilder<TMetadata, TMetadataBuilder>
        where TMetadata : MetadataBase
        where TMetadataBuilder : MetadataBuilder<TMetadata, TMetadataBuilder>
    {
        private readonly TMetadata _metadata;
        private readonly ModelBuilder _modelBuilder;

        internal MetadataBuilder(TMetadata metadata)
            : this(metadata, null)
        {
        }

        internal MetadataBuilder(TMetadata metadata, ModelBuilder modelBuilder)
        {
            _metadata = metadata;
            _modelBuilder = modelBuilder;
        }

        public TMetadataBuilder Annotation([NotNull] string annotation, [NotNull] string value)
        {
            Check.NotEmpty(annotation, "annotation");
            Check.NotEmpty(value, "value");

            _metadata[annotation] = value;

            return (TMetadataBuilder)this;
        }

        protected TMetadata Metadata
        {
            get { return _metadata; }
        }

        protected ModelBuilder ModelBuilder
        {
            get { return _modelBuilder; }
        }
    }
}
