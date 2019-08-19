// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FiltersInMemoryTest : FiltersTestBase<NorthwindQueryInMemoryFixture<NorthwindFiltersCustomizer>>
    {
        public FiltersInMemoryTest(NorthwindQueryInMemoryFixture<NorthwindFiltersCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact(Skip = "See issue #13857")]
        public override void Client_eval()
        {
        }

        [ConditionalFact(Skip = "See issue #13857")]
        public override void Included_one_to_many_query_with_client_eval()
        {
        }

        [ConditionalFact(Skip = "issue #16963")]
        public override void Include_query()
        {
            base.Include_query();
        }

        [ConditionalFact(Skip = "issue #16963")]
        public override void Include_query_opt_out()
        {
            base.Include_query_opt_out();
        }

        [ConditionalFact(Skip = "issue #16963")]
        public override void Included_many_to_one_query()
        {
            base.Included_many_to_one_query();
        }
    }
}
