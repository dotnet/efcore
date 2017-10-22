// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqlServerTestStoreFactory : ITestStoreFactory
    {
        public static SqlServerTestStoreFactory Instance { get; } = new SqlServerTestStoreFactory();

        protected SqlServerTestStoreFactory()
        {
        }

        public virtual TestStore Create(string storeName)
            => SqlServerTestStore.Create(storeName);

        public virtual TestStore GetOrCreate(string storeName)
            => SqlServerTestStore.GetOrCreate(storeName);

        public virtual IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());
    }
}
