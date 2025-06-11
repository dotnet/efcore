// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

[CosmosCondition(CosmosCondition.DoesNotUseTokenCredential)]
public class AddHocVectorSearchCosmosTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "AdHocVectorSearchTests";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    #region CompositeVectorIndex

    [ConditionalFact]
    public async Task Validate_composite_vector_index_throws()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextCompositeVectorIndex>())).Message;

        Assert.Equal(
            CosmosStrings.CompositeVectorIndex(
                nameof(ContextCompositeVectorIndex.Entity),
                "MyVector1,MyVector2"),
            message);
    }

    protected class ContextCompositeVectorIndex(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public ReadOnlyMemory<float> MyVector1 { get; set; } = null!;
            public byte[] MyVector2 { get; set; } = null!;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.MyVector1).IsVectorProperty(DistanceFunction.Cosine, 5);
                b.Property(x => x.MyVector2).IsVectorProperty(DistanceFunction.Euclidean, 7);
                b.HasIndex(x => new { x.MyVector1, x.MyVector2 }).IsVectorIndex(VectorIndexType.Flat);
            });
    }

    #endregion

    #region VectorPropertyOnCollectionNavigation

    [ConditionalFact]
    public async Task Validate_vector_property_on_collection_navigation_container_creation()
    {
        var message = (await Assert.ThrowsAsync<NotSupportedException>(
            () => InitializeAsync<ContextVectorPropertyOnCollectionNavigation>())).Message;

        Assert.Equal(
            CosmosStrings.CreatingContainerWithFullTextOrVectorOnCollectionNotSupported("/Collection"),
            message);
    }

    protected class ContextVectorPropertyOnCollectionNavigation(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public List<Json> Collection { get; set; } = null!;
        }

        public class Json
        {
            public byte[] Vector { get; set; } = null!;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.OwnsMany(x => x.Collection, bb =>
                {
                    bb.Property(x => x.Vector).IsVectorProperty(DistanceFunction.Cosine, 5);
                    bb.HasIndex(x => x.Vector).IsVectorIndex(VectorIndexType.QuantizedFlat);
                });
            });
    }

    #endregion
}
