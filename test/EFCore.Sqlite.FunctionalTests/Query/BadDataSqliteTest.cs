// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IsolationLevel = System.Data.IsolationLevel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class BadDataSqliteTest : IClassFixture<BadDataSqliteTest.BadDataSqliteFixture>
    {
        public BadDataSqliteTest(BadDataSqliteFixture fixture) => Fixture = fixture;

        public BadDataSqliteFixture Fixture { get; }

        [Fact]
        public void Bad_data_error_handling_invalid_cast_key()
        {
            using (var context = CreateContext("bad int"))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().Where(p => p.ProductID != 1).ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_null_key()
        {
            using (var context = CreateContext(null, true))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyNullReference("Product", "ProductID", typeof(int)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().Where(p => p.ProductID != 2).ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_invalid_cast()
        {
            using (var context = CreateContext(1, true, 1))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductName", typeof(string), typeof(int)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().Where(p => p.ProductID != 3).ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_invalid_cast_projection()
        {
            using (var context = CreateContext(1))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductName", typeof(string), typeof(int)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().Where(p => p.ProductID != 4)
                                .Select(p => p.ProductName)
                                .ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_invalid_cast_no_tracking()
        {
            using (var context = CreateContext("bad int"))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>()
                                .Where(p => p.ProductID != 5)
                                .AsNoTracking()
                                .ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_null()
        {
            using (var context = CreateContext(1, null))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().Where(p => p.ProductID != 6).ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_null_projection()
        {
            using (var context = CreateContext(new object[] { null }))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>()
                                .Where(p => p.ProductID != 7)
                                .Select(p => p.Discontinued)
                                .ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_null_no_tracking()
        {
            using (var context = CreateContext(null, true))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyNullReference("Product", "ProductID", typeof(int)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>()
                                .Where(p => p.ProductID != 8)
                                .AsNoTracking()
                                .ToList()).Message);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class BadDataCommandBuilderFactory : RelationalCommandBuilderFactory
        {
            public BadDataCommandBuilderFactory(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                IRelationalTypeMappingSource typeMappingSource)
                : base(logger, typeMappingSource)
            {
            }

            public object[] Values { private get; set; }

            protected override IRelationalCommandBuilder CreateCore(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                IRelationalTypeMappingSource relationalTypeMappingSource)
                => new BadDataRelationalCommandBuilder(
                    logger, relationalTypeMappingSource, Values);

            private class BadDataRelationalCommandBuilder : RelationalCommandBuilder
            {
                private readonly object[] _values;

                public BadDataRelationalCommandBuilder(
                    IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                    IRelationalTypeMappingSource typeMappingSource,
                    object[] values)
                    : base(logger, typeMappingSource)
                {
                    _values = values;
                }

                protected override IRelationalCommand BuildCore(
                    IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                    string commandText,
                    IReadOnlyList<IRelationalParameter> parameters)
                    => new BadDataRelationalCommand(logger, commandText, parameters, _values);

                private class BadDataRelationalCommand : RelationalCommand
                {
                    private readonly object[] _values;

                    public BadDataRelationalCommand(
                        IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                        string commandText,
                        IReadOnlyList<IRelationalParameter> parameters,
                        object[] values)
                        : base(logger, commandText, parameters)
                    {
                        _values = values;
                    }

                    public override RelationalDataReader ExecuteReader(
                        IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues)
                    {
                        var command = connection.DbConnection.CreateCommand();
                        command.CommandText = CommandText;
                        return new BadDataRelationalDataReader(command, _values, Logger);
                    }

                    private class BadDataRelationalDataReader : RelationalDataReader
                    {
                        public BadDataRelationalDataReader(
                            DbCommand command,
                            object[] values,
                            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
                            : base(new FakeConnection(), command, new BadDataDataReader(values), Guid.NewGuid(), logger)
                        {
                        }

                        private class BadDataDataReader : DbDataReader
                        {
                            private readonly object[] _values;

                            public BadDataDataReader(object[] values)
                            {
                                _values = values;
                            }

                            public override bool Read() => true;

                            public override bool IsDBNull(int ordinal)
                                => false;

                            public override int GetInt32(int ordinal) => (int)GetValue(ordinal);

                            public override short GetInt16(int ordinal) => (short)GetValue(ordinal);

                            public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);

                            public override string GetString(int ordinal) => (string)GetValue(ordinal);

                            public override object GetValue(int ordinal) => _values[ordinal];

                            #region NotImplemented members

                            public override string GetName(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override int GetValues(object[] values)
                            {
                                throw new NotImplementedException();
                            }

                            public override int FieldCount => throw new NotImplementedException();

                            public override object this[int ordinal] => throw new NotImplementedException();

                            public override object this[string name] => throw new NotImplementedException();

                            public override bool HasRows => throw new NotImplementedException();

                            public override bool IsClosed => throw new NotImplementedException();

                            public override int RecordsAffected => 0;

                            public override bool NextResult()
                            {
                                throw new NotImplementedException();
                            }

                            public override int Depth => throw new NotImplementedException();

                            public override int GetOrdinal(string name)
                            {
                                throw new NotImplementedException();
                            }

                            public override byte GetByte(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
                            {
                                throw new NotImplementedException();
                            }

                            public override char GetChar(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
                            {
                                throw new NotImplementedException();
                            }

                            public override Guid GetGuid(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override long GetInt64(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override DateTime GetDateTime(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override decimal GetDecimal(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override double GetDouble(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override float GetFloat(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override string GetDataTypeName(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override Type GetFieldType(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override IEnumerator GetEnumerator()
                            {
                                throw new NotImplementedException();
                            }

                            #endregion
                        }
                    }
                }
            }
        }

        private NorthwindContext CreateContext(params object[] values)
        {
            var context = Fixture.CreateContext();

            var badDataCommandBuilderFactory
                = (BadDataCommandBuilderFactory)context.GetService<IRelationalCommandBuilderFactory>();

            badDataCommandBuilderFactory.Values = values;

            return context;
        }

        private class FakeConnection : IRelationalConnection
        {
            public void ResetState()
            {
            }

            public IDbContextTransaction BeginTransaction() => throw new NotImplementedException();
            public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public void CommitTransaction()
            {
            }

            public void RollbackTransaction()
            {
            }

            public IDbContextTransaction CurrentTransaction => throw new NotImplementedException();
            public Transaction EnlistedTransaction { get; }
            public void EnlistTransaction(Transaction transaction) => throw new NotImplementedException();
            public SemaphoreSlim Semaphore { get; }

            public void RegisterBufferable(IBufferable bufferable)
            {
            }

            public Task RegisterBufferableAsync(IBufferable bufferable, CancellationToken cancellationToken) => throw new NotImplementedException();
            public void UnregisterBufferable(IBufferable bufferable) => throw new NotImplementedException();
            public string ConnectionString { get; }
            public DbConnection DbConnection { get; }
            public Guid ConnectionId { get; }
            public int? CommandTimeout { get; set; }
            public bool Open(bool errorsExpected = false) => true;
            public Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false) => throw new NotImplementedException();
            public bool Close() => true;
            public bool IsMultipleActiveResultSetsEnabled { get; }
            public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel) => throw new NotImplementedException();
            public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IDbContextTransaction UseTransaction(DbTransaction transaction) => throw new NotImplementedException();

            public void Dispose()
            {
            }
        }

        public class BadDataSqliteFixture : NorthwindQuerySqliteFixture<NoopModelCustomizer>
        {
            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddSingleton<IRelationalCommandBuilderFactory, BadDataCommandBuilderFactory>();
        }
    }
}
