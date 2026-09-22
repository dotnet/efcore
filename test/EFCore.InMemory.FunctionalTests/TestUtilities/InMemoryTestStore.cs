// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class InMemoryTestStore(string name = null, bool shared = true) : TestStore(name, shared)
{
    public static InMemoryTestStore GetOrCreate(string name)
        => new(name);

    public static Task<InMemoryTestStore> GetOrCreateInitializedAsync(string name)
        => new InMemoryTestStore(name).InitializeInMemoryAsync(null, (Func<DbContext>)null, null);

    public static InMemoryTestStore Create(string name)
        => new(name, shared: false);

    public static Task<InMemoryTestStore> CreateInitializedAsync(string name)
        => new InMemoryTestStore(name, shared: false).InitializeInMemoryAsync(null, (Func<DbContext>)null, null);

    public async Task<InMemoryTestStore> InitializeInMemoryAsync(
        IServiceProvider serviceProvider,
        Func<DbContext> createContext,
        Func<DbContext, Task> seed)
        => (InMemoryTestStore)await InitializeAsync(serviceProvider, createContext, seed);

    public async Task<InMemoryTestStore> InitializeInMemoryAsync(
        IServiceProvider serviceProvider,
        Func<InMemoryTestStore, DbContext> createContext,
        Func<DbContext, Task> seed)
        => (InMemoryTestStore)await InitializeAsync(serviceProvider, () => createContext(this), seed);

    protected override TestStoreIndex GetTestStoreIndex(IServiceProvider serviceProvider)
        => serviceProvider == null
            ? base.GetTestStoreIndex(null)
            : serviceProvider.GetService<TestStoreIndex>() ?? base.GetTestStoreIndex(serviceProvider);

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => builder.UseInMemoryDatabase(Name);

    public override Task CleanAsync(DbContext context)
    {
        context.GetService<IInMemoryStoreCache>().GetStore(Name).Clear();
        return context.Database.EnsureCreatedAsync();
    }
}
