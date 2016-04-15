// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class BinaryValueGenerator : ValueGenerator<byte[]>
    {
        public BinaryValueGenerator(bool generateTemporaryValues)
        {
            GeneratesTemporaryValues = generateTemporaryValues;
        }

        public override bool GeneratesTemporaryValues { get; }

        public override byte[] Next() => Guid.NewGuid().ToByteArray();
    }
}
