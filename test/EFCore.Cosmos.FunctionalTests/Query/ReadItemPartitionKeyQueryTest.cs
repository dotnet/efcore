// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class ReadItemPartitionKeyQueryTest : QueryTestBase<ReadItemPartitionKeyQueryTest.ReadItemPartitionKeyQueryFixture>
{
    public ReadItemPartitionKeyQueryTest(ReadItemPartitionKeyQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public async Task Predicate_with_hierarchical_partition_key()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3));

        AssertSql(
            """
SELECT c
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Predicate_with_single_partition_key()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1"));

        AssertSql(
            """
SELECT c
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Predicate_with_partial_values_in_hierarchical_partition_key()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1));

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["PartitionKey1"] = "PK1") AND (c["PartitionKey2"] = 1))
""");
    }

    [ConditionalFact] // #33960
    public async Task Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.Payload.Contains("3") && e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3));

        AssertSql(
            """
SELECT c
FROM root c
WHERE CONTAINS(c["Payload"], "3")
""");
    }

    [ConditionalFact]
    public async Task WithPartitionKey_with_hierarchical_partition_key()
    {
        var partitionKey2 = 1;

        await AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>().WithPartitionKey("PK1", 1, true),
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));

        AssertSql(
            """
SELECT c
FROM root c
""");
    }

    [ConditionalFact]
    public async Task WithPartitionKey_with_single_partition_key()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().WithPartitionKey("PK1"),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1"));

        AssertSql(
            """
SELECT c
FROM root c
""");
    }

    [ConditionalFact]
    public async Task WithPartitionKey_with_missing_value_in_hierarchical_partition_key()
    {
        var message = await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async: true,
                ss => ss.Set<HierarchicalPartitionKeyEntity>().WithPartitionKey("PK1", 1),
                ss => ss.Set<HierarchicalPartitionKeyEntity>()
                    .Where(e => e.PartitionKey1 == "PK1" && e.PartitionKey2 == 1 && e.PartitionKey3)));

        Assert.Equal(CosmosStrings.IncorrectPartitionKeyNumber(nameof(HierarchicalPartitionKeyEntity), 2, 3), message.Message);
    }

    [ConditionalFact]
    public async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().WithPartitionKey("PK1").Where(e => e.PartitionKey == "PK2"),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1").Where(e => e.PartitionKey == "PK2"),
            assertEmpty: true);

        AssertSql(
            """
SELECT c
FROM root c
WHERE (c["PartitionKey"] = "PK2")
""");
    }

    [ConditionalFact]
    public async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>()
                .WithPartitionKey("PK1")
                .Where(e => e.PartitionKey == "PK1"));

        AssertSql(
            """
SELECT c
FROM root c
WHERE (c["PartitionKey"] = "PK1")
""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_hierarchical_partition_key()
    {
        var partitionKey2 = 1;

        await AssertQuery(
            async: true,
            ss => ss.Set<HierarchicalPartitionKeyEntity>()
                .Where(e => e.Id == 1 && e.PartitionKey1 == "PK1" && e.PartitionKey2 == partitionKey2 && e.PartitionKey3));

        AssertSql("""ReadItem(["PK1",1.0,true], HierarchicalPartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_single_partition_key_constant()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.PartitionKey == "PK1"));

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_single_partition_key_parameter()
    {
        var partitionKey = "PK1";

        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.PartitionKey == partitionKey));

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_SingleAsync()
    {
        var partitionKey = "PK1";

        await AssertSingle(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.PartitionKey == partitionKey));

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_inverse_comparison()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => 1 == e.Id && "PK1" == e.PartitionKey));

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_EF_Property()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(
                e => EF.Property<int>(e, nameof(SinglePartitionKeyEntity.Id)) == 1
                    && EF.Property<string>(e, nameof(SinglePartitionKeyEntity.PartitionKey)) == "PK1"));

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_WithPartitionKey()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().WithPartitionKey("PK1").Where(e => e.Id == 1),
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.PartitionKey == "PK1").Where(e => e.Id == 1));

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem()
    {
        var partitionKey = "PK1";

        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1 && e.Id == 2 && e.PartitionKey == partitionKey),
            assertEmpty: true);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Id"] = 1) AND (c["Id"] = 2))
""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_no_partition_key()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<NoPartitionKeyEntity>().Where(e => e.Id == 1));

        AssertSql("ReadItem(None, NoPartitionKeyEntity|1)");
    }

    [ConditionalFact]
    public async Task ReadItem_is_not_used_without_partition_key()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 1));

        AssertSql(
            """
SELECT c
FROM root c
WHERE (c["Id"] = 1)
""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_non_existent_id()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().Where(e => e.Id == 999 && e.PartitionKey == "PK1"),
            assertEmpty: true);

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|999)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_AsNoTracking()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().AsNoTracking().Where(e => e.Id == 1 && e.PartitionKey == "PK1"));

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_AsNoTrackingWithIdentityResolution()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SinglePartitionKeyEntity>().AsNoTrackingWithIdentityResolution().Where(e => e.Id == 1 && e.PartitionKey == "PK1"));

        AssertSql("""ReadItem(["PK1"], SinglePartitionKeyEntity|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_with_shared_container()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SharedContainerEntity1>().Where(e => e.Id == 1 && e.PartitionKey == "PK1"));

        AssertSql("""ReadItem(["PK1"], SharedContainerEntity1|1)""");
    }

    [ConditionalFact]
    public async Task ReadItem_for_base_type_with_shared_container()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SharedContainerEntity2>().Where(e => e.Id == 4 && e.PartitionKey == "PK2"));

        AssertSql(
            """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("SharedContainerEntity2", "SharedContainerEntity2Child") AND (c["Id"] = 4))
