// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class SqlStatementExecutor
    {
        public virtual async Task ExecuteNonQueryAsync([NotNull] DbConnection connection, [NotNull] IEnumerable<SqlStatement> statements, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(statements, "statements");

            // TODO Deal with suppressing transactions etc.

            var closeConnection = false;
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
                closeConnection = true;
            }

            try
            {
                foreach (var item in statements)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = item.Sql;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            finally
            {
                if (closeConnection)
                {
                    connection.Close();
                }
            }
        }
    }
}
