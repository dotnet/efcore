// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsWeakQuerySqliteTest : ComplexNavigationsWeakQueryTestBase<ComplexNavigationsWeakQuerySqliteFixture>
    {
        public ComplexNavigationsWeakQuerySqliteTest(ComplexNavigationsWeakQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return Task.CompletedTask;
        }
    }
}
