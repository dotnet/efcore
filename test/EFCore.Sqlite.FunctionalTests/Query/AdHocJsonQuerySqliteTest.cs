﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocJsonQuerySqliteTest : AdHocJsonQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override void Seed29219(MyContext29219 ctx)
    {
        var entity1 = new MyEntity29219
        {
            Id = 1,
            Reference = new MyJsonEntity29219 { NonNullableScalar = 10, NullableScalar = 11 },
            Collection = new List<MyJsonEntity29219>
            {
                new() { NonNullableScalar = 100, NullableScalar = 101 },
                new() { NonNullableScalar = 200, NullableScalar = 201 },
                new() { NonNullableScalar = 300, NullableScalar = null },
            }
        };

        var entity2 = new MyEntity29219
        {
            Id = 2,
            Reference = new MyJsonEntity29219 { NonNullableScalar = 20, NullableScalar = null },
            Collection = new List<MyJsonEntity29219> { new() { NonNullableScalar = 1001, NullableScalar = null }, }
        };

        ctx.Entities.AddRange(entity1, entity2);
        ctx.SaveChanges();

        ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Id"", ""Reference"", ""Collection"")
VALUES(3, '{{ ""NonNullableScalar"" : 30 }}', '[{{ ""NonNullableScalar"" : 10001 }}]')");
    }

    protected override void Seed30028(MyContext30028 ctx)
    {
        // complete
        ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Id"", ""Json"")
VALUES(
1,
'{{""RootName"":""e1"",""Collection"":[{{""BranchName"":""e1 c1"",""Nested"":{{""LeafName"":""e1 c1 l""}}}},{{""BranchName"":""e1 c2"",""Nested"":{{""LeafName"":""e1 c2 l""}}}}],""OptionalReference"":{{""BranchName"":""e1 or"",""Nested"":{{""LeafName"":""e1 or l""}}}},""RequiredReference"":{{""BranchName"":""e1 rr"",""Nested"":{{""LeafName"":""e1 rr l""}}}}}}')");

        // missing collection
        ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Id"", ""Json"")
VALUES(
2,
'{{""RootName"":""e2"",""OptionalReference"":{{""BranchName"":""e2 or"",""Nested"":{{""LeafName"":""e2 or l""}}}},""RequiredReference"":{{""BranchName"":""e2 rr"",""Nested"":{{""LeafName"":""e2 rr l""}}}}}}')");

        // missing optional reference
        ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Id"", ""Json"")
VALUES(
3,
'{{""RootName"":""e3"",""Collection"":[{{""BranchName"":""e3 c1"",""Nested"":{{""LeafName"":""e3 c1 l""}}}},{{""BranchName"":""e3 c2"",""Nested"":{{""LeafName"":""e3 c2 l""}}}}],""RequiredReference"":{{""BranchName"":""e3 rr"",""Nested"":{{""LeafName"":""e3 rr l""}}}}}}')");

        // missing required reference
        ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Id"", ""Json"")
VALUES(
4,
'{{""RootName"":""e4"",""Collection"":[{{""BranchName"":""e4 c1"",""Nested"":{{""LeafName"":""e4 c1 l""}}}},{{""BranchName"":""e4 c2"",""Nested"":{{""LeafName"":""e4 c2 l""}}}}],""OptionalReference"":{{""BranchName"":""e4 or"",""Nested"":{{""LeafName"":""e4 or l""}}}}}}')");
    }

    protected override void SeedArrayOfPrimitives(MyContextArrayOfPrimitives ctx)
    {
        var entity1 = new MyEntityArrayOfPrimitives
        {
            Id = 1,
            Reference = new MyJsonEntityArrayOfPrimitives
            {
                IntArray = new[] { 1, 2, 3 },
                ListOfString = new List<string>
                {
                    "Foo",
                    "Bar",
                    "Baz"
                }
            },
            Collection = new List<MyJsonEntityArrayOfPrimitives>
            {
                new() { IntArray = new[] { 111, 112, 113 }, ListOfString = new List<string> { "Foo11", "Bar11" } },
                new() { IntArray = new[] { 211, 212, 213 }, ListOfString = new List<string> { "Foo12", "Bar12" } },
            }
        };

        var entity2 = new MyEntityArrayOfPrimitives
        {
            Id = 2,
            Reference = new MyJsonEntityArrayOfPrimitives
            {
                IntArray = new[] { 10, 20, 30 },
                ListOfString = new List<string>
                {
                    "A",
                    "B",
                    "C"
                }
            },
            Collection = new List<MyJsonEntityArrayOfPrimitives>
            {
                new() { IntArray = new[] { 110, 120, 130 }, ListOfString = new List<string> { "A1", "Z1" } },
                new() { IntArray = new[] { 210, 220, 230 }, ListOfString = new List<string> { "A2", "Z2" } },
            }
        };

        ctx.Entities.AddRange(entity1, entity2);
        ctx.SaveChanges();
    }

    protected override void SeedJunkInJson(MyContextJunkInJson ctx)
        => ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Collection"", ""CollectionWithCtor"", ""Reference"", ""ReferenceWithCtor"", ""Id"")
