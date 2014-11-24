// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class TemporaryValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();

        [Fact]
        public async Task Creates_negative_values()
        {
            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Id");

            var generator = new TemporaryValueGenerator();

            var generatedValue = await generator.NextAsync(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-1, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-2, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-3, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-4, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-5, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-6, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);
        }

        [Fact]
        public async Task Can_create_values_for_all_integer_types_except_byte()
        {
            var entityType = _model.GetEntityType(typeof(AnEntity));

            var intProperty = entityType.GetProperty("Id");
            var longProperty = entityType.GetProperty("Long");
            var shortProperty = entityType.GetProperty("Short");
            var nullableIntProperty = entityType.GetProperty("NullableId");
            var nullableLongProperty = entityType.GetProperty("NullableLong");
            var nullableShortProperty = entityType.GetProperty("NullableShort");

            var generator = new TemporaryValueGenerator();
            
            var generatedValue = await generator.NextAsync(longProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-1L, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(intProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-2, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(shortProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((short)-3, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(longProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-4L, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(intProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-5, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(shortProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((short)-6, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(nullableLongProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-7L, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(nullableIntProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-8, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(nullableShortProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((short)-9, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(nullableLongProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-10L, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(nullableIntProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(-11, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(nullableShortProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((short)-12, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var entityType = _model.GetEntityType(typeof(AnEntity));

            var generator = new TemporaryValueGenerator();

            Assert.Throws<OverflowException>(() => generator.Next(entityType.GetProperty("Byte")));
            Assert.Throws<OverflowException>(() => generator.Next(entityType.GetProperty("NullableByte")));
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public int? NullableId { get; set; }
            public long? NullableLong { get; set; }
            public short? NullableShort { get; set; }
            public byte? NullableByte { get; set; }
        }
    }
}
