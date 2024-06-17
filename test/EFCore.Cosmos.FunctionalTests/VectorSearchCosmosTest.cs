// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

public class VectorSearchCosmosTest : IClassFixture<VectorSearchCosmosTest.VectorSearchFixture>
{
    public VectorSearchCosmosTest(VectorSearchFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        _testOutputHelper = testOutputHelper;
        fixture.TestSqlLoggerFactory.Clear();
    }

    protected VectorSearchFixture Fixture { get; }

    private readonly ITestOutputHelper _testOutputHelper;

    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_sbyte_array()
    {
        await using var context = CreateContext();
        var inputVector = new sbyte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };

        var booksFromStore = await context
            .Set<Book>()
            .Select(e => EF.Functions.VectorDistance(e.SByteArray, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__inputVector_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT VectorDistance(c["SByteArray"], @__inputVector_1, false, {'distanceFunction':'cosine', 'dataType':'int8'}) AS c
FROM root c
WHERE (c["Discriminator"] = "Book")
""");
    }

    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_byte_array()
    {
        await using var context = CreateContext();
        var inputVector = new byte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await context
                .Set<Book>()
                .Select(e => EF.Functions.VectorDistance(e.ByteArray, inputVector))
                .ToListAsync())).Message;

        _testOutputHelper.WriteLine(message);

        AssertSql(
            """
@__inputVector_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT VectorDistance(c["ByteArray"], @__inputVector_1, false, {'distanceFunction':'cosine', 'dataType':'uint8'}) AS c
FROM root c
WHERE (c["Discriminator"] = "Book")
""");
    }

    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_double_array()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33, -0.52, 0.45, -0.67, 0.89, -0.34, 0.86, -0.78 };

        var booksFromStore = await context
            .Set<Book>()
            .Select(
                e => EF.Functions.VectorDistance(e.DoubleArray, inputVector, false, DistanceFunction.DotProduct, VectorDataType.Float32))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__inputVector_1='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VectorDistance(c["DoubleArray"], @__inputVector_1, false, {'distanceFunction':'dotproduct', 'dataType':'float32'}) AS c
FROM root c
WHERE (c["Discriminator"] = "Book")
""");
    }

    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_int_list()
    {
        await using var context = CreateContext();

        var inputVector = new List<int>
        {
            2,
            1,
            4,
            6,
            5,
            2,
            5,
            7,
            3,
            1
        };

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await context
                .Set<Book>()
                .Select(e => EF.Functions.VectorDistance(e.IntList, inputVector))
                .ToListAsync())).Message;

        _testOutputHelper.WriteLine(message);

        // Assert.Equal(3, booksFromStore.Count);
        // Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__inputVector_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT VectorDistance(c["IntList"], @__inputVector_1, false, {'distanceFunction':'cosine', 'dataType':'uint8'}) AS c
FROM root c
WHERE (c["Discriminator"] = "Book")
""");
    }


    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_float_enumerable()
    {
        await using var context = CreateContext();
        var inputVector = new List<float>
        {
            0.33f,
            -0.52f,
            0.45f,
            -0.67f,
            0.89f,
            -0.34f,
            0.86f,
            -0.78f
        };

        var booksFromStore = await context
            .Set<Book>()
            .Select(e => EF.Functions.VectorDistance(e.FloatEnumerable, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__inputVector_1='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VectorDistance(c["FloatEnumerable"], @__inputVector_1, false, {'distanceFunction':'cosine', 'dataType':'float32'}) AS c
FROM root c
WHERE (c["Discriminator"] = "Book")
""");
    }

    [ConditionalFact]
    public virtual async Task Vector_distance_sbyte_array_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new sbyte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.SByteArray, inputVector, false, DistanceFunction.DotProduct, VectorDataType.Uint8))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);

        AssertSql(
            """
@__inputVector_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Book")
ORDER BY VectorDistance(c["SByteArray"], @__inputVector_1, false, {'distanceFunction':'dotproduct', 'dataType':'uint8'})
""");
    }


    [ConditionalFact]
    public virtual async Task Vector_distance_byte_array_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new byte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.ByteArray, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);

        AssertSql(
            """
@__inputVector_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Book")
ORDER BY VectorDistance(c["ByteArray"], @__inputVector_1, false, {'distanceFunction':'cosine', 'dataType':'uint8'})
""");
    }

    [ConditionalFact]
    public virtual async Task Vector_distance_double_array_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33, -0.52, 0.45, -0.67, 0.89, -0.34, 0.86, -0.78 };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.DoubleArray, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@__inputVector_1='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Book")
