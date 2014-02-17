// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET45
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.SqlServer.Utilities;
using JetBrains.Annotations;

namespace Microsoft.Data.SqlServer
{
    /// <remarks>Not threadsafe</remarks>
    public class SequenceIdentityGenerator : IIdentityGenerator<long>
    {
        public const string DefaultSequenceName = "EF_IdentityGenerator";

        public const int DefaultCommandTimeout = 1;
        public const int DefaultIncrement = 10;

        private readonly SqlTransaction _transaction;
        private readonly SchemaQualifiedName _sequenceName;
        private readonly int _increment;
        private readonly int _commandTimeout;
        private readonly string _selectNextValueSql;

        private long _current;
        private long _max;

        public SequenceIdentityGenerator([NotNull] SqlTransaction transaction)
            : this(Check.NotNull(transaction, "transaction"), DefaultIncrement)
        {
        }

        public SequenceIdentityGenerator([NotNull] SqlTransaction transaction, int increment)
            : this(Check.NotNull(transaction, "transaction"), increment, DefaultSequenceName, DefaultCommandTimeout)
        {
        }

        public SequenceIdentityGenerator(
            [NotNull] SqlTransaction transaction, int increment, SchemaQualifiedName sequenceName, int commandTimeout)
        {
            Check.NotNull(transaction, "transaction");

            _transaction = transaction;
            _increment = increment;
            _sequenceName = sequenceName;
            _commandTimeout = commandTimeout;

            _selectNextValueSql
                = string.Format(
                    CultureInfo.InvariantCulture,
                    "SELECT NEXT VALUE FOR {0}",
                    new SqlServerMigrationOperationSqlGenerator().DelimitIdentifier(_sequenceName));
        }

        public virtual async Task<long> NextAsync()
        {
            if (_current == _max)
            {
                using (var command = _transaction.Connection.CreateCommand())
                {
                    command.CommandTimeout = _commandTimeout;
                    command.CommandText = _selectNextValueSql;
                    command.Transaction = _transaction;

                    _current = (long)await command.ExecuteScalarAsync();
                    _max = _current + _increment;
                }
            }

            return ++_current;
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

#endif
