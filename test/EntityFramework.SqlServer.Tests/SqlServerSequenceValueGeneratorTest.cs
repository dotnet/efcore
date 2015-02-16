// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
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
        [Fact]
        public void Generates_sequential_int_values()
        {
            var storeServices = CreateStoreServices();
            var generator = new SqlServerSequenceValueGenerator<int>(new FakeSqlStatementExecutor(10), "Foo", 10);

            for (var i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(storeServices));
            }
        }

        [Fact]
        public void Generates_sequential_long_values()
        {
            var storeServices = CreateStoreServices();
            var generator = new SqlServerSequenceValueGenerator<long>(new FakeSqlStatementExecutor(10), "Foo", 10);

            for (long i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(storeServices));
            }
        }

        [Fact]
        public void Generates_sequential_short_values()
        {
            var storeServices = CreateStoreServices();
            var generator = new SqlServerSequenceValueGenerator<short>(new FakeSqlStatementExecutor(10), "Foo", 10);

            for (short i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(storeServices));
            }
        }

        [Fact]
        public void Generates_sequential_byte_values()
        {
            var storeServices = CreateStoreServices();
            var generator = new SqlServerSequenceValueGenerator<byte>(new FakeSqlStatementExecutor(10), "Foo", 10);

            for (byte i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(storeServices));
            }
        }

        [Fact]
        public void Generates_sequential_uint_values()
        {
            var storeServices = CreateStoreServices();
            var generator = new SqlServerSequenceValueGenerator<uint>(new FakeSqlStatementExecutor(10), "Foo", 10);

            for (uint i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(storeServices));
            }
        }

        [Fact]
        public void Generates_sequential_ulong_values()
        {
            var storeServices = CreateStoreServices();
            var generator = new SqlServerSequenceValueGenerator<ulong>(new FakeSqlStatementExecutor(10), "Foo", 10);

            for (ulong i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(storeServices));
            }
        }

        [Fact]
        public void Generates_sequential_ushort_values()
        {
            var storeServices = CreateStoreServices();
            var generator = new SqlServerSequenceValueGenerator<ushort>(new FakeSqlStatementExecutor(10), "Foo", 10);

            for (ushort i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(storeServices));
            }
        }

        [Fact]
        public void Generates_sequential_sbyte_values()
        {
            var storeServices = CreateStoreServices();
            var generator = new SqlServerSequenceValueGenerator<sbyte>(new FakeSqlStatementExecutor(10), "Foo", 10);

            for (sbyte i = 0; i < 15; i++)
            {
                Assert.Equal(i, generator.Next(storeServices));
            }
        }

        [Fact]
        public void Multiple_threads_can_use_the_same_generator()
        {
            var serviceProvider = SqlServerTestHelpers.Instance.CreateServiceProvider();
            var generator = new SqlServerSequenceValueGenerator<long>(new FakeSqlStatementExecutor(10), "Foo", 10);

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

                            var generatedValue = generator.Next(storeServices);

                            generatedValues[testNumber].Add(generatedValue);
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
        public void Does_not_generate_temp_values()
        {
            var generator = new SqlServerSequenceValueGenerator<int>(new FakeSqlStatementExecutor(10), "Foo", 10);

            Assert.False(generator.GeneratesTemporaryValues);
        }

        private DbContextService<DataStoreServices> CreateStoreServices(IServiceProvider serviceProvider = null)
        {
            serviceProvider = serviceProvider ?? SqlServerTestHelpers.Instance.CreateServiceProvider();

            return SqlServerTestHelpers.Instance.CreateContextServices(serviceProvider).GetRequiredService<DbContextService<DataStoreServices>>();
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

            public override object ExecuteScalar(RelationalConnection connection, DbTransaction transaction, string sql)
            {
                return Interlocked.Add(ref _current, _blockSize);
            }

            public override Task<object> ExecuteScalarAsync(
                RelationalConnection connection, DbTransaction transaction, string sql, CancellationToken cancellationToken = new CancellationToken())
            {
                return Task.FromResult<object>(Interlocked.Add(ref _current, _blockSize));
            }
        }
    }
}
