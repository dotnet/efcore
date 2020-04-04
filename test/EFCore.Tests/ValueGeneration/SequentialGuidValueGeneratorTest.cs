// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class SequentialGuidValueGeneratorTest
    {
        [ConditionalFact]
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

        [ConditionalFact]
        public void Can_generates_sequential_values()
        {
            int[] orderMap = new int[] { 3, 2, 1, 0, 5, 4, 7, 6, 9, 8, 15, 14, 13, 12, 11, 10 };
            var sequentialGuidIdentityGenerator = new SequentialGuidValueGenerator();
            byte[] initialGuid = sequentialGuidIdentityGenerator.Next(null).ToByteArray();
            for (var i = 1; i < 11; i++)
            {
                byte[] expectedBytes = new byte[initialGuid.Length];
                Array.Copy(initialGuid, 0, expectedBytes, 0, expectedBytes.Length);
                int overflow = i;
                // Slow and steady, increment in order as long as the overflow is greater than one.
                for (int idx = 0; idx < initialGuid.Length && overflow > 0; idx++)
                {
                    overflow += expectedBytes[orderMap[idx]];
                    expectedBytes[orderMap[idx]] = (byte)overflow;
                    overflow >>= 8;
                }
                var expected = new Guid(expectedBytes);
                var actual = sequentialGuidIdentityGenerator.Next(null);
                Assert.Equal(expected, actual);
            }
        }

        [ConditionalFact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new SequentialGuidValueGenerator().GeneratesTemporaryValues);
        }
    }
}
