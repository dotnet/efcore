// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerSequenceValueGeneratorTest
    {
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

            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

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
                Assert.Equal((byte?)i, generator.Next(nullableByteProperty, storeServices));
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

            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

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
                Assert.Equal((byte?)i, await generator.NextAsync(nullableByteProperty, storeServices));
            }
        }

        [Fact]
        public void Multiple_threads_can_use_the_same_generator()
        {
            var serviceProvider = TestHelpers.CreateServiceProvider();

            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Long");

            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

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

                        var generatedValue = generator.Next(property, storeServices);

                        generatedValues[testNumber].Add((long)generatedValue);
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

            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Long");

            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

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
        public void Does_not_generate_temp_values()
        {
            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

            Assert.False(generator.GeneratesTemporaryValues);
        }

        private DbContextService<DataStoreServices> CreateStoreServices(IServiceProvider serviceProvider = null)
        {
            serviceProvider = serviceProvider ?? TestHelpers.CreateServiceProvider();

            return TestHelpers.CreateContextServices(serviceProvider, _model).GetRequiredService<DbContextService<DataStoreServices>>();
        }

        private class FakeSqlStatementExecutor : SqlStatementExecutor
        {
            private readonly int _blockSize;
            private long _current;

            public FakeSqlStatementExecutor(int blockSize)
                : base(new LoggerFactory())
            {
                _blockSize = blockSize;
                _current = -blockSize;
            }

            public override object ExecuteScalar(DbConnection connection, DbTransaction transaction, string sql)
            {
                return Interlocked.Add(ref _current, _blockSize);
            }

            public override Task<object> ExecuteScalarAsync(
                DbConnection connection, DbTransaction transaction, string sql, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult<object>(Interlocked.Add(ref _current, _blockSize));
            }
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
