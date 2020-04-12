// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class BinaryValueGeneratorTest
    {
        [ConditionalFact]
        public void Creates_GUID_arrays()
        {
            var generator = new BinaryValueGenerator();

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = generator.Next(null);

                values.Add(new Guid(generatedValue));
            }

            Assert.Equal(100, values.Count);
        }

        [ConditionalFact]
        public void Generates_non_temp_values()
        {
            Assert.False(new BinaryValueGenerator().GeneratesTemporaryValues);
        }
    }
}
