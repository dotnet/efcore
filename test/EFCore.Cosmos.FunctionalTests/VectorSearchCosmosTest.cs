// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

#pragma warning disable EF9103

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
    public virtual async Task Query_for_vector_distance_sbytes()
    {
        await using var context = CreateContext();
        var inputVector = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]);

        var booksFromStore = await context
            .Set<Book>()
            .Select(e => EF.Functions.VectorDistance(e.SBytes, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__inputVector_1='[2,-1,4,3,5,-2,5,-7,3,1]'

SELECT VALUE VectorDistance(c["SBytes"], @__inputVector_1, false, {'distanceFunction':'dotproduct', 'dataType':'int8'})
FROM root c
""");
    }

    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_bytes()
    {
        await using var context = CreateContext();
        var inputVector = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]);

        var booksFromStore = await context
            .Set<Book>()
            .Select(e => EF.Functions.VectorDistance(e.Bytes, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__inputVector_1='[2,1,4,3,5,2,5,7,3,1]'

SELECT VALUE VectorDistance(c["Bytes"], @__inputVector_1, false, {'distanceFunction':'cosine', 'dataType':'uint8'})
FROM root c
""");
    }

    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_singles()
    {
        await using var context = CreateContext();
        var inputVector = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]);

        var booksFromStore = await context
            .Set<Book>()
            .Select(
                e => EF.Functions.VectorDistance(e.Singles, inputVector, false, DistanceFunction.DotProduct))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__inputVector_1='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78,0.86,-0.78]'

SELECT VALUE VectorDistance(c["Singles"], @__inputVector_1, false, {'distanceFunction':'dotproduct', 'dataType':'float32'})
FROM root c
""");
    }

    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_bytes_array()
    {
        await using var context = CreateContext();
        var inputVector = new byte[] { 2, 1, 4, 3, 5, 2, 5, 7, 3, 1 };

        var booksFromStore = await context
            .Set<Book>()
            .Select(e => EF.Functions.VectorDistance(e.BytesArray, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__p_1='[2,1,4,3,5,2,5,7,3,1]'

SELECT VALUE VectorDistance(c["BytesArray"], @__p_1, false, {'distanceFunction':'cosine', 'dataType':'uint8'})
FROM root c
""");
    }

    [ConditionalFact]
    public virtual async Task Query_for_vector_distance_singles_array()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .Select(
                e => EF.Functions.VectorDistance(e.SinglesArray, inputVector, false, DistanceFunction.DotProduct))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@__p_1='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78,0.86,-0.78]'

SELECT VALUE VectorDistance(c["SinglesArray"], @__p_1, false, {'distanceFunction':'dotproduct', 'dataType':'float32'})
FROM root c
""");
    }

    [ConditionalFact]
    public virtual async Task Vector_distance_sbytes_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new sbyte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.SBytes, inputVector, false, DistanceFunction.DotProduct))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);

        AssertSql(
            """
@__p_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["SBytes"], @__p_1, false, {'distanceFunction':'dotproduct', 'dataType':'int8'})
""");
    }

    [ConditionalFact]
    public virtual async Task Vector_distance_bytes_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new byte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.Bytes, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@__p_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["Bytes"], @__p_1, false, {'distanceFunction':'cosine', 'dataType':'uint8'})
""");
    }

    [ConditionalFact]
    public virtual async Task Vector_distance_singles_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.Singles, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@__p_1='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["Singles"], @__p_1, false, {'distanceFunction':'cosine', 'dataType':'float32'})
""");
    }

    [ConditionalFact]
    public virtual async Task Vector_distance_bytes_array_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new byte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.BytesArray, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@__p_1='[2,1,4,6,5,2,5,7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["BytesArray"], @__p_1, false, {'distanceFunction':'cosine', 'dataType':'uint8'})
""");
    }

    [ConditionalFact]
    public virtual async Task Vector_distance_singles_array_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.SinglesArray, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@__p_1='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["SinglesArray"], @__p_1, false, {'distanceFunction':'cosine', 'dataType':'float32'})
