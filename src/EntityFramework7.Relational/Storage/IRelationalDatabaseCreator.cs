// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalDatabaseCreator : IDatabaseCreator
    {
        bool Exists();

        Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken));

        void Create();

        Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken));

        void Delete();

        Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));

        void CreateTables();

        Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken));

        bool HasTables();

        Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
