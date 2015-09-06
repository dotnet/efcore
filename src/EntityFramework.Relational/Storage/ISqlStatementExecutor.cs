// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage.Commands;

namespace Microsoft.Data.Entity.Storage
{
    public interface ISqlStatementExecutor
    {
        void ExecuteNonQuery(
            [NotNull] IRelationalConnection connection,
            [NotNull] RelationalCommand relationalCommand);

        void ExecuteNonQuery(
            [NotNull] IRelationalConnection connection,
            [NotNull] IEnumerable<RelationalCommand> relationalCommands);

        Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] RelationalCommand relationalCommand,
            CancellationToken cancellationToken = default(CancellationToken));

        Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] IEnumerable<RelationalCommand> relationalCommands,
            CancellationToken cancellationToken = default(CancellationToken));

        object ExecuteScalar(
            [NotNull] IRelationalConnection connection,
            [NotNull] string sql);

        Task<object> ExecuteScalarAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken));

        DbDataReader ExecuteReader(
            [NotNull] IRelationalConnection connection,
            [NotNull] string sql);

        Task<DbDataReader> ExecuteReaderAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
