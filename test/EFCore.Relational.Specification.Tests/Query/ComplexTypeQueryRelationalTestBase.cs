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

    #region 39814

    [Fact]
    public virtual async Task Select_TPC_base_with_json_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context39814>(
            seed: context =>
            {
                context.AddRange(
                    new Context39814.First
                    {
                        Id = 1,
                        Document = new Context39814.Document { Value = "A" },
                        Documents = [new Context39814.Document { Value = "A1" }]
                    },
                    new Context39814.Second
                    {
                        Id = 2,
                        Document = new Context39814.Document { Value = "B" },
                        Documents = [new Context39814.Document { Value = "B1" }, new Context39814.Document { Value = "B2" }]
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entities = await context.Set<Context39814.Base>().OrderBy(e => e.Id).ToListAsync();

        Assert.Collection(
            entities,
            e =>
            {
                Assert.IsType<Context39814.First>(e);
                Assert.Equal("A", e.Document.Value);
                Assert.Equal(["A1"], e.Documents.Select(d => d.Value));
            },
            e =>
            {
                Assert.IsType<Context39814.Second>(e);
                Assert.Equal("B", e.Document.Value);
                Assert.Equal(["B1", "B2"], e.Documents.Select(d => d.Value));
            });
    }

    protected class Context39814(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Base>(b =>
            {
                b.UseTpcMappingStrategy();
                b.Property(e => e.Id).ValueGeneratedNever();
                b.ComplexProperty(e => e.Document).ToJson();
                b.ComplexCollection(e => e.Documents).ToJson();
            });
            modelBuilder.Entity<First>().ToTable("First");
            modelBuilder.Entity<Second>().ToTable("Second");
        }

        public abstract class Base
        {
            public int Id { get; set; }
            public required Document Document { get; set; }
            public required List<Document> Documents { get; set; }
        }

        public class First : Base;

        public class Second : Base;

        public class Document
        {
            public required string Value { get; set; }
        }
    }

    #endregion 39814

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

    #region 36558

    [Fact]
    public virtual async Task Json_complex_property_nested_in_table_split_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558>(seed: Context36558.SeedAsync);

        await using (var context = contextFactory.CreateDbContext())
        {
            var order = await context.Set<Context36558.Order>().SingleAsync();
            order.Shipping.Carrier = "Courier";
            order.Shipping.Address.City = "Melbourne";
            order.Shipping.Parcels.Add(new Context36558.Parcel { Weight = 0.5 });
            order.Shipping.Customs.Declaration.Contents = "Toys";

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var order = await context.Set<Context36558.Order>()
                .SingleAsync(o => o.Shipping.Address.City == "Melbourne"
                    && o.Shipping.Parcels[1].Weight == 3
                    && o.Shipping.Customs.Declaration.Contents == "Toys");

            Assert.Equal("Courier", order.Shipping.Carrier);
            Assert.Equal("2000", order.Shipping.Address.PostCode);
            Assert.Equal([1.5, 3, 0.5], order.Shipping.Parcels.Select(p => p.Weight));
            Assert.Equal("AU", order.Shipping.Customs.Code);
        }
    }

    [Fact]
    public virtual async Task ExecuteUpdate_json_complex_property_nested_in_table_split_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558>(seed: Context36558.SeedAsync);

        await using (var context = contextFactory.CreateDbContext())
        {
            // A property in a nested JSON column and a nested JSON column as a whole can be set directly
            var declaration = new Context36558.Declaration { Contents = "Toys" };
            await context.Set<Context36558.Order>().ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Shipping.Address.City, "Perth")
                .SetProperty(o => o.Shipping.Customs.Declaration, declaration));

            // Setting the table-split complex property as a whole isn't supported, since it contains JSON complex properties
            var shipping = new Context36558.Shipping
            {
                Carrier = "Courier",
                Address = new Context36558.Address { City = "Darwin", PostCode = "0800" },
                Customs = new Context36558.Customs { Code = "NZ", Declaration = new Context36558.Declaration { Contents = "Books" } }
            };
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.Set<Context36558.Order>()
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Shipping, shipping)));

            var addressType = context.Model.FindEntityType(typeof(Context36558.Order))!
                .FindComplexProperty(nameof(Context36558.Order.Shipping))!.ComplexType
                .FindComplexProperty(nameof(Context36558.Shipping.Address))!.ComplexType;
            Assert.Equal(
                RelationalStrings.ExecuteUpdateOverJsonIsNotSupported(addressType.DisplayName()), exception.InnerException!.Message);
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var order = await context.Set<Context36558.Order>().SingleAsync();

            Assert.Equal("Post", order.Shipping.Carrier);
            Assert.Equal("Perth", order.Shipping.Address.City);
            Assert.Equal("2000", order.Shipping.Address.PostCode);
            Assert.Equal("Toys", order.Shipping.Customs.Declaration.Contents);
        }
    }

    // Shipping is mapped to columns in the Order table (table splitting), while its nested Address and Parcels are each mapped
    // to a JSON column in the same table. Customs is mapped to columns as well, with its nested Declaration mapped to a JSON column
    private class Context36558(DbContextOptions options) : DbContext(options)
    {
        public class Order
        {
            public int Id { get; set; }
            public required Shipping Shipping { get; set; }
        }

        public class Shipping
        {
            public required string Carrier { get; set; }
            public required Address Address { get; set; }
            public List<Parcel> Parcels { get; set; } = [];
            public required Customs Customs { get; set; }
        }

        public class Address
        {
            public required string City { get; set; }
            public required string PostCode { get; set; }
        }

        public class Parcel
        {
            public double Weight { get; set; }
        }

        public class Customs
        {
            public required string Code { get; set; }
            public required Declaration Declaration { get; set; }
        }

        public class Declaration
        {
            public required string Contents { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Order>(b =>
            {
                b.Property(o => o.Id).ValueGeneratedNever();
                b.ComplexProperty(
                    o => o.Shipping, sb =>
                    {
                        sb.ComplexProperty(s => s.Address).ToJson();
                        sb.ComplexCollection(s => s.Parcels).ToJson();
                        sb.ComplexProperty(s => s.Customs, cb => cb.ComplexProperty(c => c.Declaration).ToJson());
                    });
            });

        public static Task SeedAsync(Context36558 context)
        {
            context.Add(
                new Order
                {
                    Id = 1,
                    Shipping = new Shipping
                    {
                        Carrier = "Post",
                        Address = new Address { City = "Sydney", PostCode = "2000" },
                        Parcels = [new Parcel { Weight = 1.5 }, new Parcel { Weight = 3 }],
                        Customs = new Customs { Code = "AU", Declaration = new Declaration { Contents = "Clothes" } }
                    }
                });

            return context.SaveChangesAsync();
        }
    }

    [Fact]
    public virtual async Task Optional_owned_dependent_identified_by_json_complex_property_nested_in_table_split_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558Owned>(
            seed: context =>
            {
                context.AddRange(
                    new Context36558Owned.Customer
                    {
                        Id = 1,
                        Billing = new Context36558Owned.Billing
                        {
                            Contact = new Context36558Owned.Contact { Address = new Context36558Owned.Address { City = "Sydney" } },
                            Terms = new Context36558Owned.Terms { Days = 30 }
                        }
                    },
                    new Context36558Owned.Customer { Id = 2 });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();
        var customers = await context.Set<Context36558Owned.Customer>().OrderBy(c => c.Id).ToListAsync();

        Assert.Equal("Sydney", customers[0].Billing!.Contact.Address.City);
        Assert.Equal(30, customers[0].Billing!.Terms!.Days);
        Assert.Null(customers[1].Billing);

        Assert.Equal(
            [2],
            await context.Set<Context36558Owned.Customer>().Where(c => c.Billing == null).Select(c => c.Id).ToListAsync());

        var billings = await context.Set<Context36558Owned.Customer>().AsNoTracking().OrderBy(c => c.Id).Select(c => c.Billing)
            .ToListAsync();
        Assert.Equal("Sydney", billings[0]!.Contact.Address.City);
        Assert.Null(billings[1]);
    }

    [Fact]
    public virtual async Task Json_complex_property_nested_in_table_split_complex_property_set_through_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558PropertyAccess>(
            seed: context =>
            {
                context.Add(
                    new Context36558PropertyAccess.Order
                    {
                        Id = 1,
                        Shipping = new Context36558PropertyAccess.Shipping
                        {
                            Carrier = "Post", Address = new Context36558PropertyAccess.Address { City = "Sydney" }
                        }
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();
        var order = await context.Set<Context36558PropertyAccess.Order>().SingleAsync();

        Assert.Equal("Post", order.Shipping.Carrier);
        Assert.Equal("Sydney", order.Shipping.Address.City);
    }

    // Shipping is set through its property (rather than its backing field), whose setter and getter copy it,
    // so the materializer has to set Address, from its own JSON column, before setting Shipping
    private class Context36558PropertyAccess(DbContextOptions options) : DbContext(options)
    {
        public class Order
        {
            private Shipping? _shipping;

            public int Id { get; set; }

            public Shipping Shipping
            {
                get => _shipping!.Copy();
                set => _shipping = value.Copy();
            }
        }

        public class Shipping
        {
            public required string Carrier { get; set; }
            public required Address Address { get; set; }

            public Shipping Copy()
                => new() { Carrier = Carrier, Address = Address };
        }

        public class Address
        {
            public required string City { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Order>(b =>
            {
                b.Property(o => o.Id).ValueGeneratedNever();
                b.ComplexProperty(o => o.Shipping, sb => sb.ComplexProperty(s => s.Address).ToJson());
            });
        }
    }

    // The optional owned Billing shares the Customer table, and its only required column is the JSON column of Address, nested in
    // the table-split Contact; so that column identifies whether Billing exists, also for validating Billing's own dependent Terms
    private class Context36558Owned(DbContextOptions options) : DbContext(options)
    {
        public class Customer
        {
            public int Id { get; set; }
            public Billing? Billing { get; set; }
        }

        public class Billing
        {
            public required Contact Contact { get; set; }
            public Terms? Terms { get; set; }
        }

        public class Contact
        {
            public string? Phone { get; set; }
            public required Address Address { get; set; }
        }

        public class Address
        {
            public required string City { get; set; }
        }

        public class Terms
        {
            public int Days { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(b =>
            {
                b.Property(c => c.Id).ValueGeneratedNever();
                b.OwnsOne(
                    c => c.Billing, ob =>
                    {
                        ob.ComplexProperty(o => o.Contact, cb => cb.ComplexProperty(c => c.Address).ToJson());
                        ob.OwnsOne(o => o.Terms);
                    });
            });
    }

    [Fact]
    public virtual async Task Json_struct_nested_in_optional_table_split_complex_property_is_stored_as_null_when_it_is_null()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558Struct>();

        await using (var context = contextFactory.CreateDbContext())
        {
            context.AddRange(
                new Context36558Struct.Row { Id = 1 },
                new Context36558Struct.Row
                {
                    Id = 2, Outer = new Context36558Struct.Outer { Marker = "m", Payload = new Context36558Struct.Payload { Number = 0 } }
                });
            await context.SaveChangesAsync();

            // The JSON column of a value type is NULL when the optional complex property containing it is null
            Assert.Equal([1, 0], await GetPayloadIsNull(context));

            var row = await context.Set<Context36558Struct.Row>().SingleAsync(r => r.Id == 2);
            row.Outer = null;
            await context.SaveChangesAsync();

            Assert.Equal([1, 1], await GetPayloadIsNull(context));
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            Assert.All(await context.Set<Context36558Struct.Row>().ToListAsync(), r => Assert.Null(r.Outer));
        }

        Task<List<int>> GetPayloadIsNull(DbContext context)
            => context.Database.SqlQueryRaw<int>(
                    ((RelationalTestStore)NonSharedTestStore).NormalizeDelimitersInRawString(
                        "SELECT CASE WHEN [PayloadJson] IS NULL THEN 1 ELSE 0 END AS [Value] FROM [Row] ORDER BY [Id]"))
                .ToListAsync();
    }

    // The optional Outer is mapped to columns in the Row table (table splitting), while its nested Payload value type is mapped to a
    // JSON column in the same table
    private class Context36558Struct(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public Outer? Outer { get; set; }
        }

        public class Outer
        {
            public required string Marker { get; set; }
            public Payload Payload { get; set; }
        }

        public struct Payload
        {
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(r => r.Outer, ob => ob.ComplexProperty(o => o.Payload).ToJson("PayloadJson"));
            });
    }

    [Fact]
    public virtual async Task Json_complex_property_nested_in_table_split_complex_property_with_default_name_used_by_another_column()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558ColumnName>(
            seed: context =>
            {
                context.Add(
                    new Context36558ColumnName.Order
                    {
                        Id = 1,
                        Shipping_Address = "Leave at the door",
                        Shipping = new Context36558ColumnName.Shipping { Address = new Context36558ColumnName.Address { City = "Sydney" } }
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        // The default name of the JSON column, Shipping_Address, is already used by the column of the Shipping_Address property
        var table = context.Model.GetRelationalModel().Tables.Single();
        var entityType = context.Model.FindEntityType(typeof(Context36558ColumnName.Order))!;
        var addressProperty = entityType.FindComplexProperty(nameof(Context36558ColumnName.Order.Shipping))!.ComplexType
            .FindComplexProperty(nameof(Context36558ColumnName.Shipping.Address))!;
        Assert.Equal(
            "Shipping_Address", table.FindColumn(entityType.FindProperty(nameof(Context36558ColumnName.Order.Shipping_Address))!)!.Name);
        Assert.Equal("Shipping_Address1", table.FindColumn(addressProperty)!.Name);

        var order = await context.Set<Context36558ColumnName.Order>().SingleAsync();

        Assert.Equal("Leave at the door", order.Shipping_Address);
        Assert.Equal("Sydney", order.Shipping.Address.City);
    }

    private class Context36558ColumnName(DbContextOptions options) : DbContext(options)
    {
        public class Order
        {
            public int Id { get; set; }
            public required string Shipping_Address { get; set; }
            public required Shipping Shipping { get; set; }
        }

        public class Shipping
        {
            public required Address Address { get; set; }
        }

        public class Address
        {
            public required string City { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Order>(b =>
            {
                b.Property(o => o.Id).ValueGeneratedNever();
                b.ComplexProperty(o => o.Shipping, sb => sb.ComplexProperty(s => s.Address).ToJson());
            });
    }

    [Fact]
    public virtual async Task Table_split_complex_property_identified_by_nested_json_complex_property_projected_over_left_join()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558JsonOnly>(seed: Context36558JsonOnly.SeedAsync);

        await using var context = contextFactory.CreateDbContext();

        // Shipping has no columns of its own besides the JSON columns, so the JSON column of the required Address identifies it
        var shippings = await context.Set<Context36558JsonOnly.Order>()
            .LeftJoin(
                context.Set<Context36558JsonOnly.Order>().Where(o => o.Id == 1),
                o => o.Id,
                o => o.Id,
                (outer, inner) => new { outer.Id, inner!.Shipping })
            .OrderBy(r => r.Id)
            .Select(r => r.Shipping)
            .ToListAsync();

        Assert.Collection(
            shippings,
            s => Assert.Equal("Sydney", s!.Address.City),
            Assert.Null);
    }

    [Fact]
    public virtual async Task Table_split_complex_property_with_nested_json_complex_collection_compared_to_inline_instance()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558JsonOnly>(seed: Context36558JsonOnly.SeedAsync);

        await using var context = contextFactory.CreateDbContext();

        var ids = await context.Set<Context36558JsonOnly.Order>()
            .Where(o => o.Shipping
                == new Context36558JsonOnly.Shipping
                {
                    Address = new Context36558JsonOnly.Address { City = "Sydney" },
                    Parcels = new List<Context36558JsonOnly.Parcel> { new() { Weight = 2 } }
                })
            .Select(o => o.Id)
            .ToListAsync();

        Assert.Equal([1], ids);
    }

    // Shipping is mapped to columns in the Order table (table splitting), but all its properties are mapped to JSON columns
    private class Context36558JsonOnly(DbContextOptions options) : DbContext(options)
    {
        public class Order
        {
            public int Id { get; set; }
            public required Shipping Shipping { get; set; }
        }

        public class Shipping
        {
            public required Address Address { get; set; }
            public List<Parcel> Parcels { get; set; } = [];
        }

        public class Address
        {
            public required string City { get; set; }
        }

        public class Parcel
        {
            public int Weight { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Order>(b =>
            {
                b.Property(o => o.Id).ValueGeneratedNever();
                b.ComplexProperty(
                    o => o.Shipping, sb =>
                    {
                        sb.ComplexProperty(s => s.Address).ToJson();
                        sb.ComplexCollection(s => s.Parcels).ToJson();
                    });
            });

        public static Task SeedAsync(Context36558JsonOnly context)
        {
            context.AddRange(
                new Order
                {
                    Id = 1,
                    Shipping = new Shipping { Address = new Address { City = "Sydney" }, Parcels = [new Parcel { Weight = 2 }] }
                },
                new Order
                {
                    Id = 2,
                    Shipping = new Shipping { Address = new Address { City = "Perth" }, Parcels = [new Parcel { Weight = 5 }] }
                });

            return context.SaveChangesAsync();
        }
    }

    [Fact]
    public virtual async Task Change_in_optional_table_split_complex_property_doesnt_overwrite_its_nested_json_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558Optional>(
            seed: context =>
            {
                context.Add(
                    new Context36558Optional.Row
                    {
                        Id = 1,
                        Outer = new Context36558Optional.Outer
                        {
                            Marker = "Original", Document = new Context36558Optional.Document { Value = "Sydney" }
                        }
                    });

                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var row = await context.Set<Context36558Optional.Row>().SingleAsync();

            // The JSON column is changed by someone else after the row is loaded
            await using (var otherContext = contextFactory.CreateDbContext())
            {
                await otherContext.Set<Context36558Optional.Row>()
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.Outer!.Document.Value, "Perth"));
            }

            row.Outer!.Marker = "Renamed";
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var outer = (await context.Set<Context36558Optional.Row>().SingleAsync()).Outer!;

            Assert.Equal("Renamed", outer.Marker);
            Assert.Equal("Perth", outer.Document.Value);
        }
    }

    // The optional Outer is mapped to columns in the Row table (table splitting), while its nested Document is mapped to a JSON column
    private class Context36558Optional(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public Outer? Outer { get; set; }
        }

        public class Outer
        {
            public required string Marker { get; set; }
            public required Document Document { get; set; }
        }

        public class Document
        {
            public required string Value { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(r => r.Outer, ob => ob.ComplexProperty(o => o.Document).ToJson());
            });
    }

    [Fact]
    public virtual async Task Optional_table_split_complex_property_identified_by_nested_json_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558OptionalJsonOnly>(
            seed: context =>
            {
                context.AddRange(
                    new Context36558OptionalJsonOnly.Row { Id = 1 },
                    new Context36558OptionalJsonOnly.Row
                    {
                        Id = 2,
                        Outer = new Context36558OptionalJsonOnly.Outer
                        {
                            Document = new Context36558OptionalJsonOnly.Document { Value = "Sydney" }
                        }
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        Assert.Collection(
            await context.Set<Context36558OptionalJsonOnly.Row>().OrderBy(r => r.Id).ToListAsync(),
            r => Assert.Null(r.Outer),
            r => Assert.Equal("Sydney", r.Outer!.Document.Value));
        Assert.Equal(
            [1],
            await context.Set<Context36558OptionalJsonOnly.Row>().Where(r => r.Outer == null).Select(r => r.Id).ToListAsync());
    }

    // The optional Outer has no columns besides the JSON column of its required Document, which identifies it
    private class Context36558OptionalJsonOnly(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public Outer? Outer { get; set; }
        }

        public class Outer
        {
            public required Document Document { get; set; }
        }

        public class Document
        {
            public required string Value { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(r => r.Outer, ob => ob.ComplexProperty(o => o.Document).ToJson());
            });
    }

    [Fact]
    public virtual async Task Table_split_complex_property_with_optional_sibling_identified_by_nested_json_complex_property_over_left_join()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558OptionalSibling>(
            seed: context =>
            {
                context.AddRange(
                    new Context36558OptionalSibling.Row
                    {
                        Id = 1,
                        Outer = new Context36558OptionalSibling.Outer
                        {
                            Document = new Context36558OptionalSibling.Document { Value = "Sydney" }
                        }
                    },
                    new Context36558OptionalSibling.Row
                    {
                        Id = 2,
                        Outer = new Context36558OptionalSibling.Outer
                        {
                            Document = new Context36558OptionalSibling.Document { Value = "Perth" }
                        }
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();
        var rows = context.Set<Context36558OptionalSibling.Row>();

        // Outer is present when its Document is, even though the required Marker of the optional Annex is null
        var joined = rows.LeftJoin(
            rows.Where(r => r.Id == 1),
            r => r.Id,
            r => r.Id,
            (outer, inner) => new { outer.Id, inner!.Outer });

        Assert.Collection(
            await joined.OrderBy(r => r.Id).Select(r => r.Outer).ToListAsync(),
            o => Assert.Equal("Sydney", o!.Document.Value),
            Assert.Null);
        Assert.Equal([2], await joined.Where(r => r.Outer == null).Select(r => r.Id).ToListAsync());
    }

    // Outer has no columns of its own besides the JSON column of its required Document, and the columns of its optional Annex
    private class Context36558OptionalSibling(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public required Outer Outer { get; set; }
        }

        public class Outer
        {
            public Extra? Annex { get; set; }
            public required Document Document { get; set; }
        }

        public class Extra
        {
            public required string Marker { get; set; }
        }

        public class Document
        {
            public required string Value { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(
                    r => r.Outer, ob =>
                    {
                        ob.ComplexProperty(o => o.Annex);
                        ob.ComplexProperty(o => o.Document).ToJson();
                    });
            });
    }

    [Fact]
    public virtual async Task Current_values_of_json_complex_property_nested_in_table_split_complex_property_are_copied_to_object()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558PropertyAccess>();

        await using var context = contextFactory.CreateDbContext();
        var order = new Context36558PropertyAccess.Order
        {
            Id = 1,
            Shipping = new Context36558PropertyAccess.Shipping
            {
                Carrier = "Post", Address = new Context36558PropertyAccess.Address { City = "Sydney" }
            }
        };
        context.Add(order);

        var copy = (Context36558PropertyAccess.Order)context.Entry(order).CurrentValues.ToObject();

        Assert.Equal("Post", copy.Shipping.Carrier);
        Assert.Equal("Sydney", copy.Shipping.Address.City);
    }

    [Fact]
    public virtual async Task Current_values_of_json_complex_property_in_optional_table_split_complex_property_are_copied_to_object()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558Document>();

        await using var context = contextFactory.CreateDbContext();
        var row = new Context36558Document.Row
        {
            Id = 1,
            Envelope = new Context36558Document.Envelope
            {
                Document = new Context36558Document.Document
                {
                    Title = "Sydney", Details = new Context36558Document.Details { PageCount = 2 }
                }
            }
        };
        context.Add(row);

        var copy = (Context36558Document.Row)context.Entry(row).CurrentValues.ToObject();

        // The optional Envelope is identified by its JSON Document, and the complex property inside the document is copied too
        Assert.NotNull(copy.Envelope);
        Assert.Equal("Sydney", copy.Envelope.Document.Title);
        Assert.Equal(2, copy.Envelope.Document.Details.PageCount);
    }

    private class Context36558Document(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public string? Note { get; set; }
            public Envelope? Envelope { get; set; }
        }

        public class Envelope
        {
            public required Document Document { get; set; }
        }

        public class Document
        {
            public required string Title { get; set; }
            public required Details Details { get; set; }
        }

        public class Details
        {
            public int PageCount { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(
                    r => r.Envelope, eb => eb.ComplexProperty(
                        e => e.Document, db =>
                        {
                            db.ToJson();
                            db.ComplexProperty(d => d.Details);
                        }));
            });
    }

    [Fact]
    public virtual async Task Current_values_of_json_complex_collection_in_table_split_complex_property_are_copied_to_object()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558Collection>();

        await using var context = contextFactory.CreateDbContext();
        var row = new Context36558Collection.Row
        {
            Id = 1,
            Envelope = new Context36558Collection.Envelope { Items = [new Context36558Collection.Item { Value = 2 }] }
        };
        context.Add(row);

        var copy = (Context36558Collection.Row)context.Entry(row).CurrentValues.ToObject();

        Assert.Equal(2, Assert.Single(copy.Envelope.Items).Value);
    }

    // The setter of the collection doesn't accept null, so it can only be set to the populated collection
    private class Context36558Collection(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public required Envelope Envelope { get; set; }
        }

        public class Envelope
        {
            private List<Item> _items = [];

            public List<Item> Items
            {
                get => _items;
                set => _items = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public class Item
        {
            public int Value { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(r => r.Envelope, eb => eb.ComplexCollection(e => e.Items).ToJson());
            });
        }
    }

    [Fact]
    public virtual async Task Json_complex_property_nested_in_table_split_complex_property_with_readonly_backing_field()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558ReadonlyField>(
            seed: context =>
            {
                context.Add(
                    new Context36558ReadonlyField.Row
                    {
                        Id = 1,
                        Envelope = new Context36558ReadonlyField.Envelope(new Context36558ReadonlyField.Details { PageCount = 2 })
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();
        var row = await context.Set<Context36558ReadonlyField.Row>().AsNoTracking().SingleAsync();

        Assert.Equal(2, row.Envelope.Document.PageCount);
    }

    // The JSON Document is set through its readonly backing field
    private class Context36558ReadonlyField(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public required Envelope Envelope { get; set; }
        }

        public class Envelope
        {
            private readonly Details _document;

            public Envelope()
                : this(new Details())
            {
            }

            public Envelope(Details document)
                => _document = document;

            public Details Document
                => _document;
        }

        public class Details
        {
            public int PageCount { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(r => r.Envelope, eb => eb.ComplexProperty(e => e.Document).HasField("_document").ToJson());
            });
    }

    [Fact]
    public virtual async Task Select_TPC_base_with_json_complex_property_nested_in_table_split_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context36558Tpc>(
            seed: context =>
            {
                context.AddRange(
                    new Context36558Tpc.First
                    {
                        Id = 1,
                        Envelope = new Context36558Tpc.Envelope
                        {
                            Marker = "A",
                            Document = new Context36558Tpc.Document { Value = "A" },
                            Documents = [new Context36558Tpc.Document { Value = "A1" }]
                        }
                    },
                    new Context36558Tpc.Second
                    {
                        Id = 2,
                        Envelope = new Context36558Tpc.Envelope
                        {
                            Marker = "B",
                            Document = new Context36558Tpc.Document { Value = "B" },
                            Documents = [new Context36558Tpc.Document { Value = "B1" }, new Context36558Tpc.Document { Value = "B2" }]
                        }
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        // The JSON columns are projected from the UNION over the tables of the concrete types
        var entities = await context.Set<Context36558Tpc.Base>().OrderBy(e => e.Id).ToListAsync();

        Assert.Collection(
            entities,
            e =>
            {
                Assert.IsType<Context36558Tpc.First>(e);
                Assert.Equal("A", e.Envelope.Marker);
                Assert.Equal("A", e.Envelope.Document.Value);
                Assert.Equal(["A1"], e.Envelope.Documents.Select(d => d.Value));
            },
            e =>
            {
                Assert.IsType<Context36558Tpc.Second>(e);
                Assert.Equal("B", e.Envelope.Marker);
                Assert.Equal("B", e.Envelope.Document.Value);
                Assert.Equal(["B1", "B2"], e.Envelope.Documents.Select(d => d.Value));
            });
    }

    // The table-split Envelope is declared on the abstract TPC base type, with its Document and Documents mapped to JSON columns
    private class Context36558Tpc(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Base>(b =>
            {
                b.UseTpcMappingStrategy();
                b.Property(e => e.Id).ValueGeneratedNever();
                b.ComplexProperty(
                    e => e.Envelope, eb =>
                    {
                        eb.ComplexProperty(e => e.Document).ToJson();
                        eb.ComplexCollection(e => e.Documents).ToJson();
                    });
            });
            modelBuilder.Entity<First>().ToTable("First");
            modelBuilder.Entity<Second>().ToTable("Second");
        }

        public abstract class Base
        {
            public int Id { get; set; }
            public required Envelope Envelope { get; set; }
        }

        public class First : Base;

        public class Second : Base;

        public class Envelope
        {
            public required string Marker { get; set; }
            public required Document Document { get; set; }
            public required List<Document> Documents { get; set; }
        }

        public class Document
        {
            public required string Value { get; set; }
        }
    }

    #endregion 36558

    #region Optional complex property identified by nested required property

    [Fact]
    public virtual async Task Optional_complex_property_identified_by_required_property_in_nested_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<ContextNestedRequired>(
            seed: context =>
            {
                context.AddRange(
                    new ContextNestedRequired.Row { Id = 1 },
                    new ContextNestedRequired.Row
                    {
                        Id = 2, Outer = new ContextNestedRequired.Outer { Inner = new ContextNestedRequired.Inner { Value = "A" } }
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        // Outer has no properties of its own, but the required Value of its required Inner identifies it
        Assert.Collection(
            await context.Set<ContextNestedRequired.Row>().OrderBy(r => r.Id).ToListAsync(),
            r => Assert.Null(r.Outer),
            r => Assert.Equal("A", r.Outer!.Inner.Value));
        Assert.Equal(
            [1],
            await context.Set<ContextNestedRequired.Row>().Where(r => r.Outer == null).Select(r => r.Id).ToListAsync());
    }

    private class ContextNestedRequired(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public Outer? Outer { get; set; }
        }

        public class Outer
        {
            public required Inner Inner { get; set; }
        }

        public class Inner
        {
            public required string Value { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(r => r.Outer, ob => ob.ComplexProperty(o => o.Inner));
            });
    }

    [Fact]
    public virtual async Task Database_values_of_optional_complex_property_identified_by_nested_json_complex_property_with_null_optional_sibling()
    {
        var contextFactory = await InitializeNonSharedTest<ContextNestedRequiredSibling>(
            seed: context =>
            {
                context.Add(
                    new ContextNestedRequiredSibling.Row
                    {
                        Id = 1,
                        Outer = new ContextNestedRequiredSibling.Outer
                        {
                            Document = new ContextNestedRequiredSibling.Document { Value = "A" }
                        }
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();
        var row = await context.Set<ContextNestedRequiredSibling.Row>().SingleAsync();
        Assert.Equal("A", row.Outer!.Document.Value);

        // Outer is identified by the required Value of its required Document, not by the one of its null Annex
        var entry = context.Entry(row);
        Assert.Equal("A", ((ContextNestedRequiredSibling.Row)entry.CurrentValues.ToObject()).Outer!.Document.Value);
        Assert.Equal("A", ((ContextNestedRequiredSibling.Row)entry.GetDatabaseValues()!.ToObject()).Outer!.Document.Value);
        Assert.Equal(
            "A", ((ContextNestedRequiredSibling.Row)(await entry.GetDatabaseValuesAsync())!.ToObject()).Outer!.Document.Value);

        entry.Reload();
        Assert.Equal("A", row.Outer!.Document.Value);

        await entry.ReloadAsync();
        Assert.Equal("A", row.Outer!.Document.Value);
    }

    private class ContextNestedRequiredSibling(DbContextOptions options) : DbContext(options)
    {
        public class Row
        {
            public int Id { get; set; }
            public Outer? Outer { get; set; }
        }

        public class Outer
        {
            public Annex? Annex { get; set; }
            public required Document Document { get; set; }
        }

        public class Annex
        {
            public required string Value { get; set; }
        }

        public class Document
        {
            public required string Value { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>(b =>
            {
                b.Property(r => r.Id).ValueGeneratedNever();
                b.ComplexProperty(
                    r => r.Outer, ob =>
                    {
                        ob.ComplexProperty(o => o.Annex);
                        ob.ComplexProperty(o => o.Document).ToJson();
                    });
            });
    }

    #endregion

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

    #region Key on complex property

    [Fact]
    public virtual async Task Distinct_over_entity_with_key_on_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(seed: SeedPeopleAsync);
        await using var context = contextFactory.CreateDbContext();

        var people = await context.Set<Context31246.Person>().Distinct().OrderBy(p => p.Id.Id).ToListAsync();

        Assert.Equal([1, 2], people.Select(p => p.Id.Id));
    }

    [Fact]
    public virtual async Task Union_over_entity_with_key_on_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(seed: SeedPeopleAsync);
        await using var context = contextFactory.CreateDbContext();

        var people = await context.Set<Context31246.Person>().Where(p => p.Address.City == "Seattle")
            .Union(context.Set<Context31246.Person>().Where(p => p.Address.City == "Redmond"))
            .OrderBy(p => p.Id.Id)
            .ToListAsync();

        Assert.Equal([1, 2], people.Select(p => p.Id.Id));
    }

    [Fact]
    public virtual async Task Entity_with_key_on_complex_property_equals_parameter()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(seed: SeedPeopleAsync);
        await using var context = contextFactory.CreateDbContext();
        var person = CreatePerson(2, "Redmond", "98052");

        var found = await context.Set<Context31246.Person>().SingleAsync(p => p == person);

        Assert.Equal("Redmond", found.Address.City);
    }

    [Fact]
    public virtual async Task Entity_with_key_on_complex_property_in_local_collection()
    {
        var contextFactory = await InitializeNonSharedTest<Context31246>(seed: SeedPeopleAsync);
        await using var context = contextFactory.CreateDbContext();
        var people = new[] { CreatePerson(2, "Redmond", "98052") };

        var found = await context.Set<Context31246.Person>().SingleAsync(p => people.Contains(p));

        Assert.Equal("Redmond", found.Address.City);
    }

    private static Task SeedPeopleAsync(Context31246 context)
    {
        context.AddRange(CreatePerson(1, "Seattle", "98101"), CreatePerson(2, "Redmond", "98052"));

        return context.SaveChangesAsync();
    }

    private static Context31246.Person CreatePerson(int id, string city, string postalCode)
        => new()
        {
            Id = new Context31246.StronglyTypedId(id), Address = new Context31246.Address { City = city, PostalCode = postalCode }
        };

    #endregion Key on complex property

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    #endregion Non-shared test resources

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
