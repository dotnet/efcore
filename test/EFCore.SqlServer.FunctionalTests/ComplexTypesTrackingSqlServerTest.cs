// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ComplexTypesTrackingSqlServerTest(
    ComplexTypesTrackingSqlServerTest.SqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTypesTrackingSqlServerTestBase<ComplexTypesTrackingSqlServerTest.SqlServerFixture>(fixture, testOutputHelper)
{
    [ConditionalFact]
    public virtual async Task Can_read_original_values_with_TPH_shared_complex_property_column_null()
    {
        await using var context = CreateContext();

        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            context,
            async context =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                context.Add(new InheritedItem1 { Name = "Item1" });
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();
                var item = await context.Set<InheritedItemBase>().FirstAsync();
                var entry = context.ChangeTracker.Entries().First();
                var originalItem = (InheritedItemBase)entry.OriginalValues.ToObject();

                Assert.NotNull(originalItem);
                Assert.Equal("Item1", originalItem.Name);
            });
    }

    [ConditionalFact]
    public virtual async Task Can_read_original_values_with_TPH_shared_complex_property_column_with_value()
    {
        await using var context = CreateContext();

        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            context,
            async context =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                context.Add(new InheritedItem1 { Name = "Item1", SharedPrice = new SharedPrice { Amount = "10.99" } });
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();
                var item = await context.Set<InheritedItem1>().FirstAsync();
                var entry = context.ChangeTracker.Entries().First();
                var originalItem = (InheritedItemBase)entry.OriginalValues.ToObject();

                Assert.NotNull(originalItem);
                Assert.Equal("Item1", originalItem.Name);
                var item1 = Assert.IsType<InheritedItem1>(originalItem);
                Assert.NotNull(item1.SharedPrice);
                Assert.Equal("10.99", item1.SharedPrice.Amount);
            });
    }

    public class SqlServerFixture : SqlServerFixtureBase
    {
        protected override string StoreName
            => nameof(ComplexTypesTrackingSqlServerTest);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<InheritedItemBase>().HasDiscriminator<string>("Discriminator")
                .HasValue<InheritedItem1>("Item1")
                .HasValue<InheritedItem2>("Item2");

            modelBuilder.Entity<InheritedItem1>().ComplexProperty(
                x => x.SharedPrice,
                p => p.Property(a => a.Amount).HasColumnName("SharedPriceAmount"));

            modelBuilder.Entity<InheritedItem2>().ComplexProperty(
                x => x.SharedPrice,
                p => p.Property(a => a.Amount).HasColumnName("SharedPriceAmount"));
        }
    }
}

public abstract class InheritedItemBase
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class InheritedItem1 : InheritedItemBase
{
    public SharedPrice? SharedPrice { get; set; }
}

public class InheritedItem2 : InheritedItemBase
{
    public SharedPrice? SharedPrice { get; set; }
}

public class SharedPrice
{
    public required string Amount { get; init; }
}

public class ComplexTypesTrackingProxiesSqlServerTest(
    ComplexTypesTrackingProxiesSqlServerTest.SqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTypesTrackingSqlServerTestBase<ComplexTypesTrackingProxiesSqlServerTest.SqlServerFixture>(fixture, testOutputHelper)
{
    // Fields can't be proxied
    public override Task Can_track_entity_with_complex_objects_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_mark_complex_type_properties_modified_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override Task Can_track_entity_with_complex_structs_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_mark_complex_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override Task Can_track_entity_with_complex_readonly_structs_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_mark_complex_readonly_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override Task Can_track_entity_with_complex_record_objects_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_mark_complex_record_type_properties_modified_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_added_elements_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override Task Can_track_entity_with_complex_type_collections(EntityState state, bool async)
        => Task.CompletedTask;

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_mark_complex_type_collection_properties_modified(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_read_original_values_for_properties_of_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_write_original_values_for_properties_of_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override Task Can_track_entity_with_complex_struct_collections(EntityState state, bool async)
        => Task.CompletedTask;

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_mark_complex_struct_collection_properties_modified(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_read_original_values_for_properties_of_complex_struct_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_write_original_values_for_properties_of_complex_struct_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override Task Can_track_entity_with_complex_readonly_struct_collections(EntityState state, bool async)
        => Task.CompletedTask;

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_mark_complex_readonly_struct_collection_properties_modified(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_read_original_values_for_properties_of_readonly_struct_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_write_original_values_for_properties_of_readonly_struct_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override Task Can_track_entity_with_complex_readonly_struct_collections_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_mark_complex_readonly_struct_collections_with_fields_properties_modified(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_read_original_values_for_properties_of_complex_readonly_struct_collections_with_fields(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_write_original_values_for_properties_of_complex_readonly_struct_collections_with_fields(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_reordered_elements_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_removed_elements_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_replaced_elements_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_duplicates_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_handle_null_elements_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_changes_to_struct_collection_elements(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_changes_to_readonly_struct_collection_elements(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_handle_collection_with_mixed_null_and_duplicate_elements(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_changes_to_record_collection_elements(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_nested_collection_changes_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_changes_to_nested_teams_members_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_changes_to_nested_struct_teams_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_changes_to_nested_readonly_struct_teams_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_read_original_values_for_properties_of_complex_record_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_write_original_values_for_properties_of_complex_record_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_mark_complex_record_collection_properties_modified(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_swapped_complex_objects_in_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Throws_when_accessing_complex_entries_using_incorrect_cardinality()
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override Task Can_track_entity_with_complex_record_collections(EntityState state, bool async)
        => Task.CompletedTask;

    // Issue #36175: Complex types with notification change tracking are not supported
    public override Task Can_track_entity_with_complex_record_collections_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_mark_complex_record_collections_with_fields_properties_modified(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_read_original_values_for_properties_of_complex_record_collections_with_fields(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_write_original_values_for_properties_of_complex_record_collections_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied  
    public override Task Can_track_entity_with_complex_field_collections(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_complex_field_collections(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_mark_complex_field_collection_properties_modified(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_complex_field_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_detect_changes_to_record_teams_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_handle_empty_nested_teams_in_complex_type_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_mark_complex_property_bag_collection_properties_modified(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_read_original_values_for_properties_of_complex_property_bag_collections(bool trackFromQuery)
    {
    }

    // Issue #36175: Complex types with notification change tracking are not supported
    public override Task Can_track_entity_with_complex_property_bag_collections(EntityState state, bool async)
        => Task.CompletedTask;

    // Issue #36175: Complex types with notification change tracking are not supported
    public override void Can_write_original_values_for_properties_of_complex_property_bag_collections(bool trackFromQuery)
    {
    }

    public class SqlServerFixture : SqlServerFixtureBase
    {
        protected override string StoreName
            => nameof(ComplexTypesTrackingProxiesSqlServerTest);

        public override bool UseProxies
            => true;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.UseLazyLoadingProxies().UseChangeTrackingProxies());

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
    }
}

public abstract class ComplexTypesTrackingSqlServerTestBase<TFixture> : ComplexTypesTrackingRelationalTestBase<TFixture>
    where TFixture : ComplexTypesTrackingSqlServerTestBase<TFixture>.SqlServerFixtureBase, new()
{
    protected ComplexTypesTrackingSqlServerTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    public abstract class SqlServerFixtureBase : RelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
