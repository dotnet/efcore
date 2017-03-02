// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class DatabindingInMemoryTest : DatabindingTestBase<InMemoryTestStore, F1InMemoryFixture>
    {
        public DatabindingInMemoryTest(F1InMemoryFixture fixture)
            : base(fixture)
        {
        }
    }
}
