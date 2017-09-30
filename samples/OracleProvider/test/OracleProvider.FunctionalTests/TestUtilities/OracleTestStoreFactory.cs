// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class OracleTestStoreFactory : ITestStoreFactory
    {
        public static OracleTestStoreFactory Instance { get; } = new OracleTestStoreFactory();

        protected OracleTestStoreFactory()
        {
        }

        public virtual TestStore Create(string storeName)
            => OracleTestStore.Create(storeName);

        public virtual TestStore GetOrCreate(string storeName)
            => OracleTestStore.GetOrCreate(storeName);

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkOracle()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());
    }
}
