// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class TestStore : IDisposable
    {
        protected static readonly TestStoreIndex GlobalTestStoreIndex = new TestStoreIndex();

        protected TestStore(string name)
        {
            Name = name;
        }
        
        public string Name { get; protected set; }

        public abstract TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed);
        public abstract IServiceCollection AddProviderServices(IServiceCollection serviceCollection);
        public abstract DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder);

        public virtual void Dispose()
        {
        }
    }
}
