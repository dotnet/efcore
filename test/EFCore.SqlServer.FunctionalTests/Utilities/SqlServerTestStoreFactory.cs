// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqlServerTestStoreFactory : TestStoreFactory<SqlServerTestStore>
    {
        public static SqlServerTestStoreFactory Instance { get; } = new SqlServerTestStoreFactory();

        private SqlServerTestStoreFactory()
        {
        }

        public override SqlServerTestStore CreateShared(
            string storeName,
            IServiceProvider serviceProvider,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions,
            Func<DbContextOptions, DbContext> createContext,
            Action<DbContext> seed)
            => SqlServerTestStore.GetOrCreateShared(storeName, serviceProvider, addOptions, createContext, seed);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());
    }
}
