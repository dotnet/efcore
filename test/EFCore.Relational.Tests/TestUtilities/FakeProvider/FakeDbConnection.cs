// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeDbConnection(
    string connectionString,
    FakeCommandExecutor commandExecutor = null,
    ConnectionState state = ConnectionState.Closed) : DbConnection
{
    private readonly FakeCommandExecutor _commandExecutor = commandExecutor ?? new FakeCommandExecutor();

    private ConnectionState _state = state;
    private readonly List<FakeDbCommand> _dbCommands = [];
    private readonly List<FakeDbTransaction> _dbTransactions = [];

    public void SetState(ConnectionState state)
        => _state = state;

    public override ConnectionState State
        => _state;

    public IReadOnlyList<FakeDbCommand> DbCommands
        => _dbCommands;

    public override string ConnectionString { get; set; } = connectionString;

    public override string Database { get; } = "Fake Database";

    public override string DataSource { get; } = "Fake DataSource";

    public override string ServerVersion
        => throw new NotImplementedException();

    public override void ChangeDatabase(string databaseName)
        => throw new NotImplementedException();

    public int OpenCount { get; private set; }

    public override void Open()
    {
        OpenCount++;
        _state = ConnectionState.Open;
    }

    public int OpenAsyncCount { get; private set; }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        OpenAsyncCount++;
        return base.OpenAsync(cancellationToken);
    }

    public int CloseCount { get; private set; }

    public override void Close()
    {
        CloseCount++;
        _state = ConnectionState.Closed;
    }

    protected override DbCommand CreateDbCommand()
    {
        var command = new FakeDbCommand(this, _commandExecutor);

        _dbCommands.Add(command);

        return command;
    }

    public IReadOnlyList<FakeDbTransaction> DbTransactions
        => _dbTransactions;

    public FakeDbTransaction ActiveTransaction { get; set; }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        ActiveTransaction = new FakeDbTransaction(this, isolationLevel);

        _dbTransactions.Add(ActiveTransaction);

        return ActiveTransaction;
    }

    public int DisposeCount { get; private set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeCount++;
        }

        base.Dispose(disposing);
    }
}
