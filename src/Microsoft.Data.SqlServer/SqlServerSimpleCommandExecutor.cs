// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerSimpleCommandExecutor : IDbCommandExecutor
    {
        private const int DefaultCommandTimeout = 1;

        private readonly string _connectionString;
        private readonly int _commandTimeout;

        public SqlServerSimpleCommandExecutor([NotNull] string connectionString)
            : this(Check.NotEmpty(connectionString, "connectionString"), DefaultCommandTimeout)
        {
        }

        public SqlServerSimpleCommandExecutor([NotNull] string connectionString, int commandTimeout)
        {
            Check.NotEmpty(connectionString, "connectionString");

            _connectionString = connectionString;
            _commandTimeout = commandTimeout;
        }

        public async Task<T> ExecuteScalarAsync<T>(string commandText, CancellationToken cancellationToken, params object[] parameters)
        {
            Check.NotEmpty(commandText, "commandText");
            Check.NotNull(parameters, "parameters");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.CommandTimeout = _commandTimeout;

                    return (T)await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
