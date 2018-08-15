// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities
{
    public class CosmosSqlTestStoreFactory : ITestStoreFactory
    {
        public static CosmosSqlTestStoreFactory Instance { get; } = new CosmosSqlTestStoreFactory();

        protected CosmosSqlTestStoreFactory()
        {
        }

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection
                .AddEntityFrameworkCosmosSql()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .AddSingleton<TestStoreIndex>();

        public TestStore Create(string storeName) => CosmosSqlTestStore.Create(storeName);

        public virtual TestStore GetOrCreate(string storeName) => CosmosSqlTestStore.GetOrCreate(storeName);

        public virtual ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
            => new TestSqlLoggerFactory(shouldLogCategory);
    }
}
