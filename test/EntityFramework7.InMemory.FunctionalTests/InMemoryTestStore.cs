// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryTestStore : TestStore
    {
        public static InMemoryTestStore GetOrCreateShared(string name, Action initializeDatabase)
        {
            return new InMemoryTestStore().CreateShared(name, initializeDatabase);
        }

        private new InMemoryTestStore CreateShared(string name, Action initializeDatabase)
        {
            base.CreateShared(typeof(InMemoryTestStore).Name + name, initializeDatabase);

            return this;
        }
    }
}
