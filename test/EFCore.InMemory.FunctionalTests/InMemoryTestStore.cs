// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryTestStore : TestStore
    {
        private Action<InMemoryTestStore> _deleteDatabase;

        public InMemoryTestStore(string name = null)
            : base(name)
        {
        }

        public static InMemoryTestStore GetOrCreateShared(string name, Action initializeDatabase)
            => new InMemoryTestStore(name).InitializeShared(null, initializeDatabase);

        public static InMemoryTestStore GetOrCreateShared(string name)
            => new InMemoryTestStore(name);

        public static InMemoryTestStore CreateScratch(
            IServiceProvider serviceProvider,
            string databaseName,
            Action initializeDatabase)
            => CreateScratch(
                initializeDatabase,
                s => serviceProvider.GetRequiredService<IInMemoryStoreCache>().GetStore(databaseName).Clear());

        public static InMemoryTestStore CreateScratch(Action initializeDatabase, Action<InMemoryTestStore> deleteDatabase)
            => new InMemoryTestStore().InitializeTransient(initializeDatabase, deleteDatabase);

        public override TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => InitializeShared(serviceProvider, () =>
                {
                    using (var context = createContext())
                    {
                        context.Database.EnsureCreated();
                        seed(context);
                    }
                });

        private InMemoryTestStore InitializeShared(IServiceProvider serviceProvider, Action initializeDatabase)
        {
            var testStoreIndex = serviceProvider == null ? GlobalTestStoreIndex : serviceProvider.GetRequiredService<TestStoreIndex>();
            testStoreIndex.CreateShared(typeof(InMemoryTestStore).Name + Name, initializeDatabase);

            return this;
        }

        private InMemoryTestStore InitializeTransient(Action initializeDatabase, Action<InMemoryTestStore> deleteDatabase)
        {
            initializeDatabase?.Invoke();

            _deleteDatabase = deleteDatabase;
            return this;
        }

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<ILoggerFactory>(new TestLoggerFactory())
                .AddSingleton<TestStoreIndex>();

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseInMemoryDatabase(Name);

        public override void Dispose()
        {
            _deleteDatabase?.Invoke(this);

            base.Dispose();
        }
    }
}
