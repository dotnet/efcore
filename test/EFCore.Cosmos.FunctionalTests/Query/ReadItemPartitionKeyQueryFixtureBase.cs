// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ReadItemPartitionKeyQueryFixtureBase : SharedStoreFixtureBase<DbContext>, IQueryFixtureBase
{
    protected PartitionKeyData? ExpectedData { get; set; }

    protected override string StoreName
        => null!;

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<HierarchicalPartitionKeyEntity>()
            .ToContainer(nameof(HierarchicalPartitionKeyEntity))
            .HasPartitionKey(
                h => new
                {
                    h.PartitionKey1,
                    h.PartitionKey2,
                    h.PartitionKey3
                });

        modelBuilder.Entity<OnlyHierarchicalPartitionKeyEntity>()
            .ToContainer(nameof(HierarchicalPartitionKeyEntity))
            .HasPartitionKey(
                h => new
                {
                    h.PartitionKey1,
                    h.PartitionKey2,
                    h.PartitionKey3
                })
            .HasKey(
                h => new
                {
                    h.PartitionKey1,
                    h.PartitionKey2,
                    h.PartitionKey3
                });

        modelBuilder.Entity<SinglePartitionKeyEntity>()
            .ToContainer(nameof(SinglePartitionKeyEntity))
            .HasPartitionKey(h => h.PartitionKey);

        modelBuilder.Entity<OnlySinglePartitionKeyEntity>()
            .ToContainer(nameof(OnlySinglePartitionKeyEntity))
            .HasPartitionKey(h => h.PartitionKey)
            .HasKey(h => h.PartitionKey);

        modelBuilder.Entity<NoPartitionKeyEntity>()
            .ToContainer(nameof(NoPartitionKeyEntity));

        modelBuilder.Entity<SharedContainerEntity1>()
            .ToContainer("SharedContainer")
            .HasPartitionKey(e => e.PartitionKey)
            .HasKey(e => new { e.Id, e.PartitionKey });

        modelBuilder.Entity<SharedContainerEntity2>()
            .ToContainer("SharedContainer")
            .HasPartitionKey(e => e.PartitionKey)
            .HasKey(e => new { e.Id, e.PartitionKey });

        modelBuilder.Entity<SharedContainerEntity2Child>();

        modelBuilder.Entity<FancyDiscriminatorEntity>()
            .ToContainer("Cat35224")
            .HasPartitionKey(e => e.Id)
            .HasDiscriminator<string>("Discriminator");
    }

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(
            builder.ConfigureWarnings(
                w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    protected override Task SeedAsync(DbContext context)
    {
        var data = (PartitionKeyData)GetExpectedData();

        context.AddRange(data.HierarchicalPartitionKeyEntities);
        context.AddRange(data.SinglePartitionKeyEntities);
        context.AddRange(data.OnlyHierarchicalPartitionKeyEntities);
        context.AddRange(data.OnlySinglePartitionKeyEntities);
        context.AddRange(data.NoPartitionKeyEntities);
        context.AddRange(data.SharedContainerEntities1);
        context.AddRange(data.SharedContainerEntities2);
        context.AddRange(data.SharedContainerEntities2Children);
        context.AddRange(data.Cat35224Entities);

        return context.SaveChangesAsync();
    }

    public virtual ISetSource GetExpectedData()
        => ExpectedData ??= new PartitionKeyData();

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(HierarchicalPartitionKeyEntity), e => ((HierarchicalPartitionKeyEntity?)e)?.Id },
        { typeof(OnlyHierarchicalPartitionKeyEntity), e => ((OnlyHierarchicalPartitionKeyEntity?)e)?.Payload },
        { typeof(SinglePartitionKeyEntity), e => ((SinglePartitionKeyEntity?)e)?.Id },
        { typeof(FancyDiscriminatorEntity), e => ((FancyDiscriminatorEntity?)e)?.Id },
        { typeof(OnlySinglePartitionKeyEntity), e => ((OnlySinglePartitionKeyEntity?)e)?.Payload },
        { typeof(NoPartitionKeyEntity), e => ((NoPartitionKeyEntity?)e)?.Id },
        { typeof(SharedContainerEntity1), e => ((SharedContainerEntity1?)e)?.Id },
        { typeof(SharedContainerEntity2), e => ((SharedContainerEntity2?)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object?, object?>>
    {
        {
            typeof(HierarchicalPartitionKeyEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (HierarchicalPartitionKeyEntity)e!;
                    var aa = (HierarchicalPartitionKeyEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.PartitionKey1, aa.PartitionKey1);
                    Assert.Equal(ee.PartitionKey2, aa.PartitionKey2);
                    Assert.Equal(ee.PartitionKey3, aa.PartitionKey3);
                    Assert.Equal(ee.Payload, aa.Payload);
                }
            }
        },
        {
            typeof(SinglePartitionKeyEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (SinglePartitionKeyEntity)e!;
                    var aa = (SinglePartitionKeyEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.PartitionKey, aa.PartitionKey);
                    Assert.Equal(ee.Payload, aa.Payload);
                }
            }
        },
        {
            typeof(OnlyHierarchicalPartitionKeyEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (OnlyHierarchicalPartitionKeyEntity)e!;
                    var aa = (OnlyHierarchicalPartitionKeyEntity)a;

                    Assert.Equal(ee.PartitionKey1, aa.PartitionKey1);
                    Assert.Equal(ee.PartitionKey2, aa.PartitionKey2);
                    Assert.Equal(ee.PartitionKey3, aa.PartitionKey3);
                    Assert.Equal(ee.Payload, aa.Payload);
                }
            }
        },
        {
            typeof(OnlySinglePartitionKeyEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (OnlySinglePartitionKeyEntity)e!;
                    var aa = (OnlySinglePartitionKeyEntity)a;

                    Assert.Equal(ee.PartitionKey, aa.PartitionKey);
                    Assert.Equal(ee.Payload, aa.Payload);
                }
            }
        },
        {
            typeof(NoPartitionKeyEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (NoPartitionKeyEntity)e!;
                    var aa = (NoPartitionKeyEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Payload, aa.Payload);
                }
            }
        },
        {
            typeof(SharedContainerEntity1), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (SharedContainerEntity1)e!;
                    var aa = (SharedContainerEntity1)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.PartitionKey, aa.PartitionKey);
                    Assert.Equal(ee.Payload1, aa.Payload1);
                }
            }
        },
        {
            typeof(SharedContainerEntity2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (SharedContainerEntity2)e!;
                    var aa = (SharedContainerEntity2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.PartitionKey, aa.PartitionKey);
                    Assert.Equal(ee.Payload2, aa.Payload2);
                }
            }
        },
        {
            typeof(SharedContainerEntity2Child), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (SharedContainerEntity2Child)e!;
                    var aa = (SharedContainerEntity2Child)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.PartitionKey, aa.PartitionKey);
                    Assert.Equal(ee.Payload2, aa.Payload2);
                    Assert.Equal(ee.ChildPayload, aa.ChildPayload);
                }
            }
        },
        {
            typeof(FancyDiscriminatorEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (FancyDiscriminatorEntity)e!;
                    var aa = (FancyDiscriminatorEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Discriminator, aa.Discriminator);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public class PartitionKeyData : ISetSource
    {
        public List<HierarchicalPartitionKeyEntity> HierarchicalPartitionKeyEntities { get; } = CreateHierarchicalPartitionKeyEntities();
        public List<SinglePartitionKeyEntity> SinglePartitionKeyEntities { get; } = CreateSinglePartitionKeyEntities();

        public List<OnlyHierarchicalPartitionKeyEntity> OnlyHierarchicalPartitionKeyEntities { get; } =
            CreateOnlyHierarchicalPartitionKeyEntities();

        public List<OnlySinglePartitionKeyEntity> OnlySinglePartitionKeyEntities { get; } = CreateOnlySinglePartitionKeyEntities();
        public List<NoPartitionKeyEntity> NoPartitionKeyEntities { get; } = CreateNoPartitionKeyEntities();
        public List<SharedContainerEntity1> SharedContainerEntities1 { get; } = CreateSharedContainerEntities1();
        public List<SharedContainerEntity2> SharedContainerEntities2 { get; } = CreateSharedContainerEntities2();
        public List<SharedContainerEntity2Child> SharedContainerEntities2Children { get; } = CreateSharedContainerEntities2Children();

        public List<FancyDiscriminatorEntity> Cat35224Entities { get; } = CreateCat35224Entities();

        public virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(HierarchicalPartitionKeyEntity))
            {
                return (IQueryable<TEntity>)HierarchicalPartitionKeyEntities.AsQueryable();
            }

            if (typeof(TEntity) == typeof(SinglePartitionKeyEntity))
            {
                return (IQueryable<TEntity>)SinglePartitionKeyEntities.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OnlyHierarchicalPartitionKeyEntity))
            {
                return (IQueryable<TEntity>)OnlyHierarchicalPartitionKeyEntities.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OnlySinglePartitionKeyEntity))
            {
                return (IQueryable<TEntity>)OnlySinglePartitionKeyEntities.AsQueryable();
            }

            if (typeof(TEntity) == typeof(NoPartitionKeyEntity))
            {
                return (IQueryable<TEntity>)NoPartitionKeyEntities.AsQueryable();
            }

            if (typeof(TEntity) == typeof(SharedContainerEntity1))
            {
                return (IQueryable<TEntity>)SharedContainerEntities1.AsQueryable();
            }

            if (typeof(TEntity) == typeof(SharedContainerEntity2))
            {
                return (IQueryable<TEntity>)SharedContainerEntities2.AsQueryable();
            }

            if (typeof(TEntity) == typeof(SharedContainerEntity2Child))
            {
                return (IQueryable<TEntity>)SharedContainerEntities2Children.AsQueryable();
            }

            if (typeof(TEntity) == typeof(FancyDiscriminatorEntity))
            {
                return (IQueryable<TEntity>)Cat35224Entities.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        private static List<HierarchicalPartitionKeyEntity> CreateHierarchicalPartitionKeyEntities()
            => new()
            {
                new HierarchicalPartitionKeyEntity
                {
                    Id = Guid.Parse("31887258-BDF9-49B8-89B2-01B6AA741A4A"),
                    PartitionKey1 = "PK1",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload1"
                },
                new HierarchicalPartitionKeyEntity
                {
                    Id = Guid.Parse("31887258-BDF9-49B8-89B2-01B6AA741A4A"), // Same Id as previous; different partition.
                    PartitionKey1 = "PK2",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload2"
                },
                new HierarchicalPartitionKeyEntity
                {
                    Id = Guid.Parse("BBA46A5D-BDB8-40F0-BA80-BA5731147B9A"), // Different Id.
                    PartitionKey1 = "PK1",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload3"
                },
                new HierarchicalPartitionKeyEntity
                {
                    Id = Guid.Parse("BBA46A5D-BDB8-40F0-BA80-BA5731147B9A"), // Same Id as previous; different partition.
                    PartitionKey1 = "PK2",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload4"
                }
            };

        private static List<SinglePartitionKeyEntity> CreateSinglePartitionKeyEntities()
            => new()
            {
                new SinglePartitionKeyEntity
                {
                    Id = Guid.Parse("B29BCED8-E1E5-420E-82D7-1C7A51703D34"),
                    PartitionKey = "PK1",
                    Payload = "Payload1"
                },
                new SinglePartitionKeyEntity
                {
                    Id = Guid.Parse("B29BCED8-E1E5-420E-82D7-1C7A51703D34"),
                    PartitionKey = "PK2",
                    Payload = "Payload2"
                },
                new SinglePartitionKeyEntity
                {
                    Id = Guid.Parse("3307A33B-7F28-49EF-9857-48F4E3EBCAED"),
                    PartitionKey = "PK1",
                    Payload = "Payload3"
                },
                new SinglePartitionKeyEntity
                {
                    Id = Guid.Parse("3307A33B-7F28-49EF-9857-48F4E3EBCAED"),
                    PartitionKey = "PK2",
                    Payload = "Payload4"
                }
            };

        private static List<OnlyHierarchicalPartitionKeyEntity> CreateOnlyHierarchicalPartitionKeyEntities()
            => new()
            {
                new OnlyHierarchicalPartitionKeyEntity
                {
                    PartitionKey1 = "PK1a",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload1"
                },
                new OnlyHierarchicalPartitionKeyEntity
                {
                    PartitionKey1 = "PK2a",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload2"
                },
                new OnlyHierarchicalPartitionKeyEntity
                {
                    PartitionKey1 = "PK1b",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload3"
                },
                new OnlyHierarchicalPartitionKeyEntity
                {
                    PartitionKey1 = "PK2b",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload4"
                }
            };

        private static List<OnlySinglePartitionKeyEntity> CreateOnlySinglePartitionKeyEntities()
            => new()
            {
                new OnlySinglePartitionKeyEntity { PartitionKey = "PK1a", Payload = "Payload1" },
                new OnlySinglePartitionKeyEntity { PartitionKey = "PK2a", Payload = "Payload2" },
                new OnlySinglePartitionKeyEntity { PartitionKey = "PK1b", Payload = "Payload3" },
                new OnlySinglePartitionKeyEntity { PartitionKey = "PK2b", Payload = "Payload4" }
            };

        private static List<NoPartitionKeyEntity> CreateNoPartitionKeyEntities()
            => new()
            {
                new NoPartitionKeyEntity { Id = 1, Payload = "Payload1" }, new NoPartitionKeyEntity { Id = 2, Payload = "Payload2" }
            };

        private static List<SharedContainerEntity1> CreateSharedContainerEntities1()
            => new()
            {
                new SharedContainerEntity1
                {
                    Id = "1",
                    PartitionKey = "PK1",
                    Payload1 = "Payload1"
                },
                new SharedContainerEntity1
                {
                    Id = "1",
                    PartitionKey = "PK2",
                    Payload1 = "Payload2"
                },
                new SharedContainerEntity1
                {
                    Id = "2",
                    PartitionKey = "PK1",
                    Payload1 = "Payload3"
                },
                new SharedContainerEntity1
                {
                    Id = "2",
                    PartitionKey = "PK2",
                    Payload1 = "Payload4"
                }
            };

        private static List<SharedContainerEntity2> CreateSharedContainerEntities2()
            => new()
            {
                new SharedContainerEntity2
                {
                    Id = 4,
                    PartitionKey = "PK1",
                    Payload2 = "Payload4"
                },
                new SharedContainerEntity2
                {
                    Id = 4,
                    PartitionKey = "PK2",
                    Payload2 = "Payload5"
                }
            };

        private static List<SharedContainerEntity2Child> CreateSharedContainerEntities2Children()
            => new()
            {
                new SharedContainerEntity2Child
                {
                    Id = 5,
                    PartitionKey = "PK1",
                    Payload2 = "Payload6",
                    ChildPayload = "Child1"
                },
                new SharedContainerEntity2Child
                {
                    Id = 5,
                    PartitionKey = "PK2",
                    Payload2 = "Payload7",
                    ChildPayload = "Child2"
                }
            };

        private static List<FancyDiscriminatorEntity> CreateCat35224Entities()
            => new()
            {
                new FancyDiscriminatorEntity
                {
                    Id = "Cat|1",
                    Name = "Smokey"
                },
                new FancyDiscriminatorEntity
                {
                    Id = "Cat2||",
                    Name = "Clippy"
                },
                new FancyDiscriminatorEntity
                {
                    Id = "Cat|3|$|5",
                    Name = "Sid"
                },
                new FancyDiscriminatorEntity
                {
                    Id = "|Cat|",
                    Name = "Killes"
                }
            };

    }
}

