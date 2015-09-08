// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerSequenceValueGeneratorTest
    {
        [Fact]
        public void Generates_sequential_int_values()
        {
            Generates_sequential_values<int>();
        }

        [Fact]
        public void Generates_sequential_long_values()
        {
            Generates_sequential_values<long>();
        }

        [Fact]
        public void Generates_sequential_short_values()
        {
            Generates_sequential_values<short>();
        }

        [Fact]
        public void Generates_sequential_byte_values()
        {
            Generates_sequential_values<byte>();
        }

        [Fact]
        public void Generates_sequential_uint_values()
        {
            Generates_sequential_values<uint>();
        }

        [Fact]
        public void Generates_sequential_ulong_values()
        {
            Generates_sequential_values<ulong>();
        }

        [Fact]
        public void Generates_sequential_ushort_values()
        {
            Generates_sequential_values<ushort>();
        }

        [Fact]
        public void Generates_sequential_sbyte_values()
        {
            Generates_sequential_values<sbyte>();
        }

        public void Generates_sequential_values<TValue>()
        {
            const int blockSize = 4;
            const int poolSize = 3;

            var state = new SqlServerSequenceValueGeneratorState("Foo", blockSize, poolSize);
            var generator = new SqlServerSequenceValueGenerator<TValue>(
                new FakeSqlStatementExecutor(blockSize),
                new SqlServerUpdateSqlGenerator(),
                state,
                CreateConnection());

            var generatedValues = new List<TValue>();
            for (var i = 0; i < 27; i++)
            {
                generatedValues.Add(generator.Next());
            }

            Assert.Equal(
                new[] { 1, 5, 9, 2, 6, 10, 3, 7, 11, 4, 8, 12, 13, 17, 21, 14, 18, 22, 15, 19, 23, 16, 20, 24, 25, 29, 33 },
                generatedValues.Select(v => (int)Convert.ChangeType(v, typeof(int), CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void Multiple_threads_can_use_the_same_generator_state()
        {
            const int threadCount = 50;
            const int valueCount = 35;
            const int poolSize = 1;

            var generatedValues = GenerateValuesInMultipleThreads(poolSize, threadCount, valueCount);

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

        [Fact]
        public void Multiple_threads_can_use_the_same_generator_state_with_pools()
        {
            const int threadCount = 50;
            const int valueCount = 35;
            const int poolSize = 5;

            var generatedValues = GenerateValuesInMultipleThreads(poolSize, threadCount, valueCount);

            // Check that no values are repeated
            var checks = new bool[threadCount * valueCount * poolSize];
            foreach (var values in generatedValues)
            {
                Assert.Equal(valueCount, values.Count);
                foreach (var value in values)
                {
                    Assert.False(checks[value - 1]);
                    checks[value - 1] = true;
                }
            }
        }

        private IList<long>[] GenerateValuesInMultipleThreads(int poolSize, int threadCount, int valueCount)
        {
            const int blockSize = 10;

            var serviceProvider = SqlServerTestHelpers.Instance.CreateServiceProvider();
            var state = new SqlServerSequenceValueGeneratorState("Foo", blockSize, poolSize);
            var executor = new FakeSqlStatementExecutor(blockSize);
            var sqlGenerator = new SqlServerUpdateSqlGenerator();

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
                            var generator = new SqlServerSequenceValueGenerator<long>(executor, sqlGenerator, state, connection);

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
            var state = new SqlServerSequenceValueGeneratorState("Foo", 4, 3);
            var generator = new SqlServerSequenceValueGenerator<int>(
                new FakeSqlStatementExecutor(4),
                new SqlServerUpdateSqlGenerator(),
                state,
                CreateConnection());

            Assert.False(generator.GeneratesTemporaryValues);
        }

        private ISqlServerConnection CreateConnection(IServiceProvider serviceProvider = null)
        {
            serviceProvider = serviceProvider ?? SqlServerTestHelpers.Instance.CreateServiceProvider();

            return SqlServerTestHelpers.Instance.CreateContextServices(serviceProvider).GetRequiredService<ISqlServerConnection>();
        }

        private class FakeSqlStatementExecutor : SqlStatementExecutor
        {
            private readonly int _blockSize;
            private long _current;

            public FakeSqlStatementExecutor(int blockSize)
                : base(new LoggerFactory())
            {
                _blockSize = blockSize;
                _current = -blockSize + 1;
            }

            public override object ExecuteScalar(IRelationalConnection connection, DbTransaction transaction, string sql)
            {
                return Interlocked.Add(ref _current, _blockSize);
            }

            public override Task<object> ExecuteScalarAsync(
                IRelationalConnection connection, DbTransaction transaction, string sql, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult<object>(Interlocked.Add(ref _current, _blockSize));
            }
        }
    }
}
