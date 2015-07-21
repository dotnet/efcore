// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class CrossStoreFixture
    {
        public abstract TestStore CreateTestStore(Type testStoreType);

        public abstract CrossStoreContext CreateContext(TestStore testStore);
    }
}
