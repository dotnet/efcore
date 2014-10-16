// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class AtsMetadataExtensions
    {
        public static AtsPropertyExtensions AzureTableStorage([NotNull] this Property property)
        {
            Check.NotNull(property, "property");

            return new AtsPropertyExtensions(property);
        }

        public static IAtsPropertyExtensions AzureTableStorage([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            return new ReadOnlyAtsPropertyExtensions(property);
        }

        public static AtsEntityTypeExtensions AzureTableStorage([NotNull] this EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new AtsEntityTypeExtensions(entityType);
        }

        public static IAtsEntityTypeExtensions AzureTableStorage([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new ReadOnlyAtsEntityTypeExtensions(entityType);
        }
    }
}
