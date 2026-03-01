// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CosmosCloudTestStore(string name, bool shared = true, Action<CosmosDbContextOptionsBuilder>? extensionConfiguration = null) : CosmosTestStore(name, shared, extensionConfiguration)
{
    private static readonly SemaphoreSlim _semaphore = new(Environment.ProcessorCount);

    private bool _aquired;

    public override async Task<TestStore> InitializeAsync(
        IServiceProvider? serviceProvider,
        Func<DbContext>? createContext,
        Func<DbContext, Task>? seed = null,
        Func<DbContext, Task>? clean = null)
    {
        if (!_aquired)
        {
            await _semaphore.WaitAsync();
            _aquired = true;
        }
        return await base.InitializeAsync(serviceProvider, createContext, seed, clean).ConfigureAwait(false);
    }

    public override ValueTask DisposeAsync()
    {
        if (_aquired)
        {
            _semaphore.Release();
            _aquired = false;
        }
        return base.DisposeAsync();
    }
}
