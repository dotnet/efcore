// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    //issue #15264
    internal class FiltersInMemoryTest : FiltersTestBase<NorthwindQueryInMemoryFixture<NorthwindFiltersCustomizer>>
    {
        public FiltersInMemoryTest(NorthwindQueryInMemoryFixture<NorthwindFiltersCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }
    }
}
