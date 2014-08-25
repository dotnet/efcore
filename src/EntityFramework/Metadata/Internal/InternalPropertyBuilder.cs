// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalPropertyBuilder : InternalMetadataItemBuilder<Property>
    {
        public InternalPropertyBuilder([NotNull] Property property, [NotNull] InternalModelBuilder modelBuilder)
            : base(property, modelBuilder)
        {
        }

        public virtual void Required(bool isRequired = true)
        {
            Metadata.IsNullable = !isRequired;
        }

        public virtual void ConcurrencyToken(bool isConcurrencyToken = true)
        {
            Metadata.IsConcurrencyToken = isConcurrencyToken;
        }

        public virtual void Shadow(bool isShadowProperty = true)
        {
            Metadata.IsShadowProperty = isShadowProperty;
        }

        // TODO Consider if this should be relational only
        public virtual void UseStoreSequence()
        {
            Metadata.ValueGenerationOnAdd = ValueGenerationOnAdd.Server;
            Metadata.ValueGenerationOnSave = ValueGenerationOnSave.None;
        }

        // TODO Consider if this should be relational only
        public virtual void UseStoreSequence([NotNull] string sequenceName, int blockSize)
        {
            Check.NotEmpty(sequenceName, "sequenceName");

            UseStoreSequence();

            // TODO: Make these constants in some class once decided if this should be relational-only
            Metadata["StoreSequenceName"] = sequenceName;
            Metadata["StoreSequenceBlockSize"] = blockSize.ToString();
        }
    }
}