VALUES(
'[{{""JunkReference"":{{""Something"":""SomeValue"" }},""Name"":""c11"",""JunkProperty1"":50,""Number"":11.5,""JunkCollection1"":[],""JunkCollection2"":[{{""Foo"":""junk value""}}],""NestedCollection"":[{{""DoB"":""2002-04-01T00:00:00"",""DummyProp"":""Dummy value""}},{{""DoB"":""2002-04-02T00:00:00"",""DummyReference"":{{""Foo"":5}}}}],""NestedReference"":{{""DoB"":""2002-03-01T00:00:00""}}}},{{""Name"":""c12"",""Number"":12.5,""NestedCollection"":[{{""DoB"":""2002-06-01T00:00:00""}},{{""DoB"":""2002-06-02T00:00:00""}}],""NestedDummy"":59,""NestedReference"":{{""DoB"":""2002-05-01T00:00:00""}}}}]',
'[{{""MyBool"":true,""Name"":""c11 ctor"",""JunkReference"":{{""Something"":""SomeValue"",""JunkCollection"":[{{""Foo"":""junk value""}}]}},""NestedCollection"":[{{""DoB"":""2002-08-01T00:00:00""}},{{""DoB"":""2002-08-02T00:00:00""}}],""NestedReference"":{{""DoB"":""2002-07-01T00:00:00""}}}},{{""MyBool"":false,""Name"":""c12 ctor"",""NestedCollection"":[{{""DoB"":""2002-10-01T00:00:00""}},{{""DoB"":""2002-10-02T00:00:00""}}],""JunkCollection"":[{{""Foo"":""junk value""}}],""NestedReference"":{{""DoB"":""2002-09-01T00:00:00""}}}}]',
'{{""Name"":""r1"",""JunkCollection"":[{{""Foo"":""junk value""}}],""JunkReference"":{{""Something"":""SomeValue"" }},""Number"":1.5,""NestedCollection"":[{{""DoB"":""2000-02-01T00:00:00"",""JunkReference"":{{""Something"":""SomeValue""}}}},{{""DoB"":""2000-02-02T00:00:00""}}],""NestedReference"":{{""DoB"":""2000-01-01T00:00:00""}}}}',
'{{""MyBool"":true,""JunkCollection"":[{{""Foo"":""junk value""}}],""Name"":""r1 ctor"",""JunkReference"":{{""Something"":""SomeValue"" }},""NestedCollection"":[{{""DoB"":""2001-02-01T00:00:00""}},{{""DoB"":""2001-02-02T00:00:00""}}],""NestedReference"":{{""JunkCollection"":[{{""Foo"":""junk value""}}],""DoB"":""2001-01-01T00:00:00""}}}}',
1)");

    protected override void SeedTrickyBuffering(MyContextTrickyBuffering ctx)
        => ctx.Database.ExecuteSqlRaw(
"""
INSERT INTO "Entities" ("Reference", "Id")
VALUES(
'{{"Name": "r1", "Number": 7, "JunkReference":{{"Something": "SomeValue" }}, "JunkCollection": [{{"Foo": "junk value"}}], "NestedReference": {{"DoB": "2000-01-01T00:00:00"}}, "NestedCollection": [{{"DoB": "2000-02-01T00:00:00", "JunkReference": {{"Something": "SomeValue"}}}}, {{"DoB": "2000-02-02T00:00:00"}}]}}',1)
""");

    protected override void SeedShadowProperties(MyContextShadowProperties ctx)
        => ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Collection"", ""CollectionWithCtor"", ""Reference"", ""ReferenceWithCtor"", ""Id"", ""Name"")
VALUES(
'[{{""Name"":""e1_c1"",""ShadowDouble"":5.5}},{{""ShadowDouble"":20.5,""Name"":""e1_c2""}}]',
'[{{""Name"":""e1_c1 ctor"",""ShadowNullableByte"":6}},{{""ShadowNullableByte"":null,""Name"":""e1_c2 ctor""}}]',
'{{""Name"":""e1_r"", ""ShadowString"":""Foo""}}',
'{{""ShadowInt"":143,""Name"":""e1_r ctor""}}',
1,
'e1')");

    protected override void SeedNotICollection(MyContextNotICollection ctx)
    {
        ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Json"", ""Id"")
VALUES(
'{{""Collection"":[{{""Bar"":11,""Foo"":""c11""}},{{""Bar"":12,""Foo"":""c12""}},{{""Bar"":13,""Foo"":""c13""}}]}}',
1)");

        ctx.Database.ExecuteSqlRaw(
            @"INSERT INTO ""Entities"" (""Json"", ""Id"")
VALUES(
'{{""Collection"":[{{""Bar"":21,""Foo"":""c21""}},{{""Bar"":22,""Foo"":""c22""}}]}}',
2)");
    }
}
