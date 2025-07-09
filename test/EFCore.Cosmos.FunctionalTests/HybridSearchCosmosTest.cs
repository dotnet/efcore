// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;

namespace Microsoft.EntityFrameworkCore;

[CosmosCondition(CosmosCondition.DoesNotUseTokenCredential | CosmosCondition.IsNotEmulator)]
public class HybridSearchCosmosTest : IClassFixture<HybridSearchCosmosTest.HybridSearchFixture>
{
    public HybridSearchCosmosTest(HybridSearchFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        _testOutputHelper = testOutputHelper;
        fixture.TestSqlLoggerFactory.Clear();
    }

    protected HybridSearchFixture Fixture { get; }

    private readonly ITestOutputHelper _testOutputHelper;

    [ConditionalFact]
    public virtual async Task Hybrid_search_vector_distance_and_FullTextScore_in_OrderByRank()
    {
        await using var context = CreateContext();

        var inputVector = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]);

        var result = await context.Set<HybridSearchAnimals>()
            .OrderBy(x => EF.Functions.Rrf(
                EF.Functions.FullTextScore(x.Description, new string[] { "beaver", "otter" }),
                EF.Functions.VectorDistance(x.SBytes, inputVector)))
            .ToListAsync();

        AssertSql(
"""
@inputVector='[2,-1,4,3,5,-2,5,-7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY RANK RRF(FullTextScore(c["Description"], "beaver", "otter"), VectorDistance(c["SBytes"], @inputVector, false, {'distanceFunction':'dotproduct', 'dataType':'int8'}))
""");
    }

    [ConditionalFact]
    public virtual async Task Hybrid_search_vector_distance_and_FullTextScore_with_single_constant_argument()
    {
        await using var context = CreateContext();

        var inputVector = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]);

        var result = await context.Set<HybridSearchAnimals>()
            .OrderBy(x => EF.Functions.Rrf(
                EF.Functions.FullTextScore(x.Description, "beaver"),
                EF.Functions.VectorDistance(x.SBytes, inputVector)))
            .ToListAsync();

        AssertSql(
"""
@inputVector='[2,-1,4,3,5,-2,5,-7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY RANK RRF(FullTextScore(c["Description"], "beaver"), VectorDistance(c["SBytes"], @inputVector, false, {'distanceFunction':'dotproduct', 'dataType':'int8'}))
""");
    }

    [ConditionalFact]
    public virtual async Task Hybrid_search_vector_distance_and_FullTextScore_in_OrderByRank_from_owned_type()
    {
        await using var context = CreateContext();

        var inputVector = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]);
        var result = await context.Set<HybridSearchAnimals>()
            .OrderBy(x => EF.Functions.Rrf(
                EF.Functions.FullTextScore(x.Owned.AnotherDescription, new string[] { "beaver" }),
                EF.Functions.VectorDistance(x.Owned.Singles, inputVector)))
            .ToListAsync();

        AssertSql(
"""
@inputVector='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY RANK RRF(FullTextScore(c["Owned"]["AnotherDescription"], "beaver"), VectorDistance(c["Owned"]["Singles"], @inputVector, false, {'distanceFunction':'cosine', 'dataType':'float32'}))
""");
    }

    [ConditionalFact]
    public virtual async Task Hybrid_search_vector_distance_and_FullTextScore_in_OrderByRank_with_array_args()
    {
        await using var context = CreateContext();

        var prm = new string[] { "beaver", "otter" };
        var inputVector = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]);
        var result = await context.Set<HybridSearchAnimals>()
            .OrderBy(x => EF.Functions.Rrf(
                EF.Functions.VectorDistance(x.Owned.Singles, inputVector),
                EF.Functions.FullTextScore(x.Owned.AnotherDescription, prm)))
            .ToListAsync();

        AssertSql(
"""
@inputVector='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY RANK RRF(VectorDistance(c["Owned"]["Singles"], @inputVector, false, {'distanceFunction':'cosine', 'dataType':'float32'}), FullTextScore(c["Owned"]["AnotherDescription"], "beaver", "otter"))
""");
    }

    private class HybridSearchAnimals
    {
        public int Id { get; set; }

        public string PartitionKey { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public ReadOnlyMemory<byte> Bytes { get; set; } = null!;

        public ReadOnlyMemory<sbyte> SBytes { get; set; } = null!;

        public byte[] BytesArray { get; set; } = null!;

        public float[] SinglesArray { get; set; } = null!;

        public HybridOwned Owned { get; set; } = null!;
    }

    public class HybridOwned
    {
        public string AnotherDescription { get; set; } = null!;
        public ReadOnlyMemory<float> Singles { get; set; } = null!;
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class HybridSearchFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "HybridSearchTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<HybridSearchAnimals>(b =>
            {
                b.ToContainer("HybridSearchAnimals");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.HasIndex(x => x.Name).IsFullTextIndex();

                b.Property(x => x.Description).EnableFullTextSearch();
                b.HasIndex(x => x.Description).IsFullTextIndex();

                b.HasIndex(e => e.Bytes).IsVectorIndex(VectorIndexType.Flat);
                b.HasIndex(e => e.SBytes).IsVectorIndex(VectorIndexType.Flat);
                b.HasIndex(e => e.BytesArray).IsVectorIndex(VectorIndexType.Flat);
                b.HasIndex(e => e.SinglesArray).IsVectorIndex(VectorIndexType.Flat);

                b.Property(e => e.Bytes).IsVectorProperty(DistanceFunction.Cosine, 10);
                b.Property(e => e.SBytes).IsVectorProperty(DistanceFunction.DotProduct, 10);
                b.Property(e => e.BytesArray).IsVectorProperty(DistanceFunction.Cosine, 10);
                b.Property(e => e.SinglesArray).IsVectorProperty(DistanceFunction.Cosine, 10);

                b.OwnsOne(x => x.Owned, bb =>
                {
                    bb.HasIndex(e => e.Singles).IsVectorIndex(VectorIndexType.Flat);
                    bb.Property(e => e.Singles).IsVectorProperty(DistanceFunction.Cosine, 10);

                    bb.Property(x => x.AnotherDescription).EnableFullTextSearch();
                    bb.HasIndex(x => x.AnotherDescription).IsFullTextIndex();
                });
            });
        }

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var landAnimals = new HybridSearchAnimals
            {
                Id = 1,
                PartitionKey = "habitat",
                Name = "List of several land animals",
                Description = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",

                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),

                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],

                Owned = new HybridOwned
                {
                    AnotherDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, duck, turtle, frog",
                    Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                }
            };


            var airAnimals = new HybridSearchAnimals
            {
                Id = 2,
                PartitionKey = "habitat",
                Name = "List of several air animals",
                Description = "duck, bat, eagle, butterfly, sparrow",

                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],

                Owned = new HybridOwned
                {
                    AnotherDescription = "duck, bat, eagle, butterfly, sparrow",
                    Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                }
            };

            var mammals = new HybridSearchAnimals
            {
                Id = 3,
                PartitionKey = "taxonomy",
                Name = "List of several mammals",
                Description = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",

                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),

                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],

                Owned = new HybridOwned
                {
                    AnotherDescription = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",
                    Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                }
            };

            context.Set<HybridSearchAnimals>().AddRange(landAnimals, airAnimals, mammals);
            return context.SaveChangesAsync();
        }

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}
