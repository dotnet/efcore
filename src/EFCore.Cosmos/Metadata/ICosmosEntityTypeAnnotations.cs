// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata
{
    public interface ICosmosEntityTypeAnnotations
    {
        string ContainerName { get; }
        IProperty DiscriminatorProperty { get; }
        object DiscriminatorValue { get; }
    }
}
