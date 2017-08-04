// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryTestStore : TestStore
    {
        public InMemoryTestStore(string name = null)
            : base(name)
        {
        }

        public static InMemoryTestStore GetOrCreate(string name)
            => new InMemoryTestStore(name);

        public static InMemoryTestStore GetOrCreateInitialized(string name)
            => new InMemoryTestStore(name).InitializeInMemory(null, null, null);

        public override void Clean(DbContext context)
            => context.GetService<IInMemoryStoreCache>().GetStore(Name).Clear();

        public override TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => InitializeInMemory(serviceProvider, createContext, seed);

        public InMemoryTestStore InitializeInMemory(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
        {
            ServiceProvider = serviceProvider;
            if (createContext == null)
            {
                createContext = CreateDefaultContext;
            }
            if (seed == null)
            {
                seed = c => { };
            }

            var testStoreIndex = serviceProvider == null ? GlobalTestStoreIndex : serviceProvider.GetRequiredService<TestStoreIndex>();
            testStoreIndex.CreateShared(typeof(InMemoryTestStore).Name + Name, () =>
                {
                    using (var context = createContext())
                    {
                        context.Database.EnsureCreated();
                        seed(context);
                    }
                });

            return this;
        }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseInMemoryDatabase(Name);
    }
}
