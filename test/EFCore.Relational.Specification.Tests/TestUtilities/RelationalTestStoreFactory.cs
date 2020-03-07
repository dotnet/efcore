// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public abstract class RelationalTestStoreFactory : ITestStoreFactory
    {
        public abstract TestStore Create(string storeName);
        public abstract TestStore GetOrCreate(string storeName);
        public abstract IServiceCollection AddProviderServices(IServiceCollection serviceCollection);

        public virtual ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
            => new TestSqlLoggerFactory(shouldLogCategory);
    }
}
