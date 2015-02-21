// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class InternalMetadataItemBuilder<TMetadata> : InternalMetadataBuilder<TMetadata>
        where TMetadata : MetadataBase
    {
        protected InternalMetadataItemBuilder([NotNull] TMetadata metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            ModelBuilder = modelBuilder;
        }

        public override InternalModelBuilder ModelBuilder { get; }
    }
}
