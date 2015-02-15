// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerSequenceValueGenerator : HiLoValuesGenerator
    {
        private readonly SqlStatementExecutor _executor;

        public SqlServerSequenceValueGenerator(
            [NotNull] SqlStatementExecutor executor,
            [NotNull] string sequenceName,
            int blockSize)
            : base(sequenceName, blockSize)
        {
            Check.NotNull(executor, nameof(executor));

            _executor = executor;
        }

        protected override long GetNewHighValue(IProperty property, DbContextService<DataStoreServices> dataStoreServices)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(dataStoreServices, nameof(dataStoreServices));

            var commandInfo = PrepareCommand((RelationalConnection)dataStoreServices.Service.Connection);
            var nextValue = _executor.ExecuteScalar(commandInfo.Item1, commandInfo.Item1.DbTransaction, commandInfo.Item2);

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
