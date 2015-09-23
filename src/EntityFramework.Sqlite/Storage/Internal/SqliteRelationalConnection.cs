// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Infrastructure.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqliteRelationalConnection : RelationalConnection
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly bool _enforceForeignKeys = true;

        public SqliteRelationalConnection(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] IDbContextOptions options,
            [NotNull] ILoggerFactory loggerFactory)
            : base(options, loggerFactory)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _commandBuilderFactory = commandBuilderFactory;

            var optionsExtension = options.Extensions.OfType<SqliteOptionsExtension>().FirstOrDefault();
            if (optionsExtension != null)
            {
                _enforceForeignKeys = optionsExtension.EnforceForeignKeys;
            }
        }

        protected override DbConnection CreateDbConnection() => new SqliteConnection(ConnectionString);

        public override bool IsMultipleActiveResultSetsEnabled => true;

        public override void Open()
        {
            base.Open();
            EnableForeignKeys();
        }

        public override async Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OpenAsync(cancellationToken);
            EnableForeignKeys();
        }

        private void EnableForeignKeys()
        {
            if (!_enforceForeignKeys)
            {
                return;
            }

            _commandBuilderFactory
                .Create()
                .Append("PRAGMA foreign_keys=ON;")
                .BuildRelationalCommand()
                .CreateCommand(this)
                .ExecuteNonQuery();
        }
    }
}
