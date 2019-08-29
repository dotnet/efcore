// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceInMemoryTest : InheritanceTestBase<InheritanceInMemoryFixture>
    {
        public InheritanceInMemoryTest(InheritanceInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact(Skip = "Issue#17504")]
        public override void Can_query_all_animal_views()
        {
            base.Can_query_all_animal_views();
        }

        protected override bool EnforcesFkConstraints => false;
    }
}
