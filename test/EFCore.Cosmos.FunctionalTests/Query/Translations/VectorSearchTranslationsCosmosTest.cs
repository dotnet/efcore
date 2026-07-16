// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

[ConditionalClass(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
public class VectorSearchTranslationsCosmosTest : IClassFixture<VectorSearchTranslationsCosmosTest.VectorSearchFixture>
{
    public VectorSearchTranslationsCosmosTest(VectorSearchFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        _testOutputHelper = testOutputHelper;
        fixture.TestSqlLoggerFactory.Clear();
    }

    protected VectorSearchFixture Fixture { get; }

    private readonly ITestOutputHelper _testOutputHelper;

    [Fact]
    public virtual async Task OrderBy_VectorDistance_singles_memory()
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
@p='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["SinglesArray"], @p)
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_singles_array()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .Select(e => EF.Functions.VectorDistance(
                e.SinglesArray, inputVector, useBruteForce: false,
                new VectorDistanceOptions { DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@p='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78,0.86,-0.78]'

SELECT VALUE VectorDistance(c["SinglesArray"], @p, false, { 'distanceFunction': 'dotproduct' })
FROM root c
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_bytes_memory()
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
@p='[2,1,4,6,5,2,5,7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["Bytes"], @p)
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_bytes_array()
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
@p='[2,1,4,6,5,2,5,7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["BytesArray"], @p)
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_sbyte()
    {
        await using var context = CreateContext();
        var inputVector = new sbyte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.SBytes, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);

        AssertSql(
            """
@p='[2,1,4,6,5,2,5,7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["SBytes"], @p)
""");
    }

    #region Brute force and options

    [Fact]
    public virtual async Task VectorDistance_with_brute_force_true()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.SinglesArray, inputVector, useBruteForce: true))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@p='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["SinglesArray"], @p, true)
""");
    }

    [Fact]
    public virtual async Task VectorDistance_with_brute_force_and_distance_function()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(
                e.SinglesArray, inputVector, useBruteForce: true,
                new VectorDistanceOptions { DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@p='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["SinglesArray"], @p, true, { 'distanceFunction': 'dotproduct' })
""");
    }

    [Fact]
    public virtual async Task VectorDistance_with_distance_function_and_brute_force_null()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(
                e.SinglesArray, inputVector, useBruteForce: null,
                new VectorDistanceOptions { DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@p='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["SinglesArray"], @p, false, { 'distanceFunction': 'dotproduct' })
""");
    }

    [Fact]
    public virtual async Task VectorDistance_with_data_type_and_distance_function()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(
                e.SinglesArray, inputVector, useBruteForce: false,
                new VectorDistanceOptions { DataType = "float32", DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@p='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["SinglesArray"], @p, false, { 'distanceFunction': 'dotproduct', 'dataType': 'float32' })
""");
    }

    #endregion Brute force and options

    // issue #35898: vector indexes on collection wildcard paths are not supported
    //[ConditionalFact(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotEmulator))]
    public virtual async Task Vector_index_through_complex_collection_roundtrips()
    {
        await using var context = CreateContext();
        var books = await context.Set<Book>().ToListAsync();

        Assert.Equal(3, books.Count);
        Assert.All(books, b => Assert.Single(b.ComplexNestedCollection));

        // Verify that VectorDistance can be queried over the property inside the complex collection,
        // exercising the vector index created for "ComplexNestedCollection[].NestedSingles".
        Fixture.TestSqlLoggerFactory.Clear();

        var inputVector = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]);
        var distances = await context
            .Set<Book>()
            .SelectMany(b => b.ComplexNestedCollection)
            .Select(c => EF.Functions.VectorDistance(c.NestedSingles, inputVector))
            .ToListAsync();

        Assert.Equal(3, distances.Count);
        Assert.All(distances, d => Assert.NotEqual(0.0, d));

        AssertSql(
            """
@inputVector='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78,0.86,-0.78]'

SELECT VALUE VectorDistance(c0["NestedSingles"], @inputVector)
FROM root c
JOIN c0 IN c["ComplexNestedCollection"]
""");
    }

    [Fact]
    public virtual async Task Select_VectorDistance()
    {
        await using var context = CreateContext();
        var inputVector = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]);

        var booksFromStore = await context
            .Set<Book>()
            .Select(e => EF.Functions.VectorDistance(e.OwnedReference.NestedOwned.NestedSingles, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            """
@inputVector='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78,0.86,-0.78]'

SELECT VALUE VectorDistance(c["OwnedReference"]["NestedOwned"]["NestedSingles"], @inputVector)
FROM root c
""");
    }

    [Fact]
    public virtual async Task RRF_with_two_Vector_distance_functions_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector1 = new byte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };
        var inputVector2 = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.Rrf(
                EF.Functions.VectorDistance(e.BytesArray, inputVector1),
                EF.Functions.VectorDistance(e.SinglesArray, inputVector2)))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);

        AssertSql(
            """
@p='[2,1,4,6,5,2,5,7,3,1]'
@p3='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY RANK RRF(VectorDistance(c["BytesArray"], @p), VectorDistance(c["SinglesArray"], @p3))
""");
    }

    [Fact]
    public virtual async Task VectorDistance_throws_when_used_on_non_vector()
    {
        await using var context = CreateContext();
        var inputVector = Array.Empty<byte>();

        Assert.Equal(
            CosmosStrings.VectorSearchRequiresVector,
            (await Assert.ThrowsAsync<InvalidOperationException>(async () => await context
                .Set<Book>()
                .OrderBy(e => EF.Functions.VectorDistance(e.Isbn, inputVector))
                .ToListAsync())).Message);

        Assert.Equal(
            CosmosStrings.VectorSearchRequiresVector,
            (await Assert.ThrowsAsync<InvalidOperationException>(async () => await context
                .Set<Book>()
                .OrderBy(e => EF.Functions.VectorDistance(inputVector, e.Isbn))
                .ToListAsync())).Message);
    }

    [Fact]
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
            (await Assert.ThrowsAsync<InvalidOperationException>(async () => await context
                .Set<Book>()
                .OrderBy(e => EF.Functions.VectorDistance(e.OwnedReference.NestedOwned.NestedSingles, inputVector, e.IsPublished))
                .ToListAsync())).Message);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(
                e.OwnedReference.NestedOwned.NestedSingles, inputVector, useBruteForce: false,
                new VectorDistanceOptions { DistanceFunction = e.DistanceFunction }))
            .ToListAsync());
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

        public byte[] BytesArray { get; set; } = null!;

        public float[] SinglesArray { get; set; } = null!;

        public Owned1 OwnedReference { get; set; } = null!;
        public List<Owned1> OwnedCollection { get; set; } = null!;
        public List<ComplexNested> ComplexNestedCollection { get; set; } = null!;
    }

    protected class Owned1
    {
        public int Prop { get; set; }
        public Owned2 NestedOwned { get; set; } = null!;
        public List<Owned2> NestedOwnedCollection { get; set; } = null!;
    }

    protected class Owned2
    {
        public string Prop { get; set; } = null!;
        public ReadOnlyMemory<float> NestedSingles { get; set; } = null!;
    }

    protected class ComplexNested
    {
        public ReadOnlyMemory<float> NestedSingles { get; set; }
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
            => modelBuilder.Entity<Book>(b =>
            {
                b.Property(e => e.Id).ValueGeneratedOnAdd();
                b.HasKey(e => e.Id);
                b.HasPartitionKey(e => e.Publisher);

                b.HasIndex(e => e.Bytes).IsVectorIndex(VectorIndexType.Flat);
                b.HasIndex(e => e.SBytes).IsVectorIndex(VectorIndexType.Flat);
                b.HasIndex(e => e.BytesArray).IsVectorIndex(VectorIndexType.Flat);
                b.HasIndex(e => e.SinglesArray).IsVectorIndex(VectorIndexType.Flat);

                b.Property(e => e.Bytes).IsVectorProperty(DistanceFunction.Cosine, 10);
                b.Property(e => e.SBytes).IsVectorProperty(DistanceFunction.DotProduct, 10);
                b.Property(e => e.BytesArray).IsVectorProperty(DistanceFunction.Cosine, 10);
                b.Property(e => e.SinglesArray).IsVectorProperty(DistanceFunction.Cosine, 10);

                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.OwnsOne(
                            x => x.NestedOwned, bbb =>
                            {
                                bbb.HasIndex(x => x.NestedSingles).IsVectorIndex(VectorIndexType.Flat);
                                bbb.Property(x => x.NestedSingles).IsVectorProperty(DistanceFunction.Cosine, 10);
                            });

                        bb.OwnsMany(x => x.NestedOwnedCollection, bbb => bbb.Ignore(x => x.NestedSingles));
                    });

                b.OwnsMany(
                    x => x.OwnedCollection, bb =>
                    {
                        bb.OwnsOne(x => x.NestedOwned, bbb => bbb.Ignore(x => x.NestedSingles));
                        bb.OwnsMany(x => x.NestedOwnedCollection, bbb => bbb.Ignore(x => x.NestedSingles));
                    });

                // issue #35898: vector indexes on collection wildcard paths are not supported
                //if (!CosmosTestEnvironment.IsEmulator)
                //{
                //    b.ComplexCollection(x => x.ComplexNestedCollection, cb => cb.Property(c => c.NestedSingles).IsVectorProperty(DistanceFunction.Cosine, 10));
                //    b.HasIndex(x => x.ComplexNestedCollection.Select(c => c.NestedSingles)).IsVectorIndex(VectorIndexType.Flat);
                //}
                //else
                //{
                b.Ignore(x => x.ComplexNestedCollection);
                //}
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
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
                OwnedReference = new Owned1
                {
                    Prop = 7,
                    NestedOwned = new Owned2
                    {
                        Prop = "7",
                        NestedSingles =
                            new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f])
                    },
                    NestedOwnedCollection = [new() { Prop = "71" }, new() { Prop = "72" }]
                },
                OwnedCollection = [new() { Prop = 71 }, new() { Prop = 72 }],
                ComplexNestedCollection =
                [
                    new ComplexNested
                    {
                        NestedSingles = new ReadOnlyMemory<float>(
                            [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f])
                    }
                ]
            };

            var book2 = new Book
            {
                Publisher = "O'Reilly",
                Author = "Julie Lerman",
                Title = "Programming Entity Framework: DbContext",
                Isbn = new ReadOnlyMemory<byte>("978-1449312961"u8.ToArray()),
                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
                OwnedReference = new Owned1
                {
                    Prop = 7,
                    NestedOwned = new Owned2
                    {
                        Prop = "7",
                        NestedSingles =
                            new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f])
                    },
                    NestedOwnedCollection = [new() { Prop = "71" }, new() { Prop = "72" }]
                },
                OwnedCollection = [new() { Prop = 71 }, new() { Prop = 72 }],
                ComplexNestedCollection =
                [
                    new ComplexNested
                    {
                        NestedSingles = new ReadOnlyMemory<float>(
                            [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f])
                    }
                ]
            };

            var book3 = new Book
            {
                Publisher = "O'Reilly",
                Author = "Julie Lerman",
                Title = "Programming Entity Framework",
                Isbn = new ReadOnlyMemory<byte>("978-0596807269"u8.ToArray()),
                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
                OwnedReference = new Owned1
                {
                    Prop = 7,
                    NestedOwned = new Owned2
                    {
                        Prop = "7",
                        NestedSingles =
                            new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                    },
                    NestedOwnedCollection = [new() { Prop = "71" }, new() { Prop = "72" }]
                },
                OwnedCollection = [new() { Prop = 71 }, new() { Prop = 72 }],
                ComplexNestedCollection =
                [
                    new ComplexNested
                    {
                        NestedSingles = new ReadOnlyMemory<float>(
                            [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f])
                    }
                ]
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
