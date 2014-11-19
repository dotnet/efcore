// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Data.Entity.Storage;
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
            var storeServices = CreateStoreServices();
            var entityType = _model.GetEntityType(typeof(AnEntity));
            var property = entityType.GetProperty("Id");
            var sequenceName = RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
            const int blockSize = 1;

            var intProperty = entityType.GetProperty("Id");
            var longProperty = entityType.GetProperty("Long");
            var shortProperty = entityType.GetProperty("Short");
            var byteProperty = entityType.GetProperty("Byte");
            var uintProperty = entityType.GetProperty("UnsignedInt");
            var ulongProperty = entityType.GetProperty("UnsignedLong");
            var ushortProperty = entityType.GetProperty("UnsignedShort");
            var sbyteProperty = entityType.GetProperty("SignedByte");

            var generator = new RedisSequenceValueGenerator(sequenceName, blockSize);

            for (var i = 0; i < 15; i++)
            {
                var generatedValue = generator.Next(intProperty, storeServices);

                Assert.Equal(i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 15; i < 30; i++)
            {
                var generatedValue = generator.Next(longProperty, storeServices);

                Assert.Equal((long)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 30; i < 45; i++)
            {
                var generatedValue = generator.Next(shortProperty, storeServices);

                Assert.Equal((short)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 45; i < 60; i++)
            {
                var generatedValue = generator.Next(byteProperty, storeServices);

                Assert.Equal((byte)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 60; i < 75; i++)
            {
                var generatedValue = generator.Next(uintProperty, storeServices);

                Assert.Equal((uint)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 75; i < 90; i++)
            {
                var generatedValue = generator.Next(ulongProperty, storeServices);

                Assert.Equal((ulong)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 90; i < 105; i++)
            {
                var generatedValue = generator.Next(ushortProperty, storeServices);

                Assert.Equal((ushort)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 105; i < 120; i++)
            {
                var generatedValue = generator.Next(sbyteProperty, storeServices);

                Assert.Equal((sbyte)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }
        }

        [Fact]
        public async Task Generates_sequential_values_async()
        {
            var storeServices = CreateStoreServices();
            var entityType = _model.GetEntityType(typeof(AnEntity));
            var property = entityType.GetProperty("Id");
            var sequenceName = RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
            const int blockSize = 1;

            var intProperty = entityType.GetProperty("Id");
            var longProperty = entityType.GetProperty("Long");
            var shortProperty = entityType.GetProperty("Short");
            var byteProperty = entityType.GetProperty("Byte");
            var uintProperty = entityType.GetProperty("UnsignedInt");
            var ulongProperty = entityType.GetProperty("UnsignedLong");
            var ushortProperty = entityType.GetProperty("UnsignedShort");
            var sbyteProperty = entityType.GetProperty("SignedByte");

            var generator = new RedisSequenceValueGenerator(sequenceName, blockSize);

            for (var i = 0; i < 15; i++)
            {
                var generatedValue = await generator.NextAsync(intProperty, storeServices);

                Assert.Equal(i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 15; i < 30; i++)
            {
                var generatedValue = await generator.NextAsync(longProperty, storeServices);

                Assert.Equal((long)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 30; i < 45; i++)
            {
                var generatedValue = await generator.NextAsync(shortProperty, storeServices);

                Assert.Equal((short)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 45; i < 60; i++)
            {
                var generatedValue = await generator.NextAsync(byteProperty, storeServices);

                Assert.Equal((byte)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 60; i < 75; i++)
            {
                var generatedValue = await generator.NextAsync(uintProperty, storeServices);

                Assert.Equal((uint)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 75; i < 90; i++)
            {
                var generatedValue = await generator.NextAsync(ulongProperty, storeServices);

                Assert.Equal((ulong)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 90; i < 105; i++)
            {
                var generatedValue = await generator.NextAsync(ushortProperty, storeServices);

                Assert.Equal((ushort)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
            }

            for (var i = 105; i < 120; i++)
            {
                var generatedValue = await generator.NextAsync(sbyteProperty, storeServices);

                Assert.Equal((sbyte)i, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
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
                    var storeServices = CreateStoreServices(serviceProvider);

                    for (var j = 0; j < valueCount; j++)
                    {
                        var generatedValue = generator.Next(property, storeServices);

                        generatedValues[testNumber].Add((long)generatedValue.Value);
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
                    var storeServices = CreateStoreServices(serviceProvider);

                    for (var j = 0; j < valueCount; j++)
                    {
                        var generatedValue = await generator.NextAsync(property, storeServices);

                        generatedValues[testNumber].Add((long)generatedValue.Value);
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
            var storeServices = CreateStoreServices();

            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Long");

            for (var l = 0L; l < 100L; l++)
            {
                var generatedValue = generator.Next(property, storeServices);

                Assert.Equal(l, generatedValue.Value);
                Assert.False(generatedValue.IsTemporary);
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

        private LazyRef<DataStoreServices> CreateStoreServices(IServiceProvider serviceProvider = null)
        {
            serviceProvider = serviceProvider ?? new ServiceCollection()
                .AddEntityFramework()
                .AddRedis().ServiceCollection
                .AddScoped<RedisDatabase, FakeRedisDatabase>()
                .AddInstance(new FakeRedisSequence())
                .BuildServiceProvider();

            var contextServices = ((IDbContextServices)new DbContext(
                serviceProvider,
                new DbContextOptions()
                    .UseModel(_model)
                    .UseRedis("127.0.0.1", 6375))).ScopedServiceProvider;

            return contextServices.GetRequiredService<LazyRef<DataStoreServices>>();
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
