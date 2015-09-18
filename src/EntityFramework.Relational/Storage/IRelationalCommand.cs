// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalCommand
    {
        string CommandText { get; }

        IReadOnlyList<RelationalParameter> Parameters { get; }

        void ExecuteNonQuery([NotNull] IRelationalConnection connection);

        object ExecuteScalar([NotNull] IRelationalConnection connection);

        DbDataReader ExecuteReader([NotNull] IRelationalConnection connection);

        Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<object> ExecuteScalarAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<DbDataReader> ExecuteReaderAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
