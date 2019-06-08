// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeRelationalDatabaseCreator : IRelationalDatabaseCreator
    {
        public bool EnsureDeleted() => throw new NotImplementedException();

        public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = new CancellationToken()) =>
            throw new NotImplementedException();

        public bool EnsureCreated() => throw new NotImplementedException();

        public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = new CancellationToken()) =>
            throw new NotImplementedException();

        public bool CanConnect() => throw new NotImplementedException();
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public bool Exists() => throw new NotImplementedException();
        public Task<bool> ExistsAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
        public bool HasTables() => throw new NotImplementedException();
        public Task<bool> HasTablesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void Create() => throw new NotImplementedException();
        public Task CreateAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
        public void Delete() => throw new NotImplementedException();
        public Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
        public void CreateTables() => throw new NotImplementedException();
        public Task CreateTablesAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
        public string GenerateCreateScript() => throw new NotImplementedException();
    }
}
