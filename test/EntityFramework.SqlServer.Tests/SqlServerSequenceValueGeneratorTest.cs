// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerSequenceValueGeneratorTest
    {
        [Fact]
        public void Generates_sequential_int_values() => Generates_sequential_values<int>();

        [Fact]
        public void Generates_sequential_long_values() => Generates_sequential_values<long>();

        [Fact]
        public void Generates_sequential_short_values() => Generates_sequential_values<short>();

        [Fact]
        public void Generates_sequential_byte_values() => Generates_sequential_values<byte>();

        [Fact]
        public void Generates_sequential_uint_values() => Generates_sequential_values<uint>();

        [Fact]
        public void Generates_sequential_ulong_values() => Generates_sequential_values<ulong>();

        [Fact]
        public void Generates_sequential_ushort_values() => Generates_sequential_values<ushort>();

        [Fact]
        public void Generates_sequential_sbyte_values() => Generates_sequential_values<sbyte>();

        public void Generates_sequential_values<TValue>()
        {
            const int blockSize = 4;

            var state = new SqlServerSequenceValueGeneratorState(
                new Sequence(
                    new Model(), RelationalAnnotationNames.Prefix, "Foo")
                {
                    IncrementBy = blockSize
                });

            var generator = new SqlServerSequenceHiLoValueGenerator<TValue>(
                new FakeSqlStatementExecutor(blockSize),
                new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator()),
                state,
                CreateConnection());

            for (var i = 1; i <= 27; i++)
            {
                Assert.Equal(i, (int)Convert.ChangeType(generator.Next(), typeof(int), CultureInfo.InvariantCulture));
            }
        }

        [Fact]
        public void Multiple_threads_can_use_the_same_generator_state()
        {
            const int threadCount = 50;
            const int valueCount = 35;

            var generatedValues = GenerateValuesInMultipleThreads(threadCount, valueCount);

            // Check that each value was generated once and only once
            var checks = new bool[threadCount * valueCount];
            foreach (var values in generatedValues)
            {
                Assert.Equal(valueCount, values.Count);
                foreach (var value in values)
                {
                    checks[value - 1] = true;
                }
            }

            Assert.True(checks.All(c => c));
        }

        private IEnumerable<List<long>> GenerateValuesInMultipleThreads(int threadCount, int valueCount)
        {
            const int blockSize = 10;

            var serviceProvider = SqlServerTestHelpers.Instance.CreateServiceProvider();

            var state = new SqlServerSequenceValueGeneratorState(
                new Sequence(
                    new Model(), RelationalAnnotationNames.Prefix, "Foo")
                {
                    IncrementBy = blockSize
                });

            var executor = new FakeSqlStatementExecutor(blockSize);
            var sqlGenerator = new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator());

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
                            var connection = CreateConnection(serviceProvider);
                            var generator = new SqlServerSequenceHiLoValueGenerator<long>(executor, sqlGenerator, state, connection);

                            generatedValues[testNumber].Add(generator.Next());
                        }
                    };
            }

            Parallel.Invoke(tests);

            return generatedValues;
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            var state = new SqlServerSequenceValueGeneratorState(
                new Sequence(
                    new Model(), RelationalAnnotationNames.Prefix, "Foo")
                {
                    IncrementBy = 4
                });

            var generator = new SqlServerSequenceHiLoValueGenerator<int>(
                new FakeSqlStatementExecutor(4),
                new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator()),
                state,
                CreateConnection());

            Assert.False(generator.GeneratesTemporaryValues);
        }

        private static ISqlServerConnection CreateConnection(IServiceProvider serviceProvider = null)
        {
            serviceProvider = serviceProvider ?? SqlServerTestHelpers.Instance.CreateServiceProvider();

            return SqlServerTestHelpers.Instance.CreateContextServices(serviceProvider).GetRequiredService<ISqlServerConnection>();
        }

        private class FakeSqlStatementExecutor : SqlStatementExecutor
        {
            private readonly int _blockSize;
            private long _current;

            public FakeSqlStatementExecutor(int blockSize)
                : base(
                    new RelationalCommandBuilderFactory(new SqlServerTypeMapper()),
                    new Mock<ISensitiveDataLogger<SqlStatementExecutor>>().Object)
            {
                _blockSize = blockSize;
                _current = -blockSize + 1;
            }

            public override object ExecuteScalar(IRelationalConnection connection, string sql)
                => Interlocked.Add(ref _current, _blockSize);

            public override Task<object> ExecuteScalarAsync(
                IRelationalConnection connection,
                string sql,
                CancellationToken cancellationToken = new CancellationToken())
                => Task.FromResult<object>(Interlocked.Add(ref _current, _blockSize));
        }
    }
}
