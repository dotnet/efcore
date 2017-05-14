// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class ComplexNavigationsOwnedQuerySqliteTest : ComplexNavigationsOwnedQueryTestBase<SqliteTestStore, ComplexNavigationsOwnedQuerySqliteFixture>
    {
        public ComplexNavigationsOwnedQuerySqliteTest(ComplexNavigationsOwnedQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "issue #4311")]
        public override void Nested_group_join_with_take()
        {
            base.Nested_group_join_with_take();
        }

        [ConditionalFact(Skip = "issue #8492")]
        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection2()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2();
        }
    }
}
