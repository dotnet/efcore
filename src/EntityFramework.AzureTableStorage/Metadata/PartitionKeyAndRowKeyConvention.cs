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
            if (entityType.TryGetKey() == null)
            {
                var pk = entityType.TryGetProperty("PartitionKey");
                var rk = entityType.TryGetProperty("RowKey");
                if (pk != null
                    && rk != null)
                {
                    entityType.SetKey(pk, rk);
                }
            }
        }
    }
}
