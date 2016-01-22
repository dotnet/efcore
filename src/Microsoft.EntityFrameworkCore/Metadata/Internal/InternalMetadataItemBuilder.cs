// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public abstract class InternalMetadataItemBuilder<TMetadata> : InternalMetadataBuilder<TMetadata>
        where TMetadata : ConventionalAnnotatable
    {
        protected InternalMetadataItemBuilder([NotNull] TMetadata metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata)
        {
            ModelBuilder = modelBuilder;
        }

        public override InternalModelBuilder ModelBuilder { get; }
    }
}
