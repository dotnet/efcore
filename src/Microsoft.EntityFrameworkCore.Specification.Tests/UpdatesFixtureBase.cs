// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.UpdatesModel;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class UpdatesFixtureBase<TTestStore>
    {
        public abstract TTestStore CreateTestStore();

        public abstract UpdatesContext CreateContext(TTestStore testStore);
    }
}
