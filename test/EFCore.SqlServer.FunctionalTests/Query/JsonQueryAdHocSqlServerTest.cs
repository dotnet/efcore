// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonQueryAdHocSqlServerTest : JsonQueryAdHocTestBase
{
    public JsonQueryAdHocSqlServerTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

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
}
