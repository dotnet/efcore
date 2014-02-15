// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Microsoft.Data.SQLite
{
    public class SQLiteTransaction : DbTransaction
    {
        private readonly SQLiteConnection _connection;
        private readonly IsolationLevel _isolationLevel;

        internal SQLiteTransaction(SQLiteConnection connection, IsolationLevel isolationLevel)
        {
            Debug.Assert(connection != null, "connection is null.");
            Debug.Assert(
                isolationLevel == IsolationLevel.ReadUncommitted || isolationLevel == IsolationLevel.Serializable,
                "isolationLevel is not ReadUncommitted or Serializable");
            Debug.Assert(connection.State == ConnectionState.Open, "connection.State is not Open.");

            _connection = connection;
            _isolationLevel = isolationLevel;

            // TODO: Consider nested transactions
            // TODO: BEGIN TRANSACTION
        }

        public override void Commit()
        {
            // TODO: COMMIT
        }

        protected override DbConnection DbConnection
        {
            get { return _connection; }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
        }

        public override void Rollback()
        {
            // TODO: ROLLBACK
        }
    }
}
