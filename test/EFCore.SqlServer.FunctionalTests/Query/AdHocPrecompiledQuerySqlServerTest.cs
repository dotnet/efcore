// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQuerySqlServerTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(fixture, testOutputHelper)
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public override async Task Index_no_evaluatability()
    {
        if (!SqlServerTestEnvironment.SupportsJsonPathExpressions)

        {
            throw SkipException.ForSkip("Requires SupportsJsonPathExpressions");
        }

        await base.Index_no_evaluatability();

        AssertSql(
            """
SELECT [j].[Id], [j].[IntList], [j].[JsonThing]
FROM [JsonEntities] AS [j]
WHERE CAST(JSON_VALUE([j].[IntList], '$[' + CAST([j].[Id] AS nvarchar(max)) + ']') AS int) = 2
""");
    }

    public override async Task Index_with_captured_variable()
    {
        if (!SqlServerTestEnvironment.SupportsJsonPathExpressions)

        {
            throw SkipException.ForSkip("Requires SupportsJsonPathExpressions");
        }

        await base.Index_with_captured_variable();

        AssertSql(
            """
@id='1'

SELECT [j].[Id], [j].[IntList], [j].[JsonThing]
FROM [JsonEntities] AS [j]
WHERE CAST(JSON_VALUE([j].[IntList], '$[' + CAST(@id AS nvarchar(max)) + ']') AS int) = 2
""");
    }

    public override async Task JsonScalar()
    {
        await base.JsonScalar();

        AssertSql(
            """
SELECT [j].[Id], [j].[IntList], [j].[JsonThing]
FROM [JsonEntities] AS [j]
WHERE JSON_VALUE([j].[JsonThing], '$.StringProperty') = N'foo'
""");
    }

    public override async Task Materialize_non_public()
    {
        await base.Materialize_non_public();

        AssertSql(
            """
@p0='10' (Nullable = true)
@p1='9' (Nullable = true)
@p2='8' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NonPublicEntities] ([PrivateAutoProperty], [PrivateProperty], [_privateField])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[PrivateAutoProperty], [n].[PrivateProperty], [n].[_privateField]
FROM [NonPublicEntities] AS [n]
""");
    }

    public override async Task Projecting_property_requiring_converter_with_closure_is_not_supported()
    {
        await base.Projecting_property_requiring_converter_with_closure_is_not_supported();

        AssertSql();
    }

    public override async Task Projecting_expression_requiring_converter_without_closure_works()
    {
        await base.Projecting_expression_requiring_converter_without_closure_works();

        AssertSql(
            """
SELECT [b].[AudiobookDate]
FROM [Books] AS [b]
""");
    }

    public override async Task Projecting_entity_with_property_requiring_converter_with_closure_works()
    {
        await base.Projecting_entity_with_property_requiring_converter_with_closure_works();

        AssertSql(
            """
SELECT [b].[Id], [b].[AudiobookDate], [b].[Name], [b].[PublishDate]
FROM [Books] AS [b]
""");
    }

    public override async Task Invalid_identifier_json_property_name()
    {
        await base.Invalid_identifier_json_property_name();

        AssertSql(
            """
SELECT [e].[Id], [e].[Nested]
FROM [Entities] AS [e]
""");
    }

    public override async Task Query_reusing_a_runtime_constant_of_a_query_that_failed_to_precompile()
    {
        await base.Query_reusing_a_runtime_constant_of_a_query_that_failed_to_precompile();

        AssertSql(
            """
SELECT [e].[Id], [e].[Nested]
FROM [Entities] AS [e]
ORDER BY [e].[Id]
""");
    }

    public override async Task Query_that_fails_to_precompile_leaves_the_other_queries_compilable()
    {
        await base.Query_that_fails_to_precompile_leaves_the_other_queries_compilable();

        AssertSql(
            """
SELECT [s].[Id]
FROM [Seconds] AS [s]
""");
    }

    public override async Task Liftable_constant_named_like_the_query_context_parameter()
    {
        await base.Liftable_constant_named_like_the_query_context_parameter();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
""");
    }

    public override async Task Liftable_constant_named_like_the_db_context_parameter()
    {
        await base.Liftable_constant_named_like_the_db_context_parameter();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
""");
    }

    public override async Task Liftable_constant_whose_sanitized_name_is_another_constants_name()
    {
        await base.Liftable_constant_whose_sanitized_name_is_another_constants_name();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
""");
    }

    public override async Task Liftable_constant_named_like_a_keyword()
    {
        await base.Liftable_constant_named_like_a_keyword();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [Entities] AS [e]
""");
    }

    public override async Task Invalid_identifier_shadow_property_name()
    {
        await base.Invalid_identifier_shadow_property_name();

        AssertSql(
            """
SELECT [e].[Id], [e].[NOT VALID !!!1]
FROM [Entities] AS [e]
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task Runtime_constant_named_like_the_executor_field()
    {
        await base.Runtime_constant_named_like_the_executor_field();

        AssertSql(
            """
SELECT [e].[Id]
FROM [Entities] AS [e]
""");
    }

    public override async Task Runtime_constant_named_like_the_interceptors_class()
    {
        await base.Runtime_constant_named_like_the_interceptors_class();

        AssertSql(
            """
SELECT [e].[Id]
FROM [Entities] AS [e]
""");
    }

    public override async Task Runtime_constant_named_like_a_type_the_generated_code_uses()
    {
        await base.Runtime_constant_named_like_a_type_the_generated_code_uses();

        AssertSql(
            """
SELECT [e].[Id]
FROM [Entities] AS [e]
""");
    }

    public override async Task Runtime_constant_named_like_a_reserved_token_once_prefixed()
    {
        await base.Runtime_constant_named_like_a_reserved_token_once_prefixed();

        AssertSql(
            """
SELECT [e].[Id]
FROM [Entities] AS [e]
""");
    }

    public override async Task Runtime_constants_differing_only_by_a_formatting_character_get_distinct_fields()
    {
        await base.Runtime_constants_differing_only_by_a_formatting_character_get_distinct_fields();

        AssertSql(
            """
SELECT [e].[Id], [e].[Nested]
FROM [Entities] AS [e]
""");
    }

    public override async Task Query_that_fails_to_precompile_between_two_that_succeed_leaves_both_compilable()
    {
        await base.Query_that_fails_to_precompile_between_two_that_succeed_leaves_both_compilable();

        AssertSql(
            """
SELECT [s].[Id]
FROM [Seconds] AS [s]
""",
            //
            """
SELECT [s].[Id]
FROM [Seconds] AS [s]
ORDER BY [s].[Id]
""");
    }

    public override async Task Liftable_constant_named_like_a_runtime_constant_field()
    {
        await base.Liftable_constant_named_like_a_runtime_constant_field();

        AssertSql(
            """
SELECT [e].[Id], [e].[Nested]
FROM [Entities] AS [e]
""");
    }

    public override async Task Runtime_constant_shared_by_two_queries_is_emitted_once()
    {
        await base.Runtime_constant_shared_by_two_queries_is_emitted_once();

        AssertSql(
            """
SELECT [e].[Id], [e].[Nested]
FROM [Entities] AS [e]
""",
            //
            """
SELECT [e].[Id], [e].[Nested]
FROM [Entities] AS [e]
ORDER BY [e].[Id]
""");
    }

    public override async Task Runtime_constant_named_like_a_type_with_a_leading_underscore()
    {
        await base.Runtime_constant_named_like_a_type_with_a_leading_underscore();

        AssertSql(
            """
SELECT [e].[Id]
FROM [Entities] AS [e]
""");
    }

    public override async Task Shaper_variables_named_like_the_executor_identifiers_are_uniquified()
    {
        await base.Shaper_variables_named_like_the_executor_identifiers_are_uniquified();

        AssertSql(
            """
SELECT [e].[Id]
FROM [Entities] AS [e]
""");
    }

    public override async Task Runtime_constant_named_like_an_unsafe_accessor()
    {
        await base.Runtime_constant_named_like_an_unsafe_accessor();

        AssertSql(
            """
SELECT [e].[Id], [e].[NameBytes], [e].[Nested]
FROM [Entities] AS [e]
""");
    }

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => SqlServerPrecompiledQueryTestHelpers.Instance;

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
    {
        builder = base.AddNonSharedOptions(builder);

        // TODO: Figure out if there's a nice way to continue using the retrying strategy
        var sqlServerOptionsBuilder = new SqlServerDbContextOptionsBuilder(builder);
        sqlServerOptionsBuilder.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
        return builder;
    }
}
