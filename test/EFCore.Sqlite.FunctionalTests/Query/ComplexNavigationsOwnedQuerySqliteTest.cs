// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
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
    }
}
