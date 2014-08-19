// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class InternalMetadataBuilder<TMetadata>
        where TMetadata : MetadataBase
    {
        private readonly TMetadata _metadata;

        protected InternalMetadataBuilder([NotNull] TMetadata metadata)
        {
            Check.NotNull(metadata, "metadata");

            _metadata = metadata;
        }

        public virtual void Annotation([NotNull] string annotation, [NotNull] string value)
        {
            Check.NotEmpty(annotation, "annotation");
            Check.NotEmpty(value, "value");

            _metadata[annotation] = value;
        }

        public virtual TMetadata Metadata
        {
            get { return _metadata; }
        }

        public abstract InternalModelBuilder ModelBuilder { get; }
    }
}
