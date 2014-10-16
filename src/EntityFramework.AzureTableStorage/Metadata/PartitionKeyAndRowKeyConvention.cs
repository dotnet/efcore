// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class PartitionKeyAndRowKeyConvention : IEntityTypeConvention
    {
        public virtual void Apply(InternalEntityBuilder entityBuilder)
        {
            var entityType = entityBuilder.Metadata;
            if (entityType.TryGetPrimaryKey() == null)
            {
                var partitionKey = entityType.TryGetProperty("PartitionKey");
                var rowKey = entityType.TryGetProperty("RowKey");
                if (partitionKey != null
                    && rowKey != null)
                {
                    entityBuilder.Key(new[] { partitionKey.Name, rowKey.Name }, ConfigurationSource.Convention);
                }
            }
        }
    }
}
