// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Design.Tests
{
    public class CSharpModelCodeGeneratorTest
    {
        [Fact]
        public void Generate_empty_model()
        {
            var builder = new BasicModelBuilder();

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_empty_model_with_annotations()
        {
            var builder = new BasicModelBuilder()
                .Annotation("A1", "V1")
                .Annotation("A2", "V2");

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder()
    .Annotation(""A1"", ""V1"")
    .Annotation(""A2"", ""V2"");

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_property()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Key(e => e.Id);
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_shadow_property()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property<int>("Id");
                    b.Key(e => e.Id);
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_concurrency_token()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property<int>("Id").ConcurrencyToken().Shadow(false);
                    b.Key(e => e.Id);
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false)
            .ConcurrencyToken();
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_shadow_property_and_concurrency_token()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property<int>("Id").ConcurrencyToken();
                    b.Key(e => e.Id);
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .ConcurrencyToken();
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_property_with_annotations()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id)
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                    b.Key(e => e.Id);
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false)
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_multiple_properties()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key(e => e.Id);
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<string>(""Name"")
            .Shadow(false);
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_multiple_properties_with_annotations()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id)
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                    b.Property(e => e.Name)
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                    b.Key(e => e.Id);
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false)
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
        b.Property<string>(""Name"")
            .Shadow(false)
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_composite_key()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key(e => new { e.Id, e.Name })
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<string>(""Name"")
            .Shadow(false);
        b.Key(k => k.Properties(""Id"", ""Name"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2""));
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_foreign_key()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key(e => e.Id);
                });

            builder.Entity<Order>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.CustomerId);
                    b.Key(e => e.Id);
                });

            builder.Entity<Order>()
                .ForeignKey<Customer>(e => e.CustomerId);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<string>(""Name"")
            .Shadow(false);
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"")
            .Shadow(false);
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Key(""Id"");
        b.ForeignKey(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_composite_foreign_key()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key(e => new { e.Id, e.Name });
                });

            builder.Entity<Order>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.CustomerId);
                    b.Property(e => e.CustomerName);
                    b.Key(e => e.Id);
                });

            builder.Entity<Order>()
                .ForeignKey<Customer>(e => new { e.CustomerId, e.CustomerName });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<string>(""Name"")
            .Shadow(false);
        b.Key(""Id"", ""Name"");
    });

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"")
            .Shadow(false);
        b.Property<string>(""CustomerName"")
            .Shadow(false);
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Key(""Id"");
        b.ForeignKey(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"", ""CustomerName"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_foreign_key_with_annotations()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key(e => e.Id);
                });

            builder.Entity<Order>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.CustomerId);
                    b.Key(e => e.Id);
                });

            builder.Entity<Order>()
                .ForeignKey<Customer>(e => e.CustomerId)
                .Annotation("A1", "V1")
                .Annotation("A2", "V2");

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<string>(""Name"")
            .Shadow(false);
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"")
            .Shadow(false);
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Key(""Id"");
        b.ForeignKey(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_multiple_foreign_keys()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key(e => new { e.Id, e.Name });
                });

            builder.Entity<Order>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.CustomerId);
                    b.Property(e => e.CustomerName);
                    b.Property(e => e.ProductId);
                    b.Key(e => e.Id);
                });

            builder.Entity<Product>(b =>
                {
                    b.Property(e => e.Id);
                    b.Key(e => e.Id);
                });

            builder.Entity<Order>(b =>
                {
                    b.ForeignKey<Customer>(e => new { e.CustomerId, e.CustomerName });
                    b.ForeignKey<Product>(e => e.ProductId);
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<string>(""Name"")
            .Shadow(false);
        b.Key(""Id"", ""Name"");
    });

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"")
            .Shadow(false);
        b.Property<string>(""CustomerName"")
            .Shadow(false);
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<int>(""ProductId"")
            .Shadow(false);
        b.Key(""Id"");
        b.ForeignKey(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"", ""CustomerName"");
        b.ForeignKey(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Product"", ""ProductId"");
    });

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Product"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_multiple_foreign_keys_with_annotations()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key(e => new { e.Id, e.Name });
                });

            builder.Entity<Order>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.CustomerId);
                    b.Property(e => e.CustomerName);
                    b.Property(e => e.ProductId);
                    b.Key(e => e.Id);
                });

            builder.Entity<Product>(b =>
                {
                    b.Property(e => e.Id);
                    b.Key(e => e.Id);
                });

            builder.Entity<Order>(b =>
                {
                    b.ForeignKey<Customer>(e => new { e.CustomerId, e.CustomerName })
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                    b.ForeignKey<Product>(e => e.ProductId)
                        .Annotation("A3", "V3")
                        .Annotation("A4", "V4");
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<string>(""Name"")
            .Shadow(false);
        b.Key(""Id"", ""Name"");
    });

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"")
            .Shadow(false);
        b.Property<string>(""CustomerName"")
            .Shadow(false);
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Property<int>(""ProductId"")
            .Shadow(false);
        b.Key(""Id"");
        b.ForeignKey(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"", ""CustomerName"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
        b.ForeignKey(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Product"", ""ProductId"")
            .Annotation(""A3"", ""V3"")
            .Annotation(""A4"", ""V4"");
    });

builder.Entity(""Microsoft.Data.Entity.Design.Tests.CSharpModelCodeGeneratorTest+Product"", b =>
    {
        b.Property<int>(""Id"")
            .Shadow(false);
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_model_snapshot_class()
        {
            var model = new Model();
            var entityType = new EntityType("Entity");

            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            model.AddEntityType(entityType);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().GenerateModelSnapshotClass("MyNamespace", "MyClass", model, stringBuilder);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public class MyClass : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Entity"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                stringBuilder.ToString());
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public int ProductId { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }
        }
    }
}
