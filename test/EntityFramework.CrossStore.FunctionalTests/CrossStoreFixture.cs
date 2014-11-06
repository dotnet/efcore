// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class CrossStoreFixture<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract CrossStoreContext CreateContext(TTestStore testStore);
    }
}
