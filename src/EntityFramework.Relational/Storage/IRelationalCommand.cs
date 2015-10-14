// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalCommand
    {
        string CommandText { get; }

        IReadOnlyList<RelationalParameter> Parameters { get; }

        void ExecuteNonQuery(
            [NotNull] IRelationalConnection connection,
            bool manageConnection = true);

        Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken),
            bool manageConnection = true);

        object ExecuteScalar(
            [NotNull] IRelationalConnection connection,
            bool manageConnection = true);

        Task<object> ExecuteScalarAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken),
            bool manageConnection = true);

        RelationalDataReader ExecuteReader(
            [NotNull] IRelationalConnection connection,
            bool manageConnection = true);

        Task<RelationalDataReader> ExecuteReaderAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken),
            bool manageConnection = true);
    }
}
