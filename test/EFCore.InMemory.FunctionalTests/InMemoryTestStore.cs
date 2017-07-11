// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryTestStore : TestStore
    {
        private Action _deleteDatabase;

        public InMemoryTestStore(
            string name = null,
            IServiceProvider serviceProvider = null,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions = null,
            Func<DbContextOptions, DbContext> createContext = null)
            : base(name,
                serviceProvider ??
                InMemoryTestStoreFactory.Instance.AddProviderServices(new ServiceCollection()).BuildServiceProvider(),
                addOptions,
                createContext)
        {
        }

        protected override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder
                .UseInMemoryDatabase(Name)
                .UseInternalServiceProvider(ServiceProvider);

        public static InMemoryTestStore GetOrCreateShared(string name, Action initializeDatabase)
            => new InMemoryTestStore(name).CreateShared(initializeDatabase);

        public static InMemoryTestStore GetOrCreateShared(
            string name,
            IServiceProvider serviceProvider,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions, 
            Func<DbContextOptions, DbContext> createContext,
            Action<DbContext> seed)
            => new InMemoryTestStore(name, serviceProvider, addOptions, createContext).CreateShared(seed);

        private InMemoryTestStore CreateShared(Action<DbContext> seed)
            => CreateShared(() =>
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureCreated();
                        seed(context);
                    }
                });

        private InMemoryTestStore CreateShared(Action initializeDatabase)
        {
            base.CreateShared(typeof(InMemoryTestStore).Name + Name, initializeDatabase);

            return this;
        }

        public static InMemoryTestStore CreateScratch(
            IServiceProvider serviceProvider,
            string databaseName,
            Action initializeDatabase)
            => CreateScratch(
                initializeDatabase,
                () => serviceProvider.GetRequiredService<IInMemoryStoreCache>().GetStore(databaseName).Clear());

        public static InMemoryTestStore CreateScratch(Action initializeDatabase, Action deleteDatabase)
            => new InMemoryTestStore().CreateTransient(initializeDatabase, deleteDatabase);

        private InMemoryTestStore CreateTransient(Action initializeDatabase, Action deleteDatabase)
        {
            initializeDatabase?.Invoke();

            _deleteDatabase = deleteDatabase;
            return this;
        }

        public override void Dispose()
        {
            _deleteDatabase?.Invoke();

            if (Name != null)
            {
                ServiceProvider?.GetRequiredService<IInMemoryStoreCache>().GetStore(Name).Clear();
            }

            base.Dispose();
        }
    }
}
