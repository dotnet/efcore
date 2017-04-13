// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class ComplexNavigationsQueryInMemoryTest : ComplexNavigationsQueryTestBase<InMemoryTestStore, ComplexNavigationsQueryInMemoryFixture>
    {
        public ComplexNavigationsQueryInMemoryTest(ComplexNavigationsQueryInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "issue #4311")]
        public override void Nested_group_join_with_take()
        {
            base.Nested_group_join_with_take();
        }
    }
}
