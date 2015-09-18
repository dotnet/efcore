// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.ValueGeneration;
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

            var state = new SqlServerSequenceValueGeneratorState(
                new Sequence(
                    new Model(), RelationalAnnotationNames.Prefix, "Foo")
                {
                    IncrementBy = blockSize
                },
                poolSize);


            var generator = new SqlServerSequenceValueGenerator<TValue>(
                new FakeCommandBuilderFactory(blockSize),
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

            var state = new SqlServerSequenceValueGeneratorState(
                new Sequence(
                    new Model(), RelationalAnnotationNames.Prefix, "Foo")
                {
                    IncrementBy = blockSize
                },
                poolSize);

            var executor = new FakeCommandBuilderFactory(blockSize);
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
            var state = new SqlServerSequenceValueGeneratorState(
                new Sequence(
                    new Model(), RelationalAnnotationNames.Prefix, "Foo")
                {
                    IncrementBy = 4
                }, 3);

            var generator = new SqlServerSequenceValueGenerator<int>(
                new FakeCommandBuilderFactory(4),
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

        private class FakeRelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
        {
            public ILoggerFactory LoggerFactory { get; }
            public IRelationalTypeMapper TypeMapper { get; }

            public FakeRelationalCommandBuilderFactory(ILoggerFactory loggerFactory, IRelationalTypeMapper typeMapper)
            {
                LoggerFactory = loggerFactory;
                TypeMapper = typeMapper;
            }

            public IRelationalCommandBuilder Create()
            {
                return new FakeRelationalCommandBuilder(this);
            }
        }

        private class FakeRelationalCommandBuilder : RelationalCommandBuilder
        {
            private FakeRelationalCommandBuilderFactory _factory;

            public FakeRelationalCommandBuilder(FakeRelationalCommandBuilderFactory factory)
                : base(factory.LoggerFactory, factory.TypeMapper)
            {
                _factory = factory;
            }

            public override IRelationalCommand BuildRelationalCommand()
            {
                return new FakeRelationalCommand(_factory);
            }
        }

        private class FakeRelationalCommand : RelationalCommand
        {
            private FakeRelationalCommandBuilderFactory _factory;

            public FakeRelationalCommand(FakeRelationalCommandBuilderFactory factory)
                : base(
                      factory.LoggerFactory,
                      factory.TypeMapper,
                      "CommandText",
                      new RelationalParameter[] { })
            {
                _factory = factory;
            }

            public override void ExecuteNonQuery(IRelationalConnection connection)
            {
            }

            public override Task ExecuteNonQueryAsync(IRelationalConnection connection, CancellationToken cancellationToken = default(CancellationToken))
            {
                return base.ExecuteNonQueryAsync(connection, cancellationToken);
            }
        }


        private class FakeCommandBuilderFactory : IRelationalCommandBuilderFactory
        {
            private readonly int _blockSize;
            private long _current;

            public FakeCommandBuilderFactory(int blockSize)
            {
                _blockSize = blockSize;
                _current = -blockSize + 1;
            }

            public IRelationalCommandBuilder Create()
                => new FakeCommandBuilder(this);

            private class FakeCommandBuilder : RelationalCommandBuilder
            {
                private readonly FakeCommandBuilderFactory _factory;

                public FakeCommandBuilder(FakeCommandBuilderFactory factory)
                    : base(new LoggerFactory(), new SqlServerTypeMapper())
                {
                    _factory = factory;
                }

                public override IRelationalCommand BuildRelationalCommand()
                {
                    return new FakeRelationalCommand(_factory);
                }

                private class FakeRelationalCommand : RelationalCommand
                {
                    private readonly FakeCommandBuilderFactory _factory;

                    public FakeRelationalCommand(FakeCommandBuilderFactory factory)
                        : base(new LoggerFactory(), new SqlServerTypeMapper(), "CommandText", new RelationalParameter[0])
                    {
                        _factory = factory;
                    }

                    public override object ExecuteScalar(IRelationalConnection connection)
                    {
                        return Interlocked.Add(ref _factory._current, _factory._blockSize);
                    }

                    public override Task<object> ExecuteScalarAsync(IRelationalConnection connection, CancellationToken cancellationToken = default(CancellationToken))
                    {
                        return Task.FromResult<object>(Interlocked.Add(ref _factory._current, _factory._blockSize));
                    }
                }
            }
        }
    }
}
