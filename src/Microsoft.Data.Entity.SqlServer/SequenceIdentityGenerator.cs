// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    /// <remarks>Not threadsafe</remarks>
    public class SequenceIdentityGenerator : IIdentityGenerator<long>
    {
        public const string DefaultSequenceName = "EF_IdentityGenerator";

        public const int DefaultIncrement = 10;

        private readonly IDbCommandExecutor _commandExecutor;
        private readonly SchemaQualifiedName _sequenceName;
        private readonly int _increment;
        private readonly string _selectNextValueSql;

        private long _current;
        private long _max;

        public SequenceIdentityGenerator([NotNull] IDbCommandExecutor commandExecutor)
            : this(Check.NotNull(commandExecutor, "commandExecutor"), DefaultIncrement)
        {
        }

        public SequenceIdentityGenerator([NotNull] IDbCommandExecutor commandExecutor, int increment)
            : this(Check.NotNull(commandExecutor, "commandExecutor"), increment, DefaultSequenceName)
        {
        }

        public SequenceIdentityGenerator(
            [NotNull] IDbCommandExecutor commandExecutor, int increment, SchemaQualifiedName sequenceName)
        {
            Check.NotNull(commandExecutor, "commandExecutor");

            _commandExecutor = commandExecutor;
            _increment = increment;
            _sequenceName = sequenceName;

            _selectNextValueSql
                = string.Format(
                    CultureInfo.InvariantCulture,
                    "SELECT NEXT VALUE FOR {0}",
                    new SqlServerMigrationOperationSqlGenerator(new SqlServerTypeMapper()).DelimitIdentifier(_sequenceName));
        }

        public virtual async Task<long> NextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_current == _max)
            {
                _current = await _commandExecutor.ExecuteScalarAsync<long>(_selectNextValueSql, cancellationToken).ConfigureAwait(false);
                _max = _current + _increment;
            }

            return ++_current;
        }

        async Task<object> IIdentityGenerator.NextAsync(CancellationToken cancellationToken)
        {
            return await NextAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual CreateSequenceOperation CreateMigrationOperation()
        {
            return new CreateSequenceOperation(
                new Sequence(_sequenceName)
                    {
                        StartWith = 0,
                        IncrementBy = _increment,
                        DataType = "BIGINT"
                    });
        }
    }
}
