// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class GuidValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<WithGuid>();

        [Fact]
        public async Task Creates_GUIDs()
        {
            await Creates_GUIDs_test(async: false);
        }

        [Fact]
        public async Task Creates_GUIDs_async()
        {
            await Creates_GUIDs_test(async: true);
        }

        public async Task Creates_GUIDs_test(bool async)
        {
            var sequentialGuidIdentityGenerator = new GuidValueGenerator();

            var stateEntry = TestHelpers.CreateStateEntry<WithGuid>(_model);
            var property = stateEntry.EntityType.GetProperty("Id");

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = async 
                    ? await sequentialGuidIdentityGenerator.NextAsync(property, new DbContextService<DataStoreServices>(() => null))
                    : sequentialGuidIdentityGenerator.Next(property, new DbContextService<DataStoreServices>(() => null));

                values.Add((Guid)generatedValue);
            }

            Assert.Equal(100, values.Count);
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new GuidValueGenerator().GeneratesTemporaryValues);
        }

        private class WithGuid
        {
            public Guid Id { get; set; }
        }
    }
}
