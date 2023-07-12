// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeRelationalDatabaseCreator : IRelationalDatabaseCreator
{
    public bool EnsureDeleted()
        => throw new NotImplementedException();

    public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = new())
        => throw new NotImplementedException();

    public bool EnsureCreated()
        => throw new NotImplementedException();

    public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = new())
        => throw new NotImplementedException();

    public bool CanConnect()
        => throw new NotImplementedException();

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public bool Exists()
        => throw new NotImplementedException();

    public Task<bool> ExistsAsync(CancellationToken cancellationToken = new())
        => throw new NotImplementedException();

    public bool HasTables()
        => throw new NotImplementedException();

    public Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Create()
        => throw new NotImplementedException();

    public Task CreateAsync(CancellationToken cancellationToken = new())
        => throw new NotImplementedException();

    public void Delete()
        => throw new NotImplementedException();

    public Task DeleteAsync(CancellationToken cancellationToken = new())
        => throw new NotImplementedException();

    public void CreateTables()
        => throw new NotImplementedException();

    public Task CreateTablesAsync(CancellationToken cancellationToken = new())
        => throw new NotImplementedException();

    public string GenerateCreateScript()
        => throw new NotImplementedException();
}
