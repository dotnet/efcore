// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqliteTestStoreFactory : ITestStoreFactory
    {
        public static SqliteTestStoreFactory Instance { get; } = new SqliteTestStoreFactory();

        protected SqliteTestStoreFactory()
        {
        }

        public virtual TestStore Create(string storeName)
            => SqliteTestStore.Create(storeName);

        public virtual TestStore GetOrCreate(string storeName)
            => SqliteTestStore.GetOrCreate(storeName);

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlite()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());
    }
}
