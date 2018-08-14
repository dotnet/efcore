// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeDbConnection : DbConnection
    {
        private readonly FakeCommandExecutor _commandExecutor;

        private ConnectionState _state;
        private readonly List<FakeDbCommand> _dbCommands = new List<FakeDbCommand>();
        private readonly List<FakeDbTransaction> _dbTransactions = new List<FakeDbTransaction>();

        public FakeDbConnection(
            string connectionString,
            FakeCommandExecutor commandExecutor = null,
            ConnectionState state = ConnectionState.Closed)
        {
            ConnectionString = connectionString;
            _commandExecutor = commandExecutor ?? new FakeCommandExecutor();
            _state = state;
        }

        public void SetState(ConnectionState state)
            => _state = state;

        public override ConnectionState State => _state;

        public IReadOnlyList<FakeDbCommand> DbCommands => _dbCommands;

        public override string ConnectionString { get; set; }

        public override string Database { get; } = "Fake Database";

        public override string DataSource { get; } = "Fake DataSource";

        public override string ServerVersion => throw new NotImplementedException();

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

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

        public IReadOnlyList<FakeDbTransaction> DbTransactions => _dbTransactions;

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
}
