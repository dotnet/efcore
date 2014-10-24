// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Tests;
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
            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var intProperty = stateEntry.EntityType.GetProperty("Id");
            var longProperty = stateEntry.EntityType.GetProperty("Long");
            var shortProperty = stateEntry.EntityType.GetProperty("Short");
            var byteProperty = stateEntry.EntityType.GetProperty("Byte");
            var nullableIntProperty = stateEntry.EntityType.GetProperty("NullableId");
            var nullableLongProperty = stateEntry.EntityType.GetProperty("NullableLong");
            var nullableShortProperty = stateEntry.EntityType.GetProperty("NullableShort");
            var nullableByteProperty = stateEntry.EntityType.GetProperty("NullableByte");

            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

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
                generator.Next(stateEntry, nullableIntProperty);

                Assert.Equal(i, stateEntry[nullableIntProperty]);
                Assert.False(stateEntry.HasTemporaryValue(nullableIntProperty));
            }

            for (var i = 75; i < 90; i++)
            {
                generator.Next(stateEntry, nullableLongProperty);

                Assert.Equal((long)i, stateEntry[nullableLongProperty]);
                Assert.False(stateEntry.HasTemporaryValue(nullableLongProperty));
            }

            for (var i = 90; i < 105; i++)
            {
                generator.Next(stateEntry, nullableShortProperty);

                Assert.Equal((short)i, stateEntry[nullableShortProperty]);
                Assert.False(stateEntry.HasTemporaryValue(nullableShortProperty));
            }

            for (var i = 105; i < 120; i++)
            {
                generator.Next(stateEntry, nullableByteProperty);

                Assert.Equal((byte)i, stateEntry[nullableByteProperty]);
                Assert.False(stateEntry.HasTemporaryValue(nullableByteProperty));
            }
        }

        [Fact]
        public async Task Generates_sequential_values_async()
        {
            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model);
            var intProperty = stateEntry.EntityType.GetProperty("Id");
            var longProperty = stateEntry.EntityType.GetProperty("Long");
            var shortProperty = stateEntry.EntityType.GetProperty("Short");
            var byteProperty = stateEntry.EntityType.GetProperty("Byte");
            var nullableIntProperty = stateEntry.EntityType.GetProperty("NullableId");
            var nullableLongProperty = stateEntry.EntityType.GetProperty("NullableLong");
            var nullableShortProperty = stateEntry.EntityType.GetProperty("NullableShort");
            var nullableByteProperty = stateEntry.EntityType.GetProperty("NullableByte");

            var executor = new FakeSqlStatementExecutor(10);
            var generator = new SqlServerSequenceValueGenerator(executor, "Foo", 10);

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
                await generator.NextAsync(stateEntry, nullableIntProperty);

                Assert.Equal(i, stateEntry[nullableIntProperty]);
                Assert.False(stateEntry.HasTemporaryValue(nullableIntProperty));
            }

            for (var i = 75; i < 90; i++)
            {
                await generator.NextAsync(stateEntry, nullableLongProperty);

                Assert.Equal((long)i, stateEntry[nullableLongProperty]);
                Assert.False(stateEntry.HasTemporaryValue(nullableLongProperty));
            }

            for (var i = 90; i < 105; i++)
            {
                await generator.NextAsync(stateEntry, nullableShortProperty);

                Assert.Equal((short)i, stateEntry[nullableShortProperty]);
                Assert.False(stateEntry.HasTemporaryValue(nullableShortProperty));
            }

            for (var i = 105; i < 120; i++)
            {
                await generator.NextAsync(stateEntry, nullableByteProperty);

                Assert.Equal((byte)i, stateEntry[nullableByteProperty]);
                Assert.False(stateEntry.HasTemporaryValue(nullableByteProperty));
            }
        }

        [Fact]
        public void Multiple_threads_can_use_the_same_generator()
        {
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
