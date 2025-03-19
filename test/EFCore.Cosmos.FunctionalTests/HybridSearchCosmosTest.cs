// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;

namespace Microsoft.EntityFrameworkCore;

#pragma warning disable EF9103
#pragma warning disable EF9104

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
                EF.Functions.FullTextScore(x.Description, new string[] { "beaver" }),
                EF.Functions.VectorDistance(x.SBytes, inputVector)))
            .ToListAsync();

        AssertSql(
"""
@inputVector='[2,-1,4,3,5,-2,5,-7,3,1]'

SELECT VALUE c
FROM root c
ORDER BY RANK RRF(FullTextScore(c["Description"], ["beaver"]), VectorDistance(c["SBytes"], @inputVector, false, {'distanceFunction':'dotproduct', 'dataType':'int8'}))
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

        public ReadOnlyMemory<float> Singles { get; set; } = null!;
        public byte[] BytesArray { get; set; } = null!;

        public float[] SinglesArray { get; set; } = null!;
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
                b.Property(x => x.Name).IsFullText();
                b.HasIndex(x => x.Name).ForFullText();

                b.Property(x => x.Description).IsFullText();
                b.HasIndex(x => x.Description).ForFullText();

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
                Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
            };


            var airAnimals = new HybridSearchAnimals
            {
                Id = 2,
                PartitionKey = "habitat",
                Name = "List of several air animals",
                Description = "duck, bat, eagle, butterfly, sparrow",

                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),
                Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
            };

            var mammals = new HybridSearchAnimals
            {
                Id = 3,
                PartitionKey = "taxonomy",
                Name = "List of several mammals",
                Description = "bison, beaver, moose, fox, wolf, marten, horse, shrew, hare, bat",

                Bytes = new ReadOnlyMemory<byte>([2, 1, 4, 3, 5, 2, 5, 7, 3, 1]),
                SBytes = new ReadOnlyMemory<sbyte>([2, -1, 4, 3, 5, -2, 5, -7, 3, 1]),
                Singles = new ReadOnlyMemory<float>([0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]),
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f],
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
#pragma warning restore EF9103
#pragma warning restore EF9104
