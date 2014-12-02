// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.Storage;

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

        protected override long GetNewCurrentValue(IProperty property, DbContextService<DataStoreServices> dataStoreServices)
        {
            Check.NotNull(property, "property");
            Check.NotNull(dataStoreServices, "dataStoreServices");

            var commandInfo = PrepareCommand((RelationalConnection)dataStoreServices.Service.Connection);
            var nextValue = _executor.ExecuteScalar(commandInfo.Item1.DbConnection, commandInfo.Item1.DbTransaction, commandInfo.Item2);

            return (long)Convert.ChangeType(nextValue, typeof(long), CultureInfo.InvariantCulture);
        }

        protected override async Task<long> GetNewCurrentValueAsync(
            IProperty property, DbContextService<DataStoreServices> dataStoreServices, CancellationToken cancellationToken)
        {
            Check.NotNull(property, "property");
            Check.NotNull(dataStoreServices, "dataStoreServices");

            var commandInfo = PrepareCommand((RelationalConnection)dataStoreServices.Service.Connection);
            var nextValue = await _executor
                .ExecuteScalarAsync(commandInfo.Item1.DbConnection, commandInfo.Item1.DbTransaction, commandInfo.Item2, cancellationToken)
                .WithCurrentCulture();

            return (long)Convert.ChangeType(nextValue, typeof(long), CultureInfo.InvariantCulture);
        }

        private Tuple<RelationalConnection, string> PrepareCommand(RelationalConnection connection)
        {
            // TODO: Parameterize query and/or delimit identifier without using SqlServerMigrationOperationSqlGenerator
            var sql = string.Format(
                CultureInfo.InvariantCulture,
                "SELECT NEXT VALUE FOR {0}", SequenceName);

            return Tuple.Create(connection, sql);
        }
    }
}
