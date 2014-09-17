// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
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

            var stateEntry = TestHelpers.CreateStateEntry<WithGuid>(_model, EntityState.Added);
            var property = stateEntry.EntityType.GetProperty("Id");

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                if (async)
                {
                    await sequentialGuidIdentityGenerator.NextAsync(stateEntry, property);
                }
                else
                {
                    sequentialGuidIdentityGenerator.Next(stateEntry, property);
                }

                Assert.False(stateEntry.HasTemporaryValue(property));

                values.Add((Guid)stateEntry[property]);
            }

            // Check all generated values are different--functional test checks ordering on SQL Server
            Assert.Equal(100, values.Count);
        }

        private class WithGuid
        {
            public Guid Id { get; set; }
        }
    }
}
