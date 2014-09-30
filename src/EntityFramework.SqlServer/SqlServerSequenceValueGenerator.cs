// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerSequenceValueGenerator : BlockOfSequentialValuesGenerator
    {
        private readonly SqlStatementExecutor _executor;

        public SqlServerSequenceValueGenerator(
            [NotNull] SqlStatementExecutor executor,
            [NotNull] string sequenceName,
            int blockSize)
            : base(sequenceName, blockSize)
        {
            Check.NotNull(executor, "executor");

            _executor = executor;
        }

        public override long GetNewCurrentValue(StateEntry stateEntry, IProperty property)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            var commandInfo = PrepareCommand(stateEntry.Configuration);
            var nextValue = _executor.ExecuteScalar(commandInfo.Item1.DbConnection, commandInfo.Item1.DbTransaction, commandInfo.Item2);

            return (long)Convert.ChangeType(nextValue, typeof(long), CultureInfo.InvariantCulture);
        }

        public override async Task<long> GetNewCurrentValueAsync(StateEntry stateEntry, IProperty property, CancellationToken cancellationToken)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            var commandInfo = PrepareCommand(stateEntry.Configuration);
            var nextValue = await _executor
                .ExecuteScalarAsync(commandInfo.Item1.DbConnection, commandInfo.Item1.DbTransaction, commandInfo.Item2, cancellationToken)
                .WithCurrentCulture();

            return (long)Convert.ChangeType(nextValue, typeof(long), CultureInfo.InvariantCulture);
        }

        private Tuple<RelationalConnection, SqlStatement> PrepareCommand(DbContextConfiguration contextConfiguration)
        {
            // TODO: Parameterize query and/or delimit identifier without using SqlServerMigrationOperationSqlGenerator
            var sql = new SqlStatement(string.Format(
                CultureInfo.InvariantCulture,
                "SELECT NEXT VALUE FOR {0}", SequenceName));

            // TODO: Should be able to get relational connection without cast
            var connection = (RelationalConnection)contextConfiguration.Connection;

            return Tuple.Create(connection, sql);
        }
    }
}
