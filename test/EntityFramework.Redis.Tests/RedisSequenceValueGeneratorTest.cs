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
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using StackExchange.Redis;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisSequenceValueGeneratorTest
    {
        class FakeRedisDatabase :RedisDatabase
        {
            Mock<IDatabase> redisDatabaseMock = new Mock<IDatabase>();
            public FakeRedisDatabase(DbContextService<IModel> model, RedisDataStoreCreator dataStoreCreator) :
                base(model, dataStoreCreator, Mock.Of<RedisConnection>(), new LoggerFactory())
            {
                long i = 0;
                redisDatabaseMock.Setup(r => r.StringIncrement(It.IsAny<RedisKey>(), It.IsAny<long>(), CommandFlags.None)).Returns<RedisKey, long, CommandFlags>((sn, incrementBy, flag) => { var ret = i; i += incrementBy; return ret; });
                redisDatabaseMock.Setup(r => r.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), CommandFlags.None)).Returns<RedisKey, long, CommandFlags>((sn, incrementBy, flag) => { var ret = i;  i += incrementBy; return Task.FromResult(ret); });                
            }
            

            public override IDatabase GetUnderlyingDatabase()
            {
                return redisDatabaseMock.Object;
            }
        }

        private DbContextService<DataStoreServices> CreateStoreServices(IServiceProvider serviceProvider = null)
        {
            serviceProvider = serviceProvider ?? TestHelpers.CreateServiceProvider();

            return TestHelpers.CreateContextServices(serviceProvider, _model).GetRequiredService<DbContextService<DataStoreServices>>();
        }

        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();

        [Fact]
        public void Generates_sequential_values()
        {
            var storeServices = CreateStoreServices();
            var entityType = _model.GetEntityType(typeof(AnEntity));

            var intProperty = entityType.GetProperty("Id");
            var longProperty = entityType.GetProperty("Long");
            var shortProperty = entityType.GetProperty("Short");
            var byteProperty = entityType.GetProperty("Byte");
            var nullableIntProperty = entityType.GetProperty("NullableId");
            var nullableLongProperty = entityType.GetProperty("NullableLong");
            var nullableShortProperty = entityType.GetProperty("NullableShort");
            var nullableByteProperty = entityType.GetProperty("NullableByte");
            var uintProperty = entityType.GetProperty("UnsignedInt");
            var ulongProperty = entityType.GetProperty("UnsignedLong");
            var ushortProperty = entityType.GetProperty("UnsignedShort");
            var sbyteProperty = entityType.GetProperty("SignedByte");

            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var property = stateEntry.EntityType.GetProperty("Id");
            var sequenceName = RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
            var blockSize = 1;

            var model = Mock.Of<IModel>();
            var connectionMock = new Mock<RedisConnection>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            var database = new FakeRedisDatabase(new DbContextService<IModel>(() => model),
                creator);


            var generator = new RedisSequenceValueGenerator(database, sequenceName, blockSize);

            for (var i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(intProperty, storeServices));
            }

            for (var i = 15; i < 30; i++)
            {
                Assert.Equal((long)i, generator.Next(longProperty, storeServices));
            }

            for (var i = 30; i < 45; i++)
            {
                Assert.Equal((short)i, generator.Next(shortProperty, storeServices));
            }

            for (var i = 45; i < 60; i++)
            {
                Assert.Equal((byte)i, generator.Next(byteProperty, storeServices));
            }

            for (var i = 60; i < 75; i++)
            {
                Assert.Equal((int?)i, generator.Next(nullableIntProperty, storeServices));
            }

            for (var i = 75; i < 90; i++)
            {
                Assert.Equal((long?)i, generator.Next(nullableLongProperty, storeServices));
            }

            for (var i = 90; i < 105; i++)
            {
                Assert.Equal((short?)i, generator.Next(nullableShortProperty, storeServices));
            }

            for (var i = 105; i < 120; i++)
            {
                Assert.Equal((sbyte)i, generator.Next(sbyteProperty, storeServices));
            }

            for (var i = 120; i < 135; i++)
            {
                Assert.Equal((byte?)i, generator.Next(nullableByteProperty, storeServices));
            }

            for (var i = 135; i < 150; i++)
            {
                Assert.Equal((uint)i, generator.Next(uintProperty, storeServices));
            }

            for (var i = 150; i < 165; i++)
            {
                Assert.Equal((ulong)i, generator.Next(ulongProperty, storeServices));
            }

            for (var i = 165; i < 180; i++)
            {
                Assert.Equal((ushort)i, generator.Next(ushortProperty, storeServices));
            }
        }

        [Fact]
        public async Task Generates_sequential_values_async()
        {
            var storeServices = CreateStoreServices();
            var entityType = _model.GetEntityType(typeof(AnEntity));

            var intProperty = entityType.GetProperty("Id");
            var longProperty = entityType.GetProperty("Long");
            var shortProperty = entityType.GetProperty("Short");
            var byteProperty = entityType.GetProperty("Byte");
            var nullableIntProperty = entityType.GetProperty("NullableId");
            var nullableLongProperty = entityType.GetProperty("NullableLong");
            var nullableShortProperty = entityType.GetProperty("NullableShort");
            var nullableByteProperty = entityType.GetProperty("NullableByte");
            var uintProperty = entityType.GetProperty("UnsignedInt");
            var ulongProperty = entityType.GetProperty("UnsignedLong");
            var ushortProperty = entityType.GetProperty("UnsignedShort");
            var sbyteProperty = entityType.GetProperty("SignedByte");

            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var property = stateEntry.EntityType.GetProperty("Id");
            var sequenceName = RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
            var blockSize = 1;

            var model = Mock.Of<IModel>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            var database = new FakeRedisDatabase(new DbContextService<IModel>(() => model),
                creator);

            var generator = new RedisSequenceValueGenerator(database, sequenceName, blockSize);

            for (var i = 0; i < 15; i++)
            {
                Assert.Equal(i, await generator.NextAsync(intProperty, storeServices));
            }

            for (var i = 15; i < 30; i++)
            {
                Assert.Equal((long)i, await generator.NextAsync(longProperty, storeServices));
            }

            for (var i = 30; i < 45; i++)
            {
                Assert.Equal((short)i, await generator.NextAsync(shortProperty, storeServices));
            }

            for (var i = 45; i < 60; i++)
            {
                Assert.Equal((byte)i, await generator.NextAsync(byteProperty, storeServices));
            }

            for (var i = 60; i < 75; i++)
            {
                Assert.Equal((int?)i, await generator.NextAsync(nullableIntProperty, storeServices));
            }

            for (var i = 75; i < 90; i++)
            {
                Assert.Equal((long?)i, await generator.NextAsync(nullableLongProperty, storeServices));
            }

            for (var i = 90; i < 105; i++)
            {
                Assert.Equal((short?)i, await generator.NextAsync(nullableShortProperty, storeServices));
            }

            for (var i = 105; i < 120; i++)
            {
                Assert.Equal((sbyte)i, await generator.NextAsync(sbyteProperty, storeServices));
            }

            for (var i = 120; i < 135; i++)
            {
                Assert.Equal((byte?)i, await generator.NextAsync(nullableByteProperty, storeServices));
            }

            for (var i = 135; i < 150; i++)
            {
                Assert.Equal((uint)i, await generator.NextAsync(uintProperty, storeServices));
            }

            for (var i = 150; i < 165; i++)
            {
                Assert.Equal((ulong)i, await generator.NextAsync(ulongProperty, storeServices));
            }

            for (var i = 165; i < 180; i++)
            {
                Assert.Equal((ushort)i, await generator.NextAsync(ushortProperty, storeServices));
            }
        }

        [Fact]
        public void Multiple_threads_can_use_the_same_generator()
        {
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var model = Mock.Of<IModel>();
            var connection = Mock.Of<RedisConnection>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();
            var database = new FakeRedisDatabase(new DbContextService<IModel>(() => model),
                creator);


            var generator = new RedisSequenceValueGenerator(database, "TestSequenceName", 1);

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
                        for (var j = 0; j < valueCount; j++)
                        {
                            var storeServices = CreateStoreServices(serviceProvider);
                            var valueGenerated = generator.Next(property, storeServices);

                            generatedValues[testNumber].Add((long)valueGenerated);
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
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var model = Mock.Of<IModel>();
            var connection = Mock.Of<RedisConnection>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            var database = new FakeRedisDatabase(new DbContextService<IModel>(() => model),
                creator);

            var generator = new RedisSequenceValueGenerator(database, "TestSequenceName", 1);

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
                        for (var j = 0; j < valueCount; j++)
                        {
                            var storeServices = CreateStoreServices(serviceProvider);
                            var generatedValue = await generator.NextAsync(property, storeServices);

                            generatedValues[testNumber].Add((long)generatedValue);
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
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var blockSize = 10;
            var model = Mock.Of<IModel>();
            var connection = Mock.Of<RedisConnection>();
            var contextServices = TestHelpers.CreateContextServices();
            var creator = contextServices.GetRequiredService<RedisDataStoreCreator>();

            var database = new FakeRedisDatabase(new DbContextService<IModel>(() => model),
                creator);

            var generator = new RedisSequenceValueGenerator(database, "TestSequenceName", blockSize);
            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);

            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Long");

            for (var l = 0L; l < 100L; l++)
            {
                var storeServices = CreateStoreServices(serviceProvider);
                var generatedValue = generator.Next(property, storeServices);

                Assert.Equal(l, generatedValue);
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
            public int? NullableId { get; set; }
            public long? NullableLong { get; set; }
            public short? NullableShort { get; set; }
            public byte? NullableByte { get; set; }

        }
    }
}
