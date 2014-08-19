// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class InternalMetadataItemBuilder<TMetadata> : InternalMetadataBuilder<TMetadata>
        where TMetadata : MetadataBase
    {
        private readonly InternalModelBuilder _modelBuilder;

        protected InternalMetadataItemBuilder([NotNull] TMetadata metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata)
        {
            Check.NotNull(modelBuilder, "modelBuilder");

            _modelBuilder = modelBuilder;
        }

        public override InternalModelBuilder ModelBuilder
        {
            get { return _modelBuilder; }
        }
    }
}
