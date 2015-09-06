// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer.ValueGeneration
{
    public class SqlServerSequenceValueGenerator<TValue> : HiLoValueGenerator<TValue>
    {
        private readonly ISqlStatementExecutor _executor;
        private readonly ISqlServerUpdateSqlGenerator _sqlGenerator;
        private readonly ISqlServerConnection _connection;
        private readonly ISequence _sequence;

        public SqlServerSequenceValueGenerator(
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] ISqlServerUpdateSqlGenerator sqlGenerator,
            [NotNull] SqlServerSequenceValueGeneratorState generatorState,
            [NotNull] ISqlServerConnection connection)
            : base(generatorState)
        {
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(connection, nameof(connection));

            _sequence = generatorState.Sequence;
            _executor = executor;
            _sqlGenerator = sqlGenerator;
            _connection = connection;
        }

        protected override long GetNewLowValue()
            => (long)Convert.ChangeType(
                _executor.ExecuteScalar(
                    _connection,
                    _sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema)),
                typeof(long),
                CultureInfo.InvariantCulture);

        public override bool GeneratesTemporaryValues => false;
    }
}