""");
    }

    [ConditionalFact]
    public virtual async Task VectorDistance_throws_when_used_on_non_vector()
    {
        await using var context = CreateContext();
        var inputVector = Array.Empty<byte>();

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
        var inputVector = new ReadOnlyMemory<float>(
        [
            0.33f,
            -0.52f,
            0.45f,
            -0.67f,
            0.89f,
            -0.34f,
            0.86f,
            -0.78f
        ]);

        Assert.Equal(
            CoreStrings.ArgumentNotConstant("useBruteForce", nameof(CosmosDbFunctionsExtensions.VectorDistance)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context
                    .Set<Book>()
                    .OrderBy(e => EF.Functions.VectorDistance(e.Singles, inputVector, e.IsPublished))
                    .ToListAsync())).Message);

        Assert.Equal(
            CoreStrings.ArgumentNotConstant("distanceFunction", nameof(CosmosDbFunctionsExtensions.VectorDistance)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context
                    .Set<Book>()
                    .OrderBy(
                        e => EF.Functions.VectorDistance(e.Singles, inputVector, false, e.DistanceFunction))
                    .ToListAsync())).Message);
    }

    private class Book
    {
        public Guid Id { get; set; }

        public string Publisher { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Author { get; set; } = null!;

        public ReadOnlyMemory<byte> Isbn { get; set; } = null!;

        public bool IsPublished { get; set; }

        public DistanceFunction DistanceFunction { get; set; } // Not meaningful; used for exception testing.

        public ReadOnlyMemory<byte> Bytes { get; set; } = null!;

        public ReadOnlyMemory<sbyte> SBytes { get; set; } = null!;

        public ReadOnlyMemory<float> Singles { get; set; } = null!;

        public byte[] BytesArray { get; set; } = null!;

        public float[] SinglesArray { get; set; } = null!;

        public Owned1 OwnedReference { get; set; } = null!;
        public List<Owned1> OwnedCollection { get; set; } = null!;
    }

    [Owned]
    protected class Owned1
    {
        public int Prop { get; set; }
        public Owned2 NestedOwned { get; set; } = null!;
        public List<Owned2> NestedOwnedCollection { get; set; } = null!;
    }

    [Owned]
    protected class Owned2
    {
        public string Prop { get; set; } = null!;
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class VectorSearchFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "VectorSearchTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => modelBuilder.Entity<Book>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                    b.HasKey(e => e.Id);
                    b.HasPartitionKey(e => e.Publisher);

                    b.HasIndex(e => e.Bytes).ForVectors(VectorIndexType.Flat);
                    b.HasIndex(e => e.SBytes).ForVectors(VectorIndexType.Flat);
                    b.HasIndex(e => e.Singles).ForVectors(VectorIndexType.Flat);
                    b.HasIndex(e => e.BytesArray).ForVectors(VectorIndexType.Flat);
                    b.HasIndex(e => e.SinglesArray).ForVectors(VectorIndexType.Flat);

                    b.Property(e => e.Bytes).IsVector(DistanceFunction.Cosine, 10);
                    b.Property(e => e.SBytes).IsVector(DistanceFunction.DotProduct, 10);
                    b.Property(e => e.Singles).IsVector(DistanceFunction.Cosine, 10);
                    b.Property(e => e.BytesArray).IsVector(DistanceFunction.Cosine, 10);
                    b.Property(e => e.SinglesArray).IsVector(DistanceFunction.Cosine, 10);
                });

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var book1 = new Book
            {
                Publisher = "Manning",
                Author = "Jon P Smith",
                Title = "Entity Framework Core in Action",
                Isbn = new ReadOnlyMemory<byte>("978-1617298363"u8.ToArray()),
                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),
                Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
                OwnedReference = new Owned1
                {
                    Prop = 7,
                    NestedOwned = new Owned2 { Prop = "7" },
                    NestedOwnedCollection = new List<Owned2> { new() { Prop = "71" }, new() { Prop = "72" } }
                },
                OwnedCollection = new List<Owned1> { new() { Prop = 71 }, new() { Prop = 72 } }
            };

            var book2 = new Book
            {
                Publisher = "O'Reilly",
                Author = "Julie Lerman",
                Title = "Programming Entity Framework: DbContext",
                Isbn = new ReadOnlyMemory<byte>("978-1449312961"u8.ToArray()),
                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),
                Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
                OwnedReference = new Owned1
                {
                    Prop = 7,
                    NestedOwned = new Owned2 { Prop = "7" },
                    NestedOwnedCollection = new List<Owned2> { new() { Prop = "71" }, new() { Prop = "72" } }
                },
                OwnedCollection = new List<Owned1> { new() { Prop = 71 }, new() { Prop = 72 } }
            };

            var book3 = new Book
            {
                Publisher = "O'Reilly",
                Author = "Julie Lerman",
                Title = "Programming Entity Framework",
                Isbn = new ReadOnlyMemory<byte>("978-0596807269"u8.ToArray()),
                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),
                Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
                OwnedReference = new Owned1
                {
                    Prop = 7,
                    NestedOwned = new Owned2 { Prop = "7" },
                    NestedOwnedCollection = new List<Owned2> { new() { Prop = "71" }, new() { Prop = "72" } }
                },
                OwnedCollection = new List<Owned1> { new() { Prop = 71 }, new() { Prop = 72 } }
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
#pragma warning restore EF9103
