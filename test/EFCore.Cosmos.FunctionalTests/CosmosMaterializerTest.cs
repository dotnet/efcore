// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class CosmosMaterializerTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName  { get; } = nameof(CosmosMaterializerTest);

    protected override ITestStoreFactory NonSharedTestStoreFactory { get; } = CosmosTestStoreFactory.Instance;

    #region Shadow key materialization

    [ConditionalFact]
    public async Task Materialize_entity_with_shadow_key()
    {
        var factory = await InitializeNonSharedTest<ShadowKeyContext>();

        using (var context = factory.CreateDbContext())
        {
            var entry = context.Entry(new ShadowKeyEntity { Id = 1 });
            entry.Property<string>("Name").CurrentValue = "Name";
            entry.Property<int>("Id2").CurrentValue = 1;
            entry.State = EntityState.Added;
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var entity = await context.Entities.SingleAsync();
            var entry = context.Entry(entity);
            Assert.Equal("Name", entry.Property<string>("Name").CurrentValue);
            Assert.Equal(1, entry.Property<int>("Id2").CurrentValue);
        }
    }

    public class ShadowKeyEntity
    {
        public int Id { get; set; }

    }

    public class ShadowKeyContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ShadowKeyEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ShadowKeyEntity>(e =>
            {
                e.Property<string>("Name");
                e.Property<int>("Id2");
                e.HasKey("Id", "Id2");
                e.HasPartitionKey(x => x.Id);
            });
        }
    }

    #endregion

    #region Discriminator

    [ConditionalFact]
    public async Task Materialize_entity_with_discriminator()
    {
        var factory = await InitializeNonSharedTest<DiscriminatorContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new DiscriminatorDerivedEntity());
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var entity = await context.Entities.SingleAsync();
            Assert.IsType<DiscriminatorDerivedEntity>(entity);
        }
    }

    [ConditionalFact]
    public async Task Materialize_owned_entity_with_discriminator()
    {
        var factory = await InitializeNonSharedTest<DiscriminatorContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new DiscriminatorParentDerivedEntity());
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var entity = await context.ParentEntities.SingleAsync();
            var derivedEntity = Assert.IsType<DiscriminatorParentDerivedEntity>(entity);
            Assert.NotNull(derivedEntity.Child);
            Assert.IsType<DiscriminatorChildDerivedEntity>(derivedEntity.Child);
        }
    }

    public abstract class DiscriminatorBaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class DiscriminatorDerivedEntity : DiscriminatorBaseEntity
    {
        public string Name { get; set; } = "Name";
    }

    public class DiscriminatorParentBaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class DiscriminatorParentDerivedEntity : DiscriminatorParentBaseEntity
    {
        public DiscriminatorChildBaseEntity Child { get; set; } = new DiscriminatorChildDerivedEntity();
    }

    public class DiscriminatorChildBaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class DiscriminatorChildDerivedEntity : DiscriminatorChildBaseEntity
    {
        public string Name { get; set; } = "Name";
    }

    public class DiscriminatorContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<DiscriminatorBaseEntity> Entities => Set<DiscriminatorBaseEntity>();

        public DbSet<DiscriminatorParentBaseEntity> ParentEntities => Set<DiscriminatorParentBaseEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiscriminatorBaseEntity>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<DiscriminatorDerivedEntity>();

            modelBuilder.Entity<DiscriminatorParentBaseEntity>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<DiscriminatorParentDerivedEntity>().OwnsOne(x => x.Child);
        }
    }

    #endregion

    #region Collection navigation

    [ConditionalFact]
    public async Task Materialize_entity_with_collection()
    {
        var factory = await InitializeNonSharedTest<CollectionContext>();

        var entity = new CollectionEntity();
        using (var context = factory.CreateDbContext())
        {
            context.Add(entity);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var dbEntity = await context.Entities.SingleAsync();
            Assert.Equal(2, dbEntity.Entities.Count);
            foreach (var item in dbEntity.Entities)
            {
                Assert.Equal("Name", item.Name);
            }
        }
    }

    [ConditionalFact]
    public async Task Materialize_entity_with_empty_collection()
    {
        var factory = await InitializeNonSharedTest<CollectionContext>();

        var entity = new CollectionEntity()
        {
            Entities = []
        };
        using (var context = factory.CreateDbContext())
        {
            context.Add(entity);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var dbEntity = await context.Entities.SingleAsync();
            Assert.Equal(0, dbEntity.Entities.Count);
        }
    }

    public class CollectionEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<CollectionItemEntity> Entities { get; set; } = [new(), new()];
    }

    public class CollectionItemEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Name";
    }

    public class CollectionContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<CollectionEntity> Entities => Set<CollectionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionEntity>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<CollectionEntity>().OwnsMany(x => x.Entities);
        }
    }

    #endregion
}
