// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQueryInMemoryTest : CompiledQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public CompiledQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "See issue #17386")]
        public override void Query_with_array_parameter()
        {
        }

        [ConditionalFact(Skip = "See issue #17386")]
        public override Task Query_with_array_parameter_async() => null;
    }
}
