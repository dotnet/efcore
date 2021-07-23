// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class TemporaryBinaryValueGeneratorTest
    {
        [ConditionalFact]
        public void Creates_GUID_arrays()
        {
            var generator = new TemporaryBinaryValueGenerator();

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = generator.Next(null);

                values.Add(new Guid(generatedValue));
            }

            Assert.Equal(100, values.Count);
        }

        [ConditionalFact]
        public void Generates_temp_values()
        {
            Assert.True(new TemporaryBinaryValueGenerator().GeneratesTemporaryValues);
        }
    }
}
