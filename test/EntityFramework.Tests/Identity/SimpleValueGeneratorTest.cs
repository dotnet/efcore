// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
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

            var generatedValue = generator.Next(property, new LazyRef<DataStoreServices>(() => null));

            Assert.Same(generator.Property, property);

            Assert.Equal(1, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);
        }

        [Fact]
        public async Task NextAsync_delegates_to_sync_method()
        {
            var property = TestHelpers.BuildModelFor<AnEntity>().GetEntityType(typeof(AnEntity)).GetProperty("Id");

            var generator = new TestValueGenerator();

            var generatedValue = await generator.NextAsync(property, new LazyRef<DataStoreServices>(() => null));

            Assert.Same(generator.Property, property);

            Assert.Equal(1, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);
        }

        private class TestValueGenerator : SimpleValueGenerator
        {
            public IProperty Property { get; set; }

            public override GeneratedValue Next(IProperty property)
            {
                Property = property;

                return new GeneratedValue(1, isTemporary: true);
            }
        }

        private class AnEntity
        {
            public int Id { get; set; }
        }
    }
}
