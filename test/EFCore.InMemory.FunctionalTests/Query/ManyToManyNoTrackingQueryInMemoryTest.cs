// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ManyToManyNoTrackingQueryInMemoryTest
        : ManyToManyNoTrackingQueryTestBase<ManyToManyNoTrackingQueryInMemoryTest.ManyToManyNoTrackingFixture>
    {
        public ManyToManyNoTrackingQueryInMemoryTest(ManyToManyNoTrackingFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public class ManyToManyNoTrackingFixture : ManyToManyQueryInMemoryFixture
        {
            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
}
