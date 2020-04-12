// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class StringValueGeneratorTest
    {
        [ConditionalFact]
        public void Creates_GUID_strings()
        {
            var generator = new StringValueGenerator();

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = generator.Next(null);

                values.Add(Guid.Parse(generatedValue));
            }

            Assert.Equal(100, values.Count);
        }

        [ConditionalFact]
        public void Generates_non_temp_values()
        {
            Assert.False(new StringValueGenerator().GeneratesTemporaryValues);
        }
    }
}
