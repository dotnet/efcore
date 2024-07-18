// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ReadItemPartitionKeyQueryTest : ReadItemPartitionKeyQueryTestBase<ReadItemPartitionKeyQueryTest.ReadItemPartitionKeyQueryFixture>
{
    public ReadItemPartitionKeyQueryTest(ReadItemPartitionKeyQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Predicate_with_hierarchical_partition_key()
    {
        await base.Predicate_with_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "HierarchicalPartitionKeyEntity")
""");
    }

    public override async Task Predicate_with_only_hierarchical_partition_key()
    {
        await base.Predicate_with_only_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1a",1.0,true], PK1a|1|True)""");
    }

    public override async Task Predicate_with_single_partition_key()
    {
        await base.Predicate_with_single_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Predicate_with_only_single_partition_key()
    {
        await base.Predicate_with_only_single_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task Predicate_with_partial_values_in_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_in_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "HierarchicalPartitionKeyEntity") AND ((c["PartitionKey1"] = "PK1") AND (c["PartitionKey2"] = 1)))
""");
    }

    [ConditionalFact]
    public override async Task Predicate_with_partial_values_in_only_hierarchical_partition_key()
    {
        await base.Predicate_with_partial_values_in_only_hierarchical_partition_key();

        // Not ReadItem because part of primary key value missing
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "OnlyHierarchicalPartitionKeyEntity") AND ((c["PartitionKey1"] = "PK1a") AND (c["PartitionKey2"] = 1)))
""");
    }

    public override async Task Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate()
    {
        await base.Predicate_with_hierarchical_partition_key_and_additional_things_in_predicate();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "HierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
""");
    }

    public override async Task Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate()
    {
        await base.Predicate_with_only_hierarchical_partition_key_and_additional_things_in_predicate();

        // Not ReadItem because additional filter
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "OnlyHierarchicalPartitionKeyEntity") AND CONTAINS(c["Payload"], "3"))
""");
    }

    public override async Task WithPartitionKey_with_hierarchical_partition_key()
    {
        await base.WithPartitionKey_with_hierarchical_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "HierarchicalPartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_only_hierarchical_partition_key()
    {
        await base.WithPartitionKey_with_only_hierarchical_partition_key();

        // This could be ReadItem because all primary key values have been supplied, but it is a weird corner case.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "OnlyHierarchicalPartitionKeyEntity")
""");
    }

    public override async Task WithPartitionKey_with_single_partition_key()
    {
        await base.WithPartitionKey_with_single_partition_key();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task WithPartitionKey_with_only_single_partition_key()
    {
        await base.WithPartitionKey_with_only_single_partition_key();

        // This could be ReadItem because the primary key value has been supplied, but it is a weird corner case.
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task WithPartitionKey_with_missing_value_in_hierarchical_partition_key()
    {
        await base.WithPartitionKey_with_missing_value_in_hierarchical_partition_key();

        AssertSql();
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_different_values();

        // Not ReadItem because no primary key value, among other things.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["PartitionKey"] = "PK2")
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_different_values_with_only_partition_key()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_different_values_with_only_partition_key();

        // Not ReadItem because conflicting primary key values, among other things.
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["PartitionKey"] = "PK2a")
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values();

        // Not ReadItem because no primary key value
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["PartitionKey"] = "PK1")
""");
    }

    public override async Task Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key()
    {
        await base.Both_WithPartitionKey_and_predicate_comparisons_with_same_values_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_hierarchical_partition_key()
    {
        await base.ReadItem_with_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1",1.0,true], 1)""");
    }

    public override async Task ReadItem_with_only_hierarchical_partition_key()
    {
        await base.ReadItem_with_only_hierarchical_partition_key();

        AssertSql("""ReadItem(["PK1a",1.0,true], PK1a|1|True)""");
    }

    public override async Task ReadItem_with_single_partition_key_constant()
    {
        await base.ReadItem_with_single_partition_key_constant();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_constant()
    {
        await base.ReadItem_with_only_single_partition_key_constant();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_single_partition_key_parameter()
    {
        await base.ReadItem_with_single_partition_key_parameter();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_only_single_partition_key_parameter()
    {
        await base.ReadItem_with_only_single_partition_key_parameter();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_SingleAsync()
    {
        await base.ReadItem_with_SingleAsync();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_SingleAsync_with_only_partition_key()
    {
        await base.ReadItem_with_SingleAsync_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_inverse_comparison()
    {
        await base.ReadItem_with_inverse_comparison();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_inverse_comparison_with_only_partition_key()
    {
        await base.ReadItem_with_inverse_comparison_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task ReadItem_with_EF_Property()
    {
        await base.ReadItem_with_EF_Property();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_WithPartitionKey()
    {
        await base.ReadItem_with_WithPartitionKey();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_WithPartitionKey_with_only_partition_key()
    {
        await base.ReadItem_with_WithPartitionKey_with_only_partition_key();

        AssertSql("""ReadItem(["PK1a"], PK1a)""");
    }

    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Id"] = 1) AND (c["Id"] = 2))
""");
    }


    public override async Task Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key()
    {
        await base.Multiple_incompatible_predicate_comparisons_cause_no_ReadItem_with_only_partition_key();

        // Not ReadItem because conflicting primary key values
        AssertSql(
            """
@__partitionKey_0='PK1a'

SELECT VALUE c
FROM root c
WHERE ((c["id"] = "PK1a") AND (c["id"] = @__partitionKey_0))
""");
    }

    public override async Task ReadItem_with_no_partition_key()
    {
        await base.ReadItem_with_no_partition_key();

        AssertSql("""ReadItem(None, 1)""");
    }

    public override async Task ReadItem_is_not_used_without_partition_key()
    {
        await base.ReadItem_is_not_used_without_partition_key();

        // Not ReadItem because no partition key
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Id"] = 1)
""");
    }

    public override async Task ReadItem_with_non_existent_id()
    {
        await base.ReadItem_with_non_existent_id();

        AssertSql("""ReadItem(["PK1"], 999)""");
    }

    public override async Task ReadItem_with_AsNoTracking()
    {
        await base.ReadItem_with_AsNoTracking();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_AsNoTrackingWithIdentityResolution()
    {
        await base.ReadItem_with_AsNoTrackingWithIdentityResolution();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_shared_container()
    {
        await base.ReadItem_with_shared_container();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_for_base_type_with_shared_container()
    {
        await base.ReadItem_for_base_type_with_shared_container();

        AssertSql("""ReadItem(["PK2"], 4)""");
    }

    public override async Task ReadItem_for_child_type_with_shared_container()
    {
        await base.ReadItem_for_child_type_with_shared_container();

        AssertSql("""ReadItem(["PK2"], 5)""");
    }

    public override async Task ReadItem_with_single_explicit_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_discriminator_mapping();

        AssertSql("""ReadItem(["PK1"], 1)""");
    }

    public override async Task ReadItem_with_single_explicit_incorrect_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_incorrect_discriminator_mapping();

        // No ReadItem because discriminator value is incorrect
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Id"] = 1) AND (c["Discriminator"] = "DerivedSinglePartitionKeyEntity"))
""");
    }

    public override async Task ReadItem_with_single_explicit_parameterized_discriminator_mapping()
    {
        await base.ReadItem_with_single_explicit_parameterized_discriminator_mapping();

        // No ReadItem because discriminator check is parameterized
        AssertSql(
            """
@__discriminator_0='SinglePartitionKeyEntity'

SELECT VALUE c
FROM root c
WHERE ((c["Id"] = 1) AND (c["Discriminator"] = @__discriminator_0))
OFFSET 0 LIMIT 2
""");
    }

    public class ReadItemPartitionKeyQueryFixture : ReadItemPartitionKeyQueryFixtureBase
    {
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
                .IncludeRootDiscriminatorInJsonId()
                .HasPartitionKey(e => e.PartitionKey);

            modelBuilder.Entity<SharedContainerEntity2>()
                .ToContainer("SharedContainer")
                .IncludeRootDiscriminatorInJsonId()
                .HasPartitionKey(e => e.PartitionKey);

            modelBuilder.Entity<SharedContainerEntity2Child>()
                .IncludeRootDiscriminatorInJsonId();
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
