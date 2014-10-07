// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisSequenceValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();

        [Fact]
        public void Generates_sequential_values()
        {
            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var property = stateEntry.EntityType.GetProperty("Id");
            var sequenceName = RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
            var blockSize = 1;
            var incrementingValue = 0L;
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            redisDatabaseMock
                .Setup(db => db.GetNextGeneratedValue(It.IsAny<IProperty>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns<IProperty, long, string>((p, l, s) =>
                {
                    var originalValue = incrementingValue;
                    incrementingValue += l;
                    return originalValue;
                });

            var intProperty = stateEntry.EntityType.GetProperty("Id");
            var longProperty = stateEntry.EntityType.GetProperty("Long");
            var shortProperty = stateEntry.EntityType.GetProperty("Short");
            var byteProperty = stateEntry.EntityType.GetProperty("Byte");
            var uintProperty = stateEntry.EntityType.GetProperty("UnsignedInt");
            var ulongProperty = stateEntry.EntityType.GetProperty("UnsignedLong");
            var ushortProperty = stateEntry.EntityType.GetProperty("UnsignedShort");
            var sbyteProperty = stateEntry.EntityType.GetProperty("SignedByte");

            var generator = new RedisSequenceValueGenerator(redisDatabaseMock.Object, sequenceName, blockSize);

            for (var i = 0; i < 15; i++)
            {
                generator.Next(stateEntry, intProperty);

                Assert.Equal(i, stateEntry[intProperty]);
                Assert.False(stateEntry.HasTemporaryValue(intProperty));
            }

            for (var i = 15; i < 30; i++)
            {
                generator.Next(stateEntry, longProperty);

                Assert.Equal((long)i, stateEntry[longProperty]);
                Assert.False(stateEntry.HasTemporaryValue(longProperty));
            }

            for (var i = 30; i < 45; i++)
            {
                generator.Next(stateEntry, shortProperty);

                Assert.Equal((short)i, stateEntry[shortProperty]);
                Assert.False(stateEntry.HasTemporaryValue(shortProperty));
            }

            for (var i = 45; i < 60; i++)
            {
                generator.Next(stateEntry, byteProperty);

                Assert.Equal((byte)i, stateEntry[byteProperty]);
                Assert.False(stateEntry.HasTemporaryValue(byteProperty));
            }

            for (var i = 60; i < 75; i++)
            {
                generator.Next(stateEntry, uintProperty);

                Assert.Equal((uint)i, stateEntry[uintProperty]);
                Assert.False(stateEntry.HasTemporaryValue(uintProperty));
            }

            for (var i = 75; i < 90; i++)
            {
                generator.Next(stateEntry, ulongProperty);

                Assert.Equal((ulong)i, stateEntry[ulongProperty]);
                Assert.False(stateEntry.HasTemporaryValue(ulongProperty));
            }

            for (var i = 90; i < 105; i++)
            {
                generator.Next(stateEntry, ushortProperty);

                Assert.Equal((ushort)i, stateEntry[ushortProperty]);
                Assert.False(stateEntry.HasTemporaryValue(ushortProperty));
            }

            for (var i = 105; i < 120; i++)
            {
                generator.Next(stateEntry, sbyteProperty);

                Assert.Equal((sbyte)i, stateEntry[sbyteProperty]);
                Assert.False(stateEntry.HasTemporaryValue(sbyteProperty));
            }
        }

        [Fact]
        public async Task Generates_sequential_values_async()
        {
            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var property = stateEntry.EntityType.GetProperty("Id");
            var sequenceName = RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
            var blockSize = 1;
            var incrementingValue = 0L;
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            redisDatabaseMock
                .Setup(db => db.GetNextGeneratedValueAsync(
                    It.IsAny<IProperty>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<IProperty, long, string, CancellationToken>((p, l, s, c) =>
                {
                    var originalValue = incrementingValue;
                    incrementingValue += l;
                    return Task.FromResult(originalValue);
                });

            var intProperty = stateEntry.EntityType.GetProperty("Id");
            var longProperty = stateEntry.EntityType.GetProperty("Long");
            var shortProperty = stateEntry.EntityType.GetProperty("Short");
            var byteProperty = stateEntry.EntityType.GetProperty("Byte");
            var uintProperty = stateEntry.EntityType.GetProperty("UnsignedInt");
            var ulongProperty = stateEntry.EntityType.GetProperty("UnsignedLong");
            var ushortProperty = stateEntry.EntityType.GetProperty("UnsignedShort");
            var sbyteProperty = stateEntry.EntityType.GetProperty("SignedByte");

            var generator = new RedisSequenceValueGenerator(redisDatabaseMock.Object, sequenceName, blockSize);

            for (var i = 0; i < 15; i++)
            {
                await generator.NextAsync(stateEntry, intProperty);

                Assert.Equal(i, stateEntry[intProperty]);
                Assert.False(stateEntry.HasTemporaryValue(intProperty));
            }

            for (var i = 15; i < 30; i++)
            {
                await generator.NextAsync(stateEntry, longProperty);

                Assert.Equal((long)i, stateEntry[longProperty]);
                Assert.False(stateEntry.HasTemporaryValue(longProperty));
            }

            for (var i = 30; i < 45; i++)
            {
                await generator.NextAsync(stateEntry, shortProperty);

                Assert.Equal((short)i, stateEntry[shortProperty]);
                Assert.False(stateEntry.HasTemporaryValue(shortProperty));
            }

            for (var i = 45; i < 60; i++)
            {
                await generator.NextAsync(stateEntry, byteProperty);

                Assert.Equal((byte)i, stateEntry[byteProperty]);
                Assert.False(stateEntry.HasTemporaryValue(byteProperty));
            }

            for (var i = 60; i < 75; i++)
            {
                await generator.NextAsync(stateEntry, uintProperty);

                Assert.Equal((uint)i, stateEntry[uintProperty]);
                Assert.False(stateEntry.HasTemporaryValue(uintProperty));
            }

            for (var i = 75; i < 90; i++)
            {
                await generator.NextAsync(stateEntry, ulongProperty);

                Assert.Equal((ulong)i, stateEntry[ulongProperty]);
                Assert.False(stateEntry.HasTemporaryValue(ulongProperty));
            }

            for (var i = 90; i < 105; i++)
            {
                await generator.NextAsync(stateEntry, ushortProperty);

                Assert.Equal((ushort)i, stateEntry[ushortProperty]);
                Assert.False(stateEntry.HasTemporaryValue(ushortProperty));
            }

            for (var i = 105; i < 120; i++)
            {
                await generator.NextAsync(stateEntry, sbyteProperty);

                Assert.Equal((sbyte)i, stateEntry[sbyteProperty]);
                Assert.False(stateEntry.HasTemporaryValue(sbyteProperty));
            }
        }

        [Fact]
        public void Multiple_threads_can_use_the_same_generator()
        {
            var incrementingValue = 0L;
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            redisDatabaseMock
                .Setup(db => db.GetNextGeneratedValue(It.IsAny<IProperty>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns<IProperty, long, string>((p, l, s) =>
                {
                    var originalValue = incrementingValue;
                    incrementingValue += l;
                    return originalValue;
                });

            var generator = new RedisSequenceValueGenerator(redisDatabaseMock.Object, "TestSequenceName", 1);

            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Long");

            const int threadCount = 50;
            const int valueCount = 35;

            var tests = new Action[threadCount];
            var generatedValues = new List<long>[threadCount];
            for (var i = 0; i < tests.Length; i++)
            {
                var testNumber = i;
                generatedValues[testNumber] = new List<long>();
                tests[testNumber] = () =>
                {
                    var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);

                    for (var j = 0; j < valueCount; j++)
                    {
                        generator.Next(stateEntry, property);

                        generatedValues[testNumber].Add((long)stateEntry[property]);
                    }
                };
            }

            Parallel.Invoke(tests);

            // Check that each value was generated once and only once
            var checks = new bool[threadCount * valueCount];
            foreach (var values in generatedValues)
            {
                Assert.Equal(valueCount, values.Count);
                foreach (var value in values)
                {
                    checks[value] = true;
                }
            }

            Assert.True(checks.All(c => c));
        }

        [Fact]
        public async Task Multiple_threads_can_use_the_same_generator_async()
        {
            var incrementingValue = 0L;
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            redisDatabaseMock
                .Setup(db => db.GetNextGeneratedValueAsync(
                    It.IsAny<IProperty>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<IProperty, long, string, CancellationToken>((p, l, s, c) =>
                {
                    var originalValue = incrementingValue;
                    incrementingValue += l;
                    return Task.FromResult(originalValue);
                });

            var generator = new RedisSequenceValueGenerator(redisDatabaseMock.Object, "TestSequenceName", 1);

            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Long");

            const int threadCount = 50;
            const int valueCount = 35;

            var tests = new Func<Task>[threadCount];
            var generatedValues = new List<long>[threadCount];
            for (var i = 0; i < tests.Length; i++)
            {
                var testNumber = i;
                generatedValues[testNumber] = new List<long>();
                tests[testNumber] = async () =>
                {
                    var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);

                    for (var j = 0; j < valueCount; j++)
                    {
                        await generator.NextAsync(stateEntry, property);

                        generatedValues[testNumber].Add((long)stateEntry[property]);
                    }
                };
            }

            var tasks = tests.Select(Task.Run).ToArray();

            foreach (var t in tasks)
            {
                await t;
            }

            // Check that each value was generated once and only once
            var checks = new bool[threadCount * valueCount];
            foreach (var values in generatedValues)
            {
                Assert.Equal(valueCount, values.Count);
                foreach (var value in values)
                {
                    checks[value] = true;
                }
            }

            Assert.True(checks.All(c => c));
        }

        [Fact]
        public void Generates_sequential_values_with_larger_block_size()
        {
            var blockSize = 10;
            var incrementingValue = 0L;
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            redisDatabaseMock
                .Setup(db => db.GetNextGeneratedValue(It.IsAny<IProperty>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns<IProperty, long, string>((p, l, s) =>
                {
                    var originalValue = incrementingValue;
                    incrementingValue += l;
                    return originalValue;
                });

            var generator = new RedisSequenceValueGenerator(redisDatabaseMock.Object, "TestSequenceName", blockSize);
            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);

            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Long");

            for (var l = 0L; l < 100L; l++)
            {
                generator.Next(stateEntry, property);

                Assert.Equal(l, stateEntry[property]);
                Assert.False(stateEntry.HasTemporaryValue(property));
            }
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var incrementingValue = 256L;
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            redisDatabaseMock
                .Setup(db => db.GetNextGeneratedValue(It.IsAny<IProperty>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns<IProperty, long, string>((p, l, s) => incrementingValue);

            var generator = new RedisSequenceValueGenerator(redisDatabaseMock.Object, "MyTestSequenceName", 1);

            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var byteProperty = stateEntry.EntityType.GetProperty("Byte");

            Assert.Throws<OverflowException>(() => generator.Next(stateEntry, byteProperty));
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public uint UnsignedInt { get; set; }
            public ulong UnsignedLong { get; set; }
            public ushort UnsignedShort { get; set; }
            public sbyte SignedByte { get; set; }
        }
    }
}
