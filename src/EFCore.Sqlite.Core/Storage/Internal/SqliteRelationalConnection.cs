// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteRelationalConnection : RelationalConnection, ISqliteRelationalConnection
    {
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly bool _loadSpatialite;
        private readonly bool _enforceForeignKeys = true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteRelationalConnection(
            [NotNull] RelationalConnectionDependencies dependencies,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));

            _rawSqlCommandBuilder = rawSqlCommandBuilder;

            var optionsExtension = dependencies.ContextOptions.Extensions.OfType<SqliteOptionsExtension>().FirstOrDefault();
            if (optionsExtension != null)
            {
                _loadSpatialite = optionsExtension.LoadSpatialite;
                _enforceForeignKeys = optionsExtension.EnforceForeignKeys;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override DbConnection CreateDbConnection() => new SqliteConnection(ConnectionString);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool IsMultipleActiveResultSetsEnabled => true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool Open(bool errorsExpected = false)
        {
            if (base.Open(errorsExpected))
            {
                LoadSpatialite();
                EnableForeignKeys();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
        {
            if (await base.OpenAsync(cancellationToken, errorsExpected))
            {
                LoadSpatialite();
                EnableForeignKeys();
                return true;
            }

            return false;
        }

        private void LoadSpatialite()
        {
            if (!_loadSpatialite)
            {
                return;
            }

            var connection = (SqliteConnection)DbConnection;
            connection.EnableExtensions();
            SpatialiteLoader.Load(DbConnection);
            connection.EnableExtensions(false);
        }

        private void EnableForeignKeys()
        {
            if (_enforceForeignKeys)
            {
                _rawSqlCommandBuilder.Build("PRAGMA foreign_keys=ON;").ExecuteNonQuery(this);
            }
            else
            {
                _rawSqlCommandBuilder.Build("PRAGMA foreign_keys=OFF;").ExecuteNonQuery(this);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ISqliteRelationalConnection CreateReadOnlyConnection()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder(ConnectionString)
            {
                Mode = SqliteOpenMode.ReadOnly
            };

            var contextOptions = new DbContextOptionsBuilder().UseSqlite(connectionStringBuilder.ToString()).Options;

            return new SqliteRelationalConnection(Dependencies.With(contextOptions), _rawSqlCommandBuilder);
        }
    }
}
