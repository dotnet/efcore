// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET45
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
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

        private readonly string _connectionString;
        private readonly int _increment;
        private readonly string _name;
        private readonly string _schema;
        private readonly string _schemaSql;

        private long _current;
        private long _max;

        public SequenceIdentityGenerator([NotNull] string connectionString)
            : this(Check.NotEmpty(connectionString, "connectionString"), 10)
        {
        }

        public SequenceIdentityGenerator([NotNull] string connectionString, int increment)
            : this(Check.NotEmpty(connectionString, "connectionString"), increment, DefaultSequenceName)
        {
        }

        public SequenceIdentityGenerator([NotNull] string connectionString, int increment, [NotNull] string name)
            : this(Check.NotEmpty(connectionString, "connectionString"), increment, Check.NotEmpty(name, "name"), null)
        {
        }

        public SequenceIdentityGenerator(
            [NotNull] string connectionString, int increment, [NotNull] string name, [CanBeNull] string schema)
        {
            Check.NotEmpty(connectionString, "connectionString");
            Check.NotEmpty(name, "name");

            _connectionString = connectionString;
            _increment = increment;
            _name = name;
            _schema = schema;
            _schemaSql = !string.IsNullOrWhiteSpace(_schema) ? "[" + _schema + "]." : string.Empty;
        }

        public virtual async Task<long> NextAsync()
        {
            if (_current == _max)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText
                            = string.Format(
                                CultureInfo.InvariantCulture,
                                "SELECT NEXT VALUE FOR {1}[{0}]",
                                _name,
                                _schemaSql);

                        _current = (long)await command.ExecuteScalarAsync();
                        _max = _current + _increment;
                    }
                }
            }

            return Interlocked.Increment(ref _current);
        }

        public virtual async Task EnsureSequenceAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.CommandText
                            = string.Format(
                                CultureInfo.InvariantCulture,
                                @"IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = N'{0}'{1})
                                            CREATE SEQUENCE {2}[{0}] AS bigint START WITH 0 INCREMENT BY {3}",
                                _name,
                                !string.IsNullOrWhiteSpace(_schema)
                                    ? " AND schema_id = SCHEMA_ID(N'" + _schema + "')"
                                    : string.Empty,
                                _schemaSql,
                                _increment);

                        await command.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            }
        }
    }
}

#endif
