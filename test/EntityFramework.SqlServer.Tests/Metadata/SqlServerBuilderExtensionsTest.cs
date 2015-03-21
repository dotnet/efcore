// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata
{
    public class SqlServerBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_column_name_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational()
                .Column("Eman");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .Column("MyNameIs");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("MyNameIs", property.SqlServer().Column);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .Column(null);

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("Eman", property.SqlServer().Column);
        }

        [Fact]
        public void Can_set_column_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational(b => { b.Column("Eman"); })
                .ForSqlServer(b => { b.Column("MyNameIs"); });

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("MyNameIs", property.SqlServer().Column);
        }

        [Fact]
        public void Can_set_column_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational()
                .Column("Eman");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .Column("MyNameIs");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("MyNameIs", property.SqlServer().Column);
        }

        [Fact]
        public void Can_set_column_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational(b => { b.Column("Eman"); })
                .ForSqlServer(b => { b.Column("MyNameIs"); });

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("MyNameIs", property.SqlServer().Column);
        }

        [Fact]
        public void Can_set_column_type_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational()
                .ColumnType("nvarchar(42)");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .ColumnType("nvarchar(DA)");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("nvarchar(42)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(DA)", property.SqlServer().ColumnType);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .ColumnType(null);

            Assert.Equal("nvarchar(42)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(42)", property.SqlServer().ColumnType);
        }

        [Fact]
        public void Can_set_column_type_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational(b => { b.ColumnType("nvarchar(42)"); })
                .ForSqlServer(b => { b.ColumnType("nvarchar(DA)"); });

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("nvarchar(42)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(DA)", property.SqlServer().ColumnType);
        }

        [Fact]
        public void Can_set_column_type_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational()
                .ColumnType("nvarchar(42)");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .ColumnType("nvarchar(DA)");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("nvarchar(42)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(DA)", property.SqlServer().ColumnType);
        }

        [Fact]
        public void Can_set_column_type_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational(b => { b.ColumnType("nvarchar(42)"); })
                .ForSqlServer(b => { b.ColumnType("nvarchar(DA)"); });

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("nvarchar(42)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(DA)", property.SqlServer().ColumnType);
        }

        [Fact]
        public void Can_set_column_default_expression_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational()
                .DefaultExpression("CherryCoke");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .DefaultExpression("VanillaCoke");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("CherryCoke", property.Relational().DefaultExpression);
            Assert.Equal("VanillaCoke", property.SqlServer().DefaultExpression);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .DefaultExpression(null);

            Assert.Equal("CherryCoke", property.Relational().DefaultExpression);
            Assert.Equal("CherryCoke", property.SqlServer().DefaultExpression);
        }

        [Fact]
        public void Can_set_column_default_expression_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational(b => { b.DefaultExpression("CherryCoke"); })
                .ForSqlServer(b => { b.DefaultExpression("VanillaCoke"); });

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("CherryCoke", property.Relational().DefaultExpression);
            Assert.Equal("VanillaCoke", property.SqlServer().DefaultExpression);
        }

        [Fact]
        public void Can_set_column_default_expression_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational()
                .DefaultExpression("CherryCoke");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .DefaultExpression("VanillaCoke");

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("CherryCoke", property.Relational().DefaultExpression);
            Assert.Equal("VanillaCoke", property.SqlServer().DefaultExpression);
        }

        [Fact]
        public void Can_set_column_default_expression_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational(b => { b.DefaultExpression("CherryCoke"); })
                .ForSqlServer(b => { b.DefaultExpression("VanillaCoke"); });

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal("CherryCoke", property.Relational().DefaultExpression);
            Assert.Equal("VanillaCoke", property.SqlServer().DefaultExpression);
        }

        [Fact]
        public void Can_set_column_default_value_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational()
                .DefaultValue(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)));

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .DefaultValue(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)), property.Relational().DefaultValue);
            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.SqlServer().DefaultValue);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .DefaultValue(null);

            Assert.Equal(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)), property.Relational().DefaultValue);
            Assert.Equal(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)), property.SqlServer().DefaultValue);
        }

        [Fact]
        public void Can_set_column_default_value_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational(b => { b.DefaultValue(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0))); })
                .ForSqlServer(b => { b.DefaultValue(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0))); });

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)), property.Relational().DefaultValue);
            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.SqlServer().DefaultValue);
        }

        [Fact]
        public void Can_set_column_default_value_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational()
                .DefaultValue(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)));

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqlServer()
                .DefaultValue(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)));

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)), property.Relational().DefaultValue);
            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.SqlServer().DefaultValue);
        }

        [Fact]
        public void Can_set_column_default_value_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForRelational(b => { b.DefaultValue(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0))); })
                .ForSqlServer(b => { b.DefaultValue(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0))); });

            var property = modelBuilder.Model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal(new DateTimeOffset(1973, 9, 3, 0, 10, 0, new TimeSpan(1, 0, 0)), property.Relational().DefaultValue);
            Assert.Equal(new DateTimeOffset(2006, 9, 19, 19, 0, 0, new TimeSpan(-8, 0, 0)), property.SqlServer().DefaultValue);
        }

        [Fact]
        public void Can_set_key_name_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForRelational()
                .Name("KeyLimePie");

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForSqlServer()
                .Name("LemonSupreme");

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.Equal("KeyLimePie", key.Relational().Name);
            Assert.Equal("LemonSupreme", key.SqlServer().Name);

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("KeyLimePie", key.Relational().Name);
            Assert.Equal("KeyLimePie", key.SqlServer().Name);
        }

        [Fact]
        public void Can_set_key_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForRelational(b => { b.Name("KeyLimePie"); })
                .ForSqlServer(b => { b.Name("LemonSupreme"); });

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.Equal("KeyLimePie", key.Relational().Name);
            Assert.Equal("LemonSupreme", key.SqlServer().Name);
        }

        [Fact]
        public void Can_set_key_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForRelational()
                .Name("KeyLimePie");

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForSqlServer()
                .Name("LemonSupreme");

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.Equal("KeyLimePie", key.Relational().Name);
            Assert.Equal("LemonSupreme", key.SqlServer().Name);
        }

        [Fact]
        public void Can_set_key_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForRelational(b => { b.Name("KeyLimePie"); })
                .ForSqlServer(b => { b.Name("LemonSupreme"); });

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.Equal("KeyLimePie", key.Relational().Name);
            Assert.Equal("LemonSupreme", key.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id);

            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(e => e.CustomerId)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(e => e.CustomerId)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(e => e.CustomerId)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id);

            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(e => e.CustomerId)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single(fk => fk.ReferencedEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single(fk => fk.ReferencedEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single(fk => fk.ReferencedEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single(fk => fk.ReferencedEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single(fk => fk.ReferencedEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single(fk => fk.ReferencedEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single(fk => fk.ReferencedEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single(fk => fk.ReferencedEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ReferencedKey<Order>(e => e.OrderId)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(OrderDetails)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForeignKey<OrderDetails>(e => e.Id)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForeignKey<OrderDetails>(e => e.Id)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(OrderDetails)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ReferencedKey<Order>(e => e.OrderId)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(OrderDetails)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForeignKey<OrderDetails>(e => e.Id)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(OrderDetails)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_index_name_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForRelational()
                .Name("Eeeendeeex");

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForSqlServer()
                .Name("Dexter");

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.Equal("Eeeendeeex", index.Relational().Name);
            Assert.Equal("Dexter", index.SqlServer().Name);

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("Eeeendeeex", index.Relational().Name);
            Assert.Equal("Eeeendeeex", index.SqlServer().Name);
        }

        [Fact]
        public void Can_set_index_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForRelational(b => { b.Name("Eeeendeeex"); })
                .ForSqlServer(b => { b.Name("Dexter"); });

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.Equal("Eeeendeeex", index.Relational().Name);
            Assert.Equal("Dexter", index.SqlServer().Name);
        }

        [Fact]
        public void Can_set_index_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForRelational()
                .Name("Eeeendeeex");

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForSqlServer()
                .Name("Dexter");

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.Equal("Eeeendeeex", index.Relational().Name);
            Assert.Equal("Dexter", index.SqlServer().Name);
        }

        [Fact]
        public void Can_set_index_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForRelational(b => { b.Name("Eeeendeeex"); })
                .ForSqlServer(b => { b.Name("Dexter"); });

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.Equal("Eeeendeeex", index.Relational().Name);
            Assert.Equal("Dexter", index.SqlServer().Name);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational()
                .Table("Customizer");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table(null);

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational(b => { b.Table("Customizer"); })
                .ForSqlServer(b => { b.Table("Custardizer"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational()
                .Table("Customizer");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational(b => { b.Table("Customizer"); })
                .ForSqlServer(b => { b.Table("Custardizer"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_non_generic()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational()
                .Table("Customizer");

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServer()
                .Table("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServer()
                .Table(null);

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational(b => { b.Table("Customizer"); })
                .ForSqlServer(b => { b.Table("Custardizer"); });

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
                .ForRelational()
                .Table("Customizer");

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServer()
                .Table("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational(b => { b.Table("Customizer"); })
                .ForSqlServer(b => { b.Table("Custardizer"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational()
                .Table("Customizer", "db0");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table(null, null);

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational(b => { b.Table("Customizer", "db0"); })
                .ForSqlServer(b => { b.Table("Custardizer", "dbOh"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational()
                .Table("Customizer", "db0");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational(b => { b.Table("Customizer", "db0"); })
                .ForSqlServer(b => { b.Table("Custardizer", "dbOh"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_basic_builder_non_generic()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational()
                .Table("Customizer", "db0");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table(null, null);

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_basic_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational(b => { b.Table("Customizer", "db0"); })
                .ForSqlServer(b => { b.Table("Custardizer", "dbOh"); });

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
                .ForRelational()
                .Table("Customizer", "db0");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational(b => { b.Table("Customizer", "db0"); })
                .ForSqlServer(b => { b.Table("Custardizer", "dbOh"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_index_clustering_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForSqlServer()
                .Clustered();

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.True(index.SqlServer().IsClustered.Value);

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForSqlServer()
                .Clustered(false);

            Assert.False(index.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_index_clustering_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForSqlServer(b => { b.Clustered(); });

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.True(index.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_index_clustering_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForSqlServer()
                .Clustered();

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.True(index.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_index_clustering_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .ForSqlServer(b => { b.Clustered(); });

            var index = modelBuilder.Model.GetEntityType(typeof(Customer)).Indexes.Single();

            Assert.True(index.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_key_clustering_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForSqlServer()
                .Clustered();

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.True(key.SqlServer().IsClustered.Value);

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForSqlServer()
                .Clustered(false);

            Assert.False(key.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_key_clustering_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForSqlServer(b => { b.Clustered(); });

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.True(key.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_key_clustering_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForSqlServer()
                .Clustered();

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.True(key.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_key_clustering_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .ForSqlServer(b => { b.Clustered(); });

            var key = modelBuilder.Model.GetEntityType(typeof(Customer)).GetPrimaryKey();

            Assert.True(key.SqlServer().IsClustered.Value);
        }

        [Fact]
        public void Can_set_sequences_for_model_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .UseSequence();

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequences_for_model_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.UseSequence(); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequences_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .UseSequence();

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequences_for_model_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.UseSequence(); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
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
        public void Can_set_sequences_with_name_for_model_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .UseSequence("Snook");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook"));
            ValidateNamedSequence(sqlServerExtensions.TryGetSequence("Snook"));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_model_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.UseSequence("Snook"); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook"));
            ValidateNamedSequence(sqlServerExtensions.TryGetSequence("Snook"));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .UseSequence("Snook");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook"));
            ValidateNamedSequence(sqlServerExtensions.TryGetSequence("Snook"));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_model_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.UseSequence("Snook"); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
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
        public void Can_set_sequences_with_schema_and_name_for_model_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_model_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_model_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
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
        public void Can_set_use_of_existing_relational_sequence_for_model_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForRelational()
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            ValidateSchemaNamedSpecificSequence(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_model_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForRelational(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    })
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            ValidateSchemaNamedSpecificSequence(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForRelational()
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            ValidateSchemaNamedSpecificSequence(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_model_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForRelational(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    })
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            ValidateSchemaNamedSpecificSequence(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_model_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_model_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                        b.UseSequence("Snook", "Tasty");
                    });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", sqlServerExtensions.DefaultSequenceName);
            Assert.Equal("Tasty", sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_model_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                        b.UseSequence("Snook", "Tasty");
                    });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, sqlServerExtensions.ValueGenerationStrategy);
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
        public void Can_set_identities_for_model_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .UseIdentity();

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Identity, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_identities_for_model_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.UseIdentity(); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Identity, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_identities_for_model_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .UseIdentity();

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Identity, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_identities_for_model_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.UseIdentity(); });

            var relationalExtensions = modelBuilder.Model.Relational();
            var sqlServerExtensions = modelBuilder.Model.SqlServer();

            Assert.Equal(SqlServerValueGenerationStrategy.Identity, sqlServerExtensions.ValueGenerationStrategy);
            Assert.Null(sqlServerExtensions.DefaultSequenceName);
            Assert.Null(sqlServerExtensions.DefaultSequenceSchema);

            Assert.Null(relationalExtensions.TryGetSequence(Sequence.DefaultName));
            Assert.Null(sqlServerExtensions.TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequence_for_property_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence();

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequence_for_property_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence(); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequence_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence();

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequence_for_property_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence(); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_property_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence("Snook");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Null(property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook"));
            ValidateNamedSequence(model.SqlServer().TryGetSequence("Snook"));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_property_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence("Snook"); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Null(property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook"));
            ValidateNamedSequence(model.SqlServer().TryGetSequence("Snook"));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence("Snook");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Null(property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook"));
            ValidateNamedSequence(model.SqlServer().TryGetSequence("Snook"));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_property_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence("Snook"); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Null(property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook"));
            ValidateNamedSequence(model.SqlServer().TryGetSequence("Snook"));
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_property_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_property_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_property_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForRelational()
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForRelational(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    })
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForRelational()
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
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
                .ForRelational(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    })
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    })
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook", "Tasty")
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseSequence("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
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
                .ForSqlServer(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    })
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseSequence("Snook", "Tasty"); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);

            Assert.Null(model.Relational().TryGetSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.SqlServer().TryGetSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_identities_for_property_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseIdentity();

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Identity, property.SqlServer().ValueGenerationStrategy);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_identities_for_property_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer(b => { b.UseIdentity(); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Identity, property.SqlServer().ValueGenerationStrategy);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_set_identities_for_property_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServer()
                .UseIdentity();

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Identity, property.SqlServer().ValueGenerationStrategy);
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
                .ForSqlServer(b => { b.UseIdentity(); });

            var model = modelBuilder.Model;
            var property = model.GetEntityType(typeof(Customer)).GetProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.Identity, property.SqlServer().ValueGenerationStrategy);
            Assert.Null(property.SqlServer().SequenceName);

            Assert.Null(model.Relational().TryGetSequence(Sequence.DefaultName));
            Assert.Null(model.SqlServer().TryGetSequence(Sequence.DefaultName));
        }

        [Fact]
        public void Can_create_default_sequence_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence();

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence(Sequence.DefaultName));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence(Sequence.DefaultName);

            ValidateDefaultSequence(sequence);
        }

        [Fact]
        public void Can_create_default_sequence_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.Sequence(); });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence(Sequence.DefaultName));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence(Sequence.DefaultName);

            ValidateDefaultSequence(sequence);
        }

        [Fact]
        public void Can_create_default_sequence_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence();

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence(Sequence.DefaultName));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence(Sequence.DefaultName);

            ValidateDefaultSequence(sequence);
        }

        [Fact]
        public void Can_create_default_sequence_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.Sequence(); });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence(Sequence.DefaultName));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence(Sequence.DefaultName);

            ValidateDefaultSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook");

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook");

            ValidateNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.Sequence("Snook"); });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook");

            ValidateNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook");

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook");

            ValidateNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.Sequence("Snook"); });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook");

            ValidateNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook", "Tasty");

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.Sequence("Snook", "Tasty"); });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook", "Tasty");

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b => { b.Sequence("Snook", "Tasty"); });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSequence(sequence);
        }

        [Fact]
        public void Can_create_default_sequence_with_specific_facets_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence()
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence(Sequence.DefaultName));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence(Sequence.DefaultName);

            ValidateDefaultSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_default_sequence_with_specific_facets_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b =>
                    {
                        b.Sequence()
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence(Sequence.DefaultName));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence(Sequence.DefaultName);

            ValidateDefaultSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_default_sequence_with_specific_facets_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence()
                .IncrementBy(11)
                .Start(1729)
                .Min(111)
                .Max(2222)
                .Type<int>();

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence(Sequence.DefaultName));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence(Sequence.DefaultName);

            ValidateDefaultSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_default_sequence_with_specific_facets_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b =>
                    {
                        b.Sequence()
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence(Sequence.DefaultName));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence(Sequence.DefaultName);

            ValidateDefaultSpecificSequence(sequence);
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
        public void Can_create_named_sequence_with_specific_facets_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook")
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
        public void Can_create_named_sequence_with_specific_facets_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b =>
                    {
                        b.Sequence("Snook")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook")
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
                .ForSqlServer(b =>
                    {
                        b.Sequence("Snook")
                            .IncrementBy(11)
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
        public void Can_create_schema_named_sequence_with_specific_facets_with_basic_builder()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook", "Tasty")
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
        public void Can_create_schema_named_sequence_with_specific_facets_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = CreateNonConventionModelBuilder();

            modelBuilder
                .ForSqlServer(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_with_convention_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForSqlServer()
                .Sequence("Snook", "Tasty")
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
                .ForSqlServer(b =>
                    {
                        b.Sequence("Snook", "Tasty")
                            .IncrementBy(11)
                            .Start(1729)
                            .Min(111)
                            .Max(2222)
                            .Type<int>();
                    });

            Assert.Null(modelBuilder.Model.Relational().TryGetSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.SqlServer().TryGetSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
        {
            return SqlServerTestHelpers.Instance.CreateConventionBuilder();
        }

        protected virtual BasicModelBuilder CreateNonConventionModelBuilder()
        {
            return new BasicModelBuilder();
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
