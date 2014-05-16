// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SequentialGuidValueGeneratorTest
    {
        [Fact]
        public async Task Can_get_next_values()
        {
            await Can_get_next_values_test(async: false);
        }

        [Fact]
        public async Task Can_get_next_values_async()
        {
            await Can_get_next_values_test(async: true);
        }

        public async Task Can_get_next_values_test(bool async)
        {
            var sequentialGuidIdentityGenerator = new SequentialGuidValueGenerator();

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var guid = async
                    ? await sequentialGuidIdentityGenerator.NextAsync(Mock.Of<DbContextConfiguration>(), Mock.Of<IProperty>())
                    : sequentialGuidIdentityGenerator.Next(Mock.Of<DbContextConfiguration>(), Mock.Of<IProperty>());

                values.Add((Guid)guid);
            }

            // Check all generated values are different--functional test checks ordering on SQL Server
            Assert.Equal(100, values.Count);
        }
    }
}
