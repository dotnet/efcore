// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class CosmosTestStoreFactory : ITestStoreFactory
    {
        public static CosmosTestStoreFactory Instance { get; } = new CosmosTestStoreFactory();

        protected CosmosTestStoreFactory()
        {
        }

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection
                .AddEntityFrameworkCosmos()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .AddSingleton<TestStoreIndex>();

        public TestStore Create(string storeName)
            => CosmosTestStore.Create(storeName);

        public virtual TestStore GetOrCreate(string storeName)
            => CosmosTestStore.GetOrCreate(storeName);

        public virtual ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
            => new TestSqlLoggerFactory(shouldLogCategory);
    }
}
