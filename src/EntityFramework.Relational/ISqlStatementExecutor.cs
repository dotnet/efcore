// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational
{
    public interface ISqlStatementExecutor
    {
        Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] IEnumerable<SqlBatch> sqlBatches,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<object> ExecuteScalarAsync(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<object> ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<Task<object>> action,
            CancellationToken cancellationToken = default(CancellationToken));

        void ExecuteNonQuery(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] IEnumerable<SqlBatch> sqlBatches);

        object ExecuteScalar(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] string sql);

        object Execute(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<object> action);
    }
}
