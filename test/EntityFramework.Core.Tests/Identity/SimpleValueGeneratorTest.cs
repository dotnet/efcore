// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class SimpleValueGeneratorTest
    {
        [Fact]
        public void Next_with_services_delegates_to_non_services_method()
        {
            var property = TestHelpers.BuildModelFor<AnEntity>().GetEntityType(typeof(AnEntity)).GetProperty("Id");

            var generator = new TestValueGenerator();

            var generatedValue = generator.Next(property, new DbContextService<DataStoreServices>(() => null));

            Assert.Same(generator.Property, property);

            Assert.Equal(1, generatedValue);
            Assert.True(generator.GeneratesTemporaryValues);
        }

        [Fact]
        public async Task NextAsync_delegates_to_sync_method()
        {
            var property = TestHelpers.BuildModelFor<AnEntity>().GetEntityType(typeof(AnEntity)).GetProperty("Id");

            var generator = new TestValueGenerator();

            var generatedValue = await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null));

            Assert.Same(generator.Property, property);

            Assert.Equal(1, generatedValue);
            Assert.True(generator.GeneratesTemporaryValues);
        }

        private class TestValueGenerator : SimpleValueGenerator
        {
            public IProperty Property { get; set; }

            public override object Next(IProperty property)
            {
                Property = property;

                return 1;
            }

            public override bool GeneratesTemporaryValues => true;
        }

        private class AnEntity
        {
            public int Id { get; set; }
        }
    }
}
