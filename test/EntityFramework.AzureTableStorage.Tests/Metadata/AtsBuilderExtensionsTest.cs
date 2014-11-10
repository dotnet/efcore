// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Metadata
{
    public class AtsBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_column_name_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForAzureTableStorage()
                .Column("Eman");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.AzureTableStorage().Column);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForAzureTableStorage()
                .Column(null);

            Assert.Equal("Name", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_column_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForAzureTableStorage(b => b.Column("Eman"));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_column_name_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForAzureTableStorage()
                .Column("Eman");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_column_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForAzureTableStorage(b => b.Column("Eman"));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Table("Customizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Table(null);

            Assert.Equal("Customer", entityType.AzureTableStorage().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.Table("Customizer"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Table("Customizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.Table("Customizer"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage()
                .Table("Customizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage(b => b.Table("Customizer"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage()
                .Table("Customizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage(b => b.Table("Customizer"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.AzureTableStorage().Table);
        }

        [Fact]
        public void Can_set_shadow_timestamp_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Timestamp("ShadowTimestamp", shadowProperty: true);

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ShadowTimestamp");

            Assert.True(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_shadow_timestamp_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.Timestamp("ShadowTimestamp", shadowProperty: true));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ShadowTimestamp");

            Assert.True(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_shadow_timestamp_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Timestamp("ShadowTimestamp", shadowProperty: true);

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ShadowTimestamp");

            Assert.True(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_shadow_timestamp_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.Timestamp("ShadowTimestamp", shadowProperty: true));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ShadowTimestamp");

            Assert.True(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_shadow_timestamp_with_basic_builder_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage()
                .Timestamp("ShadowTimestamp", shadowProperty: true);

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ShadowTimestamp");

            Assert.True(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_shadow_timestamp_with_basic_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage(b => b.Timestamp("ShadowTimestamp", shadowProperty: true));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ShadowTimestamp");

            Assert.True(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_shadow_timestamp_with_convention_builder_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage()
                .Timestamp("ShadowTimestamp", shadowProperty: true);

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ShadowTimestamp");

            Assert.True(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_shadow_timestamp_with_convention_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage(b => b.Timestamp("ShadowTimestamp", shadowProperty: true));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ShadowTimestamp");

            Assert.True(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_using_string_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Timestamp("ClrTimestamp");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_using_string_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.Timestamp("ClrTimestamp"));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_using_string_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Timestamp("ClrTimestamp");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_using_string_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.Timestamp("ClrTimestamp"));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_using_string_with_basic_builder_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage()
                .Timestamp("ClrTimestamp");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_using_string_with_basic_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage(b => b.Timestamp("ClrTimestamp"));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_using_string_with_convention_builder_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage()
                .Timestamp("ClrTimestamp");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_using_string_with_convention_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage(b => b.Timestamp("ClrTimestamp"));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Timestamp(e => e.ClrTimestamp);

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.Timestamp(e => e.ClrTimestamp));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .Timestamp(e => e.ClrTimestamp);

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Can_set_non_shadow_timestamp_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.Timestamp(e => e.ClrTimestamp));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("ClrTimestamp");

            Assert.False(property.IsShadowProperty);
            Assert.Equal(typeof(DateTimeOffset), property.PropertyType);
            Assert.Equal("Timestamp", property.AzureTableStorage().Column);
        }

        [Fact]
        public void Throws_if_existing_property_is_not_DateTimeOffset()
        {
            var modelBuilder = new BasicModelBuilder();

            Assert.Equal(
                Strings.BadTimestampType("Name", "Customer", "String"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<Customer>()
                        .ForAzureTableStorage()
                        .Timestamp(e => e.Name)).Message);
        }

        [Fact]
        public void Can_set_partition_and_row_key_using_string_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>(
                    b =>
                        {
                            b.Property(e => e.PKey);
                            b.Property(e => e.RKey);
                            b.ForAzureTableStorage().PartitionAndRowKey("PKey", "RKey");
                        });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_using_string_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>(
                    b =>
                        {
                            b.Property(e => e.PKey);
                            b.Property(e => e.RKey);
                            b.ForAzureTableStorage(ab => ab.PartitionAndRowKey("PKey", "RKey"));
                        });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_using_string_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage().PartitionAndRowKey("PKey", "RKey");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_using_string_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(ab => ab.PartitionAndRowKey("PKey", "RKey"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_using_string_with_basic_builder_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer),
                    b =>
                        {
                            b.Property<int>("PKey");
                            b.Property<int>("RKey");
                            b.ForAzureTableStorage().PartitionAndRowKey("PKey", "RKey");
                        });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_using_string_with_basic_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer),
                    b =>
                        {
                            b.Property<int>("PKey");
                            b.Property<int>("RKey");
                            b.ForAzureTableStorage(ab => ab.PartitionAndRowKey("PKey", "RKey"));
                        });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_using_string_with_convention_builder_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage().PartitionAndRowKey("PKey", "RKey");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_using_string_with_convention_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForAzureTableStorage(ab => ab.PartitionAndRowKey("PKey", "RKey"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .PartitionAndRowKey(e => e.PKey, e => e.RKey);

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.PartitionAndRowKey(e => e.PKey, e => e.RKey));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage()
                .PartitionAndRowKey(e => e.PKey, e => e.RKey);

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Can_set_partition_and_row_key_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage(b => b.PartitionAndRowKey(e => e.PKey, e => e.RKey));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));
            var partitionKey = entityType.GetProperty("PKey");
            var rowKey = entityType.GetProperty("RKey");

            Assert.Equal("PartitionKey", partitionKey.AzureTableStorage().Column);
            Assert.Equal("RowKey", rowKey.AzureTableStorage().Column);
            Assert.Equal(new[] { partitionKey, rowKey }, entityType.GetPrimaryKey().Properties);
        }

        [Fact]
        public void Generic_ATS_entity_builder_methods_return_generic_builder_for_chaining()
        {
            var modelBuilder = new ModelBuilder();

            var atsBuilder = modelBuilder
                .Entity<Customer>()
                .ForAzureTableStorage();

            AssertIsGenericBuilderType(atsBuilder.Table("Chair"));
            AssertIsGenericBuilderType(atsBuilder.Timestamp("ClrTimestamp"));
            AssertIsGenericBuilderType(atsBuilder.Timestamp(e => e.ClrTimestamp));
            AssertIsGenericBuilderType(atsBuilder.PartitionAndRowKey("PKey", "RKey"));
            AssertIsGenericBuilderType(atsBuilder.PartitionAndRowKey(e => e.PKey, e => e.RKey));
        }

        private static void AssertIsGenericBuilderType(AtsEntityBuilder<Customer> _)
        {
        }

        private class Customer
        {
            public int Id { get; set; }
            public int PKey { get; set; }
            public int RKey { get; set; }
            public string Name { get; set; }
            public DateTimeOffset ClrTimestamp { get; set; }
        }
    }
}
