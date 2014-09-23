// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

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

        public virtual void GenerateValuesOnAdd(bool generateValues = true)
        {
            Metadata.ValueGeneration = generateValues ? ValueGeneration.OnAdd : ValueGeneration.None;
        }

        public virtual void StoreComputed(bool computed = true)
        {
            Metadata.ValueGeneration = computed ? ValueGeneration.OnAddAndUpdate : ValueGeneration.None;
        }
    }
}
