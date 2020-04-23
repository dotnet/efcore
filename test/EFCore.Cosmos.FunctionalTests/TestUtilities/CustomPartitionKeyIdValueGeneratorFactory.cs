// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Cosmos.TestUtilities
{
    class CustomPartitionKeyIdValueGeneratorFactory : ValueGeneratorFactory
    {
        public override EntityFrameworkCore.ValueGeneration.ValueGenerator Create(IProperty property)
        {
            return new CustomPartitionKeyIdGenerator<string>();
        }
    }
}
