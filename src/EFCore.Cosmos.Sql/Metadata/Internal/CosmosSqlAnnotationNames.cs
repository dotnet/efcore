// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata.Internal
{
    public static class CosmosSqlAnnotationNames
    {
        public const string Prefix = "Cosmos.Sql:";
        public const string CollectionName = Prefix + "CollectionName";
        public const string DiscriminatorProperty = Prefix + "DiscriminatorProperty";
        public const string DiscriminatorValue = Prefix + "DiscriminatorValue";
    }
}
