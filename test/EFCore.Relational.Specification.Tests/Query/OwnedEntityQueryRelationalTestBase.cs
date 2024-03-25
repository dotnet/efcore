// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class OwnedEntityQueryRelationalTestBase : OwnedEntityQueryTestBase
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    #region 23198

    [ConditionalFact]
    public virtual async Task An_optional_dependent_without_any_columns_and_nested_dependent_throws()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => InitializeAsync<Context23198>())).Message;

        Assert.Equal(
            RelationalStrings.OptionalDependentWithDependentWithoutIdentifyingProperty(nameof(Context23198.AnOwnedTypeWithOwnedProperties)),
            message);
    }

    private class Context23198(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<AnAggregateRoot>().OwnsOne(
                e => e.AnOwnedTypeWithOwnedProperties,
                b =>
                {
                    b.OwnsOne(e => e.AnOwnedTypeWithPrimitiveProperties1);
                    b.OwnsOne(e => e.AnOwnedTypeWithPrimitiveProperties2);
                });

        public class AnAggregateRoot
        {
            public string Id { get; set; }
            public AnOwnedTypeWithOwnedProperties AnOwnedTypeWithOwnedProperties { get; set; }
        }

        public class AnOwnedTypeWithOwnedProperties
        {
            public AnOwnedTypeWithPrimitiveProperties1 AnOwnedTypeWithPrimitiveProperties1 { get; set; }
            public AnOwnedTypeWithPrimitiveProperties2 AnOwnedTypeWithPrimitiveProperties2 { get; set; }
        }

        public class AnOwnedTypeWithPrimitiveProperties1
        {
            public string Name { get; set; }
        }

        public class AnOwnedTypeWithPrimitiveProperties2
        {
            public string Name { get; set; }
        }
    }

    #endregion

    #region 24777

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

    private class Context24777(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Root> Roots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Root>(
                b =>
                {
                    b.ToTable(nameof(Root));
                    b.HasKey(x => x.Id);
                    b.OwnsOne(
                        x => x.ModdleA, ob =>
                        {
                            ob.ToTable(nameof(ModdleA));
                            ob.HasKey(x => x.Id);
                            ob.WithOwner().HasForeignKey(e => e.RootId);
                            ob.OwnsMany(
                                x => x.Leaves, oob =>
                                {
                                    oob.ToTable(nameof(Leaf));
                                    oob.HasKey(x => new { ProductCommissionRulesetId = x.ModdleAId, x.UnitThreshold });
                                    oob.Property(x => x.ModdleAId).ValueGeneratedNever();
                                    oob.Property(x => x.UnitThreshold).ValueGeneratedNever();
                                    oob.WithOwner().HasForeignKey(e => e.ModdleAId);
                                    oob.HasData(
                                        new Leaf { ModdleAId = 1, UnitThreshold = 1 },
                                        new Leaf { ModdleAId = 3, UnitThreshold = 1 },
                                        new Leaf { ModdleAId = 3, UnitThreshold = 15 });
                                });

                            ob.HasData(
                                new ModdleA { Id = 1, RootId = 1 },
                                new ModdleA { Id = 2, RootId = 2 },
                                new ModdleA { Id = 3, RootId = 3 });
                        });

                    b.OwnsOne(
                        x => x.MiddleB, ob =>
                        {
                            ob.ToTable(nameof(MiddleB));
                            ob.HasKey(x => x.Id);
                            ob.WithOwner().HasForeignKey(e => e.RootId);
                            ob.HasData(
                                new MiddleB
                                {
                                    Id = 1,
                                    RootId = 1,
                                    Enabled = true
                                },
                                new MiddleB
                                {
                                    Id = 2,
                                    RootId = 3,
                                    Enabled = true
                                });
                        });

                    b.HasData(
                        new Root { Id = 1 },
                        new Root { Id = 2 },
                        new Root { Id = 3 });
                });

        public class Root
        {
            public int Id { get; init; }
            public ModdleA ModdleA { get; init; }
            public MiddleB MiddleB { get; init; }
        }

        public class ModdleA
        {
            public int Id { get; init; }
            public int RootId { get; init; }
            public List<Leaf> Leaves { get; init; }
        }

        public class MiddleB
        {
            public int Id { get; init; }
            public int RootId { get; init; }
            public bool Enabled { get; init; }
        }

        public class Leaf
        {
            public int ModdleAId { get; init; }
            public int UnitThreshold { get; init; }
        }
    }

    #endregion

    #region 25680

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

    protected class Context25680(DbContextOptions options) : DbContext(options)
    {
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

    #endregion

    #region 26592

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_reference_mapped_to_different_table_updated_correctly_after_subquery_pushdown(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26592>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        await base.Owned_references_on_same_level_expanded_at_different_times_around_take_helper(context, async);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_reference_mapped_to_different_table_nested_updated_correctly_after_subquery_pushdown(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext26592>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        await base.Owned_references_on_same_level_nested_expanded_at_different_times_around_take_helper(context, async);
    }

    protected class MyContext26592(DbContextOptions options) : MyContext26592Base(options)
    {
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

    #endregion

    #region 28347

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_materializes_when_not_containing_another_owned_entity(bool async)
    {
        var contextFactory = await InitializeAsync<Context28247>(seed: c => c.SeedAsync());

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
        var contextFactory = await InitializeAsync<Context28247>(seed: c => c.SeedAsync());

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
    public virtual async Task Owned_entity_with_all_null_properties_in_compared_to_null_in_conditional_projection(bool async)
    {
        var contextFactory = await InitializeAsync<Context28247>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateContext();
        var query = context.RotRutCases
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(
                e => e.Rot == null
                    ? null
                    : new Context28247.RotDto { MyApartmentNo = e.Rot.ApartmentNo, MyServiceType = e.Rot.ServiceType });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal("1", t.MyApartmentNo);
                Assert.Equal(1, t.MyServiceType);
            },
            t =>
            {
                Assert.Null(t);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_in_compared_to_non_null_in_conditional_projection(bool async)
    {
        var contextFactory = await InitializeAsync<Context28247>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateContext();
        var query = context.RotRutCases
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(
                e => e.Rot != null
                    ? new Context28247.RotDto { MyApartmentNo = e.Rot.ApartmentNo, MyServiceType = e.Rot.ServiceType }
                    : null);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal("1", t.MyApartmentNo);
                Assert.Equal(1, t.MyServiceType);
            },
            t =>
            {
                Assert.Null(t);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_property_access_when_not_containing_another_owned_entity(bool async)
    {
        var contextFactory = await InitializeAsync<Context28247>(seed: c => c.SeedAsync());

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

    private class Context28247(DbContextOptions options) : DbContext(options)
    {
        public DbSet<RotRutCase> RotRutCases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<RotRutCase>(
                b =>
                {
                    b.ToTable("RotRutCases");

                    b.OwnsOne(e => e.Rot);
                    b.OwnsOne(e => e.Rut);
                });

        public Task SeedAsync()
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

            return SaveChangesAsync();
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

        public class RotDto
        {
            public int? MyServiceType { get; set; }
            public string MyApartmentNo { get; set; }
        }

        public class Rut
        {
            public int? Value { get; set; }
        }
    }

    #endregion

    #region 30358

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_selects_with_duplicating_aliases_and_owned_expansion_uniquifies_correctly(bool async)
    {
        var contextFactory = await InitializeAsync<Context30358>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var query = from monarch in context.Monarchs
                    join magus in context.Magi.Where(x => x.Name.Contains("Bayaz")) on monarch.RulerOf equals magus.Affiliation
                    select new { monarch, magus };

        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Single(result);
        Assert.Equal("The Union", result[0].monarch.RulerOf);
        Assert.Equal("The Divider", result[0].magus.ToolUsed.Name);
    }

    private class Context30358(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Monarch> Monarchs { get; set; }
        public DbSet<Magus> Magi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Magus>().OwnsOne(x => x.ToolUsed, x => x.ToTable("MagicTools"));

        public Task SeedAsync()
        {
            Add(
                new Monarch
                {
                    Name = "His August Majesty Guslav the Fifth", RulerOf = "The Union",
                });

            Add(
                new Monarch
                {
                    Name = "Emperor Uthman-ul-Dosht", RulerOf = "The Gurkish Empire",
                });

            Add(
                new Magus
                {
                    Name = "Bayaz, the First of the Magi",
                    Affiliation = "The Union",
                    ToolUsed = new MagicTool { Name = "The Divider" }
                });

            Add(
                new Magus
                {
                    Name = "The Prophet Khalul",
                    Affiliation = "The Gurkish Empire",
                    ToolUsed = new MagicTool { Name = "The Hundred Words" }
                });

            return SaveChangesAsync();
        }

        public class Monarch
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string RulerOf { get; set; }
        }

        public class Magus
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Affiliation { get; set; }
            public MagicTool ToolUsed { get; set; }
        }

        public class MagicTool
        {
            public string Name { get; set; }
        }
    }

    #endregion

    #region 31107

    [ConditionalFact]
    public async Task Can_have_required_owned_type_on_derived_type()
    {
        var contextFactory = await InitializeAsync<Context31107>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        context.Set<Context31107.BaseEntity>().ToList();
    }

    private class Context31107(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseEntity>();
            modelBuilder.Entity<Child1Entity>(
                b =>
                {
                    b.OwnsOne(
                        entity => entity.Data, builder =>
                        {
                            builder.ToTable("Child1EntityData");
                            builder.WithOwner().HasForeignKey("Child1EntityId");
                        });
                    b.Navigation(e => e.Data).IsRequired();
                });

            modelBuilder.Entity<Child2Entity>();
        }

        public Task SeedAsync()
        {
            Add(new Child2Entity { Id = Guid.NewGuid() });

            return SaveChangesAsync();
        }

        public abstract class BaseEntity
        {
            public Guid Id { get; set; }
        }

        public sealed class ChildData
        {
            public Guid Id { get; set; }
        }

        public sealed class Child1Entity : BaseEntity
        {
            public ChildData Data { get; set; }
        }

        public sealed class Child2Entity : BaseEntity;
    }

    #endregion

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            c => c
                .Log(RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning)
                .Log(RelationalEventId.OptionalDependentWithAllNullPropertiesWarning));
}
