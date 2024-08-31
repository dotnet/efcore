// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ReadItemPartitionKeyQueryInheritanceFixtureBase : ReadItemPartitionKeyQueryFixtureBase
{
    public ReadItemPartitionKeyQueryInheritanceFixtureBase()
    {
        var asserters = (Dictionary<Type, object>)EntityAsserters;

        asserters[typeof(DerivedHierarchicalPartitionKeyEntity)] =
            new Action<object, object>(
                (e, a) =>
                {
                    ((Action<object, object>)EntityAsserters[typeof(HierarchicalPartitionKeyEntity)])(e, a);

                    Assert.Equal(
                        ((DerivedHierarchicalPartitionKeyEntity?)e)?.DerivedPayload,
                        ((DerivedHierarchicalPartitionKeyEntity?)a)?.DerivedPayload);
                });

        asserters[typeof(DerivedSinglePartitionKeyEntity)] =
            new Action<object, object>(
                (e, a) =>
                {
                    ((Action<object, object>)EntityAsserters[typeof(SinglePartitionKeyEntity)])(e, a);

                    Assert.Equal(
                        ((DerivedSinglePartitionKeyEntity?)e)?.DerivedPayload,
                        ((DerivedSinglePartitionKeyEntity?)a)?.DerivedPayload);
                });

        asserters[typeof(DerivedOnlyHierarchicalPartitionKeyEntity)] =
            new Action<object, object>(
                (e, a) =>
                {
                    ((Action<object, object>)EntityAsserters[typeof(OnlyHierarchicalPartitionKeyEntity)])(e, a);

                    Assert.Equal(
                        ((DerivedOnlyHierarchicalPartitionKeyEntity?)e)?.DerivedPayload,
                        ((DerivedOnlyHierarchicalPartitionKeyEntity?)a)?.DerivedPayload);
                });

        asserters[typeof(DerivedOnlySinglePartitionKeyEntity)] =
            new Action<object, object>(
                (e, a) =>
                {
                    ((Action<object, object>)EntityAsserters[typeof(OnlySinglePartitionKeyEntity)])(e, a);

                    Assert.Equal(
                        ((DerivedOnlySinglePartitionKeyEntity?)e)?.DerivedPayload,
                        ((DerivedOnlySinglePartitionKeyEntity?)a)?.DerivedPayload);
                });

        asserters[typeof(DerivedNoPartitionKeyEntity)] =
            new Action<object, object>(
                (e, a) =>
                {
                    ((Action<object, object>)EntityAsserters[typeof(NoPartitionKeyEntity)])(e, a);

                    Assert.Equal(
                        ((DerivedNoPartitionKeyEntity?)e)?.DerivedPayload,
                        ((DerivedNoPartitionKeyEntity?)a)?.DerivedPayload);
                });

        var sorters = (Dictionary<Type, object>)EntitySorters;

        sorters[typeof(DerivedHierarchicalPartitionKeyEntity)]
            = new Func<object, object>(e => ((DerivedHierarchicalPartitionKeyEntity)e).Id);

        sorters[typeof(DerivedOnlyHierarchicalPartitionKeyEntity)]
            = new Func<object, object>(e => ((DerivedOnlyHierarchicalPartitionKeyEntity)e).DerivedPayload);

        sorters[typeof(DerivedSinglePartitionKeyEntity)]
            = new Func<object, object>(e => ((DerivedSinglePartitionKeyEntity)e).Id);

        sorters[typeof(DerivedOnlySinglePartitionKeyEntity)]
            = new Func<object, object>(e => ((DerivedOnlySinglePartitionKeyEntity)e).DerivedPayload);

        sorters[typeof(DerivedNoPartitionKeyEntity)]
            = new Func<object, object>(e => ((DerivedNoPartitionKeyEntity)e).Id);
    }

    public override ISetSource GetExpectedData()
        => ExpectedData ??= new InheritancePartitionKeyData();

    public class InheritancePartitionKeyData : PartitionKeyData
    {
        public InheritancePartitionKeyData()
        {
            HierarchicalPartitionKeyEntities.AddRange(CreateDerivedHierarchicalPartitionKeyEntities());
            SinglePartitionKeyEntities.AddRange(CreateDerivedSinglePartitionKeyEntities());
            OnlyHierarchicalPartitionKeyEntities.AddRange(CreateDerivedOnlyHierarchicalPartitionKeyEntities());
            OnlySinglePartitionKeyEntities.AddRange(CreateDerivedOnlySinglePartitionKeyEntities());
            NoPartitionKeyEntities.AddRange(CreateDerivedNoPartitionKeyEntities());
        }

        public override IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(DerivedHierarchicalPartitionKeyEntity))
            {
                return (IQueryable<TEntity>)HierarchicalPartitionKeyEntities.OfType<DerivedHierarchicalPartitionKeyEntity>().AsQueryable();
            }

            if (typeof(TEntity) == typeof(DerivedSinglePartitionKeyEntity))
            {
                return (IQueryable<TEntity>)SinglePartitionKeyEntities.OfType<DerivedSinglePartitionKeyEntity>().AsQueryable();
            }

            if (typeof(TEntity) == typeof(DerivedOnlyHierarchicalPartitionKeyEntity))
            {
                return (IQueryable<TEntity>)OnlyHierarchicalPartitionKeyEntities.OfType<DerivedOnlyHierarchicalPartitionKeyEntity>()
                    .AsQueryable();
            }

            if (typeof(TEntity) == typeof(DerivedOnlySinglePartitionKeyEntity))
            {
                return (IQueryable<TEntity>)OnlySinglePartitionKeyEntities.OfType<DerivedOnlySinglePartitionKeyEntity>().AsQueryable();
            }

            if (typeof(TEntity) == typeof(DerivedNoPartitionKeyEntity))
            {
                return (IQueryable<TEntity>)NoPartitionKeyEntities.OfType<DerivedNoPartitionKeyEntity>().AsQueryable();
            }

            return base.Set<TEntity>();
        }

        private static List<DerivedHierarchicalPartitionKeyEntity> CreateDerivedHierarchicalPartitionKeyEntities()
            => new()
            {
                new DerivedHierarchicalPartitionKeyEntity
                {
                    Id = Guid.Parse("316C846C-787F-44B9-AADF-272F1658C5FF"),
                    PartitionKey1 = "PK1",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload1",
                    DerivedPayload = "DerivedPayload1"
                },
                new DerivedHierarchicalPartitionKeyEntity
                {
                    Id = Guid.Parse("316C846C-787F-44B9-AADF-272F1658C5FF"), // Same Id as previous; different partition.
                    PartitionKey1 = "PK2",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload2",
                    DerivedPayload = "DerivedPayload2"
                },
                new DerivedHierarchicalPartitionKeyEntity
                {
                    Id = Guid.Parse("C6E8E6D2-F33E-4695-9FA5-D0E9517EF04E"), // New Id.
                    PartitionKey1 = "PK1",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload3",
                    DerivedPayload = "DerivedPayload3"
                },
                new DerivedHierarchicalPartitionKeyEntity
                {
                    Id = Guid.Parse("C6E8E6D2-F33E-4695-9FA5-D0E9517EF04E"), // Same Id as previous; different partition.
                    PartitionKey1 = "PK2",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload4",
                    DerivedPayload = "DerivedPayload4"
                }
            };

        private static List<DerivedSinglePartitionKeyEntity> CreateDerivedSinglePartitionKeyEntities()
            => new()
            {
                new DerivedSinglePartitionKeyEntity
                {
                    Id = Guid.Parse("188D3253-81BE-4A87-B58F-A2BD07E6B98C"),
                    PartitionKey = "PK1",
                    Payload = "Payload1",
                    DerivedPayload = "DerivedPayload1"
                },
                new DerivedSinglePartitionKeyEntity
                {
                    Id = Guid.Parse("188D3253-81BE-4A87-B58F-A2BD07E6B98C"),
                    PartitionKey = "PK2",
                    Payload = "Payload2",
                    DerivedPayload = "DerivedPayload2"
                },
                new DerivedSinglePartitionKeyEntity
                {
                    Id = Guid.Parse("11F8D1FD-7472-46F5-9E20-16AF42B3B8D1"),
                    PartitionKey = "PK1",
                    Payload = "Payload3",
                    DerivedPayload = "DerivedPayload3"
                },
                new DerivedSinglePartitionKeyEntity
                {
                    Id = Guid.Parse("11F8D1FD-7472-46F5-9E20-16AF42B3B8D1"),
                    PartitionKey = "PK2",
                    Payload = "Payload4",
                    DerivedPayload = "DerivedPayload4"
                }
            };

        private static List<DerivedOnlyHierarchicalPartitionKeyEntity> CreateDerivedOnlyHierarchicalPartitionKeyEntities()
            => new()
            {
                new DerivedOnlyHierarchicalPartitionKeyEntity
                {
                    PartitionKey1 = "PK1c",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload1",
                    DerivedPayload = "DerivedPayload1"
                },
                new DerivedOnlyHierarchicalPartitionKeyEntity
                {
                    PartitionKey1 = "PK2c",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload2",
                    DerivedPayload = "DerivedPayload2"
                },
                new DerivedOnlyHierarchicalPartitionKeyEntity
                {
                    PartitionKey1 = "PK1d",
                    PartitionKey2 = 1,
                    PartitionKey3 = true,
                    Payload = "Payload3",
                    DerivedPayload = "DerivedPayload3"
                },
                new DerivedOnlyHierarchicalPartitionKeyEntity
                {
                    PartitionKey1 = "PK2d",
                    PartitionKey2 = 2,
                    PartitionKey3 = false,
                    Payload = "Payload4",
                    DerivedPayload = "DerivedPayload4"
                }
            };

        private static List<DerivedOnlySinglePartitionKeyEntity> CreateDerivedOnlySinglePartitionKeyEntities()
            => new()
            {
                new DerivedOnlySinglePartitionKeyEntity
                {
                    PartitionKey = "PK1c",
                    Payload = "Payload1",
                    DerivedPayload = "DerivedPayload1"
                },
                new DerivedOnlySinglePartitionKeyEntity
                {
                    PartitionKey = "PK2c",
                    Payload = "Payload2",
                    DerivedPayload = "DerivedPayload2"
                },
                new DerivedOnlySinglePartitionKeyEntity
                {
                    PartitionKey = "PK1d",
                    Payload = "Payload3",
                    DerivedPayload = "DerivedPayload3"
                },
                new DerivedOnlySinglePartitionKeyEntity
                {
                    PartitionKey = "PK2d",
                    Payload = "Payload4",
                    DerivedPayload = "DerivedPayload4"
                }
            };

        private static List<DerivedNoPartitionKeyEntity> CreateDerivedNoPartitionKeyEntities()
            => new()
            {
                new DerivedNoPartitionKeyEntity
                {
                    Id = 11,
                    Payload = "Payload1",
                    DerivedPayload = "DerivedPayload1"
                },
                new DerivedNoPartitionKeyEntity
                {
                    Id = 22,
                    Payload = "Payload2",
                    DerivedPayload = "DerivedPayload2"
                }
            };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<DerivedHierarchicalPartitionKeyEntity>();
        modelBuilder.Entity<DerivedSinglePartitionKeyEntity>();
        modelBuilder.Entity<DerivedOnlyHierarchicalPartitionKeyEntity>();
        modelBuilder.Entity<DerivedOnlySinglePartitionKeyEntity>();
        modelBuilder.Entity<DerivedNoPartitionKeyEntity>();
    }
}

public class DerivedHierarchicalPartitionKeyEntity : HierarchicalPartitionKeyEntity
{
    public required string DerivedPayload { get; set; }
}

public class DerivedSinglePartitionKeyEntity : SinglePartitionKeyEntity
{
    public required string DerivedPayload { get; set; }
}

public class DerivedOnlyHierarchicalPartitionKeyEntity : OnlyHierarchicalPartitionKeyEntity
{
    public required string DerivedPayload { get; set; }
}

public class DerivedOnlySinglePartitionKeyEntity : OnlySinglePartitionKeyEntity
{
    public required string DerivedPayload { get; set; }
}

public class DerivedNoPartitionKeyEntity : NoPartitionKeyEntity
{
    public required string DerivedPayload { get; set; }
}
