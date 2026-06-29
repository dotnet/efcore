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
            context.Add(new DiscriminatorDerivedEntity
            {
                Name = "Name"
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var entity = await context.Entities.SingleAsync();
            var derivedEntity = Assert.IsType<DiscriminatorDerivedEntity>(entity);
            Assert.Equal("Name", derivedEntity.Name);
        }
    }

    public abstract class DiscriminatorBaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class DiscriminatorDerivedEntity : DiscriminatorBaseEntity
    {
        public string? Name { get; set; }
    }

    public class DiscriminatorContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<DiscriminatorBaseEntity> Entities => Set<DiscriminatorBaseEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiscriminatorBaseEntity>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<DiscriminatorDerivedEntity>();
        }
    }

    #endregion

    #region Collection navigation

    [ConditionalFact]
    public async Task Materialize_entity_with_collection()
    {
        var factory = await InitializeNonSharedTest<CollectionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new CollectionEntity()
            {
                Entities = [new(), new()]
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var entity = await context.Entities.SingleAsync();
            Assert.Equal(2, entity.Entities.Count);
            foreach (var item in entity.Entities)
            {
                Assert.Equal("Name", item.Name);
            }
        }
    }

    [ConditionalFact]
    public async Task Materialize_entity_with_empty_collection()
    {
        var factory = await InitializeNonSharedTest<CollectionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new CollectionEntity()
            {
                Entities = []
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var entity = await context.Entities.SingleAsync();
            Assert.Equal(0, entity.Entities.Count);
        }
    }

    [ConditionalFact]
    public async Task Materialize_entity_with_collection_with_ordinal_key()
    {
        var factory = await InitializeNonSharedTest<CollectionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new CollectionEntityWithOrdinalKey()
            {
                Entities = [new(), new()]
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var entity = await context.EntitiesWithOrdinalKey.SingleAsync();
            Assert.Equal(2, entity.Entities.Count);
            foreach (var item in entity.Entities)
            {
                Assert.Equal("Name", item.Name);
            }
        }
    }

    public class CollectionEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<CollectionItemEntity> Entities { get; set; } = new();
    }

    public class CollectionItemEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Name";
    }

    public class CollectionEntityWithOrdinalKey
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<CollectionItemEntityWithOrdinalKey> Entities { get; set; } = new();
    }

    public class CollectionItemEntityWithOrdinalKey
    {
        public string Name { get; set; } = "Name";
    }

    public class CollectionContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<CollectionEntity> Entities => Set<CollectionEntity>();

        public DbSet<CollectionEntityWithOrdinalKey> EntitiesWithOrdinalKey => Set<CollectionEntityWithOrdinalKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionEntity>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<CollectionEntity>().OwnsMany(x => x.Entities);

            modelBuilder.Entity<CollectionEntityWithOrdinalKey>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<CollectionEntityWithOrdinalKey>().OwnsMany(x => x.Entities);
        }
    }

    #endregion

    #region AsNoTrackingWithIdentityResolution

    [ConditionalFact]
    public async Task AssociateAsNoTrackingWithIdentityResolution()
    {
        var factory = await InitializeNonSharedTest<AsNoTrackingWithIdentityResolutionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                Associates = [new(), new()]
            });
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                Associates = [new(), new()]
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var results = await context.Entities.AsNoTrackingWithIdentityResolution().Select(x => x.RequiredAssociate).ToListAsync();
            Assert.Equal(2, results.Count);
            Assert.Same(results[0], results[1]);
        }
    }

    [ConditionalFact]
    public async Task DoubleAssociateAsNoTrackingWithIdentityResolution()
    {
        var factory = await InitializeNonSharedTest<AsNoTrackingWithIdentityResolutionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
            });
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var results = await context.Entities.AsNoTrackingWithIdentityResolution().Select(x => new { first = x.RequiredAssociate, second = x.RequiredAssociate }).ToListAsync();
            Assert.Equal(2, results.Count);
            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];
                Assert.Same(result.first, result.second);
                if (i > 0)
                {
                    var otherResult = results[i - 1];
                    Assert.NotSame(otherResult.first, result.first);
                }
            }
        }
    }

    [ConditionalFact]
    public async Task ConcatAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        var factory = await InitializeNonSharedTest<AsNoTrackingWithIdentityResolutionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                Associates = [new(), new()]
            });
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                Associates = [new(), new()]
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var results = await context.Entities.AsNoTrackingWithIdentityResolution().Select(x => x.Associates.Concat(x.Associates).ToList()).ToListAsync();
            Assert.Equal(4, results.Count);

            for (var i = 0; i < results.Count / 2; i++)
            {
                var result1 = results[i];
                var result2 = results[i + results.Count / 2];

                Assert.Same(result1, result2);
            }
        }
    }

    [ConditionalFact]
    public async Task ConcatOrdinalAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        var factory = await InitializeNonSharedTest<AsNoTrackingWithIdentityResolutionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                OrdinalAssociates = [new(), new()]
            });
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                OrdinalAssociates = [new(), new()]
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var results = await context.Entities.AsNoTrackingWithIdentityResolution().Select(x => x.OrdinalAssociates.Concat(x.OrdinalAssociates).ToList()).ToListAsync();
            Assert.Equal(4, results.Count);

            for (var i = 0; i < results.Count / 2; i++)
            {
                var result1 = results[i];
                var result2 = results[i + results.Count / 2];

                Assert.Same(result1, result2);
            }
        }
    }

    [ConditionalFact]
    public async Task DoubleAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        var factory = await InitializeNonSharedTest<AsNoTrackingWithIdentityResolutionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                Associates = [new(), new()]
            });
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                Associates = [new(), new()]
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var results = await context.Entities.AsNoTrackingWithIdentityResolution().Select(x => new { first = x.Associates, second = x.Associates }).ToListAsync();
            Assert.Equal(2, results.Count);
            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];

                for (var j = 0; j < result.first.Count; j++)
                {
                    Assert.Same(result.first[j], result.second[j]);
                }

                if (i > 0)
                {
                    var otherResult = results[i - 1];
                    for (var j = 0; j < result.first.Count; j++)
                    {
                        Assert.NotSame(otherResult.first[j], result.first[j]);
                    }
                }
            }
        }
    }

    [ConditionalFact]
    public async Task DoubleOrdinalAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        var factory = await InitializeNonSharedTest<AsNoTrackingWithIdentityResolutionContext>();

        using (var context = factory.CreateDbContext())
        {
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                OrdinalAssociates = [new(), new()]
            });
            context.Add(new AsNoTrackingWithIdentityResolutionEntity()
            {
                RequiredAssociate = new(),
                OrdinalAssociates = [new(), new()]
            });
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var results = await context.Entities.AsNoTrackingWithIdentityResolution().Select(x => new { first = x.OrdinalAssociates, second = x.OrdinalAssociates }).ToListAsync();
            Assert.Equal(2, results.Count);
            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];

                for (var j = 0; j < result.first.Count; j++)
                {
                    Assert.Same(result.first[j], result.second[j]);
                }

                if (i > 0)
                {
                    var otherResult = results[i - 1];
                    for (var j = 0; j < result.first.Count; j++)
                    {
                        Assert.NotSame(otherResult.first[j], result.first[j]);
                    }
                }
            }
        }
    }

    public class AsNoTrackingWithIdentityResolutionEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public AsNoTrackingWithIdentityResolutionAssociateEntity RequiredAssociate { get; set; }

        public List<AsNoTrackingWithIdentityResolutionAssociateEntity> Associates { get; set; } = new();

        public List<AsNoTrackingWithIdentityResolutionAssociateEntity> OrdinalAssociates { get; set; } = new();

    }

    public class AsNoTrackingWithIdentityResolutionAssociateEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Name";
    }

    public class AsNoTrackingWithIdentityResolutionContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AsNoTrackingWithIdentityResolutionEntity> Entities => Set<AsNoTrackingWithIdentityResolutionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AsNoTrackingWithIdentityResolutionEntity>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<AsNoTrackingWithIdentityResolutionEntity>().OwnsOne(x => x.RequiredAssociate);
            modelBuilder.Entity<AsNoTrackingWithIdentityResolutionEntity>().OwnsMany(x => x.Associates, x => x.HasKey(x => x.Id));
            modelBuilder.Entity<AsNoTrackingWithIdentityResolutionEntity>().OwnsMany(x => x.Associates);
        }
    }

    #endregion
}
