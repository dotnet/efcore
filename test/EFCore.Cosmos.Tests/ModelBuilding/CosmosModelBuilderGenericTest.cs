// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class CosmosModelBuilderGenericTest : ModelBuilderGenericTest
    {
        public class CosmosGenericNonRelationship : GenericNonRelationship
        {
            public override void Properties_can_set_row_version()
            {
                // Fails due to ETags
            }

            public override void Properties_can_be_made_concurrency_tokens()
            {
                // Fails due to ETags
            }

            public override void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
            {
                // Fails due to extra shadow properties
            }

            [ConditionalFact]
            public virtual void Partition_key_is_added_to_the_keys()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders)
                    .HasPartitionKey(b => b.AlternateKey)
                    .Property(b => b.AlternateKey).HasConversion<string>();

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { nameof(Customer.Id), nameof(Customer.AlternateKey) },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Equal(
                    new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                    entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));

                var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName);
                Assert.Single(idProperty.GetContainingKeys());
                Assert.NotNull(idProperty.GetValueGeneratorFactory());
            }

            [ConditionalFact]
            public virtual void Partition_key_is_added_to_the_alternate_key_if_primary_key_contains_id()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders)
                    .HasPartitionKey(b => b.AlternateKey)
                    .Property(b => b.AlternateKey).HasConversion<string>();

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { StoreKeyConvention.DefaultIdPropertyName },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Equal(
                    new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                    entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));
            }

            [ConditionalFact]
            public virtual void No_id_property_created_if_another_property_mapped_to_id()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>()
                    .Property(c => c.Name)
                    .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders);

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
                Assert.Single(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

                var idProperty = entity.GetDeclaredProperties()
                    .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
                Assert.Single(idProperty.GetContainingKeys());
                Assert.NotNull(idProperty.GetValueGeneratorFactory());
            }

            [ConditionalFact]
            public virtual void No_id_property_created_if_another_property_mapped_to_id_in_pk()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>()
                    .Property(c => c.Name)
                    .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
                modelBuilder.Entity<Customer>()
                    .Ignore(c => c.Details)
                    .Ignore(c => c.Orders)
                    .HasKey(c => c.Name);

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
                Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

                var idProperty = entity.GetDeclaredProperties()
                    .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
                Assert.Single(idProperty.GetContainingKeys());
                Assert.Null(idProperty.GetValueGeneratorFactory());
            }

            [ConditionalFact]
            public virtual void No_alternate_key_is_created_if_primary_key_contains_id()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders);

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { StoreKeyConvention.DefaultIdPropertyName },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

                var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName);
                Assert.Single(idProperty.GetContainingKeys());
                Assert.Null(idProperty.GetValueGeneratorFactory());
            }

            [ConditionalFact]
            public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_partition_key()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName);
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders)
                    .HasPartitionKey(b => b.AlternateKey)
                    .Property(b => b.AlternateKey).HasConversion<string>();

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(CosmosTestHelpers.Instance);
        }

        public class CosmosGenericInheritance : GenericInheritance
        {
            public override void Can_set_and_remove_base_type()
            {
                // Fails due to presence of __jObject
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(CosmosTestHelpers.Instance);
        }

        public class CosmosGenericOneToMany : GenericOneToMany
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(CosmosTestHelpers.Instance);
        }

        public class CosmosGenericManyToOne : GenericManyToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(CosmosTestHelpers.Instance);
        }

        public class CosmosGenericOneToOne : GenericOneToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(CosmosTestHelpers.Instance);
        }

        public class CosmosGenericOwnedTypes : GenericOwnedTypes
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(CosmosTestHelpers.Instance);
        }

        public class CosmosGenericKeylessEntities : GenericKeylessEntities
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(CosmosTestHelpers.Instance);
        }
    }
}