ORDER BY VectorDistance(c["DoubleArray"], @__inputVector_1, false, {'distanceFunction':'cosine', 'dataType':'float32'})
""");
    }

    [ConditionalFact]
    public virtual async Task Vector_distance_int_list_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new List<int>
        {
            2,
            1,
            4,
            6,
            5,
            2,
            5,
            7,
            3,
            1
        };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.IntList, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);

        AssertSql(
            """
@__inputVector_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Book")
ORDER BY VectorDistance(c["IntList"], @__inputVector_1, false, {'distanceFunction':'cosine', 'dataType':'uint8'})
""");
    }


    [ConditionalFact]
    public virtual async Task Vector_distance_float_enumerable_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new List<float>
        {
            0.33f,
            -0.52f,
            0.45f,
            -0.67f,
            0.89f,
            -0.34f,
            0.86f,
            -0.78f
        };

        var booksFromStore = await context
            .Set<Book>()
            .Select(e => EF.Functions.VectorDistance(inputVector, e.FloatEnumerable, true))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__inputVector_1='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VectorDistance(@__inputVector_1, c["FloatEnumerable"], true, {'distanceFunction':'cosine', 'dataType':'float32'}) AS c
FROM root c
WHERE (c["Discriminator"] = "Book")
""");
    }

    [ConditionalFact]
    public virtual async Task VectorDistance_throws_when_used_on_non_vector()
    {
        await using var context = CreateContext();
        var inputVector = "WhatsOccuring";

        Assert.Equal(
            CosmosStrings.VectorSearchRequiresVector,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context
                    .Set<Book>()
                    .OrderBy(e => EF.Functions.VectorDistance(e.Isbn, inputVector))
                    .ToListAsync())).Message);

        Assert.Equal(
            CosmosStrings.VectorSearchRequiresVector,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context
                    .Set<Book>()
                    .OrderBy(e => EF.Functions.VectorDistance(inputVector, e.Isbn))
                    .ToListAsync())).Message);
    }

    [ConditionalFact]
    public virtual async Task VectorDistance_throws_when_used_with_non_const_args()
    {
        await using var context = CreateContext();
        var inputVector = new List<float>
        {
            0.33f,
            -0.52f,
            0.45f,
            -0.67f,
            0.89f,
            -0.34f,
            0.86f,
            -0.78f
        };

        Assert.Equal(
            CoreStrings.ArgumentNotConstant("useBruteForce", nameof(CosmosDbFunctionsExtensions.VectorDistance)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context
                    .Set<Book>()
                    .OrderBy(e => EF.Functions.VectorDistance(e.FloatEnumerable, inputVector, e.IsPublished))
                    .ToListAsync())).Message);

        Assert.Equal(
            CoreStrings.ArgumentNotConstant("distanceFunction", nameof(CosmosDbFunctionsExtensions.VectorDistance)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context
                    .Set<Book>()
                    .OrderBy(
                        e => EF.Functions.VectorDistance(e.FloatEnumerable, inputVector, false, e.DistanceFunction, VectorDataType.Float32))
                    .ToListAsync())).Message);

        Assert.Equal(
            CoreStrings.ArgumentNotConstant("dataType", nameof(CosmosDbFunctionsExtensions.VectorDistance)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context
                    .Set<Book>()
                    .OrderBy(
                        e => EF.Functions.VectorDistance(e.FloatEnumerable, inputVector, false, DistanceFunction.Cosine, e.VectorDataType))
                    .ToListAsync())).Message);
    }

    private class Book
    {
        public Guid Id { get; set; }

        public string Publisher { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Author { get; set; } = null!;

        public string Isbn { get; set; } = null!;

        public bool IsPublished { get; set; }

        public DistanceFunction DistanceFunction { get; set; } // Not meaningful; used for exception testing.

        public VectorDataType VectorDataType { get; set; } // Not meaningful; used for exception testing.

        public byte[] ByteArray { get; set; } = null!;

        public double[] DoubleArray { get; set; } = null!;

        public sbyte[] SByteArray { get; set; } = null!;

        public List<int> IntList { get; set; } = null!;

        public IEnumerable<float> FloatEnumerable { get; set; } = null!;

        // public Owned1 OwnedReference { get; set; } = null!;
        // public List<Owned1> OwnedCollection { get; set; } = null!;
    }

    // [Owned]
    // protected class Owned1
    // {
    //     public int Prop { get; set; }
    //     public Owned2 NestedOwned { get; set; } = null!;
    //     public List<Owned2> NestedOwnedCollection { get; set; } = null!;
    // }
    //
    // [Owned]
    // protected class Owned2
    // {
    //     public string Prop { get; set; } = null!;
    // }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class VectorSearchFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "VectorSearchTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Book>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                    b.HasKey(e => e.Id);
                    b.HasPartitionKey(e => e.Publisher);

                    b.HasIndex(e => e.ByteArray).ForVectors(VectorIndexType.Flat);
                    b.HasIndex(e => e.SByteArray).ForVectors(VectorIndexType.Flat);
                    b.HasIndex(e => e.FloatEnumerable).ForVectors(VectorIndexType.Flat);
                    b.HasIndex(e => e.IntList).ForVectors(VectorIndexType.Flat);
                    b.HasIndex(e => e.DoubleArray).ForVectors(VectorIndexType.Flat);

                    b.Property(e => e.ByteArray).IsVector(DistanceFunction.Cosine, 10);
                    b.Property(e => e.SByteArray).IsVector(DistanceFunction.Cosine, 10);
                    b.Property(e => e.FloatEnumerable).IsVector(DistanceFunction.Cosine, 10);
                    b.Property(e => e.IntList).IsVector(DistanceFunction.Cosine, 10, VectorDataType.Uint8);
                    b.Property(e => e.DoubleArray).IsVector(DistanceFunction.Cosine, 10, VectorDataType.Float32);
                });
        }

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var book1 = new Book
            {
                Publisher = "Manning",
                Author = "Jon P Smith",
                Title = "Entity Framework Core in Action",
                Isbn = "978-1617298363",
                ByteArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SByteArray = [2, -1, 4, 3, 5, -2, 5, -7, 3, 1],
                IntList = [2, -1, 4, 3, 5, -2, 5, -7, 3, 1],
                FloatEnumerable = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f },
                DoubleArray = [0.33, -0.52, 0.45, -0.67, 0.89, -0.34, 0.86, -0.78],

                // OwnedReference = new()
                // {
                //     Prop = 7,
                //     NestedOwned = new() { Prop = "7" },
                //     NestedOwnedCollection = new() { new() { Prop = "71" }, new() { Prop = "72" } }
                // },
                // OwnedCollection = new() { new() { Prop = 71 }, new() { Prop = 72 } }
            };

            var book2 = new Book
            {
                Publisher = "O'Reilly",
                Author = "Julie Lerman",
                Title = "Programming Entity Framework: DbContext",
                Isbn = "978-1449312961",
                ByteArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SByteArray = [2, -1, 4, 3, 5, -2, 5, -7, 3, 1],
                IntList = [2, -1, 4, 3, 5, -2, 5, -7, 3, 1],
                FloatEnumerable = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f },
                DoubleArray = [0.33, -0.52, 0.45, -0.67, 0.89, -0.34, 0.86, -0.78],

                // OwnedReference = new()
                // {
                //     Prop = 7,
                //     NestedOwned = new() { Prop = "7" },
                //     NestedOwnedCollection = new() { new() { Prop = "71" }, new() { Prop = "72" } }
                // },
                // OwnedCollection = new() { new() { Prop = 71 }, new() { Prop = 72 } }
            };

            var book3 = new Book
            {
                Publisher = "O'Reilly",
                Author = "Julie Lerman",
                Title = "Programming Entity Framework",
                Isbn = "978-0596807269",
                ByteArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SByteArray = [2, -1, 4, 3, 5, -2, 5, -7, 3, 1],
                IntList = [2, -1, 4, 3, 5, -2, 5, -7, 3, 1],
                FloatEnumerable = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f },
                DoubleArray = [0.33, -0.52, 0.45, -0.67, 0.89, -0.34, 0.86, -0.78],

                // OwnedReference = new()
                // {
                //     Prop = 7,
                //     NestedOwned = new() { Prop = "7" },
                //     NestedOwnedCollection = new() { new() { Prop = "71" }, new() { Prop = "72" } }
                // },
                // OwnedCollection = new() { new() { Prop = 71 }, new() { Prop = 72 } }
            };

            context.AddRange(book1, book2, book3);

            return context.SaveChangesAsync();
        }

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}
