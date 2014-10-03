// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata
{
    public class SqlServerBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_column_name_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
        public void Can_set_key_name_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(Order)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForSqlServer()
                .Name("ChocolateLimes");

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(OrderDetails)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForSqlServer()
                .Name(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForeignKey<OrderDetails>(e => e.Id)
                .ForRelational()
                .Name("LemonSupreme");

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
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
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForRelational(b => { b.Name("LemonSupreme"); })
                .ForSqlServer(b => { b.Name("ChocolateLimes"); });

            var foreignKey = modelBuilder.Model.GetEntityType(typeof(OrderDetails)).ForeignKeys.Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.SqlServer().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new ModelBuilder();

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
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational()
                .Table("Customizer");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table(null);

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational(b => { b.Table("Customizer"); })
                .ForSqlServer(b => { b.Table("Custardizer"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational()
                .Table("Customizer");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational(b => { b.Table("Customizer"); })
                .ForSqlServer(b => { b.Table("Custardizer"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational()
                .Table("Customizer");

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServer()
                .Table("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServer()
                .Table(null);

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_basic_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational(b => { b.Table("Customizer"); })
                .ForSqlServer(b => { b.Table("Custardizer"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational()
                .Table("Customizer");

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServer()
                .Table("Custardizer");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }

        [Fact]
        public void Can_set_table_name_with_convention_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational(b => { b.Table("Customizer"); })
                .ForSqlServer(b => { b.Table("Custardizer"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
        }


        [Fact]
        public void Can_set_table_and_schema_name_with_basic_builder()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational()
                .Table("Customizer", "db0");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table(null, null);

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_basic_builder_using_nested_closure()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational(b => { b.Table("Customizer", "db0"); })
                .ForSqlServer(b => { b.Table("Custardizer", "dbOh"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational()
                .Table("Customizer", "db0");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder_using_nested_closure()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForRelational(b => { b.Table("Customizer", "db0"); })
                .ForSqlServer(b => { b.Table("Custardizer", "dbOh"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_basic_builder_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational()
                .Table("Customizer", "db0");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table(null, null);

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_basic_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational(b => { b.Table("Customizer", "db0"); })
                .ForSqlServer(b => { b.Table("Custardizer", "dbOh"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational()
                .Table("Customizer", "db0");

            modelBuilder
                .Entity<Customer>()
                .ForSqlServer()
                .Table("Custardizer", "dbOh");

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_with_convention_builder_using_nested_closure_non_generic()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForRelational(b => { b.Table("Customizer", "db0"); })
                .ForSqlServer(b => { b.Table("Custardizer", "dbOh"); });

            var entityType = modelBuilder.Model.GetEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
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
