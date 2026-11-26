// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore;

[ConditionalClass(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.DoesNotUseTokenCredential))]
public class AdHocVectorSearchCosmosTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "AdHocVectorSearchTests";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => CosmosTestStoreFactory.Instance;

    [ConditionalFact(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
    public async Task Vector_parameters_use_the_configured_JSON_writer()
    {
        var factory = await InitializeNonSharedTest<PoolableDbContext>(
            onModelCreating: b =>
            {
                b.Entity<FormattedVectorItem>().ToContainer("Entities").HasNoDiscriminator();
                b.Entity<FormattedVectorItem>().HasPartitionKey(e => e.Partition);
                b.Entity<FormattedVectorItem>().Property(e => e.Vector)
                    .IsVectorProperty(DistanceFunction.Euclidean, 3)
                    .HasJsonValueReaderWriter<ScaledVectorReaderWriter>();
                b.Entity<FormattedVectorItem>().Property(e => e.MemoryVector)
                    .IsVectorProperty(DistanceFunction.Euclidean, 3)
                    .HasJsonValueReaderWriter<ScaledVectorReaderWriter>();
            },
            seed: context =>
            {
                context.Add(new FormattedVectorItem { Id = 1, Partition = "p", Vector = [1, 2, 3], MemoryVector = new float[] { 1, 2, 3 } });
                return context.SaveChangesAsync();
            });
        await using var context = factory.CreateDbContext();
        var vector = new float[] { 1, 2, 3 };
        var memory = new ReadOnlyMemory<float>(vector);

        Assert.Equal(
            0,
            await context.Set<FormattedVectorItem>()
                .Select(e => EF.Functions.VectorDistance(e.Vector, vector, useBruteForce: true)).SingleAsync());
        Assert.Equal(
            0,
            await context.Set<FormattedVectorItem>()
                .Select(e => EF.Functions.VectorDistance(e.MemoryVector, memory, useBruteForce: true)).SingleAsync());
        var item = await context.Set<FormattedVectorItem>().SingleAsync();
        Assert.Equal(vector, item.Vector);
        Assert.Equal(vector, item.MemoryVector.ToArray());
    }

    private sealed class FormattedVectorItem
    {
        public int Id { get; set; }
        public string Partition { get; set; } = null!;
        public float[] Vector { get; set; } = [];
        public ReadOnlyMemory<float> MemoryVector { get; set; }
    }

    public sealed class ScaledVectorReaderWriter : JsonValueReaderWriter<float[]>
    {
        private static readonly JsonCollectionOfStructsReaderWriter<float[], float> ReaderWriter = new(JsonFloatReaderWriter.Instance);

        public override float[] FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => ((float[])ReaderWriter.FromJsonTyped(ref manager, existingObject)).Select(v => v / 10).ToArray();

        public override void ToJsonTyped(Utf8JsonWriter writer, float[] value)
            => ReaderWriter.ToJsonTyped(writer, value.Select(v => v * 10).ToArray());

        public override Expression ConstructorExpression
            => Expression.New(typeof(ScaledVectorReaderWriter));
    }

    #region CompositeVectorIndex

    [Fact]
    public async Task Validate_composite_vector_index_throws()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => InitializeNonSharedTest<ContextCompositeVectorIndex>()))
            .Message;

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

    [Fact]
    public async Task Validate_vector_property_on_collection_navigation_container_creation()
    {
        var message =
            (await Assert.ThrowsAsync<NotSupportedException>(() => InitializeNonSharedTest<ContextVectorPropertyOnCollectionNavigation>()))
            .Message;

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
                b.OwnsMany(
                    x => x.Collection, bb =>
                    {
                        bb.Property(x => x.Vector).IsVectorProperty(DistanceFunction.Cosine, 5);
                        bb.HasIndex(x => x.Vector).IsVectorIndex(VectorIndexType.QuantizedFlat);
                    });
            });
    }

    #endregion
}
