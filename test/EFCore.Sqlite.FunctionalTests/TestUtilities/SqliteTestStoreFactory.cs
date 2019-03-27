// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqliteTestStoreFactory : RelationalTestStoreFactory
    {
        public static SqliteTestStoreFactory Instance { get; } = new SqliteTestStoreFactory();

        protected SqliteTestStoreFactory()
        {
        }

        public override TestStore Create(string storeName)
            => SqliteTestStore.Create(storeName);

        public override TestStore GetOrCreate(string storeName)
            => SqliteTestStore.GetOrCreate(storeName);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlite();
    }
}
