// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class InMemoryTestStoreFactory : ITestStoreFactory
    {
        public static InMemoryTestStoreFactory Instance { get; } = new InMemoryTestStoreFactory();

        protected InMemoryTestStoreFactory()
        {
        }

        public virtual TestStore Create(string storeName)
            => InMemoryTestStore.Create(storeName);

        public virtual TestStore GetOrCreate(string storeName)
            => InMemoryTestStore.GetOrCreate(storeName);

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<ILoggerFactory>(new TestLoggerFactory())
                .AddSingleton<TestStoreIndex>();
    }
}
