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
    public class TemporaryStringValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<WithString>();

        [Fact]
        public async Task Creates_GUID_strings()
        {
            await Creates_GUID_strings_test(async: false);
        }

        [Fact]
        public async Task Creates_GUID_strings_async()
        {
            await Creates_GUID_strings_test(async: true);
        }

        public async Task Creates_GUID_strings_test(bool async)
        {
            var generator = new TemporaryStringValueGenerator();

            var stateEntry = TestHelpers.CreateStateEntry<WithString>(_model);
            var property = stateEntry.EntityType.GetProperty("Id");

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var generatedValue = async
                    ? await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null))
                    : generator.Next(property, new DbContextService<DataStoreServices>(() => null));

                values.Add(Guid.Parse((string)generatedValue));
            }

            Assert.Equal(100, values.Count);
        }

        [Fact]
        public void Generates_temp_values()
        {
            Assert.True(new TemporaryStringValueGenerator().GeneratesTemporaryValues);
        }

        private class WithString
        {
            public string Id { get; set; }
        }
    }
}
