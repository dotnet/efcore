// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class InMemoryTestStore : TestStore
    {
        public InMemoryTestStore(string name = null, bool shared = true)
            : base(name, shared)
        {
        }

        public static InMemoryTestStore GetOrCreate(string name)
            => new InMemoryTestStore(name);

        public static InMemoryTestStore GetOrCreateInitialized(string name)
            => new InMemoryTestStore(name).InitializeInMemory(null, (Func<DbContext>)null, null);

        public static InMemoryTestStore Create(string name)
            => new InMemoryTestStore(name, shared: false);

        public static InMemoryTestStore CreateInitialized(string name)
            => new InMemoryTestStore(name, shared: false).InitializeInMemory(null, (Func<DbContext>)null, null);

        public InMemoryTestStore InitializeInMemory(
            IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => (InMemoryTestStore)Initialize(serviceProvider, createContext, seed);

        public InMemoryTestStore InitializeInMemory(
            IServiceProvider serviceProvider, Func<InMemoryTestStore, DbContext> createContext, Action<DbContext> seed)
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
}
