// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class InMemoryTestStore : TestStore
{
    public InMemoryTestStore(string name = null, bool shared = true)
        : base(name, shared)
    {
    }

    public static InMemoryTestStore GetOrCreate(string name)
        => new(name);

    public static InMemoryTestStore GetOrCreateInitialized(string name)
        => new InMemoryTestStore(name).InitializeInMemory(null, (Func<DbContext>)null, null);

    public static InMemoryTestStore Create(string name)
        => new(name, shared: false);

    public static InMemoryTestStore CreateInitialized(string name)
        => new InMemoryTestStore(name, shared: false).InitializeInMemory(null, (Func<DbContext>)null, null);

    public InMemoryTestStore InitializeInMemory(
        IServiceProvider serviceProvider,
        Func<DbContext> createContext,
        Action<DbContext> seed)
        => (InMemoryTestStore)Initialize(serviceProvider, createContext, seed);

    public InMemoryTestStore InitializeInMemory(
        IServiceProvider serviceProvider,
        Func<InMemoryTestStore, DbContext> createContext,
        Action<DbContext> seed)
        => (InMemoryTestStore)Initialize(serviceProvider, () => createContext(this), seed);

    protected override TestStoreIndex GetTestStoreIndex(IServiceProvider serviceProvider)
        => serviceProvider == null
            ? base.GetTestStoreIndex(null)
            : serviceProvider.GetRequiredService<TestStoreIndex>();

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => builder.UseInMemoryDatabase(Name);

    public override void Clean(DbContext context)
    {
        context.GetService<IInMemoryStoreCache>().GetStore(Name).Clear();
        context.Database.EnsureCreated();
    }
}
