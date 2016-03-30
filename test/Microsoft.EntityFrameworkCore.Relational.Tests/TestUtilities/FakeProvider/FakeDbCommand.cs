// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider
{
    public class FakeDbCommand : DbCommand
    {
        private readonly FakeCommandExecutor _commandExecutor;

        public FakeDbCommand()
        {
        }

        public FakeDbCommand(
            FakeDbConnection connection,
            FakeCommandExecutor commandExecutor)
        {
            DbConnection = connection;
            _commandExecutor = commandExecutor;
        }

        protected override DbConnection DbConnection { get; set; }

        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override string CommandText { get; set; }

        public static int DefaultCommandTimeout = 30;

        public override int CommandTimeout { get; set; } = DefaultCommandTimeout;

        public override CommandType CommandType { get; set; }

        protected override DbParameter CreateDbParameter()
            => new FakeDbParameter();

        protected override DbParameterCollection DbParameterCollection { get; }
            = new FakeDbParameterCollection();

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
            => _commandExecutor.ExecuteNonQuery(this);

        public override object ExecuteScalar()
            => _commandExecutor.ExecuteScalar(this);

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => _commandExecutor.ExecuteReader(this, behavior);

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
            => _commandExecutor.ExecuteNonQueryAsync(this, cancellationToken);

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
            => _commandExecutor.ExecuteScalarAsync(this, cancellationToken);

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
            => _commandExecutor.ExecuteReaderAsync(this, behavior, cancellationToken);

        public override bool DesignTimeVisible
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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
