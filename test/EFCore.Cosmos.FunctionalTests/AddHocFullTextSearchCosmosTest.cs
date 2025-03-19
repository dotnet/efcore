// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

#pragma warning disable EF9104
public class AddHocFullTextSearchCosmosTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "AdHocFullTextSearchTests";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    #region CompositeFullTextIndex

    [ConditionalFact]
    public async Task Validate_composite_full_text_index_throws()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextCompositeFullTextIndex>())).Message;

        Assert.Equal(
            CosmosStrings.CompositeFullTextIndex(
                nameof(ContextCompositeFullTextIndex.Entity),
                "Name,Number"),
            message);
    }

    protected class ContextCompositeFullTextIndex(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
        {
            b.ToContainer("Entities");
            b.HasPartitionKey(x => x.PartitionKey);
            b.HasIndex(x => new { x.Name, x.Number }).IsFullTextIndex();
        });
    }

    #endregion

    #region FullTextIndexWithoutProperty

    [ConditionalFact]
    public async Task Validate_full_text_index_without_property()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextFullTextIndexWithoutProperty>())).Message;

        Assert.Equal(
            CosmosStrings.FullTextIndexOnNonFullTextProperty(
                nameof(ContextFullTextIndexWithoutProperty.Entity),
                nameof(ContextFullTextIndexWithoutProperty.Entity.Name),
                nameof(CosmosPropertyBuilderExtensions.IsFullTextProperty)),
            message);
    }

    protected class ContextFullTextIndexWithoutProperty(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.HasIndex(x => x.Name).IsFullTextIndex();
            });
    }

    #endregion

    #region FullTextPropertyWithoutIndex

    [ConditionalFact]
    public async Task Validate_full_text_property_without_index()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextFullTextPropertyWithoutIndex>())).Message;

        Assert.Equal(
            CosmosStrings.FullTextPropertyWithoutFullTextIndex(
                nameof(ContextFullTextPropertyWithoutIndex.Entity),
                nameof(ContextFullTextPropertyWithoutIndex.Entity.Name),
                nameof(CosmosIndexBuilderExtensions.IsFullTextIndex)),
            message);
    }

    protected class ContextFullTextPropertyWithoutIndex(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).IsFullTextProperty();
            });
    }

    #endregion

    #region FullTextPropertyOnCollectionNavigation

    [ConditionalFact]
    public async Task Validate_full_text_property_on_collection_navigation_container_creation()
    {
        var message = (await Assert.ThrowsAsync<NotSupportedException>(
            () => InitializeAsync<ContextFullTextPropertyOnCollectionNavigation>())).Message;

        Assert.Equal(
            CosmosStrings.CreatingContainerWithFullTextOnCollectionNotSupported("/Collection"),
            message);
    }

    protected class ContextFullTextPropertyOnCollectionNavigation(DbContextOptions options) : DbContext(options)
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
            public string Name { get; set; } = null!;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.OwnsMany(x => x.Collection, bb =>
                {
                    bb.Property(x => x.Name).IsFullTextProperty();
                    bb.HasIndex(x => x.Name).IsFullTextIndex();
                });
            });
    }

    #endregion

    #region FullTextOnNonStringProperty

    [ConditionalFact]
    public async Task Validate_full_text_on_non_string_property()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextFullTextOnNonStringProperty>())).Message;

        Assert.Equal(
            CosmosStrings.FullTextSearchConfiguredForUnsupportedPropertyType(
                nameof(ContextFullTextOnNonStringProperty.Entity),
                nameof(ContextFullTextOnNonStringProperty.Entity.Number),
                typeof(int).Name),
            message);
    }

    protected class ContextFullTextOnNonStringProperty(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Number).IsFullTextProperty();
                b.HasIndex(x => x.Number).IsFullTextIndex();
            });
    }

    #endregion
}
#pragma warning restore EF9104
