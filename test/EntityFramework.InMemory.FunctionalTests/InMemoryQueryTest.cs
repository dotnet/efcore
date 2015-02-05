// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryQueryTest : QueryTestBase<InMemoryNorthwindQueryFixture>
    {
        public override void Where_simple_closure()
        {
            base.Where_simple_closure();
        }

        public InMemoryQueryTest(InMemoryNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
