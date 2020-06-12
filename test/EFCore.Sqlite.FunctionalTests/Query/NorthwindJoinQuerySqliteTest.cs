// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindJoinQuerySqliteTest : NorthwindJoinQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindJoinQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // CROSS APPLY is not supported
        public override Task SelectMany_with_client_eval(bool async) => Task.CompletedTask;
        public override Task SelectMany_with_client_eval_with_collection_shaper(bool async) => Task.CompletedTask;
        public override Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async) => Task.CompletedTask;
    }
}
