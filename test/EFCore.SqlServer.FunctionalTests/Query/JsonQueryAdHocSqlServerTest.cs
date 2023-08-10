﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonQueryAdHocSqlServerTest : JsonQueryAdHocTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override void Seed29219(MyContext29219 ctx)
    {
        var entity1 = new MyEntity29219
        {
            Id = 1,
            Reference = new MyJsonEntity29219 { NonNullableScalar = 10, NullableScalar = 11 },
            Collection = new List<MyJsonEntity29219>
            {
                new MyJsonEntity29219 { NonNullableScalar = 100, NullableScalar = 101 },
                new MyJsonEntity29219 { NonNullableScalar = 200, NullableScalar = 201 },
                new MyJsonEntity29219 { NonNullableScalar = 300, NullableScalar = null },
            }
        };

        var entity2 = new MyEntity29219
        {
            Id = 2,
            Reference = new MyJsonEntity29219 { NonNullableScalar = 20, NullableScalar = null },
            Collection = new List<MyJsonEntity29219>
            {
                new MyJsonEntity29219 { NonNullableScalar = 1001, NullableScalar = null },
            }
        };

        ctx.Entities.AddRange(entity1, entity2);
        ctx.SaveChanges();

        ctx.Database.ExecuteSqlRaw(@"INSERT INTO [Entities] ([Id], [Reference], [Collection])
VALUES(3, N'{{ ""NonNullableScalar"" : 30 }}', N'[{{ ""NonNullableScalar"" : 10001 }}]')");
    }

    protected override void Seed30028(MyContext30028 ctx)
    {
        // complete
        ctx.Database.ExecuteSqlRaw(@"INSERT INTO [Entities] ([Id], [Json])
VALUES(
1,
N'{{""RootName"":""e1"",""Collection"":[{{""BranchName"":""e1 c1"",""Nested"":{{""LeafName"":""e1 c1 l""}}}},{{""BranchName"":""e1 c2"",""Nested"":{{""LeafName"":""e1 c2 l""}}}}],""OptionalReference"":{{""BranchName"":""e1 or"",""Nested"":{{""LeafName"":""e1 or l""}}}},""RequiredReference"":{{""BranchName"":""e1 rr"",""Nested"":{{""LeafName"":""e1 rr l""}}}}}}')");

        // missing collection
        ctx.Database.ExecuteSqlRaw(@"INSERT INTO [Entities] ([Id], [Json])
VALUES(
2,
N'{{""RootName"":""e2"",""OptionalReference"":{{""BranchName"":""e2 or"",""Nested"":{{""LeafName"":""e2 or l""}}}},""RequiredReference"":{{""BranchName"":""e2 rr"",""Nested"":{{""LeafName"":""e2 rr l""}}}}}}')");

        // missing optional reference
        ctx.Database.ExecuteSqlRaw(@"INSERT INTO [Entities] ([Id], [Json])
VALUES(
3,
N'{{""RootName"":""e3"",""Collection"":[{{""BranchName"":""e3 c1"",""Nested"":{{""LeafName"":""e3 c1 l""}}}},{{""BranchName"":""e3 c2"",""Nested"":{{""LeafName"":""e3 c2 l""}}}}],""RequiredReference"":{{""BranchName"":""e3 rr"",""Nested"":{{""LeafName"":""e3 rr l""}}}}}}')");

        // missing required reference
        ctx.Database.ExecuteSqlRaw(@"INSERT INTO [Entities] ([Id], [Json])
VALUES(
4,
N'{{""RootName"":""e4"",""Collection"":[{{""BranchName"":""e4 c1"",""Nested"":{{""LeafName"":""e4 c1 l""}}}},{{""BranchName"":""e4 c2"",""Nested"":{{""LeafName"":""e4 c2 l""}}}}],""OptionalReference"":{{""BranchName"":""e4 or"",""Nested"":{{""LeafName"":""e4 or l""}}}}}}')");
    }

    protected override void SeedArrayOfPrimitives(MyContextArrayOfPrimitives ctx)
    {
        var entity1 = new MyEntityArrayOfPrimitives
        {
            Id = 1,
            Reference = new MyJsonEntityArrayOfPrimitives
            {
                IntArray = new int[] { 1, 2, 3 },
                ListOfString = new List<string> { "Foo", "Bar", "Baz" }
            },
            Collection = new List<MyJsonEntityArrayOfPrimitives>
            {
                new MyJsonEntityArrayOfPrimitives
                {
                    IntArray = new int[] { 111, 112, 113 },
                    ListOfString = new List<string> { "Foo11", "Bar11" }
                },
                new MyJsonEntityArrayOfPrimitives
                {
                    IntArray = new int[] { 211, 212, 213 },
                    ListOfString = new List<string> { "Foo12", "Bar12" }
                },
            }
        };

        var entity2 = new MyEntityArrayOfPrimitives
        {
            Id = 2,
            Reference = new MyJsonEntityArrayOfPrimitives
            {
                IntArray = new int[] { 10, 20, 30 },
                ListOfString = new List<string> { "A", "B", "C" }
            },
            Collection = new List<MyJsonEntityArrayOfPrimitives>
            {
                new MyJsonEntityArrayOfPrimitives
                {
                    IntArray = new int[] { 110, 120, 130 },
                    ListOfString = new List<string> { "A1", "Z1" }
                },
                new MyJsonEntityArrayOfPrimitives
                {
                    IntArray = new int[] { 210, 220, 230 },
                    ListOfString = new List<string> { "A2", "Z2" }
                },
            }
        };

        ctx.Entities.AddRange(entity1, entity2);
        ctx.SaveChanges();
    }

    protected override void SeedJunkInJson(MyContextJunkInJson ctx)
    {
        ctx.Database.ExecuteSqlRaw(@"INSERT INTO [Entities] ([Collection], [CollectionWithCtor], [Reference], [ReferenceWithCtor], [Id])
VALUES(
N'[{{""JunkReference"":{{""Something"":""SomeValue"" }},""Name"":""c11"",""JunkProperty1"":50,""Number"":11.5,""JunkCollection1"":[],""JunkCollection2"":[{{""Foo"":""junk value""}}],""NestedCollection"":[{{""DoB"":""2002-04-01T00:00:00"",""DummyProp"":""Dummy value""}},{{""DoB"":""2002-04-02T00:00:00"",""DummyReference"":{{""Foo"":5}}}}],""NestedReference"":{{""DoB"":""2002-03-01T00:00:00""}}}},{{""Name"":""c12"",""Number"":12.5,""NestedCollection"":[{{""DoB"":""2002-06-01T00:00:00""}},{{""DoB"":""2002-06-02T00:00:00""}}],""NestedDummy"":59,""NestedReference"":{{""DoB"":""2002-05-01T00:00:00""}}}}]',
N'[{{""MyBool"":true,""Name"":""c11 ctor"",""JunkReference"":{{""Something"":""SomeValue"",""JunkCollection"":[{{""Foo"":""junk value""}}]}},""NestedCollection"":[{{""DoB"":""2002-08-01T00:00:00""}},{{""DoB"":""2002-08-02T00:00:00""}}],""NestedReference"":{{""DoB"":""2002-07-01T00:00:00""}}}},{{""MyBool"":false,""Name"":""c12 ctor"",""NestedCollection"":[{{""DoB"":""2002-10-01T00:00:00""}},{{""DoB"":""2002-10-02T00:00:00""}}],""JunkCollection"":[{{""Foo"":""junk value""}}],""NestedReference"":{{""DoB"":""2002-09-01T00:00:00""}}}}]',
N'{{""Name"":""r1"",""JunkCollection"":[{{""Foo"":""junk value""}}],""JunkReference"":{{""Something"":""SomeValue"" }},""Number"":1.5,""NestedCollection"":[{{""DoB"":""2000-02-01T00:00:00"",""JunkReference"":{{""Something"":""SomeValue""}}}},{{""DoB"":""2000-02-02T00:00:00""}}],""NestedReference"":{{""DoB"":""2000-01-01T00:00:00""}}}}',
N'{{""MyBool"":true,""JunkCollection"":[{{""Foo"":""junk value""}}],""Name"":""r1 ctor"",""JunkReference"":{{""Something"":""SomeValue"" }},""NestedCollection"":[{{""DoB"":""2001-02-01T00:00:00""}},{{""DoB"":""2001-02-02T00:00:00""}}],""NestedReference"":{{""JunkCollection"":[{{""Foo"":""junk value""}}],""DoB"":""2001-01-01T00:00:00""}}}}',
1)");
    }

    protected override void SeedShadowProperties(MyContextShadowProperties ctx)
    {
        ctx.Database.ExecuteSqlRaw(@"INSERT INTO [Entities] ([Collection], [CollectionWithCtor], [Reference], [ReferenceWithCtor], [Id], [Name])
VALUES(
N'[{{""Name"":""e1_c1"",""ShadowDouble"":5.5}},{{""ShadowDouble"":20.5,""Name"":""e1_c2""}}]',
N'[{{""Name"":""e1_c1 ctor"",""ShadowNullableByte"":6}},{{""ShadowNullableByte"":null,""Name"":""e1_c2 ctor""}}]',
N'{{""Name"":""e1_r"", ""ShadowString"":""Foo""}}',
N'{{""ShadowInt"":143,""Name"":""e1_r ctor""}}',
1,
N'e1')");
    }

    #region EnumLegacyValues

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Read_enum_property_with_legacy_values(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextEnumLegacyValues>(
            seed: SeedEnumLegacyValues);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Select(x => new
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
        var contextFactory = await InitializeAsync<MyContextEnumLegacyValues>(
            seed: SeedEnumLegacyValues,
            shouldLogCategory: c => c == DbLoggerCategory.Query.Name);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Select(x => x.Reference).AsNoTracking();

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
            ListLoggerFactory.Log.Where(
                l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(ByteEnumLegacyValues))));
        Assert.Single(
            ListLoggerFactory.Log.Where(
                l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(IntEnumLegacyValues))));
        Assert.Single(
            ListLoggerFactory.Log.Where(
                l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(LongEnumLegacyValues))));
        Assert.Single(
            ListLoggerFactory.Log.Where(
                l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(ULongEnumLegacyValues))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Read_json_entity_collection_with_enum_properties_with_legacy_values(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextEnumLegacyValues>(
            seed: SeedEnumLegacyValues,
            shouldLogCategory: c => c == DbLoggerCategory.Query.Name);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Select(x => x.Collection).AsNoTracking();

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
            ListLoggerFactory.Log.Where(
                l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(ByteEnumLegacyValues))));
        Assert.Single(
            ListLoggerFactory.Log.Where(
                l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(IntEnumLegacyValues))));
        Assert.Single(
            ListLoggerFactory.Log.Where(
                l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(LongEnumLegacyValues))));
        Assert.Single(
            ListLoggerFactory.Log.Where(
                l => l.Message == CoreResources.LogStringEnumValueInJson(testLogger).GenerateMessage(nameof(ULongEnumLegacyValues))));
    }

    private void SeedEnumLegacyValues(MyContextEnumLegacyValues ctx)
    {
        ctx.Database.ExecuteSqlRaw(@"INSERT INTO [Entities] ([Collection], [Reference], [Id], [Name])
VALUES(
N'[{{""ByteEnum"":""Bellevue"",""IntEnum"":""Foo"",""LongEnum"":""One"",""ULongEnum"":""One"",""Name"":""e1_c1"",""NullableEnum"":""Bar""}},{{""ByteEnum"":""Seattle"",""IntEnum"":""Baz"",""LongEnum"":""Two"",""ULongEnum"":""Two"",""Name"":""e1_c2"",""NullableEnum"":null}}]',
N'{{""ByteEnum"":""Redmond"",""IntEnum"":""Foo"",""LongEnum"":""Three"",""ULongEnum"":""Three"",""Name"":""e1_r"",""NullableEnum"":""Bar""}}',
1,
N'e1')");
    }

    private class MyContextEnumLegacyValues : DbContext
    {
        public MyContextEnumLegacyValues(DbContextOptions options)
            : base((new DbContextOptionsBuilder(options)).ConfigureWarnings(b => b.Log(CoreEventId.StringEnumValueInJson)).Options)
        {
        }

        public DbSet<MyEntityEnumLegacyValues> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntityEnumLegacyValues>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<MyEntityEnumLegacyValues>().OwnsOne(x => x.Reference, b => b.ToJson());
            modelBuilder.Entity<MyEntityEnumLegacyValues>().OwnsMany(x => x.Collection, b => b.ToJson());
        }
    }

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

        public IntEnumLegacyValues IntEnum { get; set; }
        public ByteEnumLegacyValues ByteEnum { get; set; }
        public LongEnumLegacyValues LongEnum { get; set; }
        public ULongEnumLegacyValues ULongEnum { get; set; }
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
}
