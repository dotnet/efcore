// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeDbTransaction : DbTransaction
    {
        public FakeDbTransaction(FakeDbConnection connection, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            DbConnection = connection;
            IsolationLevel = isolationLevel;
        }

        protected override DbConnection DbConnection { get; }

        public override IsolationLevel IsolationLevel { get; }

        public int CommitCount { get; private set; }

        public override void Commit()
            => CommitCount++;

        public int RollbackCount { get; private set; }

        public override void Rollback()
            => RollbackCount++;

        public int DisposeCount { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;

                ((FakeDbConnection)DbConnection).ActiveTransaction = null;
            }

            base.Dispose(disposing);
        }
    }
}
