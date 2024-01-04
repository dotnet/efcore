// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestRelationalTransactionFactory(RelationalTransactionFactoryDependencies dependencies) : IRelationalTransactionFactory
{
    protected virtual RelationalTransactionFactoryDependencies Dependencies { get; } = dependencies;

    public RelationalTransaction Create(
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
        bool transactionOwned)
        => new TestRelationalTransaction(connection, transaction, logger, transactionOwned, Dependencies.SqlGenerationHelper);
}

public class TestRelationalTransaction(
    IRelationalConnection connection,
    DbTransaction transaction,
    IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
    bool transactionOwned,
    ISqlGenerationHelper sqlGenerationHelper) : RelationalTransaction(connection, transaction, new Guid(), logger, transactionOwned, sqlGenerationHelper)
{
    private readonly TestSqlServerConnection _testConnection = (TestSqlServerConnection)connection;

    public override void Commit()
    {
        if (_testConnection.CommitFailures.Count > 0)
        {
            var fail = _testConnection.CommitFailures.Dequeue();
            if (fail.HasValue)
            {
                if (fail.Value)
                {
                    this.GetDbTransaction().Rollback();
                }
                else
                {
                    this.GetDbTransaction().Commit();
                }

                _testConnection.DbConnection.Close();
                throw SqlExceptionFactory.CreateSqlException(_testConnection.ErrorNumber, _testConnection.ConnectionId);
            }
        }

        base.Commit();
    }

    public override async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_testConnection.CommitFailures.Count > 0)
        {
            var fail = _testConnection.CommitFailures.Dequeue();
            if (fail.HasValue)
            {
                if (fail.Value)
                {
                    await this.GetDbTransaction().RollbackAsync(cancellationToken);
                }
                else
                {
                    await this.GetDbTransaction().CommitAsync(cancellationToken);
                }

                await _testConnection.DbConnection.CloseAsync();
                throw SqlExceptionFactory.CreateSqlException(_testConnection.ErrorNumber, _testConnection.ConnectionId);
            }
        }

        await base.CommitAsync(cancellationToken);
    }

    public override bool SupportsSavepoints
        => true;

    /// <inheritdoc />
    public override void ReleaseSavepoint(string name) { }

    /// <inheritdoc />
    public override Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
