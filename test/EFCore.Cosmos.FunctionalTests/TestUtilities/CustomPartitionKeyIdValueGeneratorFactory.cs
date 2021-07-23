﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
