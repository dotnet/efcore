// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocJsonQuerySqliteTest : AdHocJsonQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override async Task Seed29219(DbContext ctx)
    {
        var entity1 = new MyEntity29219
        {
            Id = 1,
            Reference = new MyJsonEntity29219 { NonNullableScalar = 10, NullableScalar = 11 },
            Collection =
            [
                new MyJsonEntity29219 { NonNullableScalar = 100, NullableScalar = 101 },
                new MyJsonEntity29219 { NonNullableScalar = 200, NullableScalar = 201 },
                new MyJsonEntity29219 { NonNullableScalar = 300, NullableScalar = null }
            ]
        };

        var entity2 = new MyEntity29219
        {
            Id = 2,
            Reference = new MyJsonEntity29219 { NonNullableScalar = 20, NullableScalar = null },
            Collection = [new MyJsonEntity29219 { NonNullableScalar = 1001, NullableScalar = null }]
        };

        ctx.Set<MyEntity29219>().AddRange(entity1, entity2);
        await ctx.SaveChangesAsync();

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Id", "Reference", "Collection")
VALUES(3, '{ "NonNullableScalar" : 30 }', '[{ "NonNullableScalar" : 10001 }]')
""");
    }

    protected override async Task Seed30028(DbContext ctx)
    {
        // complete
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
1,
'{"RootName":"e1","Collection":[{"BranchName":"e1 c1","Nested":{"LeafName":"e1 c1 l"}},{"BranchName":"e1 c2","Nested":{"LeafName":"e1 c2 l"}}],"OptionalReference":{"BranchName":"e1 or","Nested":{"LeafName":"e1 or l"}},"RequiredReference":{"BranchName":"e1 rr","Nested":{"LeafName":"e1 rr l"}}}')
""");

        // missing collection
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
2,
'{"RootName":"e2","OptionalReference":{"BranchName":"e2 or","Nested":{"LeafName":"e2 or l"}},"RequiredReference":{"BranchName":"e2 rr","Nested":{"LeafName":"e2 rr l"}}}')
""");

        // missing optional reference
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
3,
'{"RootName":"e3","Collection":[{"BranchName":"e3 c1","Nested":{"LeafName":"e3 c1 l"}},{"BranchName":"e3 c2","Nested":{"LeafName":"e3 c2 l"}}],"RequiredReference":{"BranchName":"e3 rr","Nested":{"LeafName":"e3 rr l"}}}')
""");

        // missing required reference
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
4,
'{"RootName":"e4","Collection":[{"BranchName":"e4 c1","Nested":{"LeafName":"e4 c1 l"}},{"BranchName":"e4 c2","Nested":{"LeafName":"e4 c2 l"}}],"OptionalReference":{"BranchName":"e4 or","Nested":{"LeafName":"e4 or l"}}}')
""");
    }

    protected override async Task Seed33046(DbContext ctx)
        => await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Reviews" ("Rounds", "Id")
VALUES('[{"RoundNumber":11,"SubRounds":[{"SubRoundNumber":111},{"SubRoundNumber":112}]}]', 1)
""");

    protected override async Task Seed34960(Context34960 ctx)
    {
        await base.Seed34960(ctx);

        // JSON nulls
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Collection", "Reference", "Id")
VALUES(
'null',
'null',
4)
""");

        // JSON object where collection should be
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Junk" ("Collection", "Reference", "Id")
VALUES(
'{ "DoB":"2000-01-01T00:00:00","Text":"junk" }',
NULL,
1)
""");

        // JSON array where entity should be
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Junk" ("Collection", "Reference", "Id")
VALUES(
NULL,
'[{ "DoB":"2000-01-01T00:00:00","Text":"junk" }]',
2)
""");
    }

    protected override Task SeedArrayOfPrimitives(DbContext ctx)
    {
        var entity1 = new MyEntityArrayOfPrimitives
        {
            Id = 1,
            Reference = new MyJsonEntityArrayOfPrimitives
            {
                IntArray = [1, 2, 3],
                ListOfString =
                [
                    "Foo",
                    "Bar",
                    "Baz"
                ]
            },
            Collection =
            [
                new MyJsonEntityArrayOfPrimitives { IntArray = [111, 112, 113], ListOfString = ["Foo11", "Bar11"] },
                new MyJsonEntityArrayOfPrimitives { IntArray = [211, 212, 213], ListOfString = ["Foo12", "Bar12"] }
            ]
        };

        var entity2 = new MyEntityArrayOfPrimitives
        {
            Id = 2,
            Reference = new MyJsonEntityArrayOfPrimitives
            {
                IntArray = [10, 20, 30],
                ListOfString =
                [
                    "A",
                    "B",
                    "C"
                ]
            },
            Collection =
            [
                new MyJsonEntityArrayOfPrimitives { IntArray = [110, 120, 130], ListOfString = ["A1", "Z1"] },
                new MyJsonEntityArrayOfPrimitives { IntArray = [210, 220, 230], ListOfString = ["A2", "Z2"] }
            ]
        };

        ctx.Set<MyEntityArrayOfPrimitives>().AddRange(entity1, entity2);
        return ctx.SaveChangesAsync();
    }

    protected override Task SeedJunkInJson(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Collection", "CollectionWithCtor", "Reference", "ReferenceWithCtor", "Id")
VALUES(
'[{"JunkReference":{"Something":"SomeValue" },"Name":"c11","JunkProperty1":50,"Number":11.5,"JunkCollection1":[],"JunkCollection2":[{"Foo":"junk value"}],"NestedCollection":[{"DoB":"2002-04-01T00:00:00","DummyProp":"Dummy value"},{"DoB":"2002-04-02T00:00:00","DummyReference":{"Foo":5}}],"NestedReference":{"DoB":"2002-03-01T00:00:00"}},{"Name":"c12","Number":12.5,"NestedCollection":[{"DoB":"2002-06-01T00:00:00"},{"DoB":"2002-06-02T00:00:00"}],"NestedDummy":59,"NestedReference":{"DoB":"2002-05-01T00:00:00"}}]',
'[{"MyBool":true,"Name":"c11 ctor","JunkReference":{"Something":"SomeValue","JunkCollection":[{"Foo":"junk value"}]},"NestedCollection":[{"DoB":"2002-08-01T00:00:00"},{"DoB":"2002-08-02T00:00:00"}],"NestedReference":{"DoB":"2002-07-01T00:00:00"}},{"MyBool":false,"Name":"c12 ctor","NestedCollection":[{"DoB":"2002-10-01T00:00:00"},{"DoB":"2002-10-02T00:00:00"}],"JunkCollection":[{"Foo":"junk value"}],"NestedReference":{"DoB":"2002-09-01T00:00:00"}}]',
'{"Name":"r1","JunkCollection":[{"Foo":"junk value"}],"JunkReference":{"Something":"SomeValue" },"Number":1.5,"NestedCollection":[{"DoB":"2000-02-01T00:00:00","JunkReference":{"Something":"SomeValue"}},{"DoB":"2000-02-02T00:00:00"}],"NestedReference":{"DoB":"2000-01-01T00:00:00"}}',
'{"MyBool":true,"JunkCollection":[{"Foo":"junk value"}],"Name":"r1 ctor","JunkReference":{"Something":"SomeValue" },"NestedCollection":[{"DoB":"2001-02-01T00:00:00"},{"DoB":"2001-02-02T00:00:00"}],"NestedReference":{"JunkCollection":[{"Foo":"junk value"}],"DoB":"2001-01-01T00:00:00"}}',
1)
""");

    protected override Task SeedTrickyBuffering(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Reference", "Id")
VALUES(
'{"Name": "r1", "Number": 7, "JunkReference":{"Something": "SomeValue" }, "JunkCollection": [{"Foo": "junk value"}], "NestedReference": {"DoB": "2000-01-01T00:00:00"}, "NestedCollection": [{"DoB": "2000-02-01T00:00:00", "JunkReference": {"Something": "SomeValue"}}, {"DoB": "2000-02-02T00:00:00"}]}',1)
""");

    protected override Task SeedShadowProperties(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Collection", "CollectionWithCtor", "Reference", "ReferenceWithCtor", "Id", "Name")
VALUES(
'[{"Name":"e1_c1","ShadowDouble":5.5},{"ShadowDouble":20.5,"Name":"e1_c2"}]',
'[{"Name":"e1_c1 ctor","ShadowNullableByte":6},{"ShadowNullableByte":null,"Name":"e1_c2 ctor"}]',
'{"Name":"e1_r", "ShadowString":"Foo"}',
'{"ShadowInt":143,"Name":"e1_r ctor"}',
1,
'e1')
""");

    protected override async Task SeedNotICollection(DbContext ctx)
    {
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Json", "Id")
VALUES(
'{"Collection":[{"Bar":11,"Foo":"c11"},{"Bar":12,"Foo":"c12"},{"Bar":13,"Foo":"c13"}]}',
1)
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Json", "Id")
VALUES(
'{"Collection":[{"Bar":21,"Foo":"c21"},{"Bar":22,"Foo":"c22"}]}',
2)
""");
    }

    protected override async Task SeedBadJsonProperties(ContextBadJsonProperties ctx)
    {
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
1,
'baseline',
'{"NestedOptional": { "Text":"or no" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
'{"NestedOptional": { "Text":"rr no" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
'[
{"NestedOptional": { "Text":"c 1 no" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{"NestedOptional": { "Text":"c 2 no" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
2,
'duplicated navigations',
'{"NestedOptional": { "Text":"or no" }, "NestedOptional": { "Text":"or no dupnav" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ], "NestedCollection": [ { "Text":"or nc 1 dupnav" }, { "Text":"or nc 2 dupnav" } ], "NestedRequired": { "Text":"or nr dupnav" } }',
'{"NestedOptional": { "Text":"rr no" }, "NestedOptional": { "Text":"rr no dupnav" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ], "NestedCollection": [ { "Text":"rr nc 1 dupnav" }, { "Text":"rr nc 2 dupnav" } ], "NestedRequired": { "Text":"rr nr dupnav" } }',
'[
{"NestedOptional": { "Text":"c 1 no" }, "NestedOptional": { "Text":"c 1 no dupnav" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ], "NestedCollection": [ { "Text":"c 1 nc 1 dupnav" }, { "Text":"c 1 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 1 nr dupnav" } },
{"NestedOptional": { "Text":"c 2 no" }, "NestedOptional": { "Text":"c 2 no dupnav" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ], "NestedCollection": [ { "Text":"c 2 nc 1 dupnav" }, { "Text":"c 2 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 2 nr dupnav" } }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
3,
'duplicated scalars',
'{"NestedOptional": { "Text":"or no", "Text":"or no dupprop" }, "NestedRequired": { "Text":"or nr", "Text":"or nr dupprop" }, "NestedCollection": [ { "Text":"or nc 1", "Text":"or nc 1 dupprop" }, { "Text":"or nc 2", "Text":"or nc 2 dupprop" } ] }',
'{"NestedOptional": { "Text":"rr no", "Text":"rr no dupprop" }, "NestedRequired": { "Text":"rr nr", "Text":"rr nr dupprop" }, "NestedCollection": [ { "Text":"rr nc 1", "Text":"rr nc 1 dupprop" }, { "Text":"rr nc 2", "Text":"rr nc 2 dupprop" } ] }',
'[
{"NestedOptional": { "Text":"c 1 no", "Text":"c 1 no dupprop" }, "NestedRequired": { "Text":"c 1 nr", "Text":"c 1 nr dupprop" }, "NestedCollection": [ { "Text":"c 1 nc 1", "Text":"c 1 nc 1 dupprop" }, { "Text":"c 1 nc 2", "Text":"c 1 nc 2 dupprop" } ] },
{"NestedOptional": { "Text":"c 2 no", "Text":"c 2 no dupprop" }, "NestedRequired": { "Text":"c 2 nr", "Text":"c 2 nr dupprop" }, "NestedCollection": [ { "Text":"c 2 nc 1", "Text":"c 2 nc 1 dupprop" }, { "Text":"c 2 nc 2", "Text":"c 2 nc 2 dupprop" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
4,
'empty navigation property names',
'{"": { "Text":"or no" }, "": { "Text":"or nr" }, "": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
'{"": { "Text":"rr no" }, "": { "Text":"rr nr" }, "": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
'[
{"": { "Text":"c 1 no" }, "": { "Text":"c 1 nr" }, "": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{"": { "Text":"c 2 no" }, "": { "Text":"c 2 nr" }, "": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
5,
'empty scalar property names',
'{"NestedOptional": { "":"or no" }, "NestedRequired": { "":"or nr" }, "NestedCollection": [ { "":"or nc 1" }, { "":"or nc 2" } ] }',
'{"NestedOptional": { "":"rr no" }, "NestedRequired": { "":"rr nr" }, "NestedCollection": [ { "":"rr nc 1" }, { "":"rr nc 2" } ] }',
'[
{"NestedOptional": { "":"c 1 no" }, "NestedRequired": { "":"c 1 nr" }, "NestedCollection": [ { "":"c 1 nc 1" }, { "":"c 1 nc 2" } ] },
{"NestedOptional": { "":"c 2 no" }, "NestedRequired": { "":"c 2 nr" }, "NestedCollection": [ { "":"c 2 nc 1" }, { "":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
10,
'null navigation property names',
'{null: { "Text":"or no" }, null: { "Text":"or nr" }, null: [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
'{null: { "Text":"rr no" }, null: { "Text":"rr nr" }, null: [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
'[
{null: { "Text":"c 1 no" }, null: { "Text":"c 1 nr" }, null: [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{null: { "Text":"c 2 no" }, null: { "Text":"c 2 nr" }, null: [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
11,
'null scalar property names',
'{"NestedOptional": { null:"or no", "Text":"or no nonnull" }, "NestedRequired": { null:"or nr", "Text":"or nr nonnull" }, "NestedCollection": [ { null:"or nc 1", "Text":"or nc 1 nonnull" }, { null:"or nc 2", "Text":"or nc 2 nonnull" } ] }',
'{"NestedOptional": { null:"rr no", "Text":"rr no nonnull" }, "NestedRequired": { null:"rr nr", "Text":"rr nr nonnull" }, "NestedCollection": [ { null:"rr nc 1", "Text":"rr nc 1 nonnull" }, { null:"rr nc 2", "Text":"rr nc 2 nonnull" } ] }',
'[
{"NestedOptional": { null:"c 1 no", "Text":"c 1 no nonnull" }, "NestedRequired": { null:"c 1 nr", "Text":"c 1 nr nonnull" }, "NestedCollection": [ { null:"c 1 nc 1", "Text":"c 1 nc 1 nonnull" }, { null:"c 1 nc 2", "Text":"c 1 nc 2 nonnull" } ] },
{"NestedOptional": { null:"c 2 no", "Text":"c 2 no nonnull" }, "NestedRequired": { null:"c 2 nr", "Text":"c 2 nr nonnull" }, "NestedCollection": [ { null:"c 2 nc 1", "Text":"c 2 nc 1 nonnull" }, { null:"c 2 nc 2", "Text":"c 2 nc 2 nonnull" } ] }
]')
""");
    }
}
