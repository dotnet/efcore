// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsValueReaderFactory
    {
        public IValueReader Create(IEntityType type, AtsNamedValueBuffer source)
        {
            var valueBuffer = new object[type.Properties.Count];
            foreach (var property in type.Properties)
            {
                valueBuffer[property.Index] = source.TryGet(property.StorageName);
            }
            return new AtsObjectArrayValueReader(valueBuffer);
        }
    }
}
