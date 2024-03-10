// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class Ef6GroupBySqliteTest : Ef6GroupByTestBase<Ef6GroupBySqliteTest.Ef6GroupBySqliteFixture>
{
    public Ef6GroupBySqliteTest(Ef6GroupBySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Average_Grouped_from_LINQ_101(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                () => base.Average_Grouped_from_LINQ_101(async))).Message);

    public override async Task Max_Grouped_from_LINQ_101(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Max", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                () => base.Max_Grouped_from_LINQ_101(async))).Message);

    public override async Task Min_Grouped_from_LINQ_101(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Min", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                () => base.Min_Grouped_from_LINQ_101(async))).Message);

    public override async Task Whats_new_2021_sample_3(bool async)
        => await base.Whats_new_2021_sample_3(async);

    public override async Task Whats_new_2021_sample_5(bool async)
        => await base.Whats_new_2021_sample_5(async);

    public override async Task Whats_new_2021_sample_6(bool async)
        => await base.Whats_new_2021_sample_6(async);

    public override async Task Group_Join_from_LINQ_101(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Group_Join_from_LINQ_101(async))).Message);

    public class Ef6GroupBySqliteFixture : Ef6GroupByFixtureBase, ITestSqlLoggerFactory
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
