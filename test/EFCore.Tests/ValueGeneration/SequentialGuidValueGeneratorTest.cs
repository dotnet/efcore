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
            // After some look at SQL server 2019, it seems only the first 6 bytes are incremental, the rest seems constant.
            var orderMap = new int[] { 3, 2, 1, 0, 5, 4 };
            var sequentialGuidIdentityGenerator = new SequentialGuidValueGenerator();
            var initialGuid = sequentialGuidIdentityGenerator.Next(null).ToByteArray();
            for (var i = 1; i < 11; i++)
            {
                var expectedBytes = new byte[initialGuid.Length];
                Array.Copy(initialGuid, 0, expectedBytes, 0, expectedBytes.Length);
                var overflow = i;

                for (var idx = 0; idx < orderMap.Length; idx++)
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
        public void Third_group_always_contains_constant_ea11()
        {
            const string expected = "ea11";
            for (int i = 0; i < 100; i++)
            {
                var sequentialGuidIdentityGenerator = new SequentialGuidValueGenerator();
                var guidString = sequentialGuidIdentityGenerator.Next(null).ToString();
                var actual = guidString.Split('-')[2];
                Assert.Equal(expected, actual);
            }
        }

        [ConditionalFact]
        public void Fourth_group_contains_MSB_variant_bits()
        {
            const int expected = 0x04;
            for (int i = 0; i < 100; i++)
            {
                var sequentialGuidIdentityGenerator = new SequentialGuidValueGenerator();
                var guidString = sequentialGuidIdentityGenerator.Next(null).ToString();
                var actual = int.Parse(guidString.Split('-')[3], System.Globalization.NumberStyles.HexNumber) >> 13;
                Assert.Equal(expected, actual);
            }
        }

        // TODO: Create a test that can validate that the Node ID is always the same, this will be machine dependent, so not sure how to proceed with making the test.

        [ConditionalFact]
        public void Multiple_instances_uses_the_same_state()
        {
            var orderMap = new int[] { 3, 2, 1, 0, 5, 4 };
            var generatorA = new SequentialGuidValueGenerator();
            var initialGuid = generatorA.Next(null).ToByteArray();

            for (var i = 1; i < 11; i++)
            {
                var expectedBytes = new byte[initialGuid.Length];
                Array.Copy(initialGuid, 0, expectedBytes, 0, expectedBytes.Length);
                var overflow = i;

                for (var idx = 0; idx < orderMap.Length; idx++)
                {
                    overflow += expectedBytes[orderMap[idx]];
                    expectedBytes[orderMap[idx]] = (byte)overflow;
                    overflow >>= 8;
                }
                var expected = new Guid(expectedBytes);
                var actual = new SequentialGuidValueGenerator().Next(null);
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
