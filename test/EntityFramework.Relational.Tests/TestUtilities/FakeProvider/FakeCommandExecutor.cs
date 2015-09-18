// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.TestUtilities.FakeProvider
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
                ?? new Func<FakeDbCommand, int>(c => -1);

            _executeScalar = executeScalar
                ?? new Func<FakeDbCommand, object>(c => null);

            _executeReader = executeReader
                ?? new Func<FakeDbCommand, CommandBehavior, DbDataReader>((c, b) => null);

            _executeNonQueryAsync = executeNonQueryAsync
                ?? new Func<FakeDbCommand, CancellationToken, Task<int>>((c, ct) => Task.FromResult(-1));

            _executeScalarAsync = executeScalarAsync
                ?? new Func<FakeDbCommand, CancellationToken, Task<object>>((c, ct) => Task.FromResult<object>(null));

            _executeReaderAsync = executeReaderAsync
                ?? new Func<FakeDbCommand, CommandBehavior, CancellationToken, Task<DbDataReader>>((c, ct, b) => Task.FromResult<DbDataReader>(null));
        }

        public virtual int ExecuteNonQuery(FakeDbCommand command) => _executeNonQuery(command);

        public virtual object ExecuteScalar(FakeDbCommand command) => _executeScalar(command);

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
