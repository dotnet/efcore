// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class PartitionKeyAndRowKeyConvention : IModelConvention
    {
        public virtual void Apply(EntityType entityType)
        {
            if (entityType.TryGetPrimaryKey() == null)
            {
                var partitionKey = entityType.TryGetProperty("PartitionKey");
                var rowKey = entityType.TryGetProperty("RowKey");
                if (partitionKey != null
                    && rowKey != null)
                {
                    entityType.GetOrSetPrimaryKey(new[] { partitionKey, rowKey });
                }
            }
        }
    }
}
