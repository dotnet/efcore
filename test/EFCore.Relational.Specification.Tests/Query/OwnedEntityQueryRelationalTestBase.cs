// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OwnedEntityQueryRelationalTestBase : OwnedEntityQueryTestBase
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(bool async)
    {
        var contextFactory = await InitializeAsync<Context24777>();
        using var context = contextFactory.CreateContext();

        var query = context.Roots.Where(e => e.Id == 3).AsSplitQuery();
        var root3 = async
            ? await query.SingleAsync()
            : query.Single();

        Assert.Equal(2, root3.ModdleA.Leaves.Count);
    }

    protected class Context24777 : DbContext
    {
        public Context24777(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Root24777> Roots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Root24777>(
                b =>
                {
                    b.ToTable(nameof(Root24777));
                    b.HasKey(x => x.Id);
                    b.OwnsOne(
                        x => x.ModdleA, ob =>
                        {
                            ob.ToTable(nameof(ModdleA24777));
                            ob.HasKey(x => x.Id);
                            ob.WithOwner().HasForeignKey(e => e.RootId);
                            ob.OwnsMany(
                                x => x.Leaves, oob =>
                                {
                                    oob.ToTable(nameof(Leaf24777));
                                    oob.HasKey(x => new { ProductCommissionRulesetId = x.ModdleAId, x.UnitThreshold });
                                    oob.Property(x => x.ModdleAId).ValueGeneratedNever();
                                    oob.Property(x => x.UnitThreshold).ValueGeneratedNever();
                                    oob.WithOwner().HasForeignKey(e => e.ModdleAId);
                                    oob.HasData(
                                        new Leaf24777 { ModdleAId = 1, UnitThreshold = 1 },
                                        new Leaf24777 { ModdleAId = 3, UnitThreshold = 1 },
                                        new Leaf24777 { ModdleAId = 3, UnitThreshold = 15 });
                                });

                            ob.HasData(
                                new ModdleA24777 { Id = 1, RootId = 1 },
                                new ModdleA24777 { Id = 2, RootId = 2 },
                                new ModdleA24777 { Id = 3, RootId = 3 });
                        });

                    b.OwnsOne(
                        x => x.MiddleB, ob =>
                        {
                            ob.ToTable(nameof(MiddleB24777));
                            ob.HasKey(x => x.Id);
                            ob.WithOwner().HasForeignKey(e => e.RootId);
                            ob.HasData(
                                new MiddleB24777
                                {
                                    Id = 1,
                                    RootId = 1,
                                    Enabled = true
                                },
                                new MiddleB24777
                                {
                                    Id = 2,
                                    RootId = 3,
                                    Enabled = true
                                });
                        });

                    b.HasData(
                        new Root24777 { Id = 1 },
                        new Root24777 { Id = 2 },
                        new Root24777 { Id = 3 });
                });
    }

    protected class Root24777
    {
        public int Id { get; init; }
        public ModdleA24777 ModdleA { get; init; }
        public MiddleB24777 MiddleB { get; init; }
    }

    protected class ModdleA24777
    {
        public int Id { get; init; }
        public int RootId { get; init; }
        public List<Leaf24777> Leaves { get; init; }
    }

    protected class MiddleB24777
    {
        public int Id { get; init; }
        public int RootId { get; init; }
        public bool Enabled { get; init; }
    }

    protected class Leaf24777
    {
        public int ModdleAId { get; init; }
        public int UnitThreshold { get; init; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_collection_basic_split_query(bool async)
    {
        var contextFactory = await InitializeAsync<Context25680>();
        using var context = contextFactory.CreateContext();

        var id = new Guid("6c1ae3e5-30b9-4c77-8d98-f02075974a0a");
        var query = context.Set<Location25680>().Where(e => e.Id == id).AsSplitQuery();
        var result = async
            ? await query.FirstOrDefaultAsync()
            : query.FirstOrDefault();
    }

    protected class Context25680 : DbContext
    {
        public Context25680(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Location25680>().OwnsMany(
                e => e.PublishTokenTypes,
                b =>
                {
                    b.WithOwner(e => e.Location).HasForeignKey(e => e.LocationId);
                    b.HasKey(
                        e => new
                        {
                            e.LocationId,
                            e.ExternalId,
                            e.VisualNumber,
                            e.TokenGroupId
                        });
                });
    }

    protected class Location25680
    {
        public Guid Id { get; set; }
        public ICollection<PublishTokenType25680> PublishTokenTypes { get; set; }
    }

    protected class PublishTokenType25680
    {
        public Location25680 Location { get; set; }
        public Guid LocationId { get; set; }

        public string ExternalId { get; set; }
        public string VisualNumber { get; set; }
        public string TokenGroupId { get; set; }
        public string IssuerName { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_reference_mapped_to_different_table_updated_correctly_after_subquery_pushdown(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26592>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();

        await base.Owned_references_on_same_level_expanded_at_different_times_around_take_helper(context, async);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_reference_mapped_to_different_table_nested_updated_correctly_after_subquery_pushdown(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26592>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();

        await base.Owned_references_on_same_level_nested_expanded_at_different_times_around_take_helper(context, async);
    }

    protected class MyContext26592 : MyContext26592Base
    {
        public MyContext26592(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(
                b =>
                {
                    b.OwnsOne(e => e.CustomerData).ToTable("CustomerData");
                    b.OwnsOne(e => e.SupplierData).ToTable("SupplierData");
                });

            modelBuilder.Entity<Owner>(
                b =>
                {
                    b.OwnsOne(
                        e => e.OwnedEntity, o =>
                        {
                            o.ToTable("IntermediateOwnedEntity");
                            o.OwnsOne(e => e.CustomerData).ToTable("IM_CustomerData");
                            o.OwnsOne(e => e.SupplierData).ToTable("IM_SupplierData");
                        });
                });
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_materializes_when_not_containing_another_owned_entity(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext28247>(seed: c => c.Seed());

        using var context = contextFactory.CreateContext();
        var query = context.RotRutCases.OrderBy(e => e.Buyer);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal("Buyer1", t.Buyer);
                Assert.NotNull(t.Rot);
                Assert.Equal(1, t.Rot.ServiceType);
                Assert.Equal("1", t.Rot.ApartmentNo);
                Assert.NotNull(t.Rut);
                Assert.Equal(1, t.Rut.Value);
            },
            t =>
            {
                Assert.Equal("Buyer2", t.Buyer);
                Assert.Null(t.Rot);
                Assert.Null(t.Rut);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_entity_equality_when_not_containing_another_owned_entity(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext28247>(seed: c => c.Seed());

        using var context = contextFactory.CreateContext();
        var query = context.RotRutCases.AsNoTracking().Select(e => e.Rot).Where(e => e != null);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal(1, t.ServiceType);
                Assert.Equal("1", t.ApartmentNo);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_property_access_when_not_containing_another_owned_entity(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext28247>(seed: c => c.Seed());

        using var context = contextFactory.CreateContext();
        var query = context.RotRutCases.AsNoTracking().Select(e => e.Rot.ApartmentNo);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal("1", t);
            },
            t =>
            {
                Assert.Null(t);
            });
    }

    protected class MyContext28247 : DbContext
    {
        public MyContext28247(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<RotRutCase> RotRutCases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<RotRutCase>(
                b =>
                {
                    b.ToTable("RotRutCases");

                    b.OwnsOne(e => e.Rot);
                    b.OwnsOne(e => e.Rut);
                });

        public void Seed()
        {
            Add(
                new RotRutCase
                {
                    Buyer = "Buyer1",
                    Rot = new Rot { ServiceType = 1, ApartmentNo = "1" },
                    Rut = new Rut { Value = 1 }
                });

            Add(
                new RotRutCase
                {
                    Buyer = "Buyer2",
                    Rot = new Rot { ServiceType = null, ApartmentNo = null },
                    Rut = new Rut { Value = null }
                });

            SaveChanges();
        }
    }

    public class RotRutCase
    {
        public int Id { get; set; }
        public string Buyer { get; set; }
        public Rot Rot { get; set; }
        public Rut Rut { get; set; }
    }

    public class Rot
    {
        public int? ServiceType { get; set; }
        public string ApartmentNo { get; set; }
    }

    public class Rut
    {
        public int? Value { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_selects_with_duplicating_aliases_and_owned_expansion_uniquifies_correctly(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext30358>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();

        var query = from monarch in context.Monarchs
                    join magus in context.Magi.Where(x => x.Name.Contains("Bayaz")) on monarch.RulerOf equals magus.Affiliation
                    select new { monarch, magus };

        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Single(result);
        Assert.Equal("The Union", result[0].monarch.RulerOf);
        Assert.Equal("The Divider", result[0].magus.ToolUsed.Name);
    }

    protected class MyContext30358 : DbContext
    {
        public DbSet<Monarch30358> Monarchs { get; set; }
        public DbSet<Magus30358> Magi { get; set; }

        public MyContext30358(DbContextOptions options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Magus30358>().OwnsOne(x => x.ToolUsed, x => x.ToTable("MagicTools"));
        }

        public void Seed()
        {
            Add(new Monarch30358
            {
                Name = "His August Majesty Guslav the Fifth",
                RulerOf = "The Union",
            });

            Add(new Monarch30358
            {
                Name = "Emperor Uthman-ul-Dosht",
                RulerOf = "The Gurkish Empire",
            });

            Add(new Magus30358
            {
                Name = "Bayaz, the First of the Magi",
                Affiliation = "The Union",
                ToolUsed = new MagicTool30358 { Name = "The Divider" }
            });

            Add(new Magus30358
            {
                Name = "The Prophet Khalul",
                Affiliation = "The Gurkish Empire",
                ToolUsed = new MagicTool30358 { Name = "The Hundred Words" }
            });

            SaveChanges();
        }
    }

    public class Monarch30358
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RulerOf { get; set; }
    }

    public class Magus30358
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Affiliation { get; set; }
        public MagicTool30358 ToolUsed { get; set; }
    }

    public class MagicTool30358
    {
        public string Name { get; set; }
    }

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            c => c
                .Log(RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning)
                .Log(RelationalEventId.OptionalDependentWithAllNullPropertiesWarning));
}
