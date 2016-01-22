// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.FunctionalTests;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public abstract class GearsOfWarQueryRelationalFixture<TTestStore> : GearsOfWarQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
    }
}
