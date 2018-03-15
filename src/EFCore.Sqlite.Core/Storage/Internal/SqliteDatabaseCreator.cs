// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDatabaseCreator : RelationalDatabaseCreator
    {
        // ReSharper disable once InconsistentNaming
        private const int SQLITE_CANTOPEN = 14;

        private readonly ISqliteRelationalConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteDatabaseCreator(
            [NotNull] RelationalDatabaseCreatorDependencies dependencies,
            [NotNull] ISqliteRelationalConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Create()
        {
            Dependencies.Connection.Open();
            Dependencies.Connection.Close();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool Exists()
        {
            using (var readOnlyConnection = _connection.CreateReadOnlyConnection())
            {
                try
                {
                    readOnlyConnection.Open(errorsExpected: true);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == SQLITE_CANTOPEN)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool HasTables()
        {
            var count = (long)_rawSqlCommandBuilder
                .Build("SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"type\" = 'table' AND \"rootpage\" IS NOT NULL;")
                .ExecuteScalar(Dependencies.Connection);

            return count != 0;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Delete()
        {
            string path = null;

            Dependencies.Connection.Open();
            try
            {
                path = Dependencies.Connection.DbConnection.DataSource;
            }
            catch
            {
                // any exceptions here can be ignored
            }
            finally
            {
                Dependencies.Connection.Close();
            }

            if (!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }
    }
}
