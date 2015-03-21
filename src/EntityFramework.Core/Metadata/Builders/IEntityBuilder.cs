// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Builders
{
    public interface IEntityBuilder<out TMetadataBuilder> : IMetadataBuilder<EntityType, TMetadataBuilder>
        where TMetadataBuilder : IMetadataBuilder<EntityType, TMetadataBuilder>
    {
    }
}
