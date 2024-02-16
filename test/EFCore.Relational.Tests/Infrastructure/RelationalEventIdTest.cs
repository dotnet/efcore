// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data;
using System.Transactions;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Index = Microsoft.EntityFrameworkCore.Metadata.Internal.Index;
using IsolationLevel = System.Data.IsolationLevel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class RelationalEventIdTest : EventIdTestBase
{
    [ConditionalFact]
    public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
    {
        var constantExpression = Expression.Constant("A");
        var model = new Model();
        var entityType = new EntityType(typeof(object), model, owned: false, ConfigurationSource.Convention);
        var property = entityType.AddProperty("A", typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);
        var key = entityType.AddKey(property, ConfigurationSource.Convention);
        var foreignKey = new ForeignKey(new List<Property> { property }, key, entityType, entityType, ConfigurationSource.Convention);
        var index = new Index(new List<Property> { property }, "IndexName", entityType, ConfigurationSource.Convention);
        var contextServices = FakeRelationalTestHelpers.Instance.CreateContextServices(model.FinalizeModel());
        var updateEntry = new InternalEntityEntry(contextServices.GetRequiredService<IStateManager>(), entityType, new object());
        var columnOperation = new AddColumnOperation
        {
            Name = "Column1",
            Table = "Table1",
            ClrType = typeof(int)
        };

        var fakeFactories = new Dictionary<Type, Func<object>>
        {
            { typeof(string), () => "Fake" },
            { typeof(IList<string>), () => new List<string> { "Fake1", "Fake2" } },
            { typeof(IReadOnlyList<string>), () => new List<string> { "Fake1", "Fake2" } },
            {
                typeof(IEnumerable<IUpdateEntry>), () => new List<IUpdateEntry>
                {
                    new InternalEntityEntry(
                        contextServices.GetRequiredService<IStateManager>(),
                        entityType,
                        new object())
                }
            },
            { typeof(IRelationalConnection), () => new FakeRelationalConnection() },
            { typeof(LoggingDefinitions), () => new TestRelationalLoggingDefinitions() },
            { typeof(DbCommand), () => new FakeDbCommand() },
            { typeof(DbTransaction), () => new FakeDbTransaction() },
            { typeof(DbDataReader), () => new FakeDbDataReader() },
            { typeof(Transaction), () => new CommittableTransaction() },
            { typeof(IMigrator), () => new FakeMigrator() },
            { typeof(Migration), () => new FakeMigration() },
            { typeof(IMigrationsAssembly), () => new FakeMigrationsAssembly() },
            { typeof(MethodCallExpression), () => Expression.Call(constantExpression, typeof(object).GetMethod("ToString")) },
            { typeof(Expression), () => constantExpression },
            { typeof(IEntityType), () => entityType },
            { typeof(IProperty), () => property },
            { typeof(IKey), () => key },
            { typeof(IForeignKey), () => foreignKey },
            { typeof(IIndex), () => index },
            { typeof(TypeInfo), () => typeof(object).GetTypeInfo() },
            { typeof(Type), () => typeof(object) },
            { typeof(ValueConverter), () => new BoolToZeroOneConverter<int>() },
            { typeof(DbContext), () => new FakeDbContext() },
            { typeof(SqlExpression), () => new FakeSqlExpression() },
            { typeof(IUpdateEntry), () => updateEntry },
            { typeof(ColumnOperation), () => columnOperation }
        };

        TestEventLogging(
            typeof(RelationalEventId),
            typeof(RelationalLoggerExtensions),
            [typeof(IRelationalConnectionDiagnosticsLogger), typeof(IRelationalCommandDiagnosticsLogger)],
            new TestRelationalLoggingDefinitions(),
            fakeFactories,
            services => FakeRelationalOptionsExtension.AddEntityFrameworkRelationalDatabase(services),
            new Dictionary<string, IList<string>>
            {
                {
                    nameof(RelationalEventId.CommandExecuting),
                    new List<string>
                    {
                        nameof(IRelationalCommandDiagnosticsLogger.CommandReaderExecuting),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandScalarExecuting),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandNonQueryExecuting),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandReaderExecutingAsync),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandScalarExecutingAsync),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandNonQueryExecutingAsync)
                    }
                },
                {
                    nameof(RelationalEventId.CommandExecuted),
                    new List<string>
                    {
                        nameof(IRelationalCommandDiagnosticsLogger.CommandReaderExecutedAsync),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandScalarExecutedAsync),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandNonQueryExecutedAsync),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandReaderExecuted),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandScalarExecuted),
                        nameof(IRelationalCommandDiagnosticsLogger.CommandNonQueryExecuted)
                    }
                }
            });
    }

    private class FakeDbContext : DbContext;

    [Migration("00000000000000_FakeMigration")]
    private class FakeMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
            => throw new NotImplementedException();
    }

    private class FakeSqlExpression : SqlExpression
    {
        public FakeSqlExpression()
            : base(typeof(object), null)
        {
        }

        public override Expression Quote()
            => throw new NotSupportedException();

        protected override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append("FakeSqlExpression");
    }

    private class FakeMigrator : IMigrator
    {
        public void Migrate(string targetMigration = null)
            => throw new NotImplementedException();

        public Task MigrateAsync(string targetMigration = null, CancellationToken cancellationToken = new())
            => throw new NotImplementedException();

        public string GenerateScript(
            string fromMigration = null,
            string toMigration = null,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
            => throw new NotImplementedException();
    }

    private class FakeMigrationsAssembly : IMigrationsAssembly
    {
        public IReadOnlyDictionary<string, TypeInfo> Migrations
            => throw new NotImplementedException();

        public ModelSnapshot ModelSnapshot
            => throw new NotImplementedException();

        public Assembly Assembly
            => typeof(FakeMigrationsAssembly).Assembly;

        public Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
            => throw new NotImplementedException();

        public string FindMigrationId(string nameOrId)
            => throw new NotImplementedException();
    }

    private class FakeRelationalConnection : IRelationalConnection
    {
        public string ConnectionString { get; set; }

        public DbConnection DbConnection { get; set; } = new FakeDbConnection();

        public void SetDbConnection(DbConnection value, bool contextOwnsConnection)
            => throw new NotImplementedException();

        public DbContext Context
            => null;

        public Guid ConnectionId
            => Guid.NewGuid();

        public int? CommandTimeout { get; set; }

        public Task<bool> CloseAsync()
            => throw new NotImplementedException();

        public IDbContextTransaction CurrentTransaction
            => throw new NotImplementedException();

        public SemaphoreSlim Semaphore
            => throw new NotImplementedException();

        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
            => throw new NotImplementedException();

        public IDbContextTransaction BeginTransaction()
            => throw new NotImplementedException();

        public Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public bool Close()
            => throw new NotImplementedException();

        public void CommitTransaction()
            => throw new NotImplementedException();

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public void Dispose()
            => throw new NotImplementedException();

        public bool Open(bool errorsExpected = false)
            => throw new NotImplementedException();

        public Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
            => throw new NotImplementedException();

        public void ResetState()
            => throw new NotImplementedException();

        public Task ResetStateAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public void RollbackTransaction()
            => throw new NotImplementedException();

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
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

        public ValueTask DisposeAsync()
            => throw new NotImplementedException();

        public IRelationalCommand RentCommand()
            => throw new NotImplementedException();

        public void ReturnCommand(IRelationalCommand command)
            => throw new NotImplementedException();
    }

    private class FakeDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; }

        public override string Database
            => "Database";

        public override string DataSource
            => "DataSource";

        public override string ServerVersion
            => throw new NotImplementedException();

        public override ConnectionState State
            => throw new NotImplementedException();

        public override void ChangeDatabase(string databaseName)
            => throw new NotImplementedException();

        public override void Close()
            => throw new NotImplementedException();

        public override void Open()
            => throw new NotImplementedException();

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => throw new NotImplementedException();

        protected override DbCommand CreateDbCommand()
            => throw new NotImplementedException();
    }

    private class FakeDbCommand : DbCommand
    {
        public override string CommandText
        {
            get => "CommandText";
            set { }
        }

        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; } = new FakeDbConnection();

        protected override DbParameterCollection DbParameterCollection
            => new FakeDbParameterCollection();

        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel()
            => throw new NotImplementedException();

        public override int ExecuteNonQuery()
            => throw new NotImplementedException();

        public override object ExecuteScalar()
            => throw new NotImplementedException();

        public override void Prepare()
            => throw new NotImplementedException();

        protected override DbParameter CreateDbParameter()
            => throw new NotImplementedException();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => throw new NotImplementedException();
    }

    private class FakeDbParameterCollection : DbParameterCollection
    {
        public override int Count
            => 0;

        public override object SyncRoot
            => throw new NotImplementedException();

        public override int Add(object value)
            => throw new NotImplementedException();

        public override void AddRange(Array values)
            => throw new NotImplementedException();

        public override void Clear()
            => throw new NotImplementedException();

        public override bool Contains(object value)
            => throw new NotImplementedException();

        public override bool Contains(string value)
            => throw new NotImplementedException();

        public override void CopyTo(Array array, int index)
            => throw new NotImplementedException();

        public override IEnumerator GetEnumerator()
            => new List<object>().GetEnumerator();

        public override int IndexOf(object value)
            => throw new NotImplementedException();

        public override int IndexOf(string parameterName)
            => throw new NotImplementedException();

        public override void Insert(int index, object value)
            => throw new NotImplementedException();

        public override void Remove(object value)
            => throw new NotImplementedException();

        public override void RemoveAt(int index)
            => throw new NotImplementedException();

        public override void RemoveAt(string parameterName)
            => throw new NotImplementedException();

        protected override DbParameter GetParameter(int index)
            => throw new NotImplementedException();

        protected override DbParameter GetParameter(string parameterName)
            => throw new NotImplementedException();

        protected override void SetParameter(int index, DbParameter value)
            => throw new NotImplementedException();

        protected override void SetParameter(string parameterName, DbParameter value)
            => throw new NotImplementedException();
    }

    private class FakeDbTransaction : DbTransaction
    {
        public override IsolationLevel IsolationLevel
            => IsolationLevel.Chaos;

        protected override DbConnection DbConnection
            => throw new NotImplementedException();

        public override void Commit()
            => throw new NotImplementedException();

        public override void Rollback()
            => throw new NotImplementedException();
    }

    private class FakeDbDataReader : DbDataReader
    {
        public override bool GetBoolean(int ordinal)
            => throw new NotImplementedException();

        public override byte GetByte(int ordinal)
            => throw new NotImplementedException();

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
            => throw new NotImplementedException();

        public override char GetChar(int ordinal)
            => throw new NotImplementedException();

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
            => throw new NotImplementedException();

        public override string GetDataTypeName(int ordinal)
            => throw new NotImplementedException();

        public override DateTime GetDateTime(int ordinal)
            => throw new NotImplementedException();

        public override decimal GetDecimal(int ordinal)
            => throw new NotImplementedException();

        public override double GetDouble(int ordinal)
            => throw new NotImplementedException();

        public override Type GetFieldType(int ordinal)
            => throw new NotImplementedException();

        public override float GetFloat(int ordinal)
            => throw new NotImplementedException();

        public override Guid GetGuid(int ordinal)
            => throw new NotImplementedException();

        public override short GetInt16(int ordinal)
            => throw new NotImplementedException();

        public override int GetInt32(int ordinal)
            => throw new NotImplementedException();

        public override long GetInt64(int ordinal)
            => throw new NotImplementedException();

        public override string GetName(int ordinal)
            => throw new NotImplementedException();

        public override int GetOrdinal(string name)
            => throw new NotImplementedException();

        public override string GetString(int ordinal)
            => throw new NotImplementedException();

        public override object GetValue(int ordinal)
            => throw new NotImplementedException();

        public override int GetValues(object[] values)
            => throw new NotImplementedException();

        public override bool IsDBNull(int ordinal)
            => throw new NotImplementedException();

        public override int FieldCount
            => throw new NotImplementedException();

        public override object this[int ordinal]
            => throw new NotImplementedException();

        public override object this[string name]
            => throw new NotImplementedException();

        public override int RecordsAffected
            => throw new NotImplementedException();

        public override bool HasRows
            => throw new NotImplementedException();

        public override bool IsClosed
            => throw new NotImplementedException();

        public override bool NextResult()
            => throw new NotImplementedException();

        public override bool Read()
            => throw new NotImplementedException();

        public override int Depth
            => throw new NotImplementedException();

        public override IEnumerator GetEnumerator()
            => throw new NotImplementedException();
    }
}
