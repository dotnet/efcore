// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.FunctionalTests;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class ChangeTrackingInMemoryTest : ChangeTrackingTestBase<NorthwindQueryInMemoryFixture>
    {
        public ChangeTrackingInMemoryTest(NorthwindQueryInMemoryFixture fixture)
            : base(fixture)
        {
        }


        // TODO: See issue #4457
        public override void Entity_range_does_not_revert_when_attached_dbContext()
        {
        }

        // TODO: See issue #4457
        public override void Entity_range_does_not_revert_when_attached_dbSet()
        {
        }
    }
}
