// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    // Currently no non-relational interceptors
    public class InterceptionInMemoryTest : InterceptionTestBase
    {
        public InterceptionInMemoryTest(InterceptionFixtureBase fixture)
            : base(fixture)
        {
        }
    }
}
