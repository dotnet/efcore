// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    internal class CustomPartitionKeyIdValueGeneratorFactory : ValueGeneratorFactory
    {
        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            return new CustomPartitionKeyIdGenerator<string>();
        }
    }
}
