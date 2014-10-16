// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class ETagConvention : IEntityTypeConvention
    {
        public virtual void Apply(InternalEntityBuilder entityBuilder)
        {
            entityBuilder
                .Property(typeof(string), "ETag", ConfigurationSource.Convention)
                .ConcurrencyToken(true, ConfigurationSource.Convention);
        }
    }
}
