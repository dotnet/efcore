// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
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
                new FakeSqlCommandBuilder(blockSize),
                new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator(), new SqlServerTypeMapper()),
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

            var executor = new FakeSqlCommandBuilder(blockSize);
            var sqlGenerator = new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator(), new SqlServerTypeMapper());

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
                new FakeSqlCommandBuilder(4),
                new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator(), new SqlServerTypeMapper()),
                state,
                CreateConnection());

            Assert.False(generator.GeneratesTemporaryValues);
        }

        private static ISqlServerConnection CreateConnection(IServiceProvider serviceProvider = null)
        {
            serviceProvider = serviceProvider ?? SqlServerTestHelpers.Instance.CreateServiceProvider();

            return SqlServerTestHelpers.Instance.CreateContextServices(serviceProvider).GetRequiredService<ISqlServerConnection>();
        }

        private class FakeSqlCommandBuilder : ISqlCommandBuilder
        {
            private readonly int _blockSize;
            private long _current;

            public FakeSqlCommandBuilder(int blockSize)
            {
                _blockSize = blockSize;
                _current = -blockSize + 1;
            }

            public IRelationalCommand Build(string sql, IReadOnlyList<object> parameters = null) => new FakeRelationalCommand(this);

            private class FakeRelationalCommand : IRelationalCommand
            {
                private readonly FakeSqlCommandBuilder _commandBuilder;

                public FakeRelationalCommand(FakeSqlCommandBuilder commandBuilder)
                {
                    _commandBuilder = commandBuilder;
                }

                public string CommandText { get { throw new NotImplementedException(); } }

                public IReadOnlyList<IRelationalParameter> Parameters { get { throw new NotImplementedException(); } }

                public void ExecuteNonQuery(IRelationalConnection connection, bool manageConnection = true)
                {
                    throw new NotImplementedException();
                }

                public Task ExecuteNonQueryAsync(IRelationalConnection connection, CancellationToken cancellationToken = default(CancellationToken), bool manageConnection = true)
                {
                    throw new NotImplementedException();
                }

                public object ExecuteScalar(IRelationalConnection connection, bool manageConnection = true)
                    => Interlocked.Add(ref _commandBuilder._current, _commandBuilder._blockSize);

                public Task<object> ExecuteScalarAsync(IRelationalConnection connection, CancellationToken cancellationToken = default(CancellationToken), bool manageConnection = true)
                    => Task.FromResult<object>(Interlocked.Add(ref _commandBuilder._current, _commandBuilder._blockSize));

                public RelationalDataReader ExecuteReader(IRelationalConnection connection, bool manageConnection = true, IReadOnlyDictionary<string, object> parameters = null)
                {
                    throw new NotImplementedException();
                }

                public Task<RelationalDataReader> ExecuteReaderAsync(IRelationalConnection connection, CancellationToken cancellationToken = default(CancellationToken), bool manageConnection = true, IReadOnlyDictionary<string, object> parameters = null)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
