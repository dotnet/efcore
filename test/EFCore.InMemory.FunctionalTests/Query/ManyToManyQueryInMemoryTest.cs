// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ManyToManyQueryInMemoryTest : ManyToManyQueryTestBase<ManyToManyQueryInMemoryFixture>
    {
        public ManyToManyQueryInMemoryTest(ManyToManyQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public override Task Include_skip_navigation_then_include_inverse_throws_in_no_tracking(bool async)
            => Task.CompletedTask;
    }
}
