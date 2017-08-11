// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public abstract class TestStore : IDisposable
    {
        private readonly bool _shared;
        private static readonly TestStoreIndex _globalTestStoreIndex = new TestStoreIndex();
        public IServiceProvider ServiceProvider { get; protected set; }

        protected TestStore(string name, bool shared)
        {
            Name = name;
            _shared = shared;
        }

        public string Name { get; protected set; }

        public virtual TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
        {
            ServiceProvider = serviceProvider;
            if (createContext == null)
            {
                createContext = CreateDefaultContext;
            }
            if (seed == null)
            {
                seed = c => { };
            }

            if (_shared)
            {
                GetTestStoreIndex(serviceProvider).CreateShared(GetType().Name + Name, () => Initialize(createContext, seed));
            }
            else
            {
                Initialize(createContext, seed);
            }

            return this;
        }

        public TestStore Initialize(IServiceProvider serviceProvider, Func<TestStore, DbContext> createContext, Action<DbContext> seed)
            => Initialize(serviceProvider, () => createContext(this), seed);

        protected virtual void Initialize(Func<DbContext> createContext, Action<DbContext> seed)
        {
            using (var context = createContext())
            {
                Clean(context);
                seed(context);
            }
        }

        public abstract DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder);
        public abstract void Clean(DbContext context);

        protected virtual DbContext CreateDefaultContext()
            => new DbContext(AddProviderOptions(new DbContextOptionsBuilder()).Options);

        protected virtual TestStoreIndex GetTestStoreIndex(IServiceProvider serviceProvider) => _globalTestStoreIndex;

        public virtual void Dispose()
        {
        }
    }
}
