// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryTestStore : TestStore
    {
        public static Task<InMemoryTestStore> GetOrCreateSharedAsync(string name, Func<Task> initializeDatabase)
        {
            return new InMemoryTestStore().CreateSharedAsync(name, initializeDatabase);
        }

        private new async Task<InMemoryTestStore> CreateSharedAsync(string name, Func<Task> initializeDatabase)
        {
            await base.CreateSharedAsync(typeof(InMemoryTestStore).Name + name, initializeDatabase);

            return this;
        }
    }
}
