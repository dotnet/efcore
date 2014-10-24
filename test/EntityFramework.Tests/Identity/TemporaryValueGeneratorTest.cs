// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class TemporaryValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();

        [Fact]
        public async Task Creates_negative_values()
        {
            var generator = new TemporaryValueGenerator();

            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var property = stateEntry.EntityType.GetProperty("Id");

            await generator.NextAsync(stateEntry, property);

            Assert.Equal(-1, stateEntry[property]);
            Assert.True(stateEntry.HasTemporaryValue(property));

            await generator.NextAsync(stateEntry, property);

            Assert.Equal(-2, stateEntry[property]);
            Assert.True(stateEntry.HasTemporaryValue(property));

            await generator.NextAsync(stateEntry, property);

            Assert.Equal(-3, stateEntry[property]);
            Assert.True(stateEntry.HasTemporaryValue(property));

            generator.Next(stateEntry, property);

            Assert.Equal(-4, stateEntry[property]);
            Assert.True(stateEntry.HasTemporaryValue(property));

            generator.Next(stateEntry, property);

            Assert.Equal(-5, stateEntry[property]);
            Assert.True(stateEntry.HasTemporaryValue(property));

            generator.Next(stateEntry, property);

            Assert.Equal(-6, stateEntry[property]);
            Assert.True(stateEntry.HasTemporaryValue(property));
        }

        [Fact]
        public async Task Can_create_values_for_all_integer_types_except_byte()
        {
            var generator = new TemporaryValueGenerator();

            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var intProperty = stateEntry.EntityType.GetProperty("Id");
            var longProperty = stateEntry.EntityType.GetProperty("Long");
            var shortProperty = stateEntry.EntityType.GetProperty("Short");
            var nullableIntProperty = stateEntry.EntityType.GetProperty("NullableId");
            var nullableLongProperty = stateEntry.EntityType.GetProperty("NullableLong");
            var nullableShortProperty = stateEntry.EntityType.GetProperty("NullableShort");

            await generator.NextAsync(stateEntry, longProperty);

            Assert.Equal(-1L, stateEntry[longProperty]);
            Assert.True(stateEntry.HasTemporaryValue(longProperty));

            await generator.NextAsync(stateEntry, intProperty);

            Assert.Equal(-2, stateEntry[intProperty]);
            Assert.True(stateEntry.HasTemporaryValue(intProperty));

            await generator.NextAsync(stateEntry, shortProperty);

            Assert.Equal((short)-3, stateEntry[shortProperty]);
            Assert.True(stateEntry.HasTemporaryValue(shortProperty));

            generator.Next(stateEntry, longProperty);

            Assert.Equal(-4L, stateEntry[longProperty]);
            Assert.True(stateEntry.HasTemporaryValue(longProperty));

            generator.Next(stateEntry, intProperty);

            Assert.Equal(-5, stateEntry[intProperty]);
            Assert.True(stateEntry.HasTemporaryValue(intProperty));

            generator.Next(stateEntry, shortProperty);

            Assert.Equal((short)-6, stateEntry[shortProperty]);
            Assert.True(stateEntry.HasTemporaryValue(shortProperty));

            await generator.NextAsync(stateEntry, nullableLongProperty);

            Assert.Equal(-7L, stateEntry[nullableLongProperty]);
            Assert.True(stateEntry.HasTemporaryValue(nullableLongProperty));

            await generator.NextAsync(stateEntry, nullableIntProperty);

            Assert.Equal(-8, stateEntry[nullableIntProperty]);
            Assert.True(stateEntry.HasTemporaryValue(nullableIntProperty));

            await generator.NextAsync(stateEntry, nullableShortProperty);

            Assert.Equal((short)-9, stateEntry[nullableShortProperty]);
            Assert.True(stateEntry.HasTemporaryValue(nullableShortProperty));

            generator.Next(stateEntry, nullableLongProperty);

            Assert.Equal(-10L, stateEntry[nullableLongProperty]);
            Assert.True(stateEntry.HasTemporaryValue(nullableLongProperty));

            generator.Next(stateEntry, nullableIntProperty);

            Assert.Equal(-11, stateEntry[nullableIntProperty]);
            Assert.True(stateEntry.HasTemporaryValue(nullableIntProperty));

            generator.Next(stateEntry, nullableShortProperty);

            Assert.Equal((short)-12, stateEntry[nullableShortProperty]);
            Assert.True(stateEntry.HasTemporaryValue(nullableShortProperty));
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new TemporaryValueGenerator();

            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);

            Assert.Throws<OverflowException>(() => generator.Next(stateEntry, stateEntry.EntityType.GetProperty("Byte")));
            Assert.Throws<OverflowException>(() => generator.Next(stateEntry, stateEntry.EntityType.GetProperty("NullableByte")));
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
