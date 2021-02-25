// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeCommandExecutor
    {
        private readonly Func<FakeDbCommand, int> _executeNonQuery;
        private readonly Func<FakeDbCommand, object> _executeScalar;
        private readonly Func<FakeDbCommand, CommandBehavior, DbDataReader> _executeReader;
        private readonly Func<FakeDbCommand, CancellationToken, Task<int>> _executeNonQueryAsync;
        private readonly Func<FakeDbCommand, CancellationToken, Task<object>> _executeScalarAsync;
        private readonly Func<FakeDbCommand, CommandBehavior, CancellationToken, Task<DbDataReader>> _executeReaderAsync;

        public FakeCommandExecutor(
            Func<FakeDbCommand, int> executeNonQuery = null,
            Func<FakeDbCommand, object> executeScalar = null,
            Func<FakeDbCommand, CommandBehavior, DbDataReader> executeReader = null,
            Func<FakeDbCommand, CancellationToken, Task<int>> executeNonQueryAsync = null,
            Func<FakeDbCommand, CancellationToken, Task<object>> executeScalarAsync = null,
            Func<FakeDbCommand, CommandBehavior, CancellationToken, Task<DbDataReader>> executeReaderAsync = null)
        {
            _executeNonQuery = executeNonQuery
                ?? (c => -1);

            _executeScalar = executeScalar
                ?? (c => null);

            _executeReader = executeReader
                ?? ((c, b) => new FakeDbDataReader());

            _executeNonQueryAsync = executeNonQueryAsync
                ?? ((c, ct) => Task.FromResult(-1));

            _executeScalarAsync = executeScalarAsync
                ?? ((c, ct) => Task.FromResult<object>(null));

            _executeReaderAsync = executeReaderAsync
                ?? ((c, ct, b) => Task.FromResult<DbDataReader>(new FakeDbDataReader()));
        }

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
}
