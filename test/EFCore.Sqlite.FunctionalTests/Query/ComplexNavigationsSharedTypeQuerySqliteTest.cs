// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsSharedTypeQuerySqliteTest : ComplexNavigationsSharedTypeQueryRelationalTestBase<
        ComplexNavigationsSharedTypeQuerySqliteFixture>
    {
        public ComplexNavigationsSharedTypeQuerySqliteTest(ComplexNavigationsSharedTypeQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        public override async Task Let_let_contains_from_outer_let(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Let_let_contains_from_outer_let(async))).Message);

        public override async Task Prune_does_not_throw_null_ref(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Prune_does_not_throw_null_ref(async))).Message);

        [ConditionalTheory(Skip = "Issue#26104")]
        public override Task GroupBy_aggregate_where_required_relationship(bool async)
        {
            return base.GroupBy_aggregate_where_required_relationship(async);
        }

        [ConditionalTheory(Skip = "Issue#26104")]
        public override Task GroupBy_aggregate_where_required_relationship_2(bool async)
        {
            return base.GroupBy_aggregate_where_required_relationship_2(async);
        }
    }
}