""");
    }

    [ConditionalFact]
    public async Task ReadItem_for_child_type_with_shared_container()
    {
        await AssertQuery(
            async: true,
            ss => ss.Set<SharedContainerEntity2Child>().Where(e => e.Id == 5 && e.PartitionKey == "PK2"));

        AssertSql("""ReadItem(["PK2"], SharedContainerEntity2Child|5)""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class PartitionKeyContext(DbContextOptions options) : PoolableDbContext(options);

    public class ReadItemPartitionKeyQueryFixture : SharedStoreFixtureBase<PartitionKeyContext>, IQueryFixtureBase
    {
        private PartitionKeyData? _expectedData;

        protected override string StoreName
            => "PartitionKeyQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<HierarchicalPartitionKeyEntity>()
                .ToContainer(nameof(HierarchicalPartitionKeyEntity))
                .HasPartitionKey(h => new { h.PartitionKey1, h.PartitionKey2, h.PartitionKey3 });
            modelBuilder.Entity<SinglePartitionKeyEntity>()
                .ToContainer(nameof(SinglePartitionKeyEntity))
                .HasPartitionKey(h => h.PartitionKey);
            modelBuilder.Entity<NoPartitionKeyEntity>()
                .ToContainer(nameof(NoPartitionKeyEntity));
            modelBuilder.Entity<SharedContainerEntity1>()
                .ToContainer("SharedContainer")
                .HasPartitionKey(e => e.PartitionKey);
            modelBuilder.Entity<SharedContainerEntity2>()
                .ToContainer("SharedContainer")
                .HasPartitionKey(e => e.PartitionKey);
            modelBuilder.Entity<SharedContainerEntity2Child>()
                .HasPartitionKey(e => e.PartitionKey);
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.ConfigureWarnings(
                w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        protected override Task SeedAsync(PartitionKeyContext context)
        {
            context.AddRange(new PartitionKeyData().HierarchicalPartitionKeyEntities);
            context.AddRange(new PartitionKeyData().SinglePartitionKeyEntities);
            context.AddRange(new PartitionKeyData().NoPartitionKeyEntities);
            context.AddRange(new PartitionKeyData().SharedContainerEntities1);
            context.AddRange(new PartitionKeyData().SharedContainerEntities2);
            context.AddRange(new PartitionKeyData().SharedContainerEntities2Children);
            return context.SaveChangesAsync();
        }

        public ISetSource GetExpectedData()
            => _expectedData ??= new PartitionKeyData();

        public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
        {
            { typeof(HierarchicalPartitionKeyEntity), e => ((HierarchicalPartitionKeyEntity?)e)?.Id },
            { typeof(SinglePartitionKeyEntity), e => ((SinglePartitionKeyEntity?)e)?.Id },
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
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    public class HierarchicalPartitionKeyEntity
    {
        public int Id { get; set; }

        public required string PartitionKey1 { get; set; }
        public int PartitionKey2 { get; set; }
        public bool PartitionKey3 { get; set; }

        public required string Payload { get; set; }
    }

    public class SinglePartitionKeyEntity
    {
        public int Id { get; set; }

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
        public int Id { get; set; }
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

    public class PartitionKeyData : ISetSource
    {
        public IReadOnlyList<HierarchicalPartitionKeyEntity> HierarchicalPartitionKeyEntities { get; }
        public IReadOnlyList<SinglePartitionKeyEntity> SinglePartitionKeyEntities { get; }
        public IReadOnlyList<NoPartitionKeyEntity> NoPartitionKeyEntities { get; }
        public IReadOnlyList<SharedContainerEntity1> SharedContainerEntities1 { get; }
        public IReadOnlyList<SharedContainerEntity2> SharedContainerEntities2 { get; }
        public IReadOnlyList<SharedContainerEntity2Child> SharedContainerEntities2Children { get; }

        public PartitionKeyData(PartitionKeyContext? context = null)
        {
            HierarchicalPartitionKeyEntities = CreateHierarchicalPartitionKeyEntities();
            SinglePartitionKeyEntities = CreateSinglePartitionKeyEntities();
            NoPartitionKeyEntities = CreateNoPartitionKeyEntities();
            SharedContainerEntities1 = CreateSharedContainerEntities1();
            SharedContainerEntities2 = CreateSharedContainerEntities2();
            SharedContainerEntities2Children = CreateSharedContainerEntities2Children();
        }

        public IQueryable<TEntity> Set<TEntity>()
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

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        private static IReadOnlyList<HierarchicalPartitionKeyEntity> CreateHierarchicalPartitionKeyEntities()
            => new List<HierarchicalPartitionKeyEntity>
            {
                new()
                {
                    Id = 1,
                    PartitionKey1 = "PK1",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload1"
                },
                new()
                {
                    Id = 1,
                    PartitionKey1 = "PK2",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload2"
                },
                new()
                {
                    Id = 2,
                    PartitionKey1 = "PK1",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload3"
                },
                new()
                {
                    Id = 2,
                    PartitionKey1 = "PK2",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload4"
                }
            };

        private static IReadOnlyList<SinglePartitionKeyEntity> CreateSinglePartitionKeyEntities()
            => new List<SinglePartitionKeyEntity>
            {
                new()
                {
                    Id = 1,
                    PartitionKey = "PK1",
                    Payload = "Payload1"
                },
                new()
                {
                    Id = 1,
                    PartitionKey = "PK2",
                    Payload = "Payload2"
                },
                new()
                {
                    Id = 2,
                    PartitionKey = "PK1",
                    Payload = "Payload3"
                },
                new()
                {
                    Id = 2,
                    PartitionKey = "PK2",
                    Payload = "Payload4"
                }
            };

        private static IReadOnlyList<NoPartitionKeyEntity> CreateNoPartitionKeyEntities()
            => new List<NoPartitionKeyEntity>
            {
                new() { Id = 1, Payload = "Payload1" },
                new() { Id = 2, Payload = "Payload2" }
            };

        private static IReadOnlyList<SharedContainerEntity1> CreateSharedContainerEntities1()
            => new List<SharedContainerEntity1>
            {
                new()
                {
                    Id = 1,
                    PartitionKey = "PK1",
                    Payload1 = "Payload1"
                },
                new()
                {
                    Id = 1,
                    PartitionKey = "PK2",
                    Payload1 = "Payload2"
                },
                new()
                {
                    Id = 2,
                    PartitionKey = "PK1",
                    Payload1 = "Payload3"
                },
                new()
                {
                    Id = 2,
                    PartitionKey = "PK2",
                    Payload1 = "Payload4"
                }
            };

        private static IReadOnlyList<SharedContainerEntity2> CreateSharedContainerEntities2()
            => new List<SharedContainerEntity2>
            {
                new()
                {
                    Id = 4,
                    PartitionKey = "PK1",
                    Payload2 = "Payload4"
                },
                new()
                {
                    Id = 4,
                    PartitionKey = "PK2",
                    Payload2 = "Payload5"
                }
            };

        private static IReadOnlyList<SharedContainerEntity2Child> CreateSharedContainerEntities2Children()
            => new List<SharedContainerEntity2Child>
            {
                new()
                {
                    Id = 5,
                    PartitionKey = "PK1",
                    Payload2 = "Payload6",
                    ChildPayload = "Child1"
                },
                new()
                {
                    Id = 5,
                    PartitionKey = "PK2",
                    Payload2 = "Payload7",
                    ChildPayload = "Child2"
                }
            };
    }
}
