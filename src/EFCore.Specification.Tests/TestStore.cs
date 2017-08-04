// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class TestStore : IDisposable
    {
        protected static readonly TestStoreIndex GlobalTestStoreIndex = new TestStoreIndex();
        public IServiceProvider ServiceProvider { get; protected set; }

        protected TestStore(string name)
        {
            Name = name;
        }

        public string Name { get; protected set; }

        public abstract TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed);

        public TestStore Initialize(IServiceProvider serviceProvider, Func<TestStore, DbContext> createContext, Action<DbContext> seed)
            => Initialize(serviceProvider, () => createContext(this), seed);

        public abstract DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder);
        public abstract void Clean(DbContext context);

        protected virtual DbContext CreateDefaultContext()
            => new DbContext(AddProviderOptions(new DbContextOptionsBuilder()).Options);

        public virtual void Dispose()
        {
        }
    }
}
