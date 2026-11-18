// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class PrecompiledSqlPregenerationQuerySqlServerTest(
    PrecompiledSqlPregenerationQuerySqlServerTest.PrecompiledSqlPregenerationQuerySqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledSqlPregenerationQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledSqlPregenerationQuerySqlServerTest.PrecompiledSqlPregenerationQuerySqliteFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    [Fact]
    public Task Contains_with_captured_collection_as_single_parameter()
        => Test(
            """
List<string?>[] values = [["Blog1", null], ["Blog2"], []];
int[][] expected = [[8], [9], []];
List<string?> names = [];
for (var i = 0; i < values.Length; i++)
{
    names = values[i];
    var actual = await context.Blogs.Where(b => EF.Parameter(names).Contains(b.Name)).Select(b => b.Id).ToListAsync();
    Assert.Equal(expected[i], actual);
}
""",
            interceptorCodeAsserter: _ => { });

    public class PrecompiledSqlPregenerationQuerySqliteFixture : PrecompiledSqlPregenerationQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
            => SqlitePrecompiledQueryTestHelpers.Instance;
    }
}
