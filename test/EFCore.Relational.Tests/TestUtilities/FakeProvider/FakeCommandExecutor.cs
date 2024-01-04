// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeCommandExecutor(
    Func<FakeDbCommand, int> executeNonQuery = null,
    Func<FakeDbCommand, object> executeScalar = null,
    Func<FakeDbCommand, CommandBehavior, DbDataReader> executeReader = null,
    Func<FakeDbCommand, CancellationToken, Task<int>> executeNonQueryAsync = null,
    Func<FakeDbCommand, CancellationToken, Task<object>> executeScalarAsync = null,
    Func<FakeDbCommand, CommandBehavior, CancellationToken, Task<DbDataReader>> executeReaderAsync = null)
{
    private readonly Func<FakeDbCommand, int> _executeNonQuery = executeNonQuery
            ?? (c => -1);
    private readonly Func<FakeDbCommand, object> _executeScalar = executeScalar
            ?? (c => null);
    private readonly Func<FakeDbCommand, CommandBehavior, DbDataReader> _executeReader = executeReader
            ?? ((c, b) => new FakeDbDataReader());
    private readonly Func<FakeDbCommand, CancellationToken, Task<int>> _executeNonQueryAsync = executeNonQueryAsync
            ?? ((c, ct) => Task.FromResult(-1));
    private readonly Func<FakeDbCommand, CancellationToken, Task<object>> _executeScalarAsync = executeScalarAsync
            ?? ((c, ct) => Task.FromResult<object>(null));
    private readonly Func<FakeDbCommand, CommandBehavior, CancellationToken, Task<DbDataReader>> _executeReaderAsync = executeReaderAsync
            ?? ((c, ct, b) => Task.FromResult<DbDataReader>(new FakeDbDataReader()));

    public virtual int ExecuteNonQuery(FakeDbCommand command)
        => _executeNonQuery(command);

    public virtual object ExecuteScalar(FakeDbCommand command)
        => _executeScalar(command);

    public virtual DbDataReader ExecuteReader(FakeDbCommand command, CommandBehavior behavior)
        => _executeReader(command, behavior);

    public Task<int> ExecuteNonQueryAsync(FakeDbCommand command, CancellationToken cancellationToken)
        => _executeNonQueryAsync(command, cancellationToken);

    public Task<object> ExecuteScalarAsync(FakeDbCommand command, CancellationToken cancellationToken)
        => _executeScalarAsync(command, cancellationToken);

    public Task<DbDataReader> ExecuteReaderAsync(FakeDbCommand command, CommandBehavior behavior, CancellationToken cancellationToken)
        => _executeReaderAsync(command, behavior, cancellationToken);
}
