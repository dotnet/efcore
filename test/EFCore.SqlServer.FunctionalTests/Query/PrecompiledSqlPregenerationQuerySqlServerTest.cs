// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

// ReSharper disable InconsistentNaming
public class PrecompiledSqlPregenerationQuerySqlServerTest(
    PrecompiledSqlPregenerationQuerySqlServerTest.PrecompiledSqlPregenerationQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledSqlPregenerationQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledSqlPregenerationQuerySqlServerTest.PrecompiledSqlPregenerationQuerySqlServerFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public override async Task No_parameters()
    {
        await base.No_parameters();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] = N'foo'
""");
    }

    public override async Task Non_nullable_value_type()
    {
        await base.Non_nullable_value_type();

        AssertSql(
            """
@id='8'

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Id] = @id
""");
    }

    public override async Task Nullable_value_type()
    {
        await base.Nullable_value_type();

        AssertSql(
            """
@id='8' (Nullable = true)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Id] = @id
""");
    }

    public override async Task Nullable_reference_type()
    {
        await base.Nullable_reference_type();

        AssertSql(
            """
@name='bar' (Size = 4000)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] = @name
""");
    }

    public override async Task Non_nullable_reference_type()
    {
        await base.Non_nullable_reference_type();

        AssertSql(
            """
@name='bar' (Nullable = false) (Size = 4000)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] = @name
""");
    }

    public override async Task Nullable_and_non_nullable_value_types()
    {
        await base.Nullable_and_non_nullable_value_types();

        AssertSql(
            """
@id1='8' (Nullable = true)
@id2='9'

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Id] = @id1 OR [b].[Id] = @id2
""");
    }

    public override async Task Two_nullable_reference_types()
    {
        await base.Two_nullable_reference_types();

        AssertSql(
            """
@name1='foo' (Size = 4000)
@name2='bar' (Size = 4000)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] = @name1 OR [b].[Name] = @name2
""");
    }

    public override async Task Two_non_nullable_reference_types()
    {
        await base.Two_non_nullable_reference_types();

        AssertSql(
            """
@name1='foo' (Nullable = false) (Size = 4000)
@name2='bar' (Nullable = false) (Size = 4000)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] = @name1 OR [b].[Name] = @name2
""");
    }

    public override async Task Nullable_and_non_nullable_reference_types()
    {
        await base.Nullable_and_non_nullable_reference_types();

        AssertSql(
            """
@name1='foo' (Size = 4000)
@name2='bar' (Nullable = false) (Size = 4000)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] = @name1 OR [b].[Name] = @name2
""");
    }

    public override async Task Too_many_nullable_parameters_prevent_pregeneration()
    {
        await base.Too_many_nullable_parameters_prevent_pregeneration();

        AssertSql(
            """
@name1='foo' (Size = 4000)
@name2='bar' (Size = 4000)
@name3='baz' (Size = 4000)
@name4='baq' (Size = 4000)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] = @name1 OR [b].[Name] = @name2 OR [b].[Name] = @name3 OR [b].[Name] = @name4
""");
    }

    public override async Task Many_non_nullable_parameters_do_not_prevent_pregeneration()
    {
        await base.Many_non_nullable_parameters_do_not_prevent_pregeneration();

        AssertSql(
            """
@name1='foo' (Nullable = false) (Size = 4000)
@name2='bar' (Nullable = false) (Size = 4000)
@name3='baz' (Nullable = false) (Size = 4000)
@name4='baq' (Nullable = false) (Size = 4000)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] = @name1 OR [b].[Name] = @name2 OR [b].[Name] = @name3 OR [b].[Name] = @name4
""");
    }

    #region Tests for the different querying enumerables

    public override async Task Include_single_query()
    {
        await base.Include_single_query();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [p].[Id], [p].[BlogId], [p].[Title]
FROM [Blogs] AS [b]
LEFT JOIN [Post] AS [p] ON [b].[Id] = [p].[BlogId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Include_split_query()
    {
        await base.Include_split_query();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [p].[Id], [p].[BlogId], [p].[Title], [b].[Id]
FROM [Blogs] AS [b]
INNER JOIN [Post] AS [p] ON [b].[Id] = [p].[BlogId]
ORDER BY [b].[Id]
""");
    }

    public override async Task Final_GroupBy()
    {
        await base.Final_GroupBy();

        AssertSql(
            """
SELECT [b].[Name], [b].[Id]
FROM [Blogs] AS [b]
ORDER BY [b].[Name]
""");
    }

    #endregion Tests for the different querying enumerables

    [ConditionalFact]
    public virtual async Task Do_not_cache_is_respected()
    {
        // The "do not cache" flag in the 2nd part of the query pipeline is turned on in provider-specific situations, so we test it
        // here in SQL Server; note that SQL Server compatibility mode is set low to trigger this.
        await Test(
            """
string[] names = ["foo", "bar"];
var blogs = await context.Blogs.Where(b => names.Contains(b.Name)).ToListAsync();
""",
            interceptorCodeAsserter: code => Assert.Contains(nameof(RelationalCommandCache), code));

        AssertSql(
            """
@names1='foo' (Size = 4000)
@names2='bar' (Size = 4000)

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Name] IN (@names1, @names2)
""");
    }

    public class PrecompiledSqlPregenerationQuerySqlServerFixture : PrecompiledSqlPregenerationQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            builder = base.AddOptions(builder);

            // TODO: Figure out if there's a nice way to continue using the retrying strategy
            var sqlServerOptionsBuilder = new SqlServerDbContextOptionsBuilder(builder);
            sqlServerOptionsBuilder
                .UseCompatibilityLevel(120)
                .ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
            return builder;
        }

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
            => SqlServerPrecompiledQueryTestHelpers.Instance;
    }
}
