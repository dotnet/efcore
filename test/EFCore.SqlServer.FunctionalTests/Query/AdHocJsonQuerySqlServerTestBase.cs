// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocJsonQuerySqlServerTestBase(NonSharedFixture fixture) : AdHocJsonQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override void ConfigureWarnings(WarningsConfigurationBuilder builder)
    {
        base.ConfigureWarnings(builder);

        builder.Log(CoreEventId.StringEnumValueInJson, SqlServerEventId.JsonTypeExperimental);
    }

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    public override async Task Project_root_with_missing_scalars(bool async)
    {
        await base.Project_root_with_missing_scalars(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[Collection], [e].[OptionalReference], [e].[RequiredReference]
FROM [Entities] AS [e]
WHERE [e].[Id] < 4
""");
    }

    public override async Task Project_top_level_json_entity_with_missing_scalars(bool async)
    {
        await base.Project_top_level_json_entity_with_missing_scalars(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[OptionalReference], [e].[RequiredReference], [e].[Collection]
FROM [Entities] AS [e]
WHERE [e].[Id] < 4
""");
    }

    public override async Task Project_nested_json_entity_with_missing_scalars(bool async)
    {
        await base.Project_nested_json_entity_with_missing_scalars(async);

        AssertSql(
"""
SELECT [e].[Id], JSON_QUERY([e].[OptionalReference], '$.NestedOptionalReference'), JSON_QUERY([e].[RequiredReference], '$.NestedRequiredReference'), JSON_QUERY([e].[Collection], '$[0].NestedCollection')
FROM [Entities] AS [e]
WHERE [e].[Id] < 4
""");
    }

    public override async Task Project_root_entity_with_missing_required_navigation(bool async)
    {
        await base.Project_root_entity_with_missing_required_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[Collection], [e].[OptionalReference], [e].[RequiredReference]
FROM [Entities] AS [e]
WHERE [e].[Id] = 5
""");
    }


    public override async Task Project_missing_required_navigation(bool async)
    {
        await base.Project_missing_required_navigation(async);

        AssertSql(
            """
SELECT JSON_QUERY([e].[RequiredReference], '$.NestedRequiredReference'), [e].[Id]
FROM [Entities] AS [e]
WHERE [e].[Id] = 5
""");
    }

    public override async Task Project_root_entity_with_null_required_navigation(bool async)
    {
        await base.Project_root_entity_with_null_required_navigation(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[Collection], [e].[OptionalReference], [e].[RequiredReference]
FROM [Entities] AS [e]
WHERE [e].[Id] = 6
""");
    }

    public override async Task Project_null_required_navigation(bool async)
    {
        await base.Project_null_required_navigation(async);

        AssertSql(
            """
SELECT [e].[RequiredReference], [e].[Id]
FROM [Entities] AS [e]
WHERE [e].[Id] = 6
""");
    }

    public override async Task Project_missing_required_scalar(bool async)
    {
        await base.Project_missing_required_scalar(async);

        AssertSql(
            """
SELECT [e].[Id], CAST(JSON_VALUE([e].[RequiredReference], '$.Number') AS float) AS [Number]
FROM [Entities] AS [e]
WHERE [e].[Id] = 2
""");
    }

    public override async Task Project_null_required_scalar(bool async)
    {
        await base.Project_null_required_scalar(async);

        AssertSql(
            """
SELECT [e].[Id], CAST(JSON_VALUE([e].[RequiredReference], '$.Number') AS float) AS [Number]
FROM [Entities] AS [e]
WHERE [e].[Id] = 4
""");
    }

    protected override async Task Seed21006(Context21006 context)
    {
        await base.Seed21006(context);

        // missing scalar on top level
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO [Entities] ([Collection], [OptionalReference], [RequiredReference], [Id], [Name])
VALUES (
N'[{"Text":"e2 c1","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e2 c1 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e2 c1 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 c1 nor"},"NestedRequiredReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 c1 nrr"}},{"Text":"e2 c2","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e2 c2 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e2 c2 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 c2 nor"},"NestedRequiredReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 c2 nrr"}}]',
N'{"Text":"e2 or","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e2 or c1"},{"DoB":"2000-01-01T00:00:00","Text":"e2 or c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 or nor"},"NestedRequiredReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 or nrr"}}',
N'{"Text":"e2 rr","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e2 rr c1"},{"DoB":"2000-01-01T00:00:00","Text":"e2 rr c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 rr nor"},"NestedRequiredReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 rr nrr"}}',
2,
N'e2')
""");

        // missing scalar on nested level
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO [Entities] ([Collection], [OptionalReference], [RequiredReference], [Id], [Name])
VALUES (
N'[{"Number":7,"Text":"e3 c1","NestedCollection":[{"Text":"e3 c1 c1"},{"Text":"e3 c1 c2"}],"NestedOptionalReference":{"Text":"e3 c1 nor"},"NestedRequiredReference":{"Text":"e3 c1 nrr"}},{"Number":7,"Text":"e3 c2","NestedCollection":[{"Text":"e3 c2 c1"},{"Text":"e3 c2 c2"}],"NestedOptionalReference":{"Text":"e3 c2 nor"},"NestedRequiredReference":{"Text":"e3 c2 nrr"}}]',
N'{"Number":7,"Text":"e3 or","NestedCollection":[{"Text":"e3 or c1"},{"Text":"e3 or c2"}],"NestedOptionalReference":{"Text":"e3 or nor"},"NestedRequiredReference":{"Text":"e3 or nrr"}}',
N'{"Number":7,"Text":"e3 rr","NestedCollection":[{"Text":"e3 rr c1"},{"Text":"e3 rr c2"}],"NestedOptionalReference":{"Text":"e3 rr nor"},"NestedRequiredReference":{"Text":"e3 rr nrr"}}',
3,
N'e3')
""");

        // null scalar on top level
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO [Entities] ([Collection], [OptionalReference], [RequiredReference], [Id], [Name])
VALUES (
N'[{"Number":null,"Text":"e4 c1","NestedCollection":[{"Text":"e4 c1 c1"},{"Text":"e4 c1 c2"}],"NestedOptionalReference":{"Text":"e4 c1 nor"},"NestedRequiredReference":{"Text":"e4 c1 nrr"}},{"Number":null,"Text":"e4 c2","NestedCollection":[{"Text":"e4 c2 c1"},{"Text":"e4 c2 c2"}],"NestedOptionalReference":{"Text":"e4 c2 nor"},"NestedRequiredReference":{"Text":"e4 c2 nrr"}}]',
N'{"Number":null,"Text":"e4 or","NestedCollection":[{"Text":"e4 or c1"},{"Text":"e4 or c2"}],"NestedOptionalReference":{"Text":"e4 or nor"},"NestedRequiredReference":{"Text":"e4 or nrr"}}',
N'{"Number":null,"Text":"e4 rr","NestedCollection":[{"Text":"e4 rr c1"},{"Text":"e4 rr c2"}],"NestedOptionalReference":{"Text":"e4 rr nor"},"NestedRequiredReference":{"Text":"e4 rr nrr"}}',
4,
N'e4')
""");

        // missing required navigation
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO [Entities] ([Collection], [OptionalReference], [RequiredReference], [Id], [Name])
VALUES (
N'[{"Number":7,"Text":"e5 c1","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e5 c1 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e5 c1 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e5 c1 nor"}},{"Number":7,"Text":"e5 c2","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e5 c2 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e5 c2 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e5 c2 nor"}}]',
N'{"Number":7,"Text":"e5 or","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e5 or c1"},{"DoB":"2000-01-01T00:00:00","Text":"e5 or c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e5 or nor"}}',
N'{"Number":7,"Text":"e5 rr","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e5 rr c1"},{"DoB":"2000-01-01T00:00:00","Text":"e5 rr c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e5 rr nor"}}',
5,
N'e5')
""");

        // null required navigation
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO [Entities] ([Collection], [OptionalReference], [RequiredReference], [Id], [Name])
VALUES (
N'[{"Number":7,"Text":"e6 c1","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e6 c1 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e6 c1 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e6 c1 nor"},"NestedRequiredReference":null},{"Number":7,"Text":"e6 c2","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e6 c2 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e6 c2 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e6 c2 nor"},"NestedRequiredReference":null}]',
N'{"Number":7,"Text":"e6 or","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e6 or c1"},{"DoB":"2000-01-01T00:00:00","Text":"e6 or c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e6 or nor"},"NestedRequiredReference":null}',
N'{"Number":7,"Text":"e6 rr","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e6 rr c1"},{"DoB":"2000-01-01T00:00:00","Text":"e6 rr c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e6 rr nor"},"NestedRequiredReference":null}',
6,
N'e6')
""");
    }

    protected override async Task Seed29219(DbContext ctx)
    {
        await base.Seed29219(ctx);

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Id], [Reference], [Collection])
VALUES(3, N'{ "NonNullableScalar" : 30 }', N'[{ "NonNullableScalar" : 10001 }]')
""");
    }

    protected override async Task Seed30028(DbContext ctx)
    {
        // complete
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO [Entities] ([Id], [Json])
VALUES(
1,
N'{"RootName":"e1","Collection":[{"BranchName":"e1 c1","Nested":{"LeafName":"e1 c1 l"}},{"BranchName":"e1 c2","Nested":{"LeafName":"e1 c2 l"}}],"OptionalReference":{"BranchName":"e1 or","Nested":{"LeafName":"e1 or l"}},"RequiredReference":{"BranchName":"e1 rr","Nested":{"LeafName":"e1 rr l"}}}')
""");

        // missing collection
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO [Entities] ([Id], [Json])
VALUES(
2,
N'{"RootName":"e2","OptionalReference":{"BranchName":"e2 or","Nested":{"LeafName":"e2 or l"}},"RequiredReference":{"BranchName":"e2 rr","Nested":{"LeafName":"e2 rr l"}}}')
""");

        // missing optional reference
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO [Entities] ([Id], [Json])
VALUES(
3,
N'{"RootName":"e3","Collection":[{"BranchName":"e3 c1","Nested":{"LeafName":"e3 c1 l"}},{"BranchName":"e3 c2","Nested":{"LeafName":"e3 c2 l"}}],"RequiredReference":{"BranchName":"e3 rr","Nested":{"LeafName":"e3 rr l"}}}')
""");

        // missing required reference
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO [Entities] ([Id], [Json])
VALUES(
4,
N'{"RootName":"e4","Collection":[{"BranchName":"e4 c1","Nested":{"LeafName":"e4 c1 l"}},{"BranchName":"e4 c2","Nested":{"LeafName":"e4 c2 l"}}],"OptionalReference":{"BranchName":"e4 or","Nested":{"LeafName":"e4 or l"}}}')
""");
    }

    protected override Task Seed33046(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Reviews] ([Rounds], [Id])
