// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OwnedEntityQuerySqlServerTest : OwnedEntityQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    #region 22054

    [ConditionalFact]
    public virtual async Task Optional_dependent_is_null_when_sharing_required_column_with_principal()
    {
        var contextFactory = await InitializeAsync<Context22054>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.Set<Context22054.User22054>().OrderByDescending(e => e.Id).ToList();
        Assert.Equal(3, query.Count);
        Assert.Null(query[0].Contact);
        Assert.Null(query[0].Data);
        Assert.NotNull(query[1].Data);
        Assert.NotNull(query[1].Contact);
        Assert.Null(query[1].Contact.Address);
        Assert.NotNull(query[2].Data);
        Assert.NotNull(query[2].Contact);
        Assert.NotNull(query[2].Contact.Address);

        AssertSql(
            """
SELECT [u].[Id], [u].[RowVersion], [u].[Contact_MobileNumber], [u].[SharedProperty], [u].[Contact_Address_City], [u].[Contact_Address_Zip], [u].[Data_Data], [u].[Data_Exists], [u].[RowVersion]
FROM [User22054] AS [u]
ORDER BY [u].[Id] DESC
""");
    }

    protected class Context22054(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<User22054>(
                builder =>
                {
                    builder.HasKey(x => x.Id);

                    builder.OwnsOne(
                        x => x.Contact, contact =>
                        {
                            contact.Property(e => e.SharedProperty).IsRequired().HasColumnName("SharedProperty");

                            contact.OwnsOne(
                                c => c.Address, address =>
                                {
                                    address.Property<string>("SharedProperty").IsRequired().HasColumnName("SharedProperty");
                                });
                        });

                    builder.OwnsOne(e => e.Data)
                        .Property<byte[]>("RowVersion")
                        .IsRowVersion()
                        .IsRequired()
                        .HasColumnType("TIMESTAMP")
                        .HasColumnName("RowVersion");

                    builder.Property(x => x.RowVersion)
                        .HasColumnType("TIMESTAMP")
                        .IsRowVersion()
                        .IsRequired()
                        .HasColumnName("RowVersion");
                });

        public Task SeedAsync()
        {
            AddRange(
                new User22054
                {
                    Data = new Data22054 { Data = "Data1" },
                    Contact = new Contact22054
                    {
                        MobileNumber = "123456",
                        SharedProperty = "Value1",
                        Address = new Address22054
                        {
                            City = "Seattle",
                            Zip = 12345,
                            SharedProperty = "Value1"
                        }
                    }
                },
                new User22054
                {
                    Data = new Data22054 { Data = "Data2" },
                    Contact = new Contact22054
                    {
                        MobileNumber = "654321",
                        SharedProperty = "Value2",
                        Address = null
                    }
                },
                new User22054 { Contact = null, Data = null });

            return SaveChangesAsync();
        }

        public class User22054
        {
            public int Id { get; set; }
            public Data22054 Data { get; set; }
            public Contact22054 Contact { get; set; }
            public byte[] RowVersion { get; set; }
        }

        public class Data22054
        {
            public string Data { get; set; }
            public bool Exists { get; set; }
        }

        public class Contact22054
        {
            public string MobileNumber { get; set; }
            public string SharedProperty { get; set; }
            public Address22054 Address { get; set; }
        }

        public class Address22054
        {
            public string City { get; set; }
            public string SharedProperty { get; set; }
            public int Zip { get; set; }
        }
    }

    #endregion

    #region 22340

    [ConditionalFact]
    public virtual async Task Owned_entity_mapped_to_separate_table()
    {
        var contextFactory = await InitializeAsync<Context22340>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var masterTrunk = context.MasterTrunk.OrderBy(e => EF.Property<string>(e, "Id")).FirstOrDefault();

        Assert.NotNull(masterTrunk);

        AssertSql(
            """
SELECT [s1].[Id], [s1].[MasterTrunk22340Id], [s1].[MasterTrunk22340Id0], [f0].[CurrencyBag22340MasterTrunk22340Id], [f0].[Id], [f0].[Amount], [f0].[Code], [s0].[CurrencyBag22340MasterTrunk22340Id], [s0].[Id], [s0].[Amount], [s0].[Code]
FROM (
    SELECT TOP(1) [m].[Id], [f].[MasterTrunk22340Id], [s].[MasterTrunk22340Id] AS [MasterTrunk22340Id0]
    FROM [MasterTrunk] AS [m]
    LEFT JOIN [FungibleBag] AS [f] ON [m].[Id] = [f].[MasterTrunk22340Id]
    LEFT JOIN [StaticBag] AS [s] ON [m].[Id] = [s].[MasterTrunk22340Id]
    ORDER BY [m].[Id]
) AS [s1]
LEFT JOIN [FungibleBag_Currencies] AS [f0] ON [s1].[MasterTrunk22340Id] = [f0].[CurrencyBag22340MasterTrunk22340Id]
LEFT JOIN [StaticBag_Currencies] AS [s0] ON [s1].[MasterTrunk22340Id0] = [s0].[CurrencyBag22340MasterTrunk22340Id]
ORDER BY [s1].[Id], [s1].[MasterTrunk22340Id], [s1].[MasterTrunk22340Id0], [f0].[CurrencyBag22340MasterTrunk22340Id], [f0].[Id], [s0].[CurrencyBag22340MasterTrunk22340Id]
""");
    }

    protected class Context22340(DbContextOptions options) : DbContext(options)
    {
        public DbSet<MasterTrunk22340> MasterTrunk { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<MasterTrunk22340>();
            builder.Property<string>("Id").ValueGeneratedOnAdd();
            builder.HasKey("Id");

            builder.OwnsOne(
                p => p.FungibleBag, p =>
                {
                    p.OwnsMany(
                        p => p.Currencies, p =>
                        {
                            p.Property(p => p.Amount).IsConcurrencyToken();
                        });

                    p.ToTable("FungibleBag");
                });

            builder.OwnsOne(
                p => p.StaticBag, p =>
                {
                    p.OwnsMany(
                        p => p.Currencies, p =>
                        {
                            p.Property(p => p.Amount).IsConcurrencyToken();
                        });
                    p.ToTable("StaticBag");
                });
        }

        public Task SeedAsync()
        {
            var masterTrunk = new MasterTrunk22340
            {
                FungibleBag = new CurrencyBag22340 { Currencies = new[] { new Currency22340 { Amount = 10, Code = 999 } } },
                StaticBag = new CurrencyBag22340 { Currencies = new[] { new Currency22340 { Amount = 555, Code = 111 } } }
            };
            Add(masterTrunk);

            return SaveChangesAsync();
        }

        public class MasterTrunk22340
        {
            public CurrencyBag22340 FungibleBag { get; set; }
            public CurrencyBag22340 StaticBag { get; set; }
        }

        public class CurrencyBag22340
        {
            public IEnumerable<Currency22340> Currencies { get; set; }
        }

        public class Currency22340
        {
            [Column(TypeName = "decimal(18,2)")]
            public decimal Amount { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal Code { get; set; }
        }
    }

    #endregion

    #region 23211

    [ConditionalFact]
    public virtual async Task Collection_include_on_owner_with_owned_type_mapped_to_different_table()
    {
        var contextFactory = await InitializeAsync<Context23211>(seed: c => c.SeedAsync());
        using (var context = contextFactory.CreateContext())
        {
            var owner = context.Set<Context23211.Owner23211>().Include(e => e.Dependents).AsSplitQuery().OrderBy(e => e.Id).Single();
            Assert.NotNull(owner.Dependents);
            Assert.Equal(2, owner.Dependents.Count);
            Assert.NotNull(owner.Owned1);
            Assert.Equal("A", owner.Owned1.Value);
            Assert.NotNull(owner.Owned2);
            Assert.Equal("B", owner.Owned2.Value);

            AssertSql(
                """
SELECT TOP(2) [o].[Id], [o0].[Owner23211Id], [o0].[Value], [o1].[Owner23211Id], [o1].[Value]
FROM [Owner23211] AS [o]
LEFT JOIN [Owned1_23211] AS [o0] ON [o].[Id] = [o0].[Owner23211Id]
LEFT JOIN [Owned2_23211] AS [o1] ON [o].[Id] = [o1].[Owner23211Id]
ORDER BY [o].[Id], [o0].[Owner23211Id], [o1].[Owner23211Id]
""",
                //
                """
SELECT [d].[Id], [d].[Owner23211Id], [s].[Id], [s].[Owner23211Id], [s].[Owner23211Id0]
FROM (
    SELECT TOP(1) [o].[Id], [o0].[Owner23211Id], [o1].[Owner23211Id] AS [Owner23211Id0]
    FROM [Owner23211] AS [o]
    LEFT JOIN [Owned1_23211] AS [o0] ON [o].[Id] = [o0].[Owner23211Id]
    LEFT JOIN [Owned2_23211] AS [o1] ON [o].[Id] = [o1].[Owner23211Id]
    ORDER BY [o].[Id]
) AS [s]
INNER JOIN [Dependent23211] AS [d] ON [s].[Id] = [d].[Owner23211Id]
ORDER BY [s].[Id], [s].[Owner23211Id], [s].[Owner23211Id0]
""");
        }

        using (var context = contextFactory.CreateContext())
        {
            ClearLog();
            var owner = context.Set<Context23211.SecondOwner23211>().Include(e => e.Dependents).AsSplitQuery().OrderBy(e => e.Id)
                .Single();
            Assert.NotNull(owner.Dependents);
            Assert.Equal(2, owner.Dependents.Count);
            Assert.NotNull(owner.Owned);
            Assert.Equal("A", owner.Owned.Value);

            AssertSql(
                """
SELECT TOP(2) [s].[Id], [o].[SecondOwner23211Id], [o].[Value]
FROM [SecondOwner23211] AS [s]
LEFT JOIN [Owned23211] AS [o] ON [s].[Id] = [o].[SecondOwner23211Id]
ORDER BY [s].[Id], [o].[SecondOwner23211Id]
""",
                //
                """
SELECT [s0].[Id], [s0].[SecondOwner23211Id], [s1].[Id], [s1].[SecondOwner23211Id]
FROM (
    SELECT TOP(1) [s].[Id], [o].[SecondOwner23211Id]
    FROM [SecondOwner23211] AS [s]
    LEFT JOIN [Owned23211] AS [o] ON [s].[Id] = [o].[SecondOwner23211Id]
    ORDER BY [s].[Id]
) AS [s1]
INNER JOIN [SecondDependent23211] AS [s0] ON [s1].[Id] = [s0].[SecondOwner23211Id]
ORDER BY [s1].[Id], [s1].[SecondOwner23211Id]
""");
        }
    }

    protected class Context23211(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Owner23211>().OwnsOne(e => e.Owned1, b => b.ToTable("Owned1_23211"));
            modelBuilder.Entity<Owner23211>().OwnsOne(e => e.Owned2, b => b.ToTable("Owned2_23211"));
            modelBuilder.Entity<SecondOwner23211>().OwnsOne(e => e.Owned, b => b.ToTable("Owned23211"));
        }

        public Task SeedAsync()
        {
            Add(
                new Owner23211
                {
                    Dependents = [new(), new()],
                    Owned1 = new OwnedType23211 { Value = "A" },
                    Owned2 = new OwnedType23211 { Value = "B" }
                });

            Add(
                new SecondOwner23211 { Dependents = [new(), new()], Owned = new OwnedType23211 { Value = "A" } });

            return SaveChangesAsync();
        }

        public class Owner23211
        {
            public int Id { get; set; }
            public List<Dependent23211> Dependents { get; set; }
            public OwnedType23211 Owned1 { get; set; }
            public OwnedType23211 Owned2 { get; set; }
        }

        public class OwnedType23211
        {
            public string Value { get; set; }
        }

        public class Dependent23211
        {
            public int Id { get; set; }
        }

        public class SecondOwner23211
        {
            public int Id { get; set; }
            public List<SecondDependent23211> Dependents { get; set; }
            public OwnedType23211 Owned { get; set; }
        }

        public class SecondDependent23211
        {
            public int Id { get; set; }
        }
    }

    #endregion

    public override async Task Include_collection_for_entity_with_owned_type_works()
    {
        await base.Include_collection_for_entity_with_owned_type_works();

        AssertSql(
            """
SELECT [m].[Id], [m].[Title], [m].[Details_Info], [m].[Details_Rating], [a].[Id], [a].[MovieId], [a].[Name], [a].[Details_Info], [a].[Details_Rating]
FROM [Movies] AS [m]
LEFT JOIN [Actors] AS [a] ON [m].[Id] = [a].[MovieId]
ORDER BY [m].[Id]
""",
            //
            """
SELECT [m].[Id], [m].[Title], [m].[Details_Info], [m].[Details_Rating], [a].[Id], [a].[MovieId], [a].[Name], [a].[Details_Info], [a].[Details_Rating]
FROM [Movies] AS [m]
LEFT JOIN [Actors] AS [a] ON [m].[Id] = [a].[MovieId]
ORDER BY [m].[Id]
""");
    }

    public override async Task Multilevel_owned_entities_determine_correct_nullability()
    {
        await base.Multilevel_owned_entities_determine_correct_nullability();

        AssertSql(
            """
@p0='BaseEntity' (Nullable = false) (Size = 13)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [BaseEntities] ([Discriminator])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""");
    }

    public override async Task Correlated_subquery_with_owned_navigation_being_compared_to_null_works()
    {
        await base.Correlated_subquery_with_owned_navigation_being_compared_to_null_works();

        AssertSql(
            """
SELECT [p].[Id], CASE
    WHEN [a].[Turnovers_AmountIn] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [a].[Turnovers_AmountIn], [a].[Id]
FROM [Partners] AS [p]
LEFT JOIN [Address] AS [a] ON [p].[Id] = [a].[PartnerId]
ORDER BY [p].[Id]
""");
    }


    public override async Task Owned_entity_multiple_level_in_aggregate()
    {
        await base.Owned_entity_multiple_level_in_aggregate();

        AssertSql(
            """
SELECT [a0].[Id], [a0].[FirstValueObject_Value], [s2].[Id], [s2].[AggregateId], [s2].[FourthValueObject_Value], [s2].[Id0], [s2].[AnyValue], [s2].[SecondValueObjectId], [s2].[Id1], [s2].[SecondValueObjectId0], [s2].[FourthValueObject_Value0], [s2].[Id00], [s2].[AnyValue0], [s2].[ThirdValueObjectId]
FROM (
    SELECT TOP(1) [a].[Id], [a].[FirstValueObject_Value]
    FROM [Aggregate] AS [a]
    ORDER BY [a].[Id] DESC
) AS [a0]
LEFT JOIN (
    SELECT [s].[Id], [s].[AggregateId], [s].[FourthValueObject_Value], [s0].[Id] AS [Id0], [s0].[AnyValue], [s0].[SecondValueObjectId], [s1].[Id] AS [Id1], [s1].[SecondValueObjectId] AS [SecondValueObjectId0], [s1].[FourthValueObject_Value] AS [FourthValueObject_Value0], [s1].[Id0] AS [Id00], [s1].[AnyValue] AS [AnyValue0], [s1].[ThirdValueObjectId]
    FROM [SecondValueObject] AS [s]
    LEFT JOIN [SecondValueObject_FifthValueObjects] AS [s0] ON CASE
        WHEN [s].[FourthValueObject_Value] IS NOT NULL THEN [s].[Id]
    END = [s0].[SecondValueObjectId]
    LEFT JOIN (
        SELECT [t].[Id], [t].[SecondValueObjectId], [t].[FourthValueObject_Value], [t0].[Id] AS [Id0], [t0].[AnyValue], [t0].[ThirdValueObjectId]
        FROM [ThirdValueObject] AS [t]
        LEFT JOIN [ThirdValueObject_FifthValueObjects] AS [t0] ON CASE
            WHEN [t].[FourthValueObject_Value] IS NOT NULL THEN [t].[Id]
        END = [t0].[ThirdValueObjectId]
    ) AS [s1] ON [s].[Id] = [s1].[SecondValueObjectId]
) AS [s2] ON CASE
    WHEN [a0].[FirstValueObject_Value] IS NOT NULL THEN [a0].[Id]
END = [s2].[AggregateId]
ORDER BY [a0].[Id] DESC, [s2].[Id], [s2].[Id0], [s2].[Id1]
""");
    }

    public override async Task Multiple_single_result_in_projection_containing_owned_types(bool async)
    {
        await base.Multiple_single_result_in_projection_containing_owned_types(async);

        AssertSql(
            """
SELECT [e].[Id], [c2].[Id], [c2].[EntityId], [c2].[Owned_IsDeleted], [c2].[Owned_Value], [c2].[Type], [c2].[c], [c4].[Id], [c4].[EntityId], [c4].[Owned_IsDeleted], [c4].[Owned_Value], [c4].[Type], [c4].[c]
FROM [Entities] AS [e]
LEFT JOIN (
    SELECT [c1].[Id], [c1].[EntityId], [c1].[Owned_IsDeleted], [c1].[Owned_Value], [c1].[Type], [c1].[c]
    FROM (
        SELECT [c].[Id], [c].[EntityId], [c].[Owned_IsDeleted], [c].[Owned_Value], [c].[Type], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [c].[EntityId] ORDER BY [c].[EntityId], [c].[Id]) AS [row]
        FROM [Child] AS [c]
        WHERE [c].[Type] = 1
    ) AS [c1]
    WHERE [c1].[row] <= 1
) AS [c2] ON [e].[Id] = [c2].[EntityId]
LEFT JOIN (
    SELECT [c3].[Id], [c3].[EntityId], [c3].[Owned_IsDeleted], [c3].[Owned_Value], [c3].[Type], [c3].[c]
    FROM (
        SELECT [c0].[Id], [c0].[EntityId], [c0].[Owned_IsDeleted], [c0].[Owned_Value], [c0].[Type], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [c0].[EntityId] ORDER BY [c0].[EntityId], [c0].[Id]) AS [row]
        FROM [Child] AS [c0]
        WHERE [c0].[Type] = 2
    ) AS [c3]
    WHERE [c3].[row] <= 1
) AS [c4] ON [e].[Id] = [c4].[EntityId]
""");
    }

    public override async Task Can_auto_include_navigation_from_model()
    {
        await base.Can_auto_include_navigation_from_model();

        AssertSql(
            """
SELECT [p].[Id], [r].[Id], [c].[Id], [c].[ParentId], [p].[OwnedReference_Id], [r].[ParentId], [s].[Id], [s].[ParentId], [s].[OtherSideId]
FROM [Parents] AS [p]
LEFT JOIN [Reference] AS [r] ON [p].[Id] = [r].[ParentId]
LEFT JOIN [Collection] AS [c] ON [p].[Id] = [c].[ParentId]
LEFT JOIN (
    SELECT [o].[Id], [j].[ParentId], [j].[OtherSideId]
    FROM [JoinEntity] AS [j]
    INNER JOIN [OtherSide] AS [o] ON [j].[OtherSideId] = [o].[Id]
) AS [s] ON [p].[Id] = [s].[ParentId]
ORDER BY [p].[Id], [r].[Id], [c].[Id], [s].[ParentId], [s].[OtherSideId]
""",
            //
            """
SELECT [p].[Id], [p].[OwnedReference_Id]
FROM [Parents] AS [p]
""");
    }

    public override async Task Nested_owned_required_dependents_are_materialized()
    {
        await base.Nested_owned_required_dependents_are_materialized();

        AssertSql(
            """
SELECT [e].[Id], [e].[Contact_Name], [e].[Contact_Address_City], [e].[Contact_Address_State], [e].[Contact_Address_Street], [e].[Contact_Address_Zip]
FROM [Entity] AS [e]
""");
    }

    public override async Task Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(bool async)
    {
        await base.Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(async);

        AssertSql(
            """
SELECT TOP(2) [r].[Id], [m].[Id], [m].[Enabled], [m].[RootId], [m0].[Id], [m0].[RootId]
FROM [Root] AS [r]
LEFT JOIN [MiddleB] AS [m] ON [r].[Id] = [m].[RootId]
LEFT JOIN [ModdleA] AS [m0] ON [r].[Id] = [m0].[RootId]
WHERE [r].[Id] = 3
ORDER BY [r].[Id], [m].[Id], [m0].[Id]
""",
            //
            """
SELECT [l0].[ModdleAId], [l0].[UnitThreshold], [s].[Id], [s].[Id0], [s].[Id1]
FROM (
    SELECT TOP(1) [r].[Id], [m].[Id] AS [Id0], [m0].[Id] AS [Id1]
    FROM [Root] AS [r]
    LEFT JOIN [MiddleB] AS [m] ON [r].[Id] = [m].[RootId]
    LEFT JOIN [ModdleA] AS [m0] ON [r].[Id] = [m0].[RootId]
    WHERE [r].[Id] = 3
) AS [s]
INNER JOIN [Leaf] AS [l0] ON [s].[Id1] = [l0].[ModdleAId]
ORDER BY [s].[Id], [s].[Id0], [s].[Id1]
""");
    }

    public override async Task Projecting_owned_collection_and_aggregate(bool async)
    {
        await base.Projecting_owned_collection_and_aggregate(async);

        AssertSql(
            """
SELECT [b].[Id], (
    SELECT COALESCE(SUM([p].[CommentsCount]), 0)
    FROM [Post] AS [p]
    WHERE [b].[Id] = [p].[BlogId]), [p0].[Title], [p0].[CommentsCount], [p0].[BlogId], [p0].[Id]
FROM [Blog] AS [b]
LEFT JOIN [Post] AS [p0] ON [b].[Id] = [p0].[BlogId]
ORDER BY [b].[Id], [p0].[BlogId]
""");
    }

    public override async Task Projecting_correlated_collection_property_for_owned_entity(bool async)
    {
        await base.Projecting_correlated_collection_property_for_owned_entity(async);

        AssertSql(
            """
SELECT [w].[WarehouseCode], [w].[Id], [w0].[CountryCode], [w0].[WarehouseCode], [w0].[Id]
FROM [Warehouses] AS [w]
LEFT JOIN [WarehouseDestinationCountry] AS [w0] ON [w].[WarehouseCode] = [w0].[WarehouseCode]
ORDER BY [w].[Id], [w0].[WarehouseCode]
""");
    }

    public override async Task Accessing_scalar_property_in_derived_type_projection_does_not_load_owned_navigations()
    {
        await base.Accessing_scalar_property_in_derived_type_projection_does_not_load_owned_navigations();

        AssertSql(
            """
SELECT [o1].[Id], [o1].[OtherEntityData]
FROM [BaseEntities] AS [b]
LEFT JOIN (
    SELECT [o0].[Id], [o0].[OtherEntityData]
    FROM (
        SELECT [o].[Id], [o].[OtherEntityData], ROW_NUMBER() OVER(PARTITION BY [o].[OtherEntityData] ORDER BY [o].[Id]) AS [row]
        FROM [OtherEntities] AS [o]
    ) AS [o0]
    WHERE [o0].[row] <= 1
) AS [o1] ON [b].[Data] = [o1].[OtherEntityData]
""");
    }

    public override async Task Owned_collection_basic_split_query(bool async)
    {
        await base.Owned_collection_basic_split_query(async);

        AssertSql(
            """
@__id_0='6c1ae3e5-30b9-4c77-8d98-f02075974a0a'

SELECT TOP(1) [l].[Id]
FROM [Location25680] AS [l]
WHERE [l].[Id] = @__id_0
ORDER BY [l].[Id]
""");
    }

    public override async Task Owned_reference_mapped_to_different_table_updated_correctly_after_subquery_pushdown(bool async)
    {
        await base.Owned_reference_mapped_to_different_table_updated_correctly_after_subquery_pushdown(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [c].[Id], [c].[Name], [c0].[CompanyId], [c0].[AdditionalCustomerData], [c0].[Id], [s].[CompanyId], [s].[AdditionalSupplierData], [s].[Id]
FROM [Companies] AS [c]
LEFT JOIN [CustomerData] AS [c0] ON [c].[Id] = [c0].[CompanyId]
LEFT JOIN [SupplierData] AS [s] ON [c].[Id] = [s].[CompanyId]
WHERE [c0].[CompanyId] IS NOT NULL
ORDER BY [c].[Id]
""");
    }

    public override async Task Owned_reference_mapped_to_different_table_nested_updated_correctly_after_subquery_pushdown(bool async)
    {
        await base.Owned_reference_mapped_to_different_table_nested_updated_correctly_after_subquery_pushdown(async);

        AssertSql(
            """
@__p_0='10'

SELECT TOP(@__p_0) [o].[Id], [o].[Name], [i].[OwnerId], [i].[Id], [i].[Name], [i0].[IntermediateOwnedEntityOwnerId], [i0].[AdditionalCustomerData], [i0].[Id], [i1].[IntermediateOwnedEntityOwnerId], [i1].[AdditionalSupplierData], [i1].[Id]
FROM [Owners] AS [o]
LEFT JOIN [IntermediateOwnedEntity] AS [i] ON [o].[Id] = [i].[OwnerId]
LEFT JOIN [IM_CustomerData] AS [i0] ON [i].[OwnerId] = [i0].[IntermediateOwnedEntityOwnerId]
LEFT JOIN [IM_SupplierData] AS [i1] ON [i].[OwnerId] = [i1].[IntermediateOwnedEntityOwnerId]
WHERE [i0].[IntermediateOwnedEntityOwnerId] IS NOT NULL
ORDER BY [o].[Id]
""");
    }

    public override async Task Owned_entity_with_all_null_properties_materializes_when_not_containing_another_owned_entity(bool async)
    {
        await base.Owned_entity_with_all_null_properties_materializes_when_not_containing_another_owned_entity(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Buyer], [r].[Rot_ApartmentNo], [r].[Rot_ServiceType], [r].[Rut_Value]
FROM [RotRutCases] AS [r]
ORDER BY [r].[Buyer]
""");
    }

    public override async Task Owned_entity_with_all_null_properties_entity_equality_when_not_containing_another_owned_entity(bool async)
    {
        await base.Owned_entity_with_all_null_properties_entity_equality_when_not_containing_another_owned_entity(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Rot_ApartmentNo], [r].[Rot_ServiceType]
FROM [RotRutCases] AS [r]
WHERE [r].[Rot_ApartmentNo] IS NOT NULL OR [r].[Rot_ServiceType] IS NOT NULL
""");
    }

    public override async Task Owned_entity_with_all_null_properties_in_compared_to_null_in_conditional_projection(bool async)
    {
        await base.Owned_entity_with_all_null_properties_in_compared_to_null_in_conditional_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [r].[Rot_ApartmentNo] IS NULL AND [r].[Rot_ServiceType] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [r].[Rot_ApartmentNo], [r].[Rot_ServiceType]
FROM [RotRutCases] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Owned_entity_with_all_null_properties_in_compared_to_non_null_in_conditional_projection(bool async)
    {
        await base.Owned_entity_with_all_null_properties_in_compared_to_non_null_in_conditional_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [r].[Rot_ApartmentNo] IS NOT NULL OR [r].[Rot_ServiceType] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [r].[Rot_ApartmentNo], [r].[Rot_ServiceType]
FROM [RotRutCases] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Owned_entity_with_all_null_properties_property_access_when_not_containing_another_owned_entity(bool async)
    {
        await base.Owned_entity_with_all_null_properties_property_access_when_not_containing_another_owned_entity(async);

        AssertSql(
            """
SELECT [r].[Rot_ApartmentNo]
FROM [RotRutCases] AS [r]
""");
    }

    public override async Task Join_selects_with_duplicating_aliases_and_owned_expansion_uniquifies_correctly(bool async)
    {
        await base.Join_selects_with_duplicating_aliases_and_owned_expansion_uniquifies_correctly(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [m].[RulerOf], [m1].[Id], [m1].[Affiliation], [m1].[Name], [m1].[MagusId], [m1].[Name0]
FROM [Monarchs] AS [m]
INNER JOIN (
    SELECT [m0].[Id], [m0].[Affiliation], [m0].[Name], [m2].[MagusId], [m2].[Name] AS [Name0]
    FROM [Magi] AS [m0]
    LEFT JOIN [MagicTools] AS [m2] ON [m0].[Id] = [m2].[MagusId]
    WHERE [m0].[Name] LIKE N'%Bayaz%'
) AS [m1] ON [m].[RulerOf] = [m1].[Affiliation]
""");
    }
}
