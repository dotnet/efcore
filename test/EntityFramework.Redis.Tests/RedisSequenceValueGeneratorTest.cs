// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisSequenceValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();

        [Fact]
        public void Generates_sequential_values()
        {
            var stateEntry = CreateStateEntry();

            var property = stateEntry.EntityType.GetProperty("Id");
            var sequenceName = RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
            const int blockSize = 1;

            var intProperty = stateEntry.EntityType.GetProperty("Id");
            var longProperty = stateEntry.EntityType.GetProperty("Long");
            var shortProperty = stateEntry.EntityType.GetProperty("Short");
            var byteProperty = stateEntry.EntityType.GetProperty("Byte");
            var uintProperty = stateEntry.EntityType.GetProperty("UnsignedInt");
            var ulongProperty = stateEntry.EntityType.GetProperty("UnsignedLong");
            var ushortProperty = stateEntry.EntityType.GetProperty("UnsignedShort");
            var sbyteProperty = stateEntry.EntityType.GetProperty("SignedByte");

            var generator = new RedisSequenceValueGenerator(sequenceName, blockSize);

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
            var stateEntry = CreateStateEntry();
            var property = stateEntry.EntityType.GetProperty("Id");
            var sequenceName = RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
            const int blockSize = 1;

            var intProperty = stateEntry.EntityType.GetProperty("Id");
            var longProperty = stateEntry.EntityType.GetProperty("Long");
            var shortProperty = stateEntry.EntityType.GetProperty("Short");
            var byteProperty = stateEntry.EntityType.GetProperty("Byte");
            var uintProperty = stateEntry.EntityType.GetProperty("UnsignedInt");
            var ulongProperty = stateEntry.EntityType.GetProperty("UnsignedLong");
            var ushortProperty = stateEntry.EntityType.GetProperty("UnsignedShort");
            var sbyteProperty = stateEntry.EntityType.GetProperty("SignedByte");

            var generator = new RedisSequenceValueGenerator(sequenceName, blockSize);

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
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddRedis().ServiceCollection
                .AddScoped<RedisDatabase, FakeRedisDatabase>()
                .AddInstance(new FakeRedisSequence())
                .BuildServiceProvider();

            var generator = new RedisSequenceValueGenerator("TestSequenceName", 1);

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
                        var stateEntry = CreateStateEntry(serviceProvider);

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
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddRedis().ServiceCollection
                .AddScoped<RedisDatabase, FakeRedisDatabase>()
                .AddInstance(new FakeRedisSequence())
                .BuildServiceProvider();

            var generator = new RedisSequenceValueGenerator("TestSequenceName", 1);

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
                        var stateEntry = CreateStateEntry(serviceProvider);

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
            const int blockSize = 10;

            var generator = new RedisSequenceValueGenerator("TestSequenceName", blockSize);
            var stateEntry = CreateStateEntry();

            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Long");

            for (var l = 0L; l < 100L; l++)
            {
                generator.Next(stateEntry, property);

                Assert.Equal(l, stateEntry[property]);
                Assert.False(stateEntry.HasTemporaryValue(property));
            }
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

        private StateEntry CreateStateEntry(IServiceProvider serviceProvider = null)
        {
            serviceProvider = serviceProvider ?? new ServiceCollection()
                .AddEntityFramework()
                .AddRedis().ServiceCollection
                .AddScoped<RedisDatabase, FakeRedisDatabase>()
                .AddInstance(new FakeRedisSequence())
                .BuildServiceProvider();

            var configuration = new DbContext(
                serviceProvider,
                new DbContextOptions()
                    .UseModel(_model)
                    .UseRedis("127.0.0.1", 6375)).Configuration;

            return configuration
                .Services
                .StateEntryFactory
                .Create(_model.GetEntityType(typeof(AnEntity)), new AnEntity());
        }

        private class FakeRedisSequence
        {
            public long Value { get; set; }
        }

        private class FakeRedisDatabase : RedisDatabase
        {
            private readonly FakeRedisSequence _redisSequence;

            public FakeRedisDatabase(
                LazyRef<IModel> model,
                RedisDataStoreCreator dataStoreCreator,
                RedisConnection connection,
                FakeRedisSequence redisSequence,
                ILoggerFactory loggerFactory)
                : base(model, dataStoreCreator, connection, loggerFactory)
            {
                _redisSequence = redisSequence;
            }

            public override long GetNextGeneratedValue(IProperty property, long incrementBy, string sequenceName)
            {
                var current = _redisSequence.Value;
                _redisSequence.Value += incrementBy;
                return current;
            }

            public override Task<long> GetNextGeneratedValueAsync(
                IProperty property, long incrementBy, string sequenceName, CancellationToken cancellationToken)
            {
                return Task.FromResult(GetNextGeneratedValue(property, incrementBy, sequenceName));
            }
        }
    }
}
