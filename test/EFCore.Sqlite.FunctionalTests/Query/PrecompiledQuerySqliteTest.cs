// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public class PrecompiledQuerySqliteTest(
    PrecompiledQuerySqliteTest.PrecompiledQuerySqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledQuerySqliteTest.PrecompiledQuerySqliteFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public override async Task Skip_Take_with_parameters()
    {
        await base.Skip_Take_with_parameters();

        AssertSql(
            """
@p1='3'
@p='0'

SELECT "p"."Id"
FROM "Posts" AS "p"
ORDER BY "p"."Id"
LIMIT @p1 OFFSET @p
""",
            """
@p1='2'
@p='1'

SELECT "p"."Id"
FROM "Posts" AS "p"
ORDER BY "p"."Id"
LIMIT @p1 OFFSET @p
""",
            """
@p1='1'
@p='1'

SELECT "p"."Id"
FROM "Posts" AS "p"
ORDER BY "p"."Id"
LIMIT @p1 OFFSET @p
""",
            """
@p1='1'
@p='5'

SELECT "p"."Id"
FROM "Posts" AS "p"
ORDER BY "p"."Id"
LIMIT @p1 OFFSET @p
""");
    }

    public override async Task Skip_Take_with_captured_parameters_in_local_function()
    {
        await base.Skip_Take_with_captured_parameters_in_local_function();

        AssertSql(
            """
@p1='2'
@p='1'

SELECT "p"."Id"
FROM "Posts" AS "p"
WHERE "p"."Id" > 10
ORDER BY "p"."Id"
LIMIT @p1 OFFSET @p
""");
    }

    [Fact]
    public virtual Task Glob()
        => Test("""_ = context.Blogs.Where(b => EF.Functions.Glob(b.Name, "*foo*")).ToList();""");

    [Fact]
    public virtual Task Regexp()
        => Test("""_ = context.Blogs.Where(b => Regex.IsMatch(b.Name, "^foo")).ToList();""");

    public class PrecompiledQuerySqliteFixture : PrecompiledQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
            => SqlitePrecompiledQueryTestHelpers.Instance;
    }
}
