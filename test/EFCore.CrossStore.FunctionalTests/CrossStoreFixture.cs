// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.CrossStore.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.CrossStore.FunctionalTests
{
    public abstract class CrossStoreFixture
    {
        public abstract TestStore CreateTestStore(Type testStoreType);

        public abstract CrossStoreContext CreateContext(TestStore testStore);
    }
}
