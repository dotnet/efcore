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
        public const int CommandTimeout = 1;

        private SqlConnection _connection;
        private SqlTransaction _transaction;

        public async Task<TestDatabase> Create(string name = "Microsoft.Data.SqlServer.FunctionalTest", bool transactional = true)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                await master.OpenAsync();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText
                        = string.Format(@"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{0}')
                                            CREATE DATABASE [{0}]", name);

                    await command.ExecuteNonQueryAsync();
                }
            }

            _connection = new SqlConnection(CreateConnectionString(name));

            await _connection.OpenAsync();

            if (transactional)
            {
                _transaction = _connection.BeginTransaction();
            }

            return this;
        }

        public SqlTransaction Transaction
        {
            get { return _transaction; }
        }

        public Task<int> ExecuteAsync(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
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

        private SqlCommand CreateCommand(string sql, object[] parameters)
        {
            var command = _connection.CreateCommand();

            if (_transaction != null)
            {
                command.Transaction = _transaction;
            }

            command.CommandText = sql;
            command.CommandTimeout = CommandTimeout;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("p" + i, parameters[i]);
            }

            return command;
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }

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
