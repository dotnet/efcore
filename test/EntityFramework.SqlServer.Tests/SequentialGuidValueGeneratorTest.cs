// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SequentialGuidValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<WithGuid>();

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

            var property = _model.GetEntityType(typeof(WithGuid)).GetProperty("Id");

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = async
                    ? await sequentialGuidIdentityGenerator.NextAsync(property, new DbContextService<DataStoreServices>(() => null))
                    : sequentialGuidIdentityGenerator.Next(property, new DbContextService<DataStoreServices>(() => null));

                values.Add((Guid)generatedValue);
            }

            // Check all generated values are different--functional test checks ordering on SQL Server
            Assert.Equal(100, values.Count);
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new SequentialGuidValueGenerator().GeneratesTemporaryValues);
        }


        private class WithGuid
        {
            public Guid Id { get; set; }
        }
    }
}
