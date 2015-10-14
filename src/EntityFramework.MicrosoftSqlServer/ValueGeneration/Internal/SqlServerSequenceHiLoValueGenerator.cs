// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public class SqlServerSequenceHiLoValueGenerator<TValue> : HiLoValueGenerator<TValue>
    {
        private readonly ISqlCommandBuilder _sqlCommandBuilder;
        private readonly ISqlServerUpdateSqlGenerator _sqlGenerator;
        private readonly ISqlServerConnection _connection;
        private readonly ISequence _sequence;

        public SqlServerSequenceHiLoValueGenerator(
            [NotNull] ISqlCommandBuilder sqlCommandBuilder,
            [NotNull] ISqlServerUpdateSqlGenerator sqlGenerator,
            [NotNull] SqlServerSequenceValueGeneratorState generatorState,
            [NotNull] ISqlServerConnection connection)
            : base(generatorState)
        {
            Check.NotNull(sqlCommandBuilder, nameof(sqlCommandBuilder));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(connection, nameof(connection));

            _sequence = generatorState.Sequence;
            _sqlCommandBuilder = sqlCommandBuilder;
            _sqlGenerator = sqlGenerator;
            _connection = connection;
        }

        protected override long GetNewLowValue()
            => (long)Convert.ChangeType(
                _sqlCommandBuilder
                    .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                    .ExecuteScalar(_connection),
                typeof(long),
                CultureInfo.InvariantCulture);

        public override bool GeneratesTemporaryValues => false;
    }
}
