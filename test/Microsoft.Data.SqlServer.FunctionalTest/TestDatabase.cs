// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Data.SqlServer
{
    public class TestDatabase : IDisposable
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;

        public async Task<TestDatabase> Create(string name = "Microsoft.Data.SqlServer.FunctionalTests")
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                await master.OpenAsync();

                using (var command = new SqlCommand())
                {
                    command.Connection = master;
                    command.CommandText
                        = string.Format(@"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{0}')
                                            CREATE DATABASE [{0}]", name);

                    await command.ExecuteNonQueryAsync();
                }
            }

            _connection = new SqlConnection(CreateConnectionString(name));

            await _connection.OpenAsync();

            _transaction = _connection.BeginTransaction();

            return this;
        }

        public string ConnectionString
        {
            get { return _connection.ConnectionString; }
        }

        public Task<int> Execute(string sql, params object[] parameters)
        {
            using (var command = new SqlCommand())
            {
                InitializeCommand(command, sql, parameters);

                return command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<T>> Query<T>(string sql, params object[] parameters)
        {
            using (var command = new SqlCommand())
            {
                InitializeCommand(command, sql, parameters);

                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    var results = Enumerable.Empty<T>();

                    while (await dataReader.ReadAsync())
                    {
                        results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                    }

                    return results;
                }
            }
        }

        private void InitializeCommand(SqlCommand command, string sql, object[] parameters)
        {
            command.Connection = _connection;
            command.Transaction = _transaction;
            command.CommandText = sql;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("p" + i, parameters[i]);
            }
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
        }

        private static string CreateConnectionString(string name)
        {
            return new SqlConnectionStringBuilder
            {
                DataSource = @".\SQLEXPRESS",
                InitialCatalog = name,
                IntegratedSecurity = true
            }.ConnectionString;
        }
    }
}
