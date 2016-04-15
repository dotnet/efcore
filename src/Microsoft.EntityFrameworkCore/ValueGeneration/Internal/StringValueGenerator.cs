// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class StringValueGenerator : ValueGenerator<string>
    {
        public StringValueGenerator(bool generateTemporaryValues)
        {
            GeneratesTemporaryValues = generateTemporaryValues;
        }

        public override bool GeneratesTemporaryValues { get; }

        public override string Next() => Guid.NewGuid().ToString();
    }
}
