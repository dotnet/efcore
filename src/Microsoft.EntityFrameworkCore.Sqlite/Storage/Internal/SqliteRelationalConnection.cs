// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteRelationalConnection : RelationalConnection
    {
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly bool _enforceForeignKeys = true;
        private int _openedCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteRelationalConnection(
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IDbContextOptions options,
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] ILogger<SqliteRelationalConnection> logger)
            : base(options, logger)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));

            _rawSqlCommandBuilder = rawSqlCommandBuilder;

            var optionsExtension = options.Extensions.OfType<SqliteOptionsExtension>().FirstOrDefault();
            if (optionsExtension != null)
            {
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
        public override void Open()
        {
            base.Open();

            _openedCount++;

            if (_openedCount == 1)
            {
                EnableForeignKeys();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OpenAsync(cancellationToken);

            _openedCount++;

            if (_openedCount == 1)
            {
                EnableForeignKeys();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Close()
        {
            base.Close();

            _openedCount--;
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
        public virtual SqliteRelationalConnection CreateReadOnlyConnection()
        {
            var builder = new SqliteConnectionStringBuilder(ConnectionString)
            {
                Mode = SqliteOpenMode.ReadOnly
            };

            var options = new DbContextOptionsBuilder();
            options.UseSqlite(builder.ToString());

            return new SqliteRelationalConnection(
                _rawSqlCommandBuilder,
                options.Options,
                (ILogger<SqliteRelationalConnection>)Logger);
        }
    }
}
