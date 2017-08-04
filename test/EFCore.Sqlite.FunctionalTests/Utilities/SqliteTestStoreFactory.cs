// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqliteTestStoreFactory : ITestStoreFactory<SqliteTestStore>
    {
        public static SqliteTestStoreFactory Instance { get; } = new SqliteTestStoreFactory();

        protected SqliteTestStoreFactory()
        {
        }

        public virtual SqliteTestStore CreateShared(string storeName)
            => SqliteTestStore.GetOrCreate(storeName);

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlite()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());
    }
}
