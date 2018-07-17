// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class OracleTestStoreFactory : RelationalTestStoreFactory
    {
        public static OracleTestStoreFactory Instance { get; } = new OracleTestStoreFactory();

        protected OracleTestStoreFactory()
        {
        }

        public override TestStore Create(string storeName)
            => OracleTestStore.Create(storeName);

        public override TestStore GetOrCreate(string storeName)
            => OracleTestStore.GetOrCreate(storeName);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkOracle();
    }
}
