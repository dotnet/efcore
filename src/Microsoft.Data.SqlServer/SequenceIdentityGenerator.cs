// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Threading;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
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
                    new SqlServerMigrationOperationSqlGenerator().DelimitIdentifier(_sequenceName));
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