VALUES(N'[{"RoundNumber":11,"SubRounds":[{"SubRoundNumber":111},{"SubRoundNumber":112}]}]', 1)
""");

    protected override async Task Seed34960(Context34960 ctx)
    {
        await base.Seed34960(ctx);

        // JSON nulls
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Collection], [Reference], [Id])
VALUES(
N'null',
N'null',
4)
""");

        // JSON object where collection should be
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Junk] ([Collection], [Reference], [Id])
VALUES(
N'{ "DoB":"2000-01-01T00:00:00","Text":"junk" }',
NULL,
1)
""");

        // JSON array where entity should be
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Junk] ([Collection], [Reference], [Id])
VALUES(
NULL,
N'[{ "DoB":"2000-01-01T00:00:00","Text":"junk" }]',
2)
""");
    }

    protected override Task SeedJunkInJson(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO [Entities] ([Collection], [CollectionWithCtor], [Reference], [ReferenceWithCtor], [Id])
VALUES(
N'[{"JunkReference":{"Something":"SomeValue" },"Name":"c11","JunkProperty1":50,"Number":11.5,"JunkCollection1":[],"JunkCollection2":[{"Foo":"junk value"}],"NestedCollection":[{"DoB":"2002-04-01T00:00:00","DummyProp":"Dummy value"},{"DoB":"2002-04-02T00:00:00","DummyReference":{"Foo":5}}],"NestedReference":{"DoB":"2002-03-01T00:00:00"}},{"Name":"c12","Number":12.5,"NestedCollection":[{"DoB":"2002-06-01T00:00:00"},{"DoB":"2002-06-02T00:00:00"}],"NestedDummy":59,"NestedReference":{"DoB":"2002-05-01T00:00:00"}}]',
N'[{"MyBool":true,"Name":"c11 ctor","JunkReference":{"Something":"SomeValue","JunkCollection":[{"Foo":"junk value"}]},"NestedCollection":[{"DoB":"2002-08-01T00:00:00"},{"DoB":"2002-08-02T00:00:00"}],"NestedReference":{"DoB":"2002-07-01T00:00:00"}},{"MyBool":false,"Name":"c12 ctor","NestedCollection":[{"DoB":"2002-10-01T00:00:00"},{"DoB":"2002-10-02T00:00:00"}],"JunkCollection":[{"Foo":"junk value"}],"NestedReference":{"DoB":"2002-09-01T00:00:00"}}]',
N'{"Name":"r1","JunkCollection":[{"Foo":"junk value"}],"JunkReference":{"Something":"SomeValue" },"Number":1.5,"NestedCollection":[{"DoB":"2000-02-01T00:00:00","JunkReference":{"Something":"SomeValue"}},{"DoB":"2000-02-02T00:00:00"}],"NestedReference":{"DoB":"2000-01-01T00:00:00"}}',
N'{"MyBool":true,"JunkCollection":[{"Foo":"junk value"}],"Name":"r1 ctor","JunkReference":{"Something":"SomeValue" },"NestedCollection":[{"DoB":"2001-02-01T00:00:00"},{"DoB":"2001-02-02T00:00:00"}],"NestedReference":{"JunkCollection":[{"Foo":"junk value"}],"DoB":"2001-01-01T00:00:00"}}',
1)
""");

    protected override Task SeedTrickyBuffering(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO [Entities] ([Reference], [Id])
VALUES(
N'{"Name": "r1", "Number": 7, "JunkReference":{"Something": "SomeValue" }, "JunkCollection": [{"Foo": "junk value"}], "NestedReference": {"DoB": "2000-01-01T00:00:00"}, "NestedCollection": [{"DoB": "2000-02-01T00:00:00", "JunkReference": {"Something": "SomeValue"}}, {"DoB": "2000-02-02T00:00:00"}]}',1)
""");

    protected override Task SeedShadowProperties(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Collection], [CollectionWithCtor], [Reference], [ReferenceWithCtor], [Id], [Name])
