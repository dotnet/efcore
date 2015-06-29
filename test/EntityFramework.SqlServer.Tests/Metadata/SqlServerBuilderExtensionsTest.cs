// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata
{
    public class SqlServerBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_column_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnName("Eman");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasSqlServerColumnName("MyNameIs");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("MyNameIs", property.SqlServer().Column);
        }

        [Fact]
        public void Can_set_column_type_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnType("nvarchar(42)");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasSqlServerColumnType("nvarchar(DA)");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("nvarchar(42)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(DA)", property.SqlServer().ColumnType);
        }

        [Fact]
        public void Can_set_column_default_expression_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .DefaultValueSql("CherryCoke");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .SqlServerDefaultValueSql("VanillaCoke");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("CherryCoke", property.Relational().DefaultValueSql);
            Assert.Equal("VanillaCoke", property.SqlServer().DefaultValueSql);
        }

        [Fact]
        public void Can_set_column_default_value_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .DefaultValue(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)));

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .SqlServerDefaultValue(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)), property.Relational().DefaultValue);
            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.SqlServer().DefaultValue);
        }

        [Fact]
        public void Can_set_key_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .KeyName("KeyLimePie")
                .SqlServerKeyName("LemonSupreme");

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.Equal("KeyLimePie", key.Relational().Name);
            Assert.Equal("LemonSupreme", key.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                .ConstraintName("LemonSupreme")
                .SqlServerConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                .SqlServerConstraintName(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ConstraintName("LemonSupreme")
                .SqlServerConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().Reference(e => e.Customer).InverseCollection(e => e.Orders)
                .ConstraintName("LemonSupreme")
                .SqlServerConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Order>().Reference(e => e.Customer).InverseCollection(e => e.Orders)
                .SqlServerConstraintName(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().Reference(e => e.Customer).InverseCollection(e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ConstraintName("LemonSupreme")
                .SqlServerConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().Reference(e => e.Details).InverseReference(e => e.Order)
                .PrincipalKey<Order>(e => e.OrderId)
                .ConstraintName("LemonSupreme")
                .SqlServerConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Order>().Reference(e => e.Details).InverseReference(e => e.Order)
                .SqlServerConstraintName(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().Reference(e => e.Details).InverseReference(e => e.Order)
                .ForeignKey<OrderDetails>(e => e.Id)
                .ConstraintName("LemonSupreme")
                .SqlServerConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_index_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .IndexName("Eeeendeeex")
                .SqlServerIndexName("Dexter");

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.Equal("Eeeendeeex", index.Relational().Name);
            Assert.Equal("Dexter", index.SqlServer().Name);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer")
                .ToSqlServerTable("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Customizer")
                .ToSqlServerTable("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer", "db0")
                .ToSqlServerTable("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Customizer", "db0")
                .ToSqlServerTable("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_index_clustering_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .SqlServerClustered();

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.True(index.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_key_clustering_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .SqlServerClustered();

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.True(key.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_sequences_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.UseSqlServerSequenceHiLo();

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, sqlServerExtensions.IdentityStrategy);
            Assert.Same(Sequence.DefaultName, sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.NotNull(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        private static void ValidateDefaultSequence(Sequence sequence)
        {
            Assert.Equal(Sequence.DefaultName, sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(Sequence.DefaultIncrement, sequence.IncrementBy);
            Assert.Equal(Sequence.DefaultStartValue, sequence.StartValue);
            Assert.Same(Sequence.DefaultType, sequence.Type);
        }

        [Fact]
        public void Can_set_sequences_with_name_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.UseSqlServerSequenceHiLo("Snook");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, sqlServerExtensions.IdentityStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook"));
            ValidateNamedSequence(sqlServerExtensions.TryGetSequence("Snook"));
        }

        private static void ValidateNamedSequence(Sequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(Sequence.DefaultIncrement, sequence.IncrementBy);
            Assert.Equal(Sequence.DefaultStartValue, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(Sequence.DefaultType, sequence.Type);
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.UseSqlServerSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, sqlServerExtensions.IdentityStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        private static void ValidateSchemaNamedSequence(Sequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(Sequence.DefaultIncrement, sequence.IncrementBy);
            Assert.Equal(Sequence.DefaultStartValue, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(Sequence.DefaultType, sequence.Type);
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder.UseSqlServerSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, sqlServerExtensions.IdentityStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            ValidateSchemaNamedSpecificSequence(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .SqlServerSequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder.UseSqlServerSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, sqlServerExtensions.IdentityStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        private static void ValidateSchemaNamedSpecificSequence(Sequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_set_identities_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.UseSqlServerIdentityColumns();

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerIdentityStrategy.IdentityColumn, sqlServerExtensions.IdentityStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequence_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo();

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Same(Sequence.DefaultName, property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.NotNull(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("Snook");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Null(property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook"));
            ValidateNamedSequence(model.SqlServer().TryGetSequence("Snook"));
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Sequence("Snook", "Tasty", b => b.IncrementBy(11).Start(1729).Min(111).Max(2222).Type<int>())
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .SqlServerSequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .SqlServerSequence("Snook", "Tasty", b =>
                    {
                        b.IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    })
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_identities_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerIdentityColumn();

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.IdentityColumn, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_identities_for_property_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseSqlServerIdentityColumn();

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerIdentityStrategy.IdentityColumn, property.SqlServer().IdentityStrategy);
            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_create_named_sequence_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.SqlServerSequence("Snook");

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook");

            ValidateNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.SqlServerSequence("Snook", "Tasty");

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSequence(sequence);
        }

        private static void ValidateDefaultSpecificSequence(Sequence sequence)
        {
            Assert.Equal(Sequence.DefaultName, sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .SqlServerSequence("Snook")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .SqlServerSequence("Snook", b =>
                    {
                        b.IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        private static void ValidateNamedSpecificSequence(Sequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .SqlServerSequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .SqlServerSequence("Snook", "Tasty", b =>
                    {
                        b.IncrementBy(11).Start(1729).Min(111).Max(2222).Type<int>();
                    });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [Fact]
        public void SqlServer_entity_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ToSqlServerTable("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ToSqlServerTable("Jay", "Simon"));
        }

        [Fact]
        public void SqlServer_entity_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToSqlServerTable("Will");

            modelBuilder
                .Entity<Customer>()
                .ToSqlServerTable("Jay", "Simon");
        }

        [Fact]
        public void SqlServer_property_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasSqlServerColumnName("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .HasSqlServerColumnType("Jay"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .SqlServerDefaultValueSql("Simon"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .SqlServerComputedExpression("Simon"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .SqlServerDefaultValue("Neil"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseSqlServerSequenceHiLo());

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseSqlServerIdentityColumn());
        }

        [Fact]
        public void SqlServer_property_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .HasSqlServerColumnName("Will");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasSqlServerColumnName("Jay");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasSqlServerColumnType("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .HasSqlServerColumnType("Neil");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .SqlServerDefaultValueSql("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .SqlServerDefaultValueSql("Neil");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .SqlServerDefaultValue("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .SqlServerDefaultValue("Neil");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .SqlServerComputedExpression("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(int), "Id")
                .UseSqlServerSequenceHiLo();

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(int), "Id")
                .UseSqlServerIdentityColumn();
        }

        [Fact]
        public void SqlServer_relationship_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>().Collection(e => e.Orders)
                    .InverseReference(e => e.Customer)
                    .SqlServerConstraintName("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Order>()
                    .Reference(e => e.Customer)
                    .InverseCollection(e => e.Orders)
                    .SqlServerConstraintName("Jay"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Order>()
                    .Reference(e => e.Details)
                    .InverseReference(e => e.Order)
                    .SqlServerConstraintName("Simon"));
        }

        [Fact]
        public void SqlServer_relationship_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().Collection(typeof(Order), "Orders")
                .InverseReference("Customer")
                .SqlServerConstraintName("Will");

            modelBuilder
                .Entity<Order>()
                .Reference(e => e.Customer)
                .InverseCollection(e => e.Orders)
                .SqlServerConstraintName("Jay");

            modelBuilder
                .Entity<Order>()
                .Reference(e => e.Details)
                .InverseReference(e => e.Order)
                .SqlServerConstraintName("Simon");
        }

        private void AssertIsGeneric(EntityTypeBuilder<Customer> _)
        {
        }

        private void AssertIsGeneric(PropertyBuilder<string> _)
        {
        }

        private void AssertIsGeneric(PropertyBuilder<int> _)
        {
        }

        private void AssertIsGeneric(ReferenceCollectionBuilder<Customer, Order> _)
        {
        }
        
        private void AssertIsGeneric(ReferenceReferenceBuilder<Order, OrderDetails> _)
        {
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
        {
            return SqlServerTestHelpers.Instance.CreateConventionBuilder();
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public IEnumerable<Order> Orders { get; set; }
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
