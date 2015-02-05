// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryAsyncQueryTest : AsyncQueryTestBase<InMemoryNorthwindQueryFixture>
    {
        public override async Task SelectMany_simple2()
        {
            await base.SelectMany_simple2();
        }

        public InMemoryAsyncQueryTest(InMemoryNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
