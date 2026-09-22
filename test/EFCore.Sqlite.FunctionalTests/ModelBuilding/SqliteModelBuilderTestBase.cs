// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class SqliteModelBuilderTestBase : RelationalModelBuilderTest
{
    public abstract class SqliteNonRelationship(SqliteModelBuilderFixture fixture)
        : RelationalNonRelationshipTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>
    {
        [ConditionalFact]
        public void UseAutoincrement_sets_value_generation_strategy()
        {
            var modelBuilder = CreateModelBuilder();

            var propertyBuilder = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id);

            propertyBuilder.UseAutoincrement();

            Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, propertyBuilder.Metadata.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void Generic_UseAutoincrement_sets_value_generation_strategy()
        {
            var modelBuilder = CreateModelBuilder();

            var propertyBuilder = modelBuilder
                .Entity<Customer>()
                .Property<int>(e => e.Id);

            propertyBuilder.UseAutoincrement();

            Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, propertyBuilder.Metadata.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void Default_value_generation_strategy_for_integer_primary_key()
        {
            var modelBuilder = CreateModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            var model = modelBuilder.FinalizeModel();

            // With conventions, integer primary keys should get autoincrement
            Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, property.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void No_autoincrement_for_non_primary_key()
        {
            var modelBuilder = CreateModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.OtherId)
                .Metadata;

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void No_autoincrement_for_non_integer_primary_key()
        {
            var modelBuilder = CreateModelBuilder();

            var property = modelBuilder
                .Entity<CustomerWithStringKey>()
                .Property(e => e.Id)
                .Metadata;

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void No_autoincrement_for_composite_primary_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<CustomerWithCompositeKey>(b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                });

            var property1 = modelBuilder.Entity<CustomerWithCompositeKey>().Property(e => e.Id1).Metadata;
            var property2 = modelBuilder.Entity<CustomerWithCompositeKey>().Property(e => e.Id2).Metadata;

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(SqliteValueGenerationStrategy.None, property1.GetValueGenerationStrategy());
            Assert.Equal(SqliteValueGenerationStrategy.None, property2.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void No_autoincrement_when_default_value_set()
        {
            var modelBuilder = CreateModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValue(42)
                .Metadata;

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void No_autoincrement_when_default_value_sql_set()
        {
            var modelBuilder = CreateModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValueSql("1")
                .Metadata;

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void No_autoincrement_when_computed_column_sql_set()
        {
            var modelBuilder = CreateModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasComputedColumnSql("1")
                .Metadata;

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
        }

        [ConditionalFact]
        public void No_autoincrement_when_property_is_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Order>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.CustomerId);
                b.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId);
            });

            var property = modelBuilder.Entity<Order>().Property(e => e.CustomerId).Metadata;

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
        }

        private class Customer
        {
            public int Id { get; set; }
            public int OtherId { get; set; }
            public string? Name { get; set; }
        }

        private class CustomerWithStringKey
        {
            public string Id { get; set; } = null!;
            public string? Name { get; set; }
        }

        private class CustomerWithCompositeKey
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public string? Name { get; set; }
        }

        private class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
        }
    }

    public abstract class SqliteComplexType(SqliteModelBuilderFixture fixture)
        : RelationalComplexTypeTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteComplexCollection(SqliteModelBuilderFixture fixture)
        : RelationalComplexCollectionTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteInheritance(SqliteModelBuilderFixture fixture)
        : RelationalInheritanceTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteOneToMany(SqliteModelBuilderFixture fixture)
        : RelationalOneToManyTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteManyToOne(SqliteModelBuilderFixture fixture)
        : RelationalManyToOneTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteOneToOne(SqliteModelBuilderFixture fixture)
        : RelationalOneToOneTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteManyToMany(SqliteModelBuilderFixture fixture)
        : RelationalManyToManyTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>;

    public abstract class SqliteOwnedTypes(SqliteModelBuilderFixture fixture)
        : RelationalOwnedTypesTestBase(fixture), IClassFixture<SqliteModelBuilderFixture>
    {
        public override void Can_use_sproc_mapping_with_owned_reference()
            => Assert.Equal(
                SqliteStrings.StoredProceduresNotSupported("Book.Label#BookLabel"),
                Assert.Throws<InvalidOperationException>(base.Can_use_sproc_mapping_with_owned_reference).Message);
    }

    public class SqliteModelBuilderFixture : RelationalModelBuilderFixture
    {
        public override TestHelpers TestHelpers
            => SqliteTestHelpers.Instance;
    }
}
