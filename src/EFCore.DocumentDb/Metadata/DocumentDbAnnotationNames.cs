// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public static class DocumentDbAnnotationNames
    {
        public const string Prefix = "DocumentDb:";
        public const string DiscriminatorProperty = Prefix + "DiscriminatorProperty";
        public const string DiscriminatorValue = Prefix + "DiscriminatorValue";
        public const string CollectionName = Prefix + "CollectionName";
    }
}
