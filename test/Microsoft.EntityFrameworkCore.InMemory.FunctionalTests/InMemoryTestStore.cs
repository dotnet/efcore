// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class InMemoryTestStore : TestStore
    {
        private Action _deleteDatabase;

        public static InMemoryTestStore GetOrCreateShared(string name, Action initializeDatabase)
            => new InMemoryTestStore().CreateShared(name, initializeDatabase);

        private new InMemoryTestStore CreateShared(string name, Action initializeDatabase)
        {
            base.CreateShared(typeof(InMemoryTestStore).Name + name, initializeDatabase);

            return this;
        }

        public static InMemoryTestStore CreateScratch(Action initializeDatabase, IServiceProvider serviceProvider)
            => CreateScratch(initializeDatabase, () => serviceProvider.GetRequiredService<IInMemoryStoreSource>().GetGlobalStore().Clear());

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

            base.Dispose();
        }
    }
}
