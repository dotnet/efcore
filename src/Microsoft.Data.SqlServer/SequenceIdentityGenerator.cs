// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
#if NET45
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

        private readonly string _connectionString;
        private readonly int _increment;
        private readonly SchemaQualifiedName _schemaQualifiedName;

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

        public SequenceIdentityGenerator(
            [NotNull] string connectionString, int increment, [NotNull] string schemaQualifiedName)
        {
            Check.NotEmpty(connectionString, "connectionString");
            Check.NotEmpty(schemaQualifiedName, "schemaQualifiedName");

            _connectionString = connectionString;
            _increment = increment;
            _schemaQualifiedName = schemaQualifiedName;
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
                        var ddlSqlGenerator = new SqlServerDdlSqlGenerator();

                        command.CommandText
                            = string.Format(
                                CultureInfo.InvariantCulture,
                                "SELECT NEXT VALUE FOR {0}",
                                ddlSqlGenerator.DelimitIdentifier(_schemaQualifiedName));

                        _current = (long)await command.ExecuteScalarAsync();
                        _max = _current + _increment;
                    }
                }
            }

            return ++_current;
        }

        public virtual CreateSequenceOperation CreateDdlOperation()
        {
            return new CreateSequenceOperation(_schemaQualifiedName)
            {
                StartWith = 0,
                IncrementBy = _increment,
                DataType = "BIGINT"
            };
        }
    }
}

#endif
