// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQuerySqliteTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(fixture, testOutputHelper)
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => SqlitePrecompiledQueryTestHelpers.Instance;

    [Fact]
    public async Task Projection_cast_between_clr_types_with_same_store_type()
    {
        var contextFactory = await InitializeNonSharedTest<AdHocMiscellaneousQuerySqliteTest.Context39782>(
            seed: async context =>
            {
                context.Events.Add(
                    new AdHocMiscellaneousQuerySqliteTest.Context39782.Event
                    {
                        Timestamp = new DateOnly(2000, 2, 29),
                        NullableTimestamp = new DateOnly(2000, 2, 29)
                    });
                await context.SaveChangesAsync();
            });

        await Test(
            """
await using var context = new AdHocMiscellaneousQuerySqliteTest.Context39782(dbContextOptions);

DateOnly? first = new DateOnly(2000, 2, 29);
DateOnly? second = new DateOnly(2001, 1, 1);
DateOnly? third = null;
DateOnly? fourth = new DateOnly(2002, 1, 1);
var results = await context.Events
    .Where(e => e.NullableTimestamp == first || e.NullableTimestamp == second
        || e.NullableTimestamp == third || e.NullableTimestamp == fourth)
    .Select(e => (DateTimeOffset)(object)e.Timestamp)
    .ToListAsync();

Assert.Equal([new DateTimeOffset(2000, 2, 29, 0, 0, 0, TimeSpan.Zero)], results);
""",
            typeof(AdHocMiscellaneousQuerySqliteTest.Context39782),
            contextFactory.GetOptions(),
            interceptorCodeAsserter: code =>
            {
                Assert.Contains("RelationalCommandCache", code);
                Assert.Contains("ClrTypeChangeExpression", code);
            });
    }
}
