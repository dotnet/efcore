// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocComplexTypeQueryCosmosTest(NonSharedFixture fixture) : AdHocComplexTypeQueryTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => CosmosTestStoreFactory.Instance;

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/288 (Complex-type equality comparisons return no results)
    public override async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        CosmosTestEnvironment.SkipOnLinuxEmulator();

        await base.Complex_type_equals_parameter_with_nested_types_with_property_of_same_name();

        AssertSql(
            """
@entity_equality_container='{"Id":1,"Containee1":{"Id":2},"Containee2":{"Id":3}}'

SELECT VALUE c
FROM root c
WHERE (c["ComplexContainer"] = @entity_equality_container)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Projecting_complex_property_does_not_auto_include_owned_types()
    {
        await base.Projecting_complex_property_does_not_auto_include_owned_types();

        // #34067: Cosmos: Projecting out nested documents retrieves the entire document
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Optional_complex_type_with_discriminator()
    {
        await base.Optional_complex_type_with_discriminator();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["AllOptionalsComplexType"] = null)
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Non_optional_complex_type_with_all_nullable_properties()
    {
        await base.Non_optional_complex_type_with_all_nullable_properties();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Non_optional_complex_type_with_all_nullable_properties_via_left_join()
    {
        Assert.Equal(
            CosmosStrings.UpdateConflict("1"),
            (await Assert.ThrowsAsync<DbUpdateException>(
                () => base.Non_optional_complex_type_with_all_nullable_properties_via_left_join())).Message);

        AssertSql();
    }

    public override async Task Nullable_complex_type_with_discriminator_and_shadow_property()
    {
        await base.Nullable_complex_type_with_discriminator_and_shadow_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Update_entity_with_nullable_complex_type_and_discriminator_does_not_throw()
    {
        await base.Update_entity_with_nullable_complex_type_and_discriminator_does_not_throw();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
    {
        await base.Nullable_complex_type_with_discriminator_null_to_non_null_roundtrip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Nullable_complex_type_with_discriminator_non_null_to_null_roundtrip()
    {
        await base.Nullable_complex_type_with_discriminator_non_null_to_null_roundtrip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Nullable_complex_type_with_discriminator_update_non_null_entity_roundtrip()
    {
        await base.Nullable_complex_type_with_discriminator_update_non_null_entity_roundtrip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Nullable_complex_type_with_discriminator_set_to_different_value()
    {
        await base.Nullable_complex_type_with_discriminator_set_to_different_value();
    }

    public override async Task Nullable_complex_type_with_discriminator_set_to_null()
    {
        // On Cosmos, setting the discriminator shadow property to null doesn't affect materialization
        // because the complex property's data is still present in the JSON document.
        var contextFactory = await InitializeNonSharedTest<Context38119>();

        Guid entityId;
        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = new Context38119.EntityType
            {
                Id = Guid.NewGuid(),
                Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
            };
            context.Add(entity);
            entityId = entity.Id;

            var discriminatorEntry = context.Entry(entity).ComplexProperty(e => e.Prop).Property("Discriminator");
            Assert.Equal("OptionalComplexProperty", discriminatorEntry.CurrentValue);
            discriminatorEntry.CurrentValue = null;
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync(e => e.Id == entityId);
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }

    }

    public override async Task Nested_nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
    {
        await base.Nested_nullable_complex_type_with_discriminator_null_to_non_null_roundtrip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Can_query_by_complex_type_property_with_index()
    {
        await base.Can_query_by_complex_type_property_with_index();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Address"]["City"] = "Seattle")
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Can_update_entity_with_index_on_complex_type_property()
    {
        await base.Can_update_entity_with_index_on_complex_type_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Can_delete_entity_with_index_on_complex_type_property()
    {
        await base.Can_delete_entity_with_index_on_complex_type_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE COUNT(1)
FROM root c
""");
    }

    public override async Task Can_query_by_alternate_key_on_complex_type_property()
    {
        await base.Can_query_by_alternate_key_on_complex_type_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Address"]["City"] = "Redmond")
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Can_save_batch_swapping_alternate_key_values_on_complex_type_property()
    {
        // Unlike relational providers, Cosmos can't ORDER BY the primary-key path 'Id.Id': the model defines an
        // explicit index (on Address.PostalCode), which disables automatic indexing and therefore excludes the
        // 'Id.Id' path from the index. Order client-side instead - the test's intent (reading the alternate-key
        // values while building batch edges) is unaffected.
        var contextFactory = await InitializeNonSharedTest<Context31246>(
            seed: context =>
            {
                context.AddRange(
                    new Context31246.Person { Id = new Context31246.StronglyTypedId(1), Address = new Context31246.Address { City = "Seattle", PostalCode = "98101" } },
                    new Context31246.Person { Id = new Context31246.StronglyTypedId(2), Address = new Context31246.Address { City = "Redmond", PostalCode = "98052" } });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateDbContext())
        {
            var people = (await context.Set<Context31246.Person>().ToListAsync()).OrderBy(p => p.Id.Id).ToList();
            people[0].Address.PostalCode = "98103";
            people[1].Address.PostalCode = "98054";
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var people = (await context.Set<Context31246.Person>().ToListAsync()).OrderBy(p => p.Id.Id).ToList();
            Assert.Equal("98103", people[0].Address.PostalCode);
            Assert.Equal("98054", people[1].Address.PostalCode);
        }
    }

    [Fact]
    public virtual async Task Can_use_complex_type_key_with_discriminator_in_json_id()
    {
        var contextFactory = await InitializeNonSharedTest<ComplexKeyDiscriminatorContext>(
            seed: context =>
            {
                context.AddRange(
                    new ComplexKeyDiscriminatorContext.Customer { Id = new ComplexKeyDiscriminatorContext.CustomerId(1), Name = "Alice" },
                    new ComplexKeyDiscriminatorContext.Customer { Id = new ComplexKeyDiscriminatorContext.CustomerId(2), Name = "Bob" });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var customer = await context.Set<ComplexKeyDiscriminatorContext.Customer>().SingleAsync(c => c.Id.Value == 1);
        Assert.Equal(new ComplexKeyDiscriminatorContext.CustomerId(1), customer.Id);
        Assert.Equal("Alice", customer.Name);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Id"]["Value"] = 1)
OFFSET 0 LIMIT 2
""");
    }

    [Fact]
    public virtual async Task Can_use_nested_complex_type_key_with_discriminator_in_json_id()
    {
        var contextFactory = await InitializeNonSharedTest<NestedComplexKeyDiscriminatorContext>(
            seed: context =>
            {
                context.AddRange(
                    new NestedComplexKeyDiscriminatorContext.Order
                    {
                        Key = new NestedComplexKeyDiscriminatorContext.OrderKey { Inner = new NestedComplexKeyDiscriminatorContext.InnerKey { Value = 1 } },
                        Description = "First"
                    },
                    new NestedComplexKeyDiscriminatorContext.Order
                    {
                        Key = new NestedComplexKeyDiscriminatorContext.OrderKey { Inner = new NestedComplexKeyDiscriminatorContext.InnerKey { Value = 2 } },
                        Description = "Second"
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var order = await context.Set<NestedComplexKeyDiscriminatorContext.Order>().SingleAsync(o => o.Key.Inner.Value == 2);
        Assert.Equal(2, order.Key.Inner.Value);
        Assert.Equal("Second", order.Description);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Key"]["Inner"]["Value"] = 2)
OFFSET 0 LIMIT 2
""");
    }

    protected class ComplexKeyDiscriminatorContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(b =>
            {
                b.ComplexProperty(e => e.Id);
                b.HasKey(e => e.Id.Value);
                b.Property(e => e.Id.Value).ValueGeneratedNever();
                b.HasDiscriminatorInJsonId();
            });

        public readonly record struct CustomerId(int Value);

        public class Customer
        {
            public CustomerId Id { get; set; }
            public string Name { get; set; } = null!;
        }
    }

    protected class NestedComplexKeyDiscriminatorContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Order>(b =>
            {
                b.ComplexProperty(e => e.Key, kb => kb.ComplexProperty(k => k.Inner));
                b.HasKey(e => e.Key.Inner.Value);
                b.Property(e => e.Key.Inner.Value).ValueGeneratedNever();
                b.HasDiscriminatorInJsonId();
            });

        public class Order
        {
            public OrderKey Key { get; set; } = null!;
            public string Description { get; set; } = null!;
        }

        public class OrderKey
        {
            public InnerKey Inner { get; set; } = null!;
        }

        public class InnerKey
        {
            public int Value { get; set; }
        }
    }

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
       => base.AddNonSharedOptions(builder)
               .ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
