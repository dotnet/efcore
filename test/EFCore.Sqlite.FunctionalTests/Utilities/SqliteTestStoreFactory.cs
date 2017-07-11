// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqliteTestStoreFactory : TestStoreFactory<SqliteTestStore>
    {
        public static SqliteTestStoreFactory Instance { get; } = new SqliteTestStoreFactory();

        private SqliteTestStoreFactory()
        {
        }

        public override SqliteTestStore CreateShared(
            string storeName,
            IServiceProvider serviceProvider,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions,
            Func<DbContextOptions, DbContext> createContext,
            Action<DbContext> seed)
            => SqliteTestStore.GetOrCreateShared(storeName, serviceProvider, addOptions, createContext, seed);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlite()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());
    }
}
