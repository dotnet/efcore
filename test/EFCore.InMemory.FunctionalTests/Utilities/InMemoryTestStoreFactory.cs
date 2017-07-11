// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class InMemoryTestStoreFactory : TestStoreFactory<InMemoryTestStore>
    {
        public static InMemoryTestStoreFactory Instance { get; } = new InMemoryTestStoreFactory();

        private InMemoryTestStoreFactory()
        {
        }

        public override InMemoryTestStore CreateShared(
            string storeName,
            IServiceProvider serviceProvider,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions,
            Func<DbContextOptions, DbContext> createContext,
            Action<DbContext> seed)
            => InMemoryTestStore.GetOrCreateShared(storeName, serviceProvider, addOptions, createContext, seed);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkInMemoryDatabase();
    }
}
