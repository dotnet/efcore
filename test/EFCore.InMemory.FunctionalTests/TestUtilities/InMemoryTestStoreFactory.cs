// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class InMemoryTestStoreFactory : ITestStoreFactory
    {
        public static InMemoryTestStoreFactory Instance { get; } = new InMemoryTestStoreFactory();

        protected InMemoryTestStoreFactory()
        {
        }

        public TestStore Create(string storeName)
            => InMemoryTestStore.Create(storeName);

        public TestStore GetOrCreate(string storeName)
            => InMemoryTestStore.GetOrCreate(storeName);

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<TestStoreIndex>();

        public ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
            => new ListLoggerFactory(shouldLogCategory);
    }
}
