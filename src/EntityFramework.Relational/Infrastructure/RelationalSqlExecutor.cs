// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class RelationalSqlExecutor
    {
        private readonly ISqlCommandBuilder _sqlCommandBuilder;
        private readonly ISqlStatementExecutor _statementExecutor;
        private readonly IRelationalConnection _connection;

        public RelationalSqlExecutor(
            [NotNull] ISqlCommandBuilder sqlCommandBuilder,
            [NotNull] ISqlStatementExecutor statementExecutor,
            [NotNull] IRelationalConnection connection)
        {
            Check.NotNull(sqlCommandBuilder, nameof(sqlCommandBuilder));
            Check.NotNull(statementExecutor, nameof(statementExecutor));
            Check.NotNull(connection, nameof(connection));

            _sqlCommandBuilder = sqlCommandBuilder;
            _statementExecutor = statementExecutor;
            _connection = connection;
        }

        public virtual void ExecuteSqlCommand([NotNull] string sql, [NotNull] params object[] parameters)
        => _statementExecutor.ExecuteNonQuery(
            _connection,
            new[]
            {
                _sqlCommandBuilder.Build(
                    Check.NotNull(sql, nameof(sql)),
                    Check.NotNull(parameters, nameof(parameters)))
            });

        public virtual async Task ExecuteSqlCommandAsync(
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken),
            [NotNull] params object[] parameters)
            => await _statementExecutor.ExecuteNonQueryAsync(
                _connection,
                new[]
                {
                    _sqlCommandBuilder.Build(
                        Check.NotNull(sql, nameof(sql)),
                        Check.NotNull(parameters, nameof(parameters)))
                },
                cancellationToken);
    }
}