public class HierarchicalPartitionKeyEntity
{
    public Guid Id { get; set; }

    public required string PartitionKey1 { get; set; }
    public int PartitionKey2 { get; set; }
    public bool PartitionKey3 { get; set; }

    public required string Payload { get; set; }
}

public class SinglePartitionKeyEntity
{
    public Guid Id { get; set; }

    public required string PartitionKey { get; set; }

    public required string Payload { get; set; }
}

public class FancyDiscriminatorEntity
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string Discriminator { get; set; } = null!;
}

// This type is configured with all partition key properties, and nothing else, in the primary key.
public class OnlyHierarchicalPartitionKeyEntity
{
    public required string PartitionKey1 { get; set; }
    public int PartitionKey2 { get; set; }
    public bool PartitionKey3 { get; set; }

    public required string Payload { get; set; }
}

// This type is configured with a single partition key property that is also the primary.
public class OnlySinglePartitionKeyEntity
{
    public required string PartitionKey { get; set; }

    public required string Payload { get; set; }
}

public class NoPartitionKeyEntity
{
    public int Id { get; set; }

    public required string Payload { get; set; }
}

public class SharedContainerEntity1
{
    public string Id { get; set; } = null!;
    public required string PartitionKey { get; set; }
    public required string Payload1 { get; set; }
}

public class SharedContainerEntity2
{
    public int Id { get; set; }
    public required string PartitionKey { get; set; }
    public required string Payload2 { get; set; }
}

public class SharedContainerEntity2Child : SharedContainerEntity2
{
    public required string ChildPayload { get; set; }
}
