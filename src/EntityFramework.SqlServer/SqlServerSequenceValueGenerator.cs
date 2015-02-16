// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerSequenceValueGenerator<TValue> : HiLoValueGenerator<TValue>
    {
        private readonly SqlStatementExecutor _executor;

        public SqlServerSequenceValueGenerator(
            [NotNull] SqlStatementExecutor executor,
            [NotNull] string sequenceName,
            int blockSize)
            : base(blockSize)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotEmpty(sequenceName, nameof(sequenceName));

            SequenceName = sequenceName;

            _executor = executor;
        }

        public virtual string SequenceName { get; }

        protected override long GetNewHighValue(DbContextService<DataStoreServices> dataStoreServices)
        {
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
