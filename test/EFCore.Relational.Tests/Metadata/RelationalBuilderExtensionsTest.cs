// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalBuilderExtensionsTest
    {
        [ConditionalFact]
        public void Can_set_fixed_length()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .IsFixedLength();

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.True(property.IsFixedLength());

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .IsFixedLength(false);

            Assert.False(property.IsFixedLength());
        }

        [ConditionalFact]
        public void Can_write_index_builder_extension_with_where_clauses()
        {
            var builder = CreateConventionModelBuilder();

            var returnedBuilder = builder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .HasFilter("[Id] % 2 = 0");

            Assert.IsType<IndexBuilder<Customer>>(returnedBuilder);

            var model = builder.Model;
            var index = model.FindEntityType(typeof(Customer)).GetIndexes().Single();
            Assert.Equal("[Id] % 2 = 0", index.GetFilter());
        }

        [ConditionalFact]
        public void Can_set_column_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnName("Eman");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.GetColumnBaseName());
        }

        [ConditionalFact]
        public void Can_set_column_type()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnType("nvarchar(42)");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("nvarchar(42)", property.GetColumnType());
        }

        [ConditionalFact]
        public void Can_set_column_default_expression()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValueSql("CherryCoke");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("CherryCoke", property.GetDefaultValueSql());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Setting_column_default_expression_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CherryCoke");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("CherryCoke", property.GetDefaultValueSql());
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Can_set_column_computed_expression()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasComputedColumnSql("CherryCoke");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("CherryCoke", property.GetComputedColumnSql());
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Setting_column_computed_expression_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ValueGeneratedNever()
                .HasComputedColumnSql("CherryCoke");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("CherryCoke", property.GetComputedColumnSql());
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Can_set_column_default_value()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var stringValue = "DefaultValueString";

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValue(stringValue);

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal(stringValue, property.GetDefaultValue());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Can_set_column_default_value_implicit_conversion()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.SomeShort)
                .HasDefaultValue(7);

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("SomeShort");

            Assert.Equal((short)7, property.GetDefaultValue());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Setting_column_default_value_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var stringValue = "DefaultValueString";

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValue(stringValue);

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal(stringValue, property.GetDefaultValue());
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Can_set_column_default_value_of_enum_type()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.EnumValue)
                .HasDefaultValue(MyEnum.Tue);

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("EnumValue");

            Assert.Equal(typeof(MyEnum), property.GetDefaultValue().GetType());
            Assert.Equal(MyEnum.Tue, property.GetDefaultValue());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Default_alternate_key_name_is_based_on_key_column_names()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasAlternateKey(e => e.Name);

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            var key = entityType.FindKey(entityType.FindProperty(nameof(Customer.Name)));

            Assert.Equal("AK_Customer_Name", key.GetName());

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnName("Pie");

            Assert.Equal("AK_Customer_Pie", key.GetName());

            modelBuilder
                .Entity<Customer>()
                .HasAlternateKey(e => e.Name)
                .HasName("KeyLimePie");

            Assert.Equal("KeyLimePie", key.GetName());
        }

        [ConditionalFact]
        public void Can_set_key_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .HasName("KeyLimePie");

            var key = modelBuilder.Model.FindEntityType(typeof(Customer)).FindPrimaryKey();

            Assert.Equal("KeyLimePie", key.GetName());
        }

        [ConditionalFact]
        public void Default_foreign_key_name_is_based_on_fk_column_names()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId);

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
                .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.GetConstraintName());

            modelBuilder
                .Entity<Order>().Property(e => e.CustomerId).HasColumnName("CID");

            Assert.Equal("FK_Order_Customer_CID", foreignKey.GetConstraintName());
        }

        [ConditionalFact]
        public void Can_set_foreign_key_name_for_one_to_many()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasConstraintName("LemonSupreme");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
                .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasConstraintName(null);

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.GetConstraintName());
        }

        [ConditionalFact]
        public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("LemonSupreme");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
                .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());
        }

        [ConditionalFact]
        public void Can_set_foreign_key_name_for_many_to_one()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasConstraintName("LemonSupreme");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
                .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasConstraintName(null);

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.GetConstraintName());
        }

        [ConditionalFact]
        public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("LemonSupreme");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
                .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());
        }

        [ConditionalFact]
        public void Can_set_foreign_key_name_for_one_to_one()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasPrincipalKey<Order>(e => e.OrderId)
                .HasConstraintName("LemonSupreme");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

            Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasConstraintName(null);

            Assert.Equal("FK_OrderDetails_Order_OrderId", foreignKey.GetConstraintName());
        }

        [ConditionalFact]
        public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasForeignKey<OrderDetails>(e => e.Id)
                .HasConstraintName("LemonSupreme");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

            Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());
        }

        [ConditionalFact]
        public void Default_index_database_name_is_based_on_index_column_names()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id);

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.Equal("IX_Customer_Id", index.GetDatabaseName());

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasColumnName("Eendax");

            Assert.Equal("IX_Customer_Eendax", index.GetDatabaseName());
        }

        [ConditionalFact]
        public void Can_set_index_database_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .HasDatabaseName("Eeeendeeex");

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.Equal("Eeeendeeex", index.GetDatabaseName());
        }

        [ConditionalFact]
        public void Can_set_table_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.GetTableName());
        }

        [ConditionalFact]
        public void Can_set_table_name_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Customizer");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.GetTableName());
        }

        [ConditionalFact]
        public void Can_set_table_and_schema_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer", "db0");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.GetTableName());
            Assert.Equal("db0", entityType.GetSchema());
        }

        [ConditionalFact]
        public void Can_set_table_and_schema_name_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Customizer", "db0");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.GetTableName());
            Assert.Equal("db0", entityType.GetSchema());
        }

        [ConditionalFact]
        public void Can_create_check_constraint()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var entityType = modelBuilder.Entity<Customer>().Metadata;

            modelBuilder
                .Entity<Customer>()
                .HasCheckConstraint("CK_Customer_AlternateId", "AlternateId > Id");

            var checkConstraint = entityType.FindCheckConstraint("CK_Customer_AlternateId");

            Assert.NotNull(checkConstraint);
            Assert.Equal(entityType, checkConstraint.EntityType);
            Assert.Equal("CK_Customer_AlternateId", checkConstraint.Name);
            Assert.Equal("AlternateId > Id", checkConstraint.Sql);
        }

        [ConditionalFact]
        public void Can_create_check_constraint_with_duplicate_name_replaces_existing()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var entityType = modelBuilder.Entity<Customer>().Metadata;

            modelBuilder
                .Entity<Customer>()
                .HasCheckConstraint("CK_Customer_AlternateId", "AlternateId > Id");

            modelBuilder
                .Entity<Customer>()
                .HasCheckConstraint("CK_Customer_AlternateId", "AlternateId < Id");

            var checkConstraint = entityType.FindCheckConstraint("CK_Customer_AlternateId");

            Assert.NotNull(checkConstraint);
            Assert.Equal(entityType, checkConstraint.EntityType);
            Assert.Equal("CK_Customer_AlternateId", checkConstraint.Name);
            Assert.Equal("AlternateId < Id", checkConstraint.Sql);
        }

        [ConditionalFact]
        public void AddCheckConstraint_with_duplicate_names_throws_exception()
        {
            var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
            var entityType = entityTypeBuilder.Metadata;

            entityType.AddCheckConstraint("CK_Customer_AlternateId", "AlternateId > Id");

            Assert.Equal(
                RelationalStrings.DuplicateCheckConstraint("CK_Customer_AlternateId", entityType.DisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        entityType.AddCheckConstraint("CK_Customer_AlternateId", "AlternateId < Id")).Message);
        }

        [ConditionalFact]
        public void RemoveCheckConstraint_returns_constraint_when_constraint_exists()
        {
            var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
            var entityType = entityTypeBuilder.Metadata;

            var constraint = entityType.AddCheckConstraint("CK_Customer_AlternateId", "AlternateId > Id");

            Assert.Same(constraint, entityType.RemoveCheckConstraint("CK_Customer_AlternateId"));
        }

        [ConditionalFact]
        public void RemoveCheckConstraint_returns_null_when_constraint_is_missing()
        {
            var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
            var entityType = entityTypeBuilder.Metadata;

            Assert.Null(entityType.RemoveCheckConstraint("CK_Customer_AlternateId"));
        }

        [ConditionalFact]
        public void Can_set_discriminator_value_using_property_expression()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasDiscriminator(b => b.Name)
                .HasValue(typeof(Customer), "1")
                .HasValue(typeof(SpecialCustomer), "2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Name", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_discriminator_value_using_property_expression_separately()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasDiscriminator(b => b.Name)
                .HasValue("1");

            modelBuilder
                .Entity<SpecialCustomer>()
                .HasBaseType<Customer>()
                .HasDiscriminator(b => b.Name)
                .HasValue("2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Name", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_discriminator_value_using_property_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasDiscriminator("Name", typeof(string))
                .HasValue(typeof(Customer), "1")
                .HasValue(typeof(SpecialCustomer), "2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Name", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_discriminator_value_using_property_name_separately()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasDiscriminator<string>("Name")
                .HasValue("1");

            modelBuilder
                .Entity<SpecialCustomer>()
                .HasBaseType<Customer>()
                .HasDiscriminator<string>("Name")
                .HasValue("2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Name", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_discriminator_value_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .HasDiscriminator("Name", typeof(string))
                .HasValue(typeof(Customer), "1")
                .HasValue(typeof(SpecialCustomer), "2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Name", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_discriminator_value_non_generic_separately()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .HasDiscriminator("Name", typeof(string))
                .HasValue(typeof(Customer), "1");

            modelBuilder
                .Entity(typeof(SpecialCustomer))
                .HasBaseType(typeof(Customer))
                .HasDiscriminator("Name", typeof(string))
                .HasValue(typeof(SpecialCustomer), "2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Name", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_discriminator_value_shadow_entity()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer).FullName)
                .HasDiscriminator("Name", typeof(string))
                .HasValue(typeof(Customer).FullName, "1")
                .HasValue(typeof(SpecialCustomer).FullName, "2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Name", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_default_discriminator_value()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .HasDiscriminator()
                .HasValue(typeof(Customer), "1")
                .HasValue(typeof(SpecialCustomer), "2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Discriminator", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_default_discriminator_value_separately()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .HasDiscriminator()
                .HasValue("1");

            modelBuilder
                .Entity(typeof(SpecialCustomer))
                .HasDiscriminator()
                .HasValue("2");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            Assert.Equal("Discriminator", entityType.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), entityType.GetDiscriminatorProperty().ClrType);
            Assert.Equal("1", entityType.GetDiscriminatorValue());
            Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Can_set_schema_on_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            Assert.Null(modelBuilder.Model.GetDefaultSchema());

            modelBuilder.HasDefaultSchema("db0");

            Assert.Equal("db0", modelBuilder.Model.GetDefaultSchema());
        }

        [ConditionalFact]
        public void Model_schema_is_used_if_table_schema_not_set()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.GetTableName());
            Assert.Null(entityType.GetSchema());

            modelBuilder.HasDefaultSchema("db0");

            Assert.Equal("db0", modelBuilder.Model.GetDefaultSchema());
            Assert.Equal("Customizer", entityType.GetTableName());
            Assert.Equal("db0", entityType.GetSchema());
        }

        [ConditionalFact]
        public void Model_schema_is_not_used_if_table_schema_is_set()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.HasDefaultSchema("db0");

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer", "db1");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("db0", modelBuilder.Model.GetDefaultSchema());
            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.GetTableName());
            Assert.Equal("db1", entityType.GetSchema());
        }

        [ConditionalFact]
        public void Sequence_is_in_model_schema_if_not_specified_explicitly()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.HasDefaultSchema("Tasty");
            modelBuilder.HasSequence("Snook");

            var sequence = modelBuilder.Model.FindSequence("Snook");

            Assert.Equal("Tasty", modelBuilder.Model.GetDefaultSchema());
            ValidateSchemaNamedSequence(sequence);
        }

        [ConditionalFact]
        public void Sequence_is_not_in_model_schema_if_specified_explicitly()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.HasDefaultSchema("db0");
            modelBuilder.HasSequence("Snook", "Tasty");

            var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

            Assert.Equal("db0", modelBuilder.Model.GetDefaultSchema());
            ValidateSchemaNamedSequence(sequence);
        }

        [ConditionalFact]
        public void Can_create_named_sequence()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.HasSequence("Snook");

            var sequence = modelBuilder.Model.FindSequence("Snook");

            ValidateNamedSequence(sequence);
        }

        private static void ValidateNamedSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);
        }

        [ConditionalFact]
        public void Can_create_schema_named_sequence()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.HasSequence("Snook", "Tasty");

            var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSequence(sequence);
        }

        private static void ValidateSchemaNamedSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);
        }

        [ConditionalFact]
        public void Can_create_named_sequence_with_specific_facets()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            var sequence = modelBuilder.Model.FindSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        [ConditionalFact]
        public void Can_create_named_sequence_with_specific_facets_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence(typeof(int), "Snook")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            var sequence = modelBuilder.Model.FindSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        [ConditionalFact]
        public void Can_create_named_sequence_with_specific_facets_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>(
                    "Snook", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    });

            var sequence = modelBuilder.Model.FindSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        [ConditionalFact]
        public void Can_create_named_sequence_with_specific_facets_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence(
                    typeof(int), "Snook", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    });

            var sequence = modelBuilder.Model.FindSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        private static void ValidateNamedSpecificSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [ConditionalFact]
        public void Can_create_schema_named_sequence_with_specific_facets()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [ConditionalFact]
        public void Can_create_schema_named_sequence_with_specific_facets_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence(typeof(int), "Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [ConditionalFact]
        public void Can_create_schema_named_sequence_with_specific_facets_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty", b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222));

            var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [ConditionalFact]
        public void Can_create_schema_named_sequence_with_specific_facets_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence(typeof(int), "Snook", "Tasty", b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222));

            var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [ConditionalFact]
        public void Can_create_dbFunction()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var testMethod = typeof(TestDbFunctions).GetTypeInfo().GetDeclaredMethod(nameof(TestDbFunctions.MethodA));
            modelBuilder.HasDbFunction(testMethod);

            var dbFunc = modelBuilder.Model.FindDbFunction(testMethod) as DbFunction;

            Assert.NotNull(dbFunc);
            Assert.Equal("MethodA", dbFunc.Name);
            Assert.Null(dbFunc.Schema);
        }

        [ConditionalFact]
        public void Relational_entity_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ToTable("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ToTable("Jay", "Simon"));
        }

        [ConditionalFact]
        public void Relational_entity_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Will");

            modelBuilder
                .Entity<Customer>()
                .ToTable("Jay", "Simon");
        }

        [ConditionalFact]
        public void Relational_property_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasColumnName("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasColumnType("Jay"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasDefaultValueSql("Simon"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasComputedColumnSql("Simon"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasDefaultValue("Neil"));
        }

        [ConditionalFact]
        public void Relational_property_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasColumnName("Will");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasColumnName("Jay");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasColumnType("Simon");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasColumnType("Neil");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasDefaultValueSql("Simon");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasDefaultValueSql("Neil");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasComputedColumnSql("Simon");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasComputedColumnSql("Neil");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasDefaultValue("Simon");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasDefaultValue("Neil");
        }

        [ConditionalFact]
        public void Relational_relationship_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders)
                    .WithOne(e => e.Customer)
                    .HasConstraintName("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Order>()
                    .HasOne(e => e.Customer)
                    .WithMany(e => e.Orders)
                    .HasConstraintName("Jay"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Order>()
                    .HasOne(e => e.Details)
                    .WithOne(e => e.Order)
                    .HasConstraintName("Simon"));
        }

        [ConditionalFact]
        public void Relational_relationship_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(typeof(Order), "Orders")
                .WithOne("Customer")
                .HasConstraintName("Will");

            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .HasConstraintName("Jay");

            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Details)
                .WithOne(e => e.Order)
                .HasConstraintName("Simon");
        }

        private void AssertIsGeneric(EntityTypeBuilder<Customer> _)
        {
        }

        private void AssertIsGeneric(PropertyBuilder<string> _)
        {
        }

        private void AssertIsGeneric(ReferenceCollectionBuilder<Customer, Order> _)
        {
        }

        private void AssertIsGeneric(ReferenceReferenceBuilder<Order, OrderDetails> _)
        {
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder();

        private static void ValidateSchemaNamedSpecificSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        private enum MyEnum : ulong
        {
            Sun,
            Mon,
            Tue
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public short SomeShort { get; set; }
            public MyEnum EnumValue { get; set; }

            public IEnumerable<Order> Orders { get; set; }
        }

        private class SpecialCustomer : Customer
        {
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }
    }
}
