// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal
{
    public static class CosmosAnnotationNames
    {
        public const string Prefix = "Cosmos:";
        public const string ContainerName = Prefix + "ContainerName";
        public const string DiscriminatorProperty = Prefix + "DiscriminatorProperty";
        public const string DiscriminatorValue = Prefix + "DiscriminatorValue";
    }
}
