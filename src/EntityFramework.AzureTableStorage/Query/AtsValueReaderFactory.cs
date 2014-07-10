// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsValueReaderFactory
    {
        public virtual IValueReader Create([NotNull] IEntityType type, [NotNull] AtsNamedValueBuffer source)
        {
            Check.NotNull(type, "type");
            Check.NotNull(source, "source");

            var valueBuffer = new object[type.Properties.Count];
            foreach (var property in type.Properties)
            {
                valueBuffer[property.Index] = source.TryGet(property.ColumnName());
            }
            return new AtsObjectArrayValueReader(valueBuffer);
        }
    }
}
