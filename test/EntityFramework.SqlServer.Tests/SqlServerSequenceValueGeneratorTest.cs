// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerSequenceValueGeneratorTest
    {
        [Fact]
        public void Generates_sequential_values()
        {
            var configMock = new Mock<DbContextConfiguration>();
            configMock.Setup(m => m.Connection).Returns(new Mock<RelationalConnection>().Object);

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.Configuration).Returns(configMock.Object);

            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

            for (var i = 0; i < 15; i++)
            {
                Assert.Equal((long)i, generator.Next(entryMock.Object, CreateProperty(typeof(long))));
            }

            for (var i = 15; i < 30; i++)
            {
                Assert.Equal(i, generator.Next(entryMock.Object, CreateProperty(typeof(int))));
            }

            for (var i = 30; i < 45; i++)
            {
                Assert.Equal((short)i, generator.Next(entryMock.Object, CreateProperty(typeof(short))));
            }

            for (var i = 45; i < 60; i++)
            {
                Assert.Equal((byte)i, generator.Next(entryMock.Object, CreateProperty(typeof(byte))));
            }
        }

        [Fact]
        public async Task Generates_sequential_values_async()
        {
            var configMock = new Mock<DbContextConfiguration>();
            configMock.Setup(m => m.Connection).Returns(new Mock<RelationalConnection>().Object);

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.Configuration).Returns(configMock.Object);

            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

            for (var i = 0; i < 15; i++)
            {
                Assert.Equal((long)i, await generator.NextAsync(entryMock.Object, CreateProperty(typeof(long))));
            }

            for (var i = 15; i < 30; i++)
            {
                Assert.Equal(i, await generator.NextAsync(entryMock.Object, CreateProperty(typeof(int))));
            }

            for (var i = 30; i < 45; i++)
            {
                Assert.Equal((short)i, await generator.NextAsync(entryMock.Object, CreateProperty(typeof(short))));
            }

            for (var i = 45; i < 60; i++)
            {
                Assert.Equal((byte)i, await generator.NextAsync(entryMock.Object, CreateProperty(typeof(byte))));
            }
        }

        [Fact]
        public void Multiple_threads_can_use_the_same_generator()
        {
            var configMock = new Mock<DbContextConfiguration>();
            configMock.Setup(m => m.Connection).Returns(new Mock<RelationalConnection>().Object);

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.Configuration).Returns(configMock.Object);

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
                            generatedValues[testNumber].Add((long)generator.Next(entryMock.Object, CreateProperty(typeof(long))));
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
            var configMock = new Mock<DbContextConfiguration>();
            configMock.Setup(m => m.Connection).Returns(new Mock<RelationalConnection>().Object);

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.Configuration).Returns(configMock.Object);

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
                            generatedValues[testNumber].Add((long)await generator.NextAsync(entryMock.Object, CreateProperty(typeof(long))));
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

        private static Property CreateProperty(Type propertyType)
        {
            var entityType = new EntityType("MyType");
            return entityType.GetOrAddProperty("MyProperty", propertyType, shadowProperty: true);
        }

        private class FakeSqlStatementExecutor : SqlStatementExecutor
        {
            private readonly int _blockSize;
            private long _current;

            public FakeSqlStatementExecutor(int blockSize)
            {
                _blockSize = blockSize;
                _current = -blockSize;
            }

            public override object ExecuteScalar(DbConnection connection, DbTransaction transaction, SqlStatement statement)
            {
                return Interlocked.Add(ref _current, _blockSize);
            }

            public override Task<object> ExecuteScalarAsync(
                DbConnection connection, DbTransaction transaction, SqlStatement statement, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult<object>(Interlocked.Add(ref _current, _blockSize));
            }
        }
    }
}
