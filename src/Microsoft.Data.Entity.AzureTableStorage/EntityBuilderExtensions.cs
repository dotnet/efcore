// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public static class EntityBuilderExtensions
    {
        public static ModelBuilder.EntityBuilder<T> UseDefaultAzureTableKey<T>([NotNull] this ModelBuilder.EntityBuilder<T> builder)
        {
            Check.NotNull(builder, "builder");

            builder.Key(new string[] { "PartitionKey", "RowKey" });
            return builder;
        }
    }
}
