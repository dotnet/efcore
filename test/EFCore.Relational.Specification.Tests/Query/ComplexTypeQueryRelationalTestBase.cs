// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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

    #region Non-shared tests

    #region 37205

    [ConditionalFact]
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
            public Child Child { get; set; }
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

    [ConditionalFact]
    public virtual async Task Select_TPC_base_with_ComplexType()
    {
        var contextFactory = await InitializeNonSharedTest<Context35025>();
        using var context = contextFactory.CreateDbContext();

        var count = await context.TpcBases.ToListAsync();

        // TODO: Seed data and assert materialization as well
        // Assert.Equal(0, count);
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

    [ConditionalFact]
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
                await context.Database.ExecuteSqlRawAsync($"CREATE VIEW {Q("BlogsView")} AS SELECT {Q("Id")}, {Q("ComplexThing_Prop1")}, {Q("ComplexThing_Prop2")} FROM {Q("Blogs")}");

                context.Add(
                    new Context34706.Blog
                    {
                        ComplexThing = new Context34706.ComplexThing
                        {
                            Prop1 = 1,
                            Prop2 = 2
                        }
                    });
                await context.SaveChangesAsync();

                string Q(string name) => sqlGenerationHelper.DelimitIdentifier(name);
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

    #endregion Non-shared tests

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
