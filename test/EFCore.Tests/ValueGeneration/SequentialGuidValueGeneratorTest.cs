// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class SequentialGuidValueGeneratorTest
    {
        [Fact]
        public void Can_get_next_values()
        {
            var sequentialGuidIdentityGenerator = new SequentialGuidValueGenerator();

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = sequentialGuidIdentityGenerator.Next(null);

                values.Add(generatedValue);
            }

            // Check all generated values are different--functional test checks ordering on SQL Server
            Assert.Equal(100, values.Count);
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new SequentialGuidValueGenerator().GeneratesTemporaryValues);
        }
    }
}
