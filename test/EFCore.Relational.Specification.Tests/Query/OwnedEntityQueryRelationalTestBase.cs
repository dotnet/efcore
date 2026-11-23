// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OwnedEntityQueryRelationalTestBase(NonSharedFixture fixture) : OwnedEntityQueryTestBase(fixture)
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    #region 23198

    [Fact]
    public virtual async Task An_optional_dependent_without_any_columns_and_nested_dependent_throws()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => InitializeNonSharedTest<Context23198>())).Message;

        Assert.Equal(
            RelationalStrings.OptionalDependentWithDependentWithoutIdentifyingProperty(nameof(Context23198.AnOwnedTypeWithOwnedProperties)),
            message);
    }

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class Context23198(DbContextOptions options) : DbContext(options)
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
            public string Id { get; set; } = null!;
            public AnOwnedTypeWithOwnedProperties? AnOwnedTypeWithOwnedProperties { get; set; }
        }

        public class AnOwnedTypeWithOwnedProperties
        {
            public AnOwnedTypeWithPrimitiveProperties1? AnOwnedTypeWithPrimitiveProperties1 { get; set; }
            public AnOwnedTypeWithPrimitiveProperties2? AnOwnedTypeWithPrimitiveProperties2 { get; set; }
        }

        public class AnOwnedTypeWithPrimitiveProperties1
        {
            public string? Name { get; set; }
        }

        public class AnOwnedTypeWithPrimitiveProperties2
        {
            public string? Name { get; set; }
        }
    }

    #endregion

    #region 24777

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context24777>();
        using var context = contextFactory.CreateDbContext();

        var query = context.Roots.Where(e => e.Id == 3).AsSplitQuery();
        var root3 = async
            ? await query.SingleAsync()
            : query.Single();

        Assert.Equal(2, root3.ModdleA.Leaves.Count);
    }

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class Context24777(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Root> Roots { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Root>(b =>
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
            public ModdleA ModdleA { get; init; } = null!;
            public MiddleB? MiddleB { get; init; }
        }

        public class ModdleA
        {
            public int Id { get; init; }
            public int RootId { get; init; }
            public List<Leaf> Leaves { get; } = null!;
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

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_collection_basic_split_query(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context25680>();
        using var context = contextFactory.CreateDbContext();

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
                    b.HasKey(e => new
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
        public ICollection<PublishTokenType25680> PublishTokenTypes { get; set; } = null!;
    }

    protected class PublishTokenType25680
    {
        public Location25680 Location { get; set; } = null!;
        public Guid LocationId { get; set; }

        public string ExternalId { get; set; } = null!;
        public string VisualNumber { get; set; } = null!;
        public string TokenGroupId { get; set; } = null!;
        public string IssuerName { get; set; } = null!;
    }

    #endregion

    #region 26592

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_reference_mapped_to_different_table_updated_correctly_after_subquery_pushdown(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<MyContext26592>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        await base.Owned_references_on_same_level_expanded_at_different_times_around_take_helper(context, async);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_reference_mapped_to_different_table_nested_updated_correctly_after_subquery_pushdown(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<MyContext26592>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        await base.Owned_references_on_same_level_nested_expanded_at_different_times_around_take_helper(context, async);
    }

    protected class MyContext26592(DbContextOptions options) : MyContext26592Base(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(b =>
            {
                b.OwnsOne(e => e.CustomerData).ToTable("CustomerData");
                b.OwnsOne(e => e.SupplierData).ToTable("SupplierData");
            });

            modelBuilder.Entity<Owner>(b => b.OwnsOne(
                e => e.OwnedEntity, o =>
                {
                    o.ToTable("IntermediateOwnedEntity");
                    o.OwnsOne(e => e.CustomerData).ToTable("IM_CustomerData");
                    o.OwnsOne(e => e.SupplierData).ToTable("IM_SupplierData");
                }));
        }
    }

    #endregion

    #region 28347

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_materializes_when_not_containing_another_owned_entity(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context28247>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
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

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_entity_equality_when_not_containing_another_owned_entity(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context28247>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var query = context.RotRutCases.AsNoTracking().Select(e => e.Rot).Where(e => e != null);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal(1, t!.ServiceType);
                Assert.Equal("1", t.ApartmentNo);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_in_compared_to_null_in_conditional_projection(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context28247>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var query = context.RotRutCases
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(e => e.Rot == null
                ? null
                : new Context28247.RotDto { MyApartmentNo = e.Rot.ApartmentNo, MyServiceType = e.Rot.ServiceType });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal("1", t!.MyApartmentNo);
                Assert.Equal(1, t.MyServiceType);
            },
            Assert.Null);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_in_compared_to_non_null_in_conditional_projection(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context28247>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var query = context.RotRutCases
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(e => e.Rot != null
                ? new Context28247.RotDto { MyApartmentNo = e.Rot.ApartmentNo, MyServiceType = e.Rot.ServiceType }
                : null);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal("1", t!.MyApartmentNo);
                Assert.Equal(1, t.MyServiceType);
            },
            Assert.Null);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_with_all_null_properties_property_access_when_not_containing_another_owned_entity(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context28247>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var query = context.RotRutCases.AsNoTracking().Select(e => e.Rot!.ApartmentNo);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            result,
            t => Assert.Equal("1", t),
            Assert.Null);
    }

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class Context28247(DbContextOptions options) : DbContext(options)
    {
        public DbSet<RotRutCase> RotRutCases { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<RotRutCase>(b =>
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
            public string Buyer { get; set; } = null!;
            public Rot? Rot { get; set; }
            public Rut? Rut { get; set; }
        }

        public class Rot
        {
            public int? ServiceType { get; set; }
            public string? ApartmentNo { get; set; }
        }

        public class RotDto
        {
            public int? MyServiceType { get; set; }
            public string? MyApartmentNo { get; set; }
        }

        public class Rut
        {
            public int? Value { get; set; }
        }
    }

    #endregion

    #region 30358

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_selects_with_duplicating_aliases_and_owned_expansion_uniquifies_correctly(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context30358>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = from monarch in context.Monarchs
                    join magus in context.Magi.Where(x => x.Name.Contains("Bayaz")) on monarch.RulerOf equals magus.Affiliation
                    select new { monarch, magus };

        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Single(result);
        Assert.Equal("The Union", result[0].monarch.RulerOf);
        Assert.Equal("The Divider", result[0].magus.ToolUsed.Name);
    }

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class Context30358(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Monarch> Monarchs { get; set; } = null!;
        public DbSet<Magus> Magi { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Magus>().OwnsOne(x => x.ToolUsed, x => x.ToTable("MagicTools"));

        public Task SeedAsync()
        {
            Add(
                new Monarch
                {
                    Name = "His August Majesty Guslav the Fifth",
                    RulerOf = "The Union",
                });

            Add(
                new Monarch
                {
                    Name = "Emperor Uthman-ul-Dosht",
                    RulerOf = "The Gurkish Empire",
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
            public string Name { get; set; } = null!;
            public string RulerOf { get; set; } = null!;
        }

        public class Magus
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string Affiliation { get; set; } = null!;
            public MagicTool ToolUsed { get; set; } = null!;
        }

        public class MagicTool
        {
            public string Name { get; set; } = null!;
        }
    }

    #endregion

    #region 31107

    [Fact]
    public async Task Can_have_required_owned_type_on_derived_type()
    {
        var contextFactory = await InitializeNonSharedTest<Context31107>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();
        context.Set<Context31107.BaseEntity>().ToList();
    }

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class Context31107(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseEntity>();
            modelBuilder.Entity<Child1Entity>(b =>
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
            public ChildData Data { get; set; } = null!;
        }

        public sealed class Child2Entity : BaseEntity;
    }

    #endregion

    #region 38223

    [Fact]
    public virtual async Task Inconsistent_owned_entity_data_logs_warning_and_does_not_cause_identity_conflict()
    {
        var contextFactory = await InitializeNonSharedTest<Context38223>(
            shouldLogCategory: c => c == DbLoggerCategory.Query.Name,
            onConfiguring: b => b.ConfigureWarnings(c => c.Log(CoreEventId.InconsistentOwnedDataWarning)),
            seed: async c =>
            {
                // Insert a valid entity via EF Core, then corrupt Outer's required property to NULL to
                // create inconsistent data: Inner appears present but Outer's required property is null.
                var rootEntity = new Context38223.RootEntity
                {
                    Id = Guid.NewGuid(),
                    Outer = new Context38223.Outer { RequiredProperty = 1, Inner = new Context38223.Inner { InnerProperty = 42 } }
                };
                c.Add(rootEntity);
                await c.SaveChangesAsync();

                await c.Database.ExecuteSqlRawAsync(
                    "UPDATE RootEntity SET Outer_RequiredProperty = NULL WHERE Id = {0}", rootEntity.Id);
            });

        using var context = contextFactory.CreateDbContext();

        ListLoggerFactory.Clear();

        var root = await context.Set<Context38223.RootEntity>().SingleAsync();

        Assert.NotNull(root);
        Assert.Null(root.Outer);

        Assert.Contains(
            ListLoggerFactory.Log,
            l => l.Id == CoreEventId.InconsistentOwnedDataWarning && l.Level == LogLevel.Warning);

        // Replacing the owned entity should not throw an identity conflict exception
        root.Outer = new Context38223.Outer { RequiredProperty = 1, Inner = new Context38223.Inner { InnerProperty = 2 } };

        await context.SaveChangesAsync();
    }

    protected class Context38223(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<RootEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.OwnsOne(e => e.Outer, outer => outer.OwnsOne(o => o.Inner));
            });

        public class RootEntity
        {
            public Guid Id { get; set; }
            public Outer? Outer { get; set; }
        }

        public class Outer
        {
            public required int RequiredProperty { get; set; }
            public required Inner Inner { get; set; }
        }

        public class Inner
        {
            public required int InnerProperty { get; set; }
        }
    }

    #endregion

    #region 29593

    // The dependent lives in its own table, so expanding it into the grouped query would need a join added to a query that has
    // already been grouped. It keeps the correlated subquery it translates to today.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_on_owned_navigation_mapped_to_its_own_table(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context29593>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();
        ClearLog();

        var query = context.Set<Context29593.Invoice>()
            .GroupBy(e => e.Region)
            .Select(g => new { g.Key, Total = g.Sum(e => e.Summary!.Amount) });

        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Single(e => e.Key == "east").Total);
        Assert.Equal(12, result.Single(e => e.Key == "west").Total);
    }

    // The dependent's main fragment shares the owner's table, so it is expanded into the grouped query; the fragment holding
    // Tax is reached through a join that the grouped query can carry.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_on_owned_navigation_split_over_two_tables(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context29593>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();
        ClearLog();

        var query = context.Set<Context29593.Order>()
            .GroupBy(e => e.Region)
            .Select(g => new { g.Key, Total = g.Sum(e => e.Total!.Net), Tax = g.Sum(e => e.Total!.Tax) });

        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Single(e => e.Key == "east").Total);
        Assert.Equal(30, result.Single(e => e.Key == "east").Tax);
        Assert.Equal(12, result.Single(e => e.Key == "west").Total);
        Assert.Equal(120, result.Single(e => e.Key == "west").Tax);
    }

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class Context29593(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>().OwnsOne(e => e.Summary, b => b.ToTable("Summaries"));
            modelBuilder.Entity<Order>().OwnsOne(
                e => e.Total, b => b.SplitToTable("OrderTotalTax", t => t.Property(p => p.Tax)));
        }

        public Task SeedAsync()
        {
            AddRange(
                new Invoice { Region = "east", Summary = new Summary { Amount = 1 } },
                new Invoice { Region = "east", Summary = new Summary { Amount = 2 } },
                new Invoice { Region = "west", Summary = new Summary { Amount = 12 } });

            AddRange(
                new Order { Region = "east", Total = new Total { Net = 1, Tax = 10 } },
                new Order { Region = "east", Total = new Total { Net = 2, Tax = 20 } },
                new Order { Region = "west", Total = new Total { Net = 12, Tax = 120 } });

            return SaveChangesAsync();
        }

        public class Invoice
        {
            public int Id { get; set; }
            public string Region { get; set; } = null!;
            public Summary? Summary { get; set; }
        }

        public class Summary
        {
            public int Amount { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public string Region { get; set; } = null!;
            public Total? Total { get; set; }
        }

        public class Total
        {
            public int Net { get; set; }
            public int Tax { get; set; }
        }
    }

    #endregion

    #region 35862

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_collection_select_many_with_result_selector(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context35862>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context35862.DocumentTest>()
            .SelectMany(x => x.Versions, (d, v) => new { d, v });
        var result = async
            ? await query.ToListAsync()
            : query.ToList();
        Assert.Equal(result.Count, 2);
        Assert.Collection(
            result.OrderBy(t => t.v.Number),
            t =>
            {
                Assert.Equal("Foo", t.d.PrintMargin.Foo);
                Assert.Equal(3, t.v.Number);
            },
            t =>
            {
                Assert.Equal("Foo", t.d.PrintMargin.Foo);
                Assert.Equal(4, t.v.Number);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_collection_select_many_without_result_selector(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context35862>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context35862.DocumentTest>()
            .SelectMany(x => x.Versions.Select(y => new { d = x, v = y }));
        var result = async
            ? await query.ToListAsync()
            : query.ToList();
        Assert.Equal(result.Count, 2);
        Assert.Collection(
            result.OrderBy(t => t.v.Number),
            t =>
            {
                Assert.Equal("Foo", t.d.PrintMargin.Foo);
                Assert.Equal(3, t.v.Number);
            },
            t =>
            {
                Assert.Equal("Foo", t.d.PrintMargin.Foo);
                Assert.Equal(4, t.v.Number);
            });
    }

    //When owned entity with separate table is projected with CROSS APPLY,
    //don't reference the separate table column inside CROSS APPLY subquery
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_collection_select_many_without_result_selector_owned_in_separate_table(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context35862SeparateTable>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context35862SeparateTable.DocumentTest>()
            .SelectMany(x => x.Versions.Select(y => new { d = x, v = y }));
        var result = async
            ? await query.ToListAsync()
            : query.ToList();
        Assert.Equal(2, result.Count);
        Assert.Collection(
            result.OrderBy(t => t.v.Number),
            t =>
            {
                Assert.Equal("Foo", t.d.PrintMargin.Foo);
                Assert.Equal(3, t.v.Number);
            },
            t =>
            {
                Assert.Equal("Foo", t.d.PrintMargin.Foo);
                Assert.Equal(4, t.v.Number);
            });
    }

    protected class Context35862SeparateTable(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentTest>().OwnsOne(d => d.PrintMargin, b => b.ToTable("PrintMargins"));
        }

        public Task SeedAsync()
        {
            Add(new DocumentTest
            {
                Versions = new() { new VersionTest { Number = 3 }, new VersionTest { Number = 4 } },
                PrintMargin = new OwnedThing { Foo = "Foo" },
            });

            return SaveChangesAsync();
        }

        public class DocumentTest
        {
            public int Id { get; init; }
            public List<VersionTest> Versions { get; init; } = new List<VersionTest>();
            public OwnedThing PrintMargin { get; init; } = null!;
        }

        public class VersionTest
        {
            public int Id { get; init; }
            public int Number { get; init; }
        }

        public class OwnedThing
        {
            public string Foo { get; set; } = null!;
        }
    }

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class Context35862(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentTest>().OwnsOne(d => d.PrintMargin);
        }

        public Task SeedAsync()
        {
            Add(new DocumentTest
            {
                Versions = new() { new VersionTest { Number = 3 }, new VersionTest { Number = 4 } },
                PrintMargin = new OwnedThing { Foo = "Foo" },
            });

            return SaveChangesAsync();
        }

        public class DocumentTest
        {
            public int Id { get; init; }
            public List<VersionTest> Versions { get; init; } = new List<VersionTest>();
            public OwnedThing PrintMargin { get; init; } = null!;
        }

        public class VersionTest
        {
            public int Id { get; init; }
            public int Number { get; init; }
        }
        public class OwnedThing
        {
            public string Foo { get; set; } = null!;
        }
    }

    #endregion

    #region 30458
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_where_before_include (bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context30458>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context30458.BaseMyClass>()
                .Where(x => x.Id == 1 && x.Information.Boolean)
                .Include(x => x.Information.Nav) ;

        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Single(result);
        Assert.Equal(true, result[0].Information.Boolean);
        Assert.NotNull(result[0].Information.Nav);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_entity_where_after_include (bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context30458>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context30458.BaseMyClass>()
                .Include(x => x.Information.Nav)
                .Where(x => x.Id == 1 && x.Information.Boolean);

        var result = async ? await query.ToListAsync() : query.ToList();
        Assert.Single(result);
        Assert.Equal(true, result[0].Information.Boolean);
        Assert.NotNull(result[0].Information.Nav);
    }

    protected class Context30458(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseMyClass>().OwnsOne(e => e.Information);
        }

        public Task SeedAsync()
        {
            Add(new BaseMyClass
            {
                BaseEntityProperty = true,
                Information = new Information
                {
                    Boolean = true,
                    Nav = new()
                }
            });
            Add(new BaseMyClass
            {
                BaseEntityProperty = true,
                Information = new Information
                {
                    Boolean = false,
                    Nav = new()
                }
            });
            return SaveChangesAsync();
        }

        public class BaseMyClass
        {
            public int Id { get; set; }
            public bool BaseEntityProperty { get; set; }
            public Information Information { get; set; } = null!;
        }

        public class Information
        {
            public bool Boolean { get; set; }
            public Nav Nav { get; set; } = null!;
        }

        public class Nav
        {
            public int Id { get; set; }
        }
    }

    #endregion

    #region Complex properties on owned types

    [Fact(Skip = "Issue #35701")]
    public virtual async Task Optional_owned_reference_with_only_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: mb =>
            {
                mb.Ignore<ContextOwnedComplex.Owner>();
                mb.Entity<ContextOwnedComplex.OwnerOfOptional>(b => b.OwnsOne(e => e.Holder, ob => ob.ComplexProperty(o => o.Address)));
            },
            seed: context =>
            {
                context.AddRange(
                    new ContextOwnedComplex.OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new ContextOwnedComplex.AddressHolder
                        {
                            Address = ContextOwnedComplex.CreateOwned(string.Empty, "1 Main St", "Sydney", "Australia", "AU").Address
                        }
                    },
                    new ContextOwnedComplex.OwnerOfOptional { Id = 2 });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Set<ContextOwnedComplex.OwnerOfOptional>().OrderBy(e => e.Id).ToListAsync();

        Assert.Collection(
            owners,
            o => Assert.Equal("Sydney", o.Holder!.Address.City),
            o => Assert.Null(o.Holder));
    }

    [Fact]
    public virtual Task Update_complex_property_of_owned_reference()
        => AssertUpdateOwnedReference(ConfigureOwnedReference, nested: true);

    [Fact]
    public virtual Task Update_complex_property_of_owned_collection_element()
        => AssertUpdateOwnedCollectionElement(
            mb => mb.Entity<ContextOwnedComplex.Owner>(b =>
            {
                b.Ignore(e => e.Owned);
                b.OwnsMany(
                    e => e.OwnedCollection, ob =>
                    {
                        ob.HasKey("OwnerId", nameof(ContextOwnedComplex.Owned.Name));
                        ob.ComplexProperty(o => o.Address);
                    });
            }));

    [Fact]
    public virtual async Task Replace_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureOwnedReference,
            seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned.Address = ContextOwnedComplex.CreateOwned(string.Empty, "5 Queen St", "Brisbane", "Australia", "AU").Address;

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("5 Queen St", owner.Owned.Address.Street);
            Assert.Equal("Brisbane", owner.Owned.Address.City);
        }
    }

    [Fact]
    public virtual async Task Replace_owned_reference_with_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureOwnedReference,
            seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned = ContextOwnedComplex.CreateOwned("Owned1New", "5 Queen St", "Brisbane", "Australia", "AU");

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("Owned1New", owner.Owned.Name);
            Assert.Equal("Brisbane", owner.Owned.Address.City);
        }
    }

    [Fact]
    public virtual async Task Delete_owner_with_owned_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureOwnedReference,
            seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateDbContext())
        {
            context.Remove(await context.Owners.SingleAsync(e => e.Id == 1));

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            Assert.Equal(2, (await context.Owners.SingleAsync()).Id);
        }
    }

    [Fact]
    public virtual async Task Owned_reference_separate_table()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureSeparateTableOwnedReference,
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_separate_table_with_key_on_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureSeparateTableOwnedReferenceWithKeyOnComplexProperty,
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_with_key_on_nested_complex_property_equals_parameter()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureSeparateTableOwnedReferenceWithKeyOnComplexProperty,
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var owned = ContextOwnedComplex.CreateOwned("Owned2", "2 Hauptstrasse", "Berlin", "Germany", "DE");

        var owner = await context.Owners.SingleAsync(e => e.Owned == owned);

        Assert.Equal(2, owner.Id);
    }

    [Fact]
    public virtual async Task Owned_reference_with_key_on_nested_complex_property_in_local_collection()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureSeparateTableOwnedReferenceWithKeyOnComplexProperty,
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var owned = new[] { ContextOwnedComplex.CreateOwned("Owned2", "2 Hauptstrasse", "Berlin", "Germany", "DE") };

        var owner = await context.Owners.SingleAsync(e => owned.Contains(e.Owned));

        Assert.Equal(2, owner.Id);
    }

    [Fact]
    public virtual async Task Owned_reference_separate_table_with_key_on_complex_property_and_nested_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: mb =>
            {
                mb.Ignore<ContextOwnedComplex.Owner>();
                mb.Entity<ContextOwnedComplex.OwnerOfNested>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToTable("Owned");
                        ob.ComplexProperty(o => o.Address);
                        ob.HasKey("Address.Country.Code");
                        ob.OwnsOne(o => o.City);
                    }));
            },
            seed: context =>
            {
                context.Add(
                    new ContextOwnedComplex.OwnerOfNested
                    {
                        Id = 1,
                        Owned = new ContextOwnedComplex.NestedOwned
                        {
                            Address = ContextOwnedComplex.CreateOwned(string.Empty, "1 Main St", "Sydney", "Australia", "AU").Address,
                            City = new ContextOwnedComplex.City { Name = "Sydney" }
                        }
                    });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owner = await context.Set<ContextOwnedComplex.OwnerOfNested>().SingleAsync();

        Assert.Equal("AU", owner.Owned.Address.Country.Code);
        Assert.Equal("Sydney", owner.Owned.City.Name);
    }

    [Fact]
    public virtual async Task Owned_reference_json()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureJsonOwnedReference,
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual Task Update_complex_property_of_owned_reference_separate_table()
        => AssertUpdateOwnedReference(ConfigureSeparateTableOwnedReference, nested: true);

    [Fact]
    public virtual Task Update_json_complex_property_of_owned_reference()
        => AssertUpdateOwnedReference(ConfigureOwnedReferenceWithJsonComplexProperty, nested: false);

    [Fact]
    public virtual Task Update_complex_property_of_json_owned_reference()
        => AssertUpdateOwnedReference(ConfigureJsonOwnedReference, nested: false);

    [Fact]
    public virtual Task Update_complex_properties_of_json_owned_reference()
        => AssertUpdateOwnedReference(ConfigureJsonOwnedReference, nested: true);

    [Fact]
    public virtual Task Update_complex_property_and_nested_owned_reference_with_same_json_name()
        => AssertUpdateComplexPropertyAndNestedOwnedReference(configureJsonNames: false);

    [Fact]
    public virtual Task Update_complex_property_and_nested_owned_reference_with_same_configured_json_name()
        => AssertUpdateComplexPropertyAndNestedOwnedReference(configureJsonNames: true);

    [Fact]
    public virtual async Task Complex_property_of_owned_reference_after_pushdown()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureOwnedReference,
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners
            .Where(e => e.Owned.Name != "X")
            .OrderBy(e => e.Id)
            .Take(5)
            .Where(e => e.Owned.Address.City == "Berlin")
            .ToListAsync();

        Assert.Equal(2, Assert.Single(owners).Id);
    }

    [Fact]
    public virtual async Task Filter_on_json_complex_property_of_owned_reference_in_left_joined_subquery()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureOwnedReferenceWithJsonComplexProperty,
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();

        // The owner of the second row has no next owner, so its city is null and isn't Berlin
        var ids = await (from o in context.Owners
                         join n in context.Owners.OrderBy(e => e.Id).Take(5) on o.Id + 1 equals n.Id into next
                         from n in next.DefaultIfEmpty()
                         where n!.Owned.Address.City != "Berlin"
                         select o.Id).ToListAsync();

        Assert.Equal([2], ids);
    }

    [Fact]
    public virtual async Task Nested_complex_property_of_optional_owned_reference_after_pushdown()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: mb =>
            {
                mb.Ignore<ContextOwnedComplex.Owner>();
                mb.Entity<ContextOwnedComplex.OwnerOfOptional>(b => b.OwnsOne(
                    e => e.Holder, ob =>
                    {
                        ob.Property(o => o.Note).IsRequired();
                        ob.ComplexProperty(o => o.Address);
                    }));
            },
            seed: context =>
            {
                context.AddRange(
                    new ContextOwnedComplex.OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new ContextOwnedComplex.AddressHolder
                        {
                            Note = "Note1",
                            Address = ContextOwnedComplex.CreateOwned(string.Empty, "1 Main St", "Sydney", "Australia", "AU").Address
                        }
                    },
                    new ContextOwnedComplex.OwnerOfOptional
                    {
                        Id = 2,
                        Holder = new ContextOwnedComplex.AddressHolder
                        {
                            Note = "Note2",
                            Address = ContextOwnedComplex.CreateOwned(string.Empty, "2 Hauptstrasse", "Berlin", "Germany", "DE").Address
                        }
                    },
                    new ContextOwnedComplex.OwnerOfOptional { Id = 3 });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var countries = await context.Set<ContextOwnedComplex.OwnerOfOptional>()
            .OrderBy(e => e.Id)
            .Take(5)
            .Where(e => e.Id > 0)
            .Select(e => (string?)e.Holder!.Address.Country.Name)
            .ToListAsync();

        Assert.Equal(["Australia", "Germany", null], countries);

        var codes = await context.Set<ContextOwnedComplex.OwnerOfOptional>()
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Take(5)
            .Distinct()
            .Where(e => e.Holder!.Address.Country.Code != "DE")
            .OrderBy(e => e.Id)
            .Select(e => (string?)e.Holder!.Address.Country.Code)
            .ToListAsync();

        Assert.Equal(["AU", null], codes);
    }

    [Fact]
    public virtual async Task Remove_optional_owned_reference_with_json_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: mb =>
            {
                mb.Ignore<ContextOwnedComplex.Owner>();
                mb.Entity<ContextOwnedComplex.OwnerOfOptional>(b => b.OwnsOne(
                    e => e.Holder, ob =>
                    {
                        ob.Property(o => o.Note).IsRequired();
                        ob.ComplexProperty(o => o.Address, ab => ab.ToJson());
                    }));
            },
            seed: context =>
            {
                context.AddRange(
                    new ContextOwnedComplex.OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new ContextOwnedComplex.AddressHolder
                        {
                            Note = "Note1",
                            Address = ContextOwnedComplex.CreateOwned(string.Empty, "1 Main St", "Sydney", "Australia", "AU").Address
                        }
                    },
                    new ContextOwnedComplex.OwnerOfOptional
                    {
                        Id = 2,
                        Holder = new ContextOwnedComplex.AddressHolder
                        {
                            Note = "Note2",
                            Address = ContextOwnedComplex.CreateOwned(string.Empty, "2 Hauptstrasse", "Berlin", "Germany", "DE").Address
                        }
                    });

                return context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            (await context.Set<ContextOwnedComplex.OwnerOfOptional>().SingleAsync(e => e.Id == 1)).Holder = null;
            context.Add(new ContextOwnedComplex.OwnerOfOptional { Id = 3 });

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var addresses = await context.Set<ContextOwnedComplex.OwnerOfOptional>()
                .OrderBy(e => e.Id)
                .Select(e => e.Holder!.Address)
                .ToListAsync();

            Assert.Collection(
                addresses,
                Assert.Null,
                a => Assert.Equal("Berlin", a.City),
                Assert.Null);
        }
    }

    [Fact]
    public virtual async Task Update_scalar_and_complex_collection_of_json_owned_reference()
    {
        var contextFactory = await InitializeTaggedOwners(ob => ob.ToJson(), configureTag: null, ConfigureTagConverters);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);
            owner.Owned.Tag.Label = "Uno";
            owner.Owned.Tags.Add(new Tag { Id = new TagId(11), Label = "Eleven", Code = new TagCode("E") });

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);

            Assert.Equal("Uno", owner.Owned.Tag.Label);
            Assert.Equal(["Ten", "Eleven"], owner.Owned.Tags.Select(t => t.Label));
        }
    }

    [Fact]
    public virtual Task Set_nullable_complex_property_of_json_owned_reference()
        => AssertUpdateNullableComplexProperty(
            initialValue: null,
            o => o.Outer = new ContextOwnedComplex.OuterComplex
            {
                Inner = new ContextOwnedComplex.InnerComplex { Value = "New", Flag = true }
            },
            expectedValue: "New");

    [Fact]
    public virtual Task Clear_nullable_complex_property_of_json_owned_reference()
        => AssertUpdateNullableComplexProperty(initialValue: "Old", o => o.Outer = null, expectedValue: null);

    [Fact]
    public virtual Task Update_property_in_nullable_complex_property_of_json_owned_reference()
        => AssertUpdateNullableComplexProperty(initialValue: "Old", o => o.Outer!.Inner.Value = "New", expectedValue: "New");

    [Fact]
    public virtual Task Clear_nullable_bool_in_nullable_complex_property_of_json_owned_reference()
        => AssertUpdateNullableComplexProperty(
            initialValue: "Old", o => o.Outer!.Inner.Flag = null, expectedValue: "Old", expectedFlag: null);

    [Fact(Skip = "Issue #35701")]
    public virtual async Task Optional_owned_reference_with_null_scalars_and_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: mb =>
            {
                mb.Ignore<ContextOwnedComplex.Owner>();
                mb.Entity<ContextOwnedComplex.OwnerOfOptional>(b => b.OwnsOne(e => e.Holder, ob => ob.ComplexProperty(o => o.Address)));
            },
            seed: context =>
            {
                context.Add(
                    new ContextOwnedComplex.OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new ContextOwnedComplex.AddressHolder
                        {
                            Address = ContextOwnedComplex.CreateOwned(string.Empty, "1 Main St", "Sydney", "Australia", "AU").Address
                        }
                    });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owner = await context.Set<ContextOwnedComplex.OwnerOfOptional>().SingleAsync();

        Assert.Null(owner.Holder!.Note);
        Assert.Equal("Sydney", owner.Holder.Address.City);
    }

    [Fact]
    public virtual Task Value_converter_in_complex_type_on_json_owned_reference()
        => AssertValueConverterInComplexType(ob => ob.ToJson(), configureTag: null);

    [Fact]
    public virtual Task Value_converter_in_json_complex_type_on_owned_reference()
        => AssertValueConverterInComplexType(configureOwned: null, tb => tb.ToJson());

    [Fact]
    public virtual Task Value_comparer_in_complex_type_on_json_owned_reference()
        => AssertValueComparerInComplexType(ob => ob.ToJson(), configureTag: null);

    [Fact]
    public virtual Task Value_comparer_in_json_complex_type_on_owned_reference()
        => AssertValueComparerInComplexType(configureOwned: null, tb => tb.ToJson());

    [Fact]
    public virtual Task Json_value_reader_writer_in_complex_type_on_json_owned_reference()
        => AssertJsonValueReaderWriterInComplexType(ob => ob.ToJson(), configureTag: null);

    [Fact]
    public virtual Task Json_value_reader_writer_in_json_complex_type_on_owned_reference()
        => AssertJsonValueReaderWriterInComplexType(configureOwned: null, tb => tb.ToJson());

    [Fact]
    public virtual async Task Delete_owner_with_owned_complex_property_separate_table()
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: ConfigureSeparateTableOwnedReference,
            seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateDbContext())
        {
            context.Remove(await context.Owners.SingleAsync(e => e.Id == 1));

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            Assert.Equal(2, (await context.Owners.SingleAsync()).Id);
        }
    }

    private async Task AssertUpdateOwnedReference(Action<ModelBuilder> onModelCreating, bool nested)
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(onModelCreating: onModelCreating, seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned.Address.City = "Melbourne";
            if (nested)
            {
                owner.Owned.Address.Country.Name = "Oz";
            }

            context.ChangeTracker.DetectChanges();
            var addressEntry = context.Entry(owner.Owned).ComplexProperty(o => o.Address);
            Assert.True(addressEntry.IsModified);
            Assert.Equal("Sydney", addressEntry.Property(a => a.City).OriginalValue);
            Assert.Equal(nested, addressEntry.ComplexProperty(a => a.Country).Property(c => c.Name).IsModified);

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("Melbourne", owner.Owned.Address.City);
            Assert.Equal(nested ? "Oz" : "Australia", owner.Owned.Address.Country.Name);
            Assert.Equal("1 Main St", owner.Owned.Address.Street);
        }
    }

    private async Task AssertUpdateOwnedCollectionElement(Action<ModelBuilder> onModelCreating)
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(onModelCreating: onModelCreating, seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.OwnedCollection.Single(e => e.Name == "Owned1b").Address.City = "Fremantle";

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal(
                ["Sydney", "Fremantle"],
                owner.OwnedCollection.OrderBy(e => e.Name).Select(e => e.Address.City));
        }
    }

    private static void ConfigureOwnedReference(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextOwnedComplex.Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address));
        });

    private static void AssertOwners(List<ContextOwnedComplex.Owner> owners)
        => Assert.Collection(
            owners,
            o =>
            {
                Assert.Equal("Owned1", o.Owned.Name);
                Assert.Equal("1 Main St", o.Owned.Address.Street);
                Assert.Equal("Sydney", o.Owned.Address.City);
                Assert.Equal("Australia", o.Owned.Address.Country.Name);
                Assert.Equal("AU", o.Owned.Address.Country.Code);
            },
            o =>
            {
                Assert.Equal("Owned2", o.Owned.Name);
                Assert.Equal("2 Hauptstrasse", o.Owned.Address.Street);
                Assert.Equal("Berlin", o.Owned.Address.City);
                Assert.Equal("Germany", o.Owned.Address.Country.Name);
                Assert.Equal("DE", o.Owned.Address.Country.Code);
            });

    private static void ConfigureSeparateTableOwnedReference(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextOwnedComplex.Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(
                e => e.Owned, ob =>
                {
                    ob.ToTable("Owned");
                    ob.ComplexProperty(o => o.Address);
                });
        });

    private static void ConfigureSeparateTableOwnedReferenceWithKeyOnComplexProperty(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextOwnedComplex.Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(
                e => e.Owned, ob =>
                {
                    ob.ToTable("Owned");
                    ob.ComplexProperty(o => o.Address);
                    ob.HasKey("Address.Country.Code");
                });
        });

    private static void ConfigureOwnedReferenceWithJsonComplexProperty(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextOwnedComplex.Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson()));
        });

    private static void ConfigureJsonOwnedReference(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextOwnedComplex.Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(
                e => e.Owned, ob =>
                {
                    ob.ToJson();
                    ob.ComplexProperty(o => o.Address);
                });
        });

    // Address.City and the nested City navigation share a JSON name at different depths of the owned document
    private async Task AssertUpdateComplexPropertyAndNestedOwnedReference(bool configureJsonNames)
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: mb =>
            {
                mb.Ignore<ContextOwnedComplex.Owner>();
                mb.Entity<ContextOwnedComplex.OwnerOfNested>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToJson();
                        ob.ComplexProperty(
                            o => o.Address, ab =>
                            {
                                if (configureJsonNames)
                                {
                                    ab.Property(a => a.City).HasJsonPropertyName("Place");
                                }
                            });
                        ob.OwnsOne(
                            o => o.City, cb =>
                            {
                                if (configureJsonNames)
                                {
                                    cb.HasJsonPropertyName("Place");
                                }
                            });
                    }));
            },
            seed: context =>
            {
                context.Add(
                    new ContextOwnedComplex.OwnerOfNested
                    {
                        Id = 1,
                        Owned = new ContextOwnedComplex.NestedOwned
                        {
                            Address = ContextOwnedComplex.CreateOwned(string.Empty, "1 Main St", "Sydney", "Australia", "AU").Address,
                            City = new ContextOwnedComplex.City { Name = "Sydney" }
                        }
                    });

                return context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<ContextOwnedComplex.OwnerOfNested>().SingleAsync();
            owner.Owned.Address.City = "Melbourne";
            owner.Owned.City.Name = "Brisbane";

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<ContextOwnedComplex.OwnerOfNested>().SingleAsync();

            Assert.Equal("Melbourne", owner.Owned.Address.City);
            Assert.Equal("Brisbane", owner.Owned.City.Name);
        }
    }

    // Changing a nullable complex property between null and a value changes its JSON parent, so it can't be patched by property
    private async Task AssertUpdateNullableComplexProperty(
        string? initialValue,
        Action<ContextOwnedComplex.OwnedWithNullableComplex> change,
        string? expectedValue,
        bool? expectedFlag = true)
    {
        var contextFactory = await InitializeNonSharedTest<ContextOwnedComplex>(
            onModelCreating: mb =>
            {
                mb.Ignore<ContextOwnedComplex.Owner>();
                mb.Entity<ContextOwnedComplex.OwnerOfNullableComplex>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToJson();
                        ob.ComplexProperty(o => o.Outer, cb => cb.ComplexProperty(c => c.Inner));
                    }));
            },
            seed: context =>
            {
                context.Add(
                    new ContextOwnedComplex.OwnerOfNullableComplex
                    {
                        Id = 1,
                        Owned = new ContextOwnedComplex.OwnedWithNullableComplex
                        {
                            Name = "Owned",
                            Outer = initialValue == null
                                ? null
                                : new ContextOwnedComplex.OuterComplex
                                {
                                    Inner = new ContextOwnedComplex.InnerComplex { Value = initialValue, Flag = true }
                                }
                        }
                    });

                return context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            change((await context.Set<ContextOwnedComplex.OwnerOfNullableComplex>().SingleAsync()).Owned);

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owned = (await context.Set<ContextOwnedComplex.OwnerOfNullableComplex>().SingleAsync()).Owned;

            Assert.Equal("Owned", owned.Name);
            Assert.Equal(expectedValue, owned.Outer?.Inner.Value);
            Assert.Equal(expectedValue == null ? null : expectedFlag, owned.Outer?.Inner.Flag);
        }
    }

    private async Task AssertValueConverterInComplexType(
        Action<OwnedNavigationBuilder<OwnerWithTag, TaggedOwned>>? configureOwned,
        Action<ComplexPropertyBuilder<Tag>>? configureTag)
    {
        var contextFactory = await InitializeTaggedOwners(
            configureOwned,
            tb =>
            {
                configureTag?.Invoke(tb);
                tb.PrimitiveCollection(t => t.RelatedIds).ElementType().HasConversion<TagIdConverter>();
            },
            c =>
            {
                c.Properties<TagId>().HaveConversion<TagIdConverter>();
                c.Properties<TagCode>().HaveConversion<TagCodeConverter>();
            },
            tb => tb.PrimitiveCollection(t => t.RelatedIds).ElementType().HasConversion<TagIdConverter>());

        using (var context = contextFactory.CreateDbContext())
        {
            var owners = await context.Set<OwnerWithTag>().OrderBy(e => e.Id).ToListAsync();
            Assert.Equal([new TagId(1), new TagId(2)], owners.Select(o => o.Owned.Tag.Id));
            Assert.Equal([new TagCode("A"), new TagCode("B")], owners.Select(o => o.Owned.Tag.Code));
            Assert.Equal([new TagId(5), new TagId(6)], owners[0].Owned.Tag.RelatedIds);
            Assert.Equal([new TagId(10), new TagId(20)], owners.Select(o => o.Owned.Tags.Single().Id));

            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Owned.Tag.Id == new TagId(2));
            Assert.Equal(2, owner.Id);

            owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Owned.Tag.Code == new TagCode("A"));
            Assert.Equal(1, owner.Id);

            var ids = await context.Set<OwnerWithTag>().OrderBy(e => e.Id).Select(e => e.Owned.Tag.Id).ToListAsync();
            Assert.Equal([new TagId(1), new TagId(2)], ids);

            owners[0].Owned.Tag.Id = new TagId(3);
            owners[0].Owned.Tag.RelatedIds.Add(new TagId(7));
            owners[0].Owned.Tags[0].Id = new TagId(30);

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Owned.Tag.Id == new TagId(3));
            Assert.Equal(1, owner.Id);
            Assert.Equal([new TagId(5), new TagId(6), new TagId(7)], owner.Owned.Tag.RelatedIds);
            Assert.Equal(new TagId(30), owner.Owned.Tags.Single().Id);
        }
    }

    private async Task AssertValueComparerInComplexType(
        Action<OwnedNavigationBuilder<OwnerWithTag, TaggedOwned>>? configureOwned,
        Action<ComplexPropertyBuilder<Tag>>? configureTag)
    {
        var contextFactory = await InitializeTaggedOwners(
            configureOwned,
            configureTag,
            c =>
            {
                c.Properties<TagId>().HaveConversion<TagIdConverter>();
                c.Properties<TagCode>().HaveConversion<TagCodeConverter, TagCodeComparer>();
                c.Properties<List<TagId>>().HaveConversion<TagIdListConverter, TagIdListComparer>();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);

            // Only differs in case, so the comparer treats it as unchanged
            owner.Owned.Tag.Code = new TagCode("a");
            owner.Owned.Tags[0].Code = new TagCode("a");
            context.ChangeTracker.DetectChanges();
            Assert.False(context.Entry(owner.Owned).ComplexProperty(o => o.Tag).Property(t => t.Code).IsModified);
            Assert.Equal(EntityState.Unchanged, context.Entry(owner.Owned).State);

            ClearLog();
            Assert.Equal(0, await context.SaveChangesAsync());
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);

            // Mutated in place, so only detected by comparing with the comparer's snapshot
            owner.Owned.Tag.RelatedIds.Add(new TagId(7));
            context.ChangeTracker.DetectChanges();
            Assert.True(context.Entry(owner.Owned).ComplexProperty(o => o.Tag).Property(t => t.RelatedIds).IsModified);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);
            Assert.Equal(new TagCode("A"), owner.Owned.Tag.Code);
            Assert.Equal([new TagId(5), new TagId(6), new TagId(7)], owner.Owned.Tag.RelatedIds);
        }
    }

    private async Task AssertJsonValueReaderWriterInComplexType(
        Action<OwnedNavigationBuilder<OwnerWithTag, TaggedOwned>>? configureOwned,
        Action<ComplexPropertyBuilder<Tag>>? configureTag)
    {
        var contextFactory = await InitializeTaggedOwners(
            configureOwned,
            configureTag,
            ConfigureTagConverters,
            onModelCreating: mb =>
            {
                var owned = mb.Model.FindEntityType(typeof(OwnerWithTag))!.FindNavigation(nameof(OwnerWithTag.Owned))!.TargetEntityType;
                foreach (var complexProperty in owned.GetComplexProperties())
                {
                    complexProperty.ComplexType.FindProperty(nameof(Tag.Code))!.SetJsonValueReaderWriterType(typeof(TagCodeReaderWriter));
                }
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var owners = await context.Set<OwnerWithTag>().OrderBy(e => e.Id).ToListAsync();
            Assert.Equal([new TagCode("A"), new TagCode("B")], owners.Select(o => o.Owned.Tag.Code));
            Assert.Equal([new TagCode("A"), new TagCode("B")], owners.Select(o => o.Owned.Tags.Single().Code));

            owners[0].Owned.Tag.Code = new TagCode("C");

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);
            Assert.Equal(new TagCode("C"), owner.Owned.Tag.Code);
        }
    }

    private static void ConfigureTagConverters(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TagId>().HaveConversion<TagIdConverter>();
        configurationBuilder.Properties<TagCode>().HaveConversion<TagCodeConverter>();
        configurationBuilder.Properties<List<TagId>>().HaveConversion<TagIdListConverter, TagIdListComparer>();
    }

    private Task<ContextFactory<DbContext>> InitializeTaggedOwners(
        Action<OwnedNavigationBuilder<OwnerWithTag, TaggedOwned>>? configureOwned,
        Action<ComplexPropertyBuilder<Tag>>? configureTag,
        Action<ModelConfigurationBuilder> configureConventions,
        Action<ComplexCollectionBuilder<Tag>>? configureTags = null,
        Action<ModelBuilder>? onModelCreating = null)
        => InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<OwnerWithTag>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        configureOwned?.Invoke(ob);
                        ob.ComplexProperty(o => o.Tag, tb => configureTag?.Invoke(tb));
                        ob.ComplexCollection(
                            o => o.Tags, tb =>
                            {
                                // In relational, complex collections are only supported as JSON
                                if (!tb.Metadata.DeclaringType.IsMappedToJson())
                                {
                                    tb.ToJson();
                                }

                                configureTags?.Invoke(tb);
                            });
                    }));
                onModelCreating?.Invoke(mb);
            },
            configureConventions: configureConventions,
            seed: context =>
            {
                context.AddRange(
                    new OwnerWithTag
                    {
                        Id = 1,
                        Owned = new TaggedOwned
                        {
                            Tag = new Tag
                            {
                                Id = new TagId(1),
                                Label = "One",
                                Code = new TagCode("A"),
                                RelatedIds = [new TagId(5), new TagId(6)]
                            },
                            Tags = [new Tag { Id = new TagId(10), Label = "Ten", Code = new TagCode("A") }]
                        }
                    },
                    new OwnerWithTag
                    {
                        Id = 2,
                        Owned = new TaggedOwned
                        {
                            Tag = new Tag
                            {
                                Id = new TagId(2),
                                Label = "Two",
                                Code = new TagCode("B")
                            },
                            Tags = [new Tag { Id = new TagId(20), Label = "Twenty", Code = new TagCode("B") }]
                        }
                    });

                return context.SaveChangesAsync();
            });

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class ContextOwnedComplex(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Owner> Owners { get; set; } = null!;

        public Task SeedAsync()
        {
            Owners.AddRange(
                new Owner
                {
                    Id = 1,
                    Name = "Owner1",
                    Owned = CreateOwned("Owned1", "1 Main St", "Sydney", "Australia", "AU"),
                    OwnedCollection =
                    [
                        CreateOwned("Owned1a", "1 Main St", "Sydney", "Australia", "AU"),
                        CreateOwned("Owned1b", "3 Hay St", "Perth", "Australia", "AU")
                    ]
                },
                new Owner
                {
                    Id = 2,
                    Name = "Owner2",
                    Owned = CreateOwned("Owned2", "2 Hauptstrasse", "Berlin", "Germany", "DE"),
                    OwnedCollection = [CreateOwned("Owned2a", "2 Hauptstrasse", "Berlin", "Germany", "DE")]
                });

            return SaveChangesAsync();
        }

        public static Owned CreateOwned(string name, string street, string city, string country, string code)
            => new()
            {
                Name = name,
                Address = new Address
                {
                    Street = street,
                    City = city,
                    Country = new Country { Name = country, Code = code }
                }
            };

        public class Owner
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Name { get; set; } = null!;
            public Owned Owned { get; set; } = null!;
            public List<Owned> OwnedCollection { get; set; } = [];
        }

        public class Owned
        {
            public string Name { get; set; } = null!;
            public Address Address { get; set; } = null!;
        }

        public class Address
        {
            public string Street { get; set; } = null!;
            public string City { get; set; } = null!;
            public Country Country { get; set; } = null!;
        }

        public class Country
        {
            public string Name { get; set; } = null!;
            public string Code { get; set; } = null!;
        }

        public class OwnerOfOptional
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public AddressHolder? Holder { get; set; }
        }

        public class AddressHolder
        {
            public string? Note { get; set; }
            public Address Address { get; set; } = null!;
        }

        public class OwnerOfNested
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public NestedOwned Owned { get; set; } = null!;
        }

        public class NestedOwned
        {
            public Address Address { get; set; } = null!;
            public City City { get; set; } = null!;
        }

        public class City
        {
            public string Name { get; set; } = null!;
        }

        public class OwnerOfNullableComplex
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public OwnedWithNullableComplex Owned { get; set; } = null!;
        }

        public class OwnedWithNullableComplex
        {
            public string Name { get; set; } = null!;
            public OuterComplex? Outer { get; set; }
        }

        public class OuterComplex
        {
            public InnerComplex Inner { get; set; } = null!;
        }

        public class InnerComplex
        {
            public string Value { get; set; } = null!;
            public bool? Flag { get; set; }
        }
    }

    public class OwnerWithTag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public TaggedOwned Owned { get; set; } = null!;
    }

    public class TaggedOwned
    {
        public Tag Tag { get; set; } = null!;
        public List<Tag> Tags { get; set; } = [];
    }

    public class Tag
    {
        public TagId Id { get; set; }
        public string Label { get; set; } = null!;
        public TagCode Code { get; set; }
        public List<TagId> RelatedIds { get; set; } = [];
    }

    public readonly record struct TagId(int Value);

    public readonly record struct TagCode(string Value);

    protected class TagIdConverter() : ValueConverter<TagId, int>(v => v.Value, v => new TagId(v));

    protected class TagCodeConverter() : ValueConverter<TagCode, string>(v => v.Value, v => new TagCode(v));

    protected class TagCodeComparer() : ValueComparer<TagCode>(
        (c1, c2) => string.Equals(c1.Value, c2.Value, StringComparison.OrdinalIgnoreCase),
        c => StringComparer.OrdinalIgnoreCase.GetHashCode(c.Value));

    protected class TagIdListConverter() : ValueConverter<List<TagId>, string>(
        v => string.Join(",", v.Select(i => i.Value)),
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => new TagId(int.Parse(i))).ToList());

    protected class TagIdListComparer() : ValueComparer<List<TagId>>(
        (c1, c2) => c1!.SequenceEqual(c2!),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v)),
        c => c.ToList());

    protected class TagCodeReaderWriter : JsonValueReaderWriter<TagCode>
    {
        private static readonly ConstructorInfo Constructor = typeof(TagCodeReaderWriter).GetConstructor([])!;

        [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
        public override Expression ConstructorExpression
            => Expression.New(Constructor);

        public override TagCode FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => new(manager.CurrentReader.GetString()!.TrimStart('#'));

        public override void ToJsonTyped(Utf8JsonWriter writer, TagCode value)
            => writer.WriteStringValue("#" + value.Value);
    }

    #endregion

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => base.AddNonSharedOptions(builder).ConfigureWarnings(c => c
            .Log(RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning)
            .Log(RelationalEventId.OptionalDependentWithAllNullPropertiesWarning));
}
