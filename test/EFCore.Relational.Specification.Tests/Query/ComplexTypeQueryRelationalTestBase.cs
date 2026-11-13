// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ComplexTypeQueryRelationalTestBase<TFixture>(TFixture fixture) : ComplexTypeQueryTestBase<TFixture>(fixture)
    where TFixture : ComplexTypeQueryRelationalFixtureBase, new()
{
    public override async Task Subquery_over_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Subquery_over_complex_type(async));

        Assert.Equal(RelationalStrings.SubqueryOverComplexTypesNotSupported("Customer.ShippingAddress#Address"), exception.Message);

        AssertSql();
    }

    public override async Task Concat_two_different_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Concat_two_different_complex_type(async));

        Assert.Equal(
            RelationalStrings.SetOperationOverDifferentStructuralTypes(
                "Customer.ShippingAddress#Address", "Customer.BillingAddress#Address"), exception.Message);

        AssertSql();
    }

    public override async Task Union_two_different_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Union_two_different_complex_type(async));

        Assert.Equal(
            RelationalStrings.SetOperationOverDifferentStructuralTypes(
                "Customer.ShippingAddress#Address", "Customer.BillingAddress#Address"), exception.Message);

        AssertSql();
    }

    public override async Task Subquery_over_struct_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Subquery_over_struct_complex_type(async));

        Assert.Equal(
            RelationalStrings.SubqueryOverComplexTypesNotSupported("ValuedCustomer.ShippingAddress#AddressStruct"), exception.Message);

        AssertSql();
    }

    public override async Task Concat_two_different_struct_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Concat_two_different_struct_complex_type(async));

        Assert.Equal(
            RelationalStrings.SetOperationOverDifferentStructuralTypes(
                "ValuedCustomer.ShippingAddress#AddressStruct", "ValuedCustomer.BillingAddress#AddressStruct"), exception.Message);

        AssertSql();
    }

    public override async Task Union_two_different_struct_complex_type(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Union_two_different_struct_complex_type(async));

        Assert.Equal(
            RelationalStrings.SetOperationOverDifferentStructuralTypes(
                "ValuedCustomer.ShippingAddress#AddressStruct", "ValuedCustomer.BillingAddress#AddressStruct"), exception.Message);

        AssertSql();
    }

    #region Non-shared test resources

    #region 37205

    [Fact]
    public virtual async Task Complex_json_collection_inside_left_join_subquery()
    {
        var contextFactory = await InitializeNonSharedTest<Context37205>();

        await using var context = contextFactory.CreateDbContext();

        _ = await context.Set<Context37205.Parent>().Include(p => p.Child).ToListAsync();
    }

    private class Context37205(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>();
            modelBuilder.Entity<Child>(b =>
            {
                b.ComplexCollection(e => e.CareNeeds, cb => cb.ToJson());
                b.HasQueryFilter(child => child.IsPublic);
            });
        }

        public class Parent
        {
            public int Id { get; set; }
            public Child? Child { get; set; }
            public int? ChildId { get; set; }
        }

        public class Child
        {
            public int Id { get; set; }
            public Parent Parent { get; set; } = null!;
            public bool IsPublic { get; set; }
            public required List<CareNeedAnswer> CareNeeds { get; set; }
        }

        public record CareNeedAnswer
        {
            public required string Topic { get; set; }
        }
    }

    #endregion 37205

    #region 35025

    [Fact]
    public virtual async Task Select_TPC_base_with_ComplexType()
    {
        var contextFactory = await InitializeNonSharedTest<Context35025>();
        using var context = contextFactory.CreateDbContext();

        var entities = await context.TpcBases.ToListAsync();
        Assert.Empty(entities);
    }

    protected class Context35025(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TpcBase> TpcBases { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TpcBase>(builder =>
            {
                builder.ComplexProperty(e => e.BaseComplexProperty);
                builder.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<TpcChild1>().ComplexProperty(e => e.ChildComplexProperty);
            modelBuilder.Entity<TpcChild2>().ComplexProperty(e => e.ChildComplexProperty);
        }

        public abstract class TpcBase
        {
            public int Id { get; set; }
            public required ComplexThing BaseComplexProperty { get; set; }
        }

        public class TpcChild1 : TpcBase
        {
            public required ComplexThing ChildComplexProperty { get; set; }
            public int ChildProperty { get; set; }
        }

        public class TpcChild2 : TpcBase
        {
            public required AnotherComplexThing ChildComplexProperty { get; set; }
            public required string ChildProperty { get; set; }
        }

        public class ComplexThing
        {
            public int PropertyInsideComplexThing { get; set; }
        }

        public class AnotherComplexThing
        {
            // Another nested property with the same name but a different type.
            // We should properly uniquify the projected name coming out of the UNION.
            public required string PropertyInsideComplexThing { get; set; }
        }
    }

    #endregion 35025

    #region 34706

    [Fact]
    public virtual async Task Complex_type_on_an_entity_mapped_to_view_and_table()
    {
        var contextFactory = await InitializeNonSharedTest<Context34706>(
            onModelCreating: mb => mb.Entity<Context34706.Blog>(eb => eb
                .ToTable("Blogs")
                .ToView("BlogsView")
                .ComplexProperty(b => b.ComplexThing)),
            seed: async context =>
            {
                var sqlGenerationHelper = context.GetService<ISqlGenerationHelper>();
                await context.Database.ExecuteSqlRawAsync(
                    $"CREATE VIEW {Q("BlogsView")} AS SELECT {Q("Id")}, {Q("ComplexThing_Prop1")}, {Q("ComplexThing_Prop2")} FROM {Q("Blogs")}");

                context.Add(
                    new Context34706.Blog { ComplexThing = new Context34706.ComplexThing { Prop1 = 1, Prop2 = 2 } });
                await context.SaveChangesAsync();

                string Q(string name)
                    => sqlGenerationHelper.DelimitIdentifier(name);
            });

        await using var context = contextFactory.CreateDbContext();

        var entity = await context.Set<Context34706.Blog>().SingleAsync();

        Assert.NotNull(entity.ComplexThing);
        Assert.Equal(1, entity.ComplexThing.Prop1);
        Assert.Equal(2, entity.ComplexThing.Prop2);
    }

    private class Context34706(DbContextOptions options) : DbContext(options)
    {
        public class Blog
        {
            public int Id { get; set; }
            public required ComplexThing ComplexThing { get; set; }
        }

        public class ComplexThing
        {
            public int Prop1 { get; set; }
            public int Prop2 { get; set; }
        }
    }

    #endregion 34706

    #region 38077

    [Fact]
    public virtual async Task Complex_property_on_split_entity()
    {
        var contextFactory = await InitializeNonSharedTest<Context38077>(
            seed: async context =>
            {
                context.Set<Context38077.Hook>().Add(
                    new Context38077.Hook
                    {
                        CreatedOn = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                        Number = new Context38077.HookNumber { Raw = "123", Parsed = 123 },
                        Weight = 1.25m,
                        IsTestHook = false
                    });

                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var hooks = await context.Set<Context38077.Hook>().ToListAsync();

        Assert.Single(hooks);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), hooks[0].CreatedOn);
        Assert.Equal("123", hooks[0].Number.Raw);
        Assert.Equal(123, hooks[0].Number.Parsed);
        Assert.Equal(1.25m, hooks[0].Weight);
        Assert.False(hooks[0].IsTestHook);
    }

    private class Context38077(DbContextOptions options) : DbContext(options)
    {
        public class Hook
        {
            public int Id { get; set; }
            public DateTime CreatedOn { get; set; }
            public required HookNumber Number { get; set; }
            public decimal? Weight { get; set; }
            public bool? IsTestHook { get; set; }
        }

        public class HookNumber
        {
            public required string Raw { get; set; }
            public long Parsed { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Hook>(b =>
            {
                b.ComplexProperty(h => h.Number);
                b.Property(h => h.Weight).HasPrecision(18, 6);
                b.SplitToTable(
                    "HookMetadata",
                    tb =>
                    {
                        tb.Property(h => h.Id).HasColumnName("HookId");
                        tb.Property(h => h.Weight);
                        tb.Property(h => h.IsTestHook);
                    });
            });
    }

    #endregion 38077

    #region 38928

    [Fact]
    public virtual async Task Project_json_complex_collection_together_with_collection_navigation()
    {
        var contextFactory = await InitializeNonSharedTest<Context38928>(
            seed: async context =>
            {
                context.Set<Context38928.Parent>().Add(
                    new Context38928.Parent
                    {
                        Items = [new Context38928.ChildItem { Value = "/x" }],
                        Links = [new Context38928.Link()]
                    });

                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var result = await context.Set<Context38928.Parent>()
            .AsSplitQuery()
            .Select(p => new
            {
                p.Id,
                p.Items,
                Links = p.Links.Select(l => new { l.Id }).ToList()
            })
            .SingleAsync();

        Assert.NotEqual(0, result.Id);
        Assert.NotNull(result.Items);
        var item = Assert.Single(result.Items);
        Assert.Equal("/x", item.Value);
        Assert.Single(result.Links);
    }

    private class Context38928(DbContextOptions options) : DbContext(options)
    {
        public class Parent
        {
            public int Id { get; set; }
            public List<ChildItem>? Items { get; set; }
            public List<Link> Links { get; set; } = [];
        }

        public class ChildItem
        {
            public required string Value { get; init; }
        }

        public class Link
        {
            public int Id { get; set; }
            public List<Parent> Parents { get; set; } = [];
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(b =>
            {
                b.ComplexCollection(p => p.Items).ToJson();
                b.HasMany(p => p.Links).WithMany(l => l.Parents).UsingEntity("ParentLinks");
            });
        }
    }

    #endregion 38928

    #region SiblingJsonColumn

    [Fact]
    public virtual async Task Json_complex_properties_on_sibling_types_mapped_to_same_column()
    {
        var contextFactory = await InitializeNonSharedTest<ContextSiblingJsonColumn>(
            seed: context =>
            {
                context.AddRange(
                    new ContextSiblingJsonColumn.Dog
                    {
                        Name = "Rex", Details = new ContextSiblingJsonColumn.DogDetails { Breed = "Collie", Trained = true }
                    },
                    new ContextSiblingJsonColumn.Cat { Name = "Tom", Details = new ContextSiblingJsonColumn.CatDetails { Lives = 9 } });

                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var cat = await context.Set<ContextSiblingJsonColumn.Cat>().SingleAsync();
            cat.Details.Lives = 8;

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var animals = await context.Set<ContextSiblingJsonColumn.Animal>().OrderBy(a => a.Name).ToListAsync();

            var dog = Assert.IsType<ContextSiblingJsonColumn.Dog>(animals[0]);
            Assert.Equal("Collie", dog.Details.Breed);
            Assert.True(dog.Details.Trained);
            Assert.Equal(8, Assert.IsType<ContextSiblingJsonColumn.Cat>(animals[1]).Details.Lives);

            Assert.Equal(
                "Rex",
                (await context.Set<ContextSiblingJsonColumn.Dog>().SingleAsync(d => d.Details.Breed == "Collie")).Name);
        }
    }

    private class ContextSiblingJsonColumn(DbContextOptions options) : DbContext(options)
    {
        public abstract class Animal
        {
            public int Id { get; set; }
            public required string Name { get; set; }
        }

        public class Dog : Animal
        {
            public required DogDetails Details { get; set; }
        }

        public class Cat : Animal
        {
            public required CatDetails Details { get; set; }
        }

        public class DogDetails
        {
            public required string Breed { get; set; }
            public bool Trained { get; set; }
        }

        public class CatDetails
        {
            public int Lives { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Dog>().ComplexProperty(d => d.Details, b => b.ToJson("Details"));
            modelBuilder.Entity<Cat>().ComplexProperty(c => c.Details, b => b.ToJson("Details"));
        }
    }

    [Fact]
    public virtual async Task Json_complex_properties_on_base_and_derived_types_mapped_to_same_column_with_TPT()
    {
        var contextFactory = await InitializeNonSharedTest<ContextTptJsonColumn>(
            seed: context =>
            {
                context.Add(
                    new ContextTptJsonColumn.Dog
                    {
                        Name = "Rex",
                        Details = new ContextTptJsonColumn.AnimalDetails { Weight = 30 },
                        DogDetails = new ContextTptJsonColumn.DogDetails { Breed = "Collie" }
                    });

                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var dog = await context.Set<ContextTptJsonColumn.Dog>().SingleAsync();
            dog.Details.Weight = 31;
            dog.DogDetails.Breed = "Border Collie";

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var dog = await context.Set<ContextTptJsonColumn.Dog>().SingleAsync();

            Assert.Equal(31, dog.Details.Weight);
            Assert.Equal("Border Collie", dog.DogDetails.Breed);
        }
    }

    private class ContextTptJsonColumn(DbContextOptions options) : DbContext(options)
    {
        public class Animal
        {
            public int Id { get; set; }
            public required string Name { get; set; }
            public required AnimalDetails Details { get; set; }
        }

        public class Dog : Animal
        {
            public required DogDetails DogDetails { get; set; }
        }

        public class AnimalDetails
        {
            public int Weight { get; set; }
        }

        public class DogDetails
        {
            public required string Breed { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>(b =>
            {
                b.UseTptMappingStrategy();
                b.ComplexProperty(a => a.Details, cb => cb.ToJson("Details"));
            });
            modelBuilder.Entity<Dog>().ComplexProperty(d => d.DogDetails, cb => cb.ToJson("Details"));
        }
    }

    #endregion SiblingJsonColumn

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    #endregion Non-shared test resources

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
