// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class InternalMetadataItemBuilder<TMetadata> : InternalMetadataBuilder<TMetadata>
        where TMetadata : Annotatable
    {
        protected InternalMetadataItemBuilder([NotNull] TMetadata metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata)
        {
            ModelBuilder = modelBuilder;
        }

        public override InternalModelBuilder ModelBuilder { get; }
    }
}
