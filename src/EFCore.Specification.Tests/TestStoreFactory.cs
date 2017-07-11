// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class TestStoreFactory<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateShared(
            string storeName,
            IServiceProvider serviceProvider,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions,
            Func<DbContextOptions, DbContext> createContext,
            Action<DbContext> seed);

        public abstract IServiceCollection AddProviderServices(IServiceCollection serviceCollection);
    }
}