VALUES(
N'[{"Name":"e1_c1","ShadowDouble":5.5},{"ShadowDouble":20.5,"Name":"e1_c2"}]',
N'[{"Name":"e1_c1 ctor","ShadowNullableByte":6},{"ShadowNullableByte":null,"Name":"e1_c2 ctor"}]',
N'{"Name":"e1_r", "ShadowString":"Foo"}',
N'{"ShadowInt":143,"Name":"e1_r ctor"}',
1,
N'e1')
""");

    protected override async Task SeedNotICollection(DbContext ctx)
    {
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Json], [Id])
VALUES(
N'{"Collection":[{"Bar":11,"Foo":"c11"},{"Bar":12,"Foo":"c12"},{"Bar":13,"Foo":"c13"}]}',
1)
""");

        await ctx.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO [Entities] ([Json], [Id])
VALUES(
N'{"Collection":[{"Bar":21,"Foo":"c21"},{"Bar":22,"Foo":"c22"}]}',
2)
""");
    }

    protected override async Task SeedBadJsonProperties(ContextBadJsonProperties ctx)
    {
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Id], [Scenario], [OptionalReference], [RequiredReference], [Collection])
VALUES(
1,
N'baseline',
N'{"NestedOptional": { "Text":"or no" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
N'{"NestedOptional": { "Text":"rr no" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
N'[
{"NestedOptional": { "Text":"c 1 no" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{"NestedOptional": { "Text":"c 2 no" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Id], [Scenario], [OptionalReference], [RequiredReference], [Collection])
VALUES(
2,
N'duplicated navigations',
N'{"NestedOptional": { "Text":"or no" }, "NestedOptional": { "Text":"or no dupnav" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ], "NestedCollection": [ { "Text":"or nc 1 dupnav" }, { "Text":"or nc 2 dupnav" } ], "NestedRequired": { "Text":"or nr dupnav" } }',
N'{"NestedOptional": { "Text":"rr no" }, "NestedOptional": { "Text":"rr no dupnav" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ], "NestedCollection": [ { "Text":"rr nc 1 dupnav" }, { "Text":"rr nc 2 dupnav" } ], "NestedRequired": { "Text":"rr nr dupnav" } }',
N'[
{"NestedOptional": { "Text":"c 1 no" }, "NestedOptional": { "Text":"c 1 no dupnav" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ], "NestedCollection": [ { "Text":"c 1 nc 1 dupnav" }, { "Text":"c 1 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 1 nr dupnav" } },
{"NestedOptional": { "Text":"c 2 no" }, "NestedOptional": { "Text":"c 2 no dupnav" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ], "NestedCollection": [ { "Text":"c 2 nc 1 dupnav" }, { "Text":"c 2 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 2 nr dupnav" } }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Id], [Scenario], [OptionalReference], [RequiredReference], [Collection])
VALUES(
3,
N'duplicated scalars',
N'{"NestedOptional": { "Text":"or no", "Text":"or no dupprop" }, "NestedRequired": { "Text":"or nr", "Text":"or nr dupprop" }, "NestedCollection": [ { "Text":"or nc 1", "Text":"or nc 1 dupprop" }, { "Text":"or nc 2", "Text":"or nc 2 dupprop" } ] }',
N'{"NestedOptional": { "Text":"rr no", "Text":"rr no dupprop" }, "NestedRequired": { "Text":"rr nr", "Text":"rr nr dupprop" }, "NestedCollection": [ { "Text":"rr nc 1", "Text":"rr nc 1 dupprop" }, { "Text":"rr nc 2", "Text":"rr nc 2 dupprop" } ] }',
N'[
{"NestedOptional": { "Text":"c 1 no", "Text":"c 1 no dupprop" }, "NestedRequired": { "Text":"c 1 nr", "Text":"c 1 nr dupprop" }, "NestedCollection": [ { "Text":"c 1 nc 1", "Text":"c 1 nc 1 dupprop" }, { "Text":"c 1 nc 2", "Text":"c 1 nc 2 dupprop" } ] },
{"NestedOptional": { "Text":"c 2 no", "Text":"c 2 no dupprop" }, "NestedRequired": { "Text":"c 2 nr", "Text":"c 2 nr dupprop" }, "NestedCollection": [ { "Text":"c 2 nc 1", "Text":"c 2 nc 1 dupprop" }, { "Text":"c 2 nc 2", "Text":"c 2 nc 2 dupprop" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Id], [Scenario], [OptionalReference], [RequiredReference], [Collection])
VALUES(
4,
N'empty navigation property names',
N'{"": { "Text":"or no" }, "": { "Text":"or nr" }, "": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
N'{"": { "Text":"rr no" }, "": { "Text":"rr nr" }, "": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
N'[
{"": { "Text":"c 1 no" }, "": { "Text":"c 1 nr" }, "": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{"": { "Text":"c 2 no" }, "": { "Text":"c 2 nr" }, "": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Id], [Scenario], [OptionalReference], [RequiredReference], [Collection])
VALUES(
5,
N'empty scalar property names',
N'{"NestedOptional": { "":"or no" }, "NestedRequired": { "":"or nr" }, "NestedCollection": [ { "":"or nc 1" }, { "":"or nc 2" } ] }',
N'{"NestedOptional": { "":"rr no" }, "NestedRequired": { "":"rr nr" }, "NestedCollection": [ { "":"rr nc 1" }, { "":"rr nc 2" } ] }',
N'[
{"NestedOptional": { "":"c 1 no" }, "NestedRequired": { "":"c 1 nr" }, "NestedCollection": [ { "":"c 1 nc 1" }, { "":"c 1 nc 2" } ] },
{"NestedOptional": { "":"c 2 no" }, "NestedRequired": { "":"c 2 nr" }, "NestedCollection": [ { "":"c 2 nc 1" }, { "":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Id], [Scenario], [OptionalReference], [RequiredReference], [Collection])
VALUES(
10,
N'null navigation property names',
N'{null: { "Text":"or no" }, null: { "Text":"or nr" }, null: [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
N'{null: { "Text":"rr no" }, null: { "Text":"rr nr" }, null: [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
N'[
{null: { "Text":"c 1 no" }, null: { "Text":"c 1 nr" }, null: [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{null: { "Text":"c 2 no" }, null: { "Text":"c 2 nr" }, null: [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Id], [Scenario], [OptionalReference], [RequiredReference], [Collection])
VALUES(
11,
N'null scalar property names',
N'{"NestedOptional": { null:"or no", "Text":"or no nonnull" }, "NestedRequired": { null:"or nr", "Text":"or nr nonnull" }, "NestedCollection": [ { null:"or nc 1", "Text":"or nc 1 nonnull" }, { null:"or nc 2", "Text":"or nc 2 nonnull" } ] }',
N'{"NestedOptional": { null:"rr no", "Text":"rr no nonnull" }, "NestedRequired": { null:"rr nr", "Text":"rr nr nonnull" }, "NestedCollection": [ { null:"rr nc 1", "Text":"rr nc 1 nonnull" }, { null:"rr nc 2", "Text":"rr nc 2 nonnull" } ] }',
N'[
{"NestedOptional": { null:"c 1 no", "Text":"c 1 no nonnull" }, "NestedRequired": { null:"c 1 nr", "Text":"c 1 nr nonnull" }, "NestedCollection": [ { null:"c 1 nc 1", "Text":"c 1 nc 1 nonnull" }, { null:"c 1 nc 2", "Text":"c 1 nc 2 nonnull" } ] },
{"NestedOptional": { null:"c 2 no", "Text":"c 2 no nonnull" }, "NestedRequired": { null:"c 2 nr", "Text":"c 2 nr nonnull" }, "NestedCollection": [ { null:"c 2 nc 1", "Text":"c 2 nc 1 nonnull" }, { null:"c 2 nc 2", "Text":"c 2 nc 2 nonnull" } ] }
]')
""");
    }

    #region EnumLegacyValues

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Read_enum_property_with_legacy_values(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: BuildModelEnumLegacyValues,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedEnumLegacyValues);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Set<MyEntityEnumLegacyValues>().Select(
                x => new
                {
                    x.Reference.IntEnum,
                    x.Reference.ByteEnum,
                    x.Reference.LongEnum,
                    x.Reference.NullableEnum
                });

            var exception = async
                ? await (Assert.ThrowsAsync<SqlException>(() => query.ToListAsync()))
                : Assert.Throws<SqlException>(() => query.ToList());

            // Conversion failed when converting the nvarchar value '...' to data type int
            Assert.Equal(245, exception.Number);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Read_json_entity_with_enum_properties_with_legacy_values(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: BuildModelEnumLegacyValues,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedEnumLegacyValues,
            shouldLogCategory: c => c == DbLoggerCategory.Query.Name);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Set<MyEntityEnumLegacyValues>().Select(x => x.Reference).AsNoTracking();

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(ByteEnumLegacyValues.Redmond, result[0].ByteEnum);
            Assert.Equal(IntEnumLegacyValues.Foo, result[0].IntEnum);
            Assert.Equal(LongEnumLegacyValues.Three, result[0].LongEnum);
            Assert.Equal(ULongEnumLegacyValues.Three, result[0].ULongEnum);
            Assert.Equal(IntEnumLegacyValues.Bar, result[0].NullableEnum);
        }

        var testLogger = new TestLogger<SqlServerLoggingDefinitions>();
        Assert.Single(
            ListLoggerFactory.Log,
            l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(ByteEnumLegacyValues)));
        Assert.Single(
            ListLoggerFactory.Log,
            l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(IntEnumLegacyValues)));
        Assert.Single(
            ListLoggerFactory.Log,
            l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(LongEnumLegacyValues)));
        Assert.Single(
            ListLoggerFactory.Log,
            l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(ULongEnumLegacyValues)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Read_json_entity_collection_with_enum_properties_with_legacy_values(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: BuildModelEnumLegacyValues,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedEnumLegacyValues,
            shouldLogCategory: c => c == DbLoggerCategory.Query.Name);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Set<MyEntityEnumLegacyValues>().Select(x => x.Collection).AsNoTracking();

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(2, result[0].Count);
            Assert.Equal(ByteEnumLegacyValues.Bellevue, result[0][0].ByteEnum);
            Assert.Equal(IntEnumLegacyValues.Foo, result[0][0].IntEnum);
            Assert.Equal(LongEnumLegacyValues.One, result[0][0].LongEnum);
            Assert.Equal(ULongEnumLegacyValues.One, result[0][0].ULongEnum);
            Assert.Equal(IntEnumLegacyValues.Bar, result[0][0].NullableEnum);
            Assert.Equal(ByteEnumLegacyValues.Seattle, result[0][1].ByteEnum);
            Assert.Equal(IntEnumLegacyValues.Baz, result[0][1].IntEnum);
            Assert.Equal(LongEnumLegacyValues.Two, result[0][1].LongEnum);
            Assert.Equal(ULongEnumLegacyValues.Two, result[0][1].ULongEnum);
            Assert.Null(result[0][1].NullableEnum);
        }

        var testLogger = new TestLogger<SqlServerLoggingDefinitions>();
        Assert.Single(
            ListLoggerFactory.Log,
            l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(ByteEnumLegacyValues)));
        Assert.Single(
            ListLoggerFactory.Log,
            l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(IntEnumLegacyValues)));
        Assert.Single(
            ListLoggerFactory.Log,
            l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(LongEnumLegacyValues)));
        Assert.Single(
            ListLoggerFactory.Log,
            l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(ULongEnumLegacyValues)));
    }

    private Task SeedEnumLegacyValues(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO [Entities] ([Collection], [Reference], [Id], [Name])
VALUES(
N'[{"ByteEnum":"Bellevue","IntEnum":"Foo","LongEnum":"One","ULongEnum":"One","Name":"e1_c1","NullableEnum":"Bar"},{"ByteEnum":"Seattle","IntEnum":"Baz","LongEnum":"Two","ULongEnum":"Two","Name":"e1_c2","NullableEnum":null}]',
N'{"ByteEnum":"Redmond","IntEnum":"Foo","LongEnum":"Three","ULongEnum":"Three","Name":"e1_r","NullableEnum":"Bar"}',
1,
N'e1')
""");

    protected virtual void BuildModelEnumLegacyValues(ModelBuilder modelBuilder)
        => modelBuilder.Entity<MyEntityEnumLegacyValues>(
            b =>
            {
                b.ToTable("Entities");
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(x => x.Reference, b => b.ToJson().HasColumnType(JsonColumnType));
                b.OwnsMany(x => x.Collection, b => b.ToJson().HasColumnType(JsonColumnType));
            });

    private class MyEntityEnumLegacyValues
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public MyJsonEntityEnumLegacyValues Reference { get; set; }
        public List<MyJsonEntityEnumLegacyValues> Collection { get; set; }
    }

    private class MyJsonEntityEnumLegacyValues
    {
        public string Name { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public IntEnumLegacyValues IntEnum { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public ByteEnumLegacyValues ByteEnum { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public LongEnumLegacyValues LongEnum { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public ULongEnumLegacyValues ULongEnum { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public IntEnumLegacyValues? NullableEnum { get; set; }
    }

    private enum IntEnumLegacyValues
    {
        Foo = int.MinValue,
        Bar,
        Baz = int.MaxValue,
    }

    private enum ByteEnumLegacyValues : byte
    {
        Seattle,
        Redmond,
        Bellevue = 255,
    }

    private enum LongEnumLegacyValues : long
    {
        One = long.MinValue,
        Two = 1,
        Three = long.MaxValue,
    }

    private enum ULongEnumLegacyValues : ulong
    {
        One = ulong.MinValue,
        Two = 1,
        Three = ulong.MaxValue,
    }

    #endregion

    public override async Task Entity_splitting_with_owned_json()
    {
        await base.Entity_splitting_with_owned_json();

        AssertSql(
            """
SELECT TOP(2) [m].[Id], [m].[PropertyInMainTable], [o].[PropertyInOtherTable], [m].[Json]
FROM [MyEntity] AS [m]
INNER JOIN [OtherTable] AS [o] ON [m].[Id] = [o].[Id]
""");
    }
}
