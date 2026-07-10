// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class BadDataSqliteTest(BadDataSqliteTest.BadDataSqliteFixture fixture) : IClassFixture<BadDataSqliteTest.BadDataSqliteFixture>
{
    public BadDataSqliteFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public void Bad_data_error_handling_invalid_cast_key()
    {
        using var context = CreateContext("bad int");
        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    context.Set<Product>().Where(p => p.ProductID != 1).ToList()).Message);
    }

    [ConditionalFact]
    public void Bad_data_error_handling_null_key()
    {
        using var context = CreateContext(null, true);
        Assert.Equal(
            RelationalStrings.ErrorMaterializingPropertyNullReference("Product", "ProductID", typeof(int)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    context.Set<Product>().Where(p => p.ProductID != 2).ToList()).Message);
    }

    [ConditionalFact]
    public void Bad_data_error_handling_invalid_cast()
    {
        using var context = CreateContext(1, true, 1);
        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductName", typeof(string), typeof(int)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    context.Set<Product>().Where(p => p.ProductID != 3).ToList()).Message);
    }

    [ConditionalFact]
    public void Bad_data_error_handling_invalid_cast_projection()
    {
        using var context = CreateContext(1);
        Assert.Equal(
            RelationalStrings.ErrorMaterializingValueInvalidCast(typeof(string), typeof(int)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    context.Set<Product>().Where(p => p.ProductID != 4)
                        .Select(p => p.ProductName)
                        .ToList()).Message);
    }

    [ConditionalFact]
    public void Bad_data_error_handling_invalid_cast_no_tracking()
    {
        using var context = CreateContext("bad int");
        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    context.Set<Product>()
                        .Where(p => p.ProductID != 5)
                        .AsNoTracking()
                        .ToList()).Message);
    }

    [ConditionalFact]
    public void Bad_data_error_handling_null()
    {
        using var context = CreateContext(1, null);
        Assert.Equal(
            RelationalStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    context.Set<Product>().Where(p => p.ProductID != 6).ToList()).Message);
    }

    [ConditionalFact]
    public void Bad_data_error_handling_null_projection()
    {
        using var context = CreateContext([null]);
        Assert.Equal(
            RelationalStrings.ErrorMaterializingValueNullReference(typeof(bool)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    context.Set<Product>()
                        .Where(p => p.ProductID != 7)
                        .Select(p => p.Discontinued)
                        .ToList()).Message);
    }

    [ConditionalFact]
    public void Bad_data_error_handling_null_no_tracking()
    {
        using var context = CreateContext(null, true);
        Assert.Equal(
            RelationalStrings.ErrorMaterializingPropertyNullReference("Product", "ProductID", typeof(int)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    context.Set<Product>()
                        .Where(p => p.ProductID != 8)
                        .AsNoTracking()
                        .ToList()).Message);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class BadDataCommandBuilderFactory(
        RelationalCommandBuilderDependencies dependencies) : RelationalCommandBuilderFactory(dependencies)
    {
        public object[] Values { private get; set; }

        public override IRelationalCommandBuilder Create()
            => new BadDataRelationalCommandBuilder(Dependencies, Values);

        private class BadDataRelationalCommandBuilder(
            RelationalCommandBuilderDependencies dependencies,
            object[] values) : RelationalCommandBuilder(dependencies)
        {
            private readonly object[] _values = values;

            public override IRelationalCommand Build()
                => new BadDataRelationalCommand(Dependencies, ToString(), Parameters, _values);

            private class BadDataRelationalCommand(
                RelationalCommandBuilderDependencies dependencies,
                string commandText,
                IReadOnlyList<IRelationalParameter> parameters,
                object[] values) : RelationalCommand(dependencies, commandText, parameters)
            {
                private object[] _values = values;

                public override RelationalDataReader ExecuteReader(
                    RelationalCommandParameterObject parameterObject)
                {
                    var command = parameterObject.Connection.DbConnection.CreateCommand();
                    command.CommandText = CommandText;
                    var reader = new BadDataRelationalDataReader();
                    reader.Initialize(
                        new FakeConnection(),
                        command,
                        new BadDataDataReader(_values),
                        Guid.NewGuid(),
                        parameterObject.Logger);
                    return reader;
                }

                public override void PopulateFrom(IRelationalCommandTemplate commandTemplate)
                {
                    base.PopulateFrom(commandTemplate);
                    _values = ((BadDataRelationalCommand)commandTemplate)._values;
                }

                private class BadDataRelationalDataReader : RelationalDataReader;

                private class BadDataDataReader(object[] values) : DbDataReader
                {
                    private readonly object[] _values = values;

                    public override bool Read()
                        => true;

                    public override bool IsDBNull(int ordinal)
                        => false;

                    public override int GetInt32(int ordinal)
                        => (int)GetValue(ordinal);

                    public override short GetInt16(int ordinal)
                        => (short)GetValue(ordinal);

                    public override bool GetBoolean(int ordinal)
                        => (bool)GetValue(ordinal);

                    public override string GetString(int ordinal)
                        => (string)GetValue(ordinal);

                    public override object GetValue(int ordinal)
                        => _values[ordinal];

                    #region NotImplemented members

                    public override string GetName(int ordinal)
                        => throw new NotImplementedException();

                    public override int GetValues(object[] values)
                        => throw new NotImplementedException();

                    public override int FieldCount
                        => throw new NotImplementedException();

                    public override object this[int ordinal]
                        => throw new NotImplementedException();

                    public override object this[string name]
                        => throw new NotImplementedException();

                    public override bool HasRows
                        => throw new NotImplementedException();

                    public override bool IsClosed
                        => throw new NotImplementedException();

                    public override int RecordsAffected
                        => 0;

                    public override bool NextResult()
                        => throw new NotImplementedException();

                    public override int Depth
                        => throw new NotImplementedException();

                    public override int GetOrdinal(string name)
                        => throw new NotImplementedException();

                    public override byte GetByte(int ordinal)
                        => throw new NotImplementedException();

                    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
                        => throw new NotImplementedException();

                    public override char GetChar(int ordinal)
                        => throw new NotImplementedException();

                    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
                        => throw new NotImplementedException();

                    public override Guid GetGuid(int ordinal)
                        => throw new NotImplementedException();

                    public override long GetInt64(int ordinal)
                        => throw new NotImplementedException();

                    public override DateTime GetDateTime(int ordinal)
                        => throw new NotImplementedException();

                    public override decimal GetDecimal(int ordinal)
                        => throw new NotImplementedException();

                    public override double GetDouble(int ordinal)
                        => throw new NotImplementedException();

                    public override float GetFloat(int ordinal)
                        => throw new NotImplementedException();

                    public override string GetDataTypeName(int ordinal)
                        => throw new NotImplementedException();

                    public override Type GetFieldType(int ordinal)
                        => throw new NotImplementedException();

                    public override IEnumerator GetEnumerator()
                        => throw new NotImplementedException();

                    #endregion
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

        public Task ResetStateAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public IDbContextTransaction BeginTransaction()
            => throw new NotImplementedException();

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public void CommitTransaction() { }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void RollbackTransaction() { }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public IDbContextTransaction CurrentTransaction
            => throw new NotImplementedException();

        public SemaphoreSlim Semaphore { get; }

        public string ConnectionString { get; set; }
        public DbConnection DbConnection { get; set; } = new SqliteConnection();

        public void SetDbConnection(DbConnection value, bool contextOwnsConnection)
            => throw new NotImplementedException();

        public DbContext Context
            => null;

        public Guid ConnectionId { get; }
        public int? CommandTimeout { get; set; }

        public bool Open(bool errorsExpected = false)
            => true;

        public Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
            => throw new NotImplementedException();

        public bool Close()
            => true;

        public Task<bool> CloseAsync()
            => Task.FromResult(true);

        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
            => throw new NotImplementedException();

        public Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IDbContextTransaction UseTransaction(DbTransaction transaction)
            => throw new NotImplementedException();

        public IDbContextTransaction UseTransaction(DbTransaction transaction, Guid transactionId)
            => throw new NotImplementedException();

        public Task<IDbContextTransaction> UseTransactionAsync(
            DbTransaction transaction,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IDbContextTransaction> UseTransactionAsync(
            DbTransaction transaction,
            Guid transactionId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IRelationalCommand RentCommand()
            => throw new NotImplementedException();

        public void ReturnCommand(IRelationalCommand command)
            => throw new NotImplementedException();

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
            => default;
    }

    public class BadDataSqliteFixture : NorthwindQuerySqliteFixture<NoopModelCustomizer>
    {
        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<IRelationalCommandBuilderFactory, BadDataCommandBuilderFactory>();
    }
}
