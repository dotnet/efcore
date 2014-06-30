// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Design.Tests
{
    public class CSharpModelCodeGeneratorTest
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public int ProductId { get; set; }
        }

        public class Product
        {
            public int Id { get; set; }
        }

        [Fact]
        public void Generate_empty_model()
        {
            var builder = new ModelBuilder();

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_empty_model_with_annotations()
        {
            var builder = new ModelBuilder()
                .Annotation("A1", "V1")
                .Annotation("A2", "V2");

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder()
    .Annotation(""A1"", ""V1"")
    .Annotation(""A2"", ""V2"");
return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_property()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(ps => ps.Property(e => e.Id))
                .Key(e => e.Id);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(ps => ps.Property<int>(""Id""))
    .Key(""Id"");

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_shadow_property()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(ps => ps.Property<int>("Id", shadowProperty: true))
                .Key(e => e.Id);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(ps => ps.Property<int>(""Id"", shadowProperty: true))
    .Key(""Id"");

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_concurrency_token()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(ps => ps.Property<int>("Id", concurrencyToken: true))
                .Key(e => e.Id);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(ps => ps.Property<int>(""Id"", concurrencyToken: true))
    .Key(""Id"");

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_shadow_property_and_concurrency_token()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(ps => ps.Property<int>("Id", shadowProperty: true, concurrencyToken: true))
                .Key(e => e.Id);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(ps => ps.Property<int>(""Id"", shadowProperty: true, concurrencyToken: true))
    .Key(""Id"");

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_property_with_annotations()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(ps => ps.Property(e => e.Id)
                    .Annotation("A1", "V1")
                    .Annotation("A2", "V2"))
                .Key(e => e.Id);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(ps => ps.Property<int>(""Id"")
        .Annotation(""A1"", ""V1"")
        .Annotation(""A2"", ""V2""))
    .Key(""Id"");

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_multiple_properties()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.Name);
                        })
                .Key(e => e.Id);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""Id"");
                ps.Property<string>(""Name"");
            })
    .Key(""Id"");

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_multiple_properties_with_annotations()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id)
                                .Annotation("A1", "V1")
                                .Annotation("A2", "V2");
                            ps.Property(e => e.Name)
                                .Annotation("A1", "V1")
                                .Annotation("A2", "V2");
                        })
                .Key(e => e.Id);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""Id"")
                    .Annotation(""A1"", ""V1"")
                    .Annotation(""A2"", ""V2"");
                ps.Property<string>(""Name"")
                    .Annotation(""A1"", ""V1"")
                    .Annotation(""A2"", ""V2"");
            })
    .Key(""Id"");

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_composite_key()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.Name);
                        })
                .Key(k => k.Properties(e => new { e.Id, e.Name })
                    .Annotation("A1", "V1")
                    .Annotation("A2", "V2"));

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""Id"");
                ps.Property<string>(""Name"");
            })
    .Key(k => k.Properties(""Id"", ""Name"")
        .Annotation(""A1"", ""V1"")
        .Annotation(""A2"", ""V2""));

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_foreign_key()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.Name);
                        })
                .Key(e => e.Id);
            builder.Entity<Order>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.CustomerId);
                        })
                .Key(e => e.Id);
            builder.Entity<Order>()
                .ForeignKeys(fks => fks.ForeignKey<Customer>(e => e.CustomerId));

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""Id"");
                ps.Property<string>(""Name"");
            })
    .Key(""Id"");

builder.Entity(""Order"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""CustomerId"");
                ps.Property<int>(""Id"");
            })
    .Key(""Id"");

builder.Entity(""Order"")
    .ForeignKeys(fks => fks.ForeignKey(""Customer"", ""CustomerId""));

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_composite_foreign_key()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.Name);
                        })
                .Key(e => new { e.Id, e.Name });
            builder.Entity<Order>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.CustomerId);
                            ps.Property(e => e.CustomerName);
                        })
                .Key(e => e.Id);
            builder.Entity<Order>()
                .ForeignKeys(fks => fks.ForeignKey<Customer>(e => new { e.CustomerId, e.CustomerName }));

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""Id"");
                ps.Property<string>(""Name"");
            })
    .Key(""Id"", ""Name"");

builder.Entity(""Order"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""CustomerId"");
                ps.Property<string>(""CustomerName"");
                ps.Property<int>(""Id"");
            })
    .Key(""Id"");

builder.Entity(""Order"")
    .ForeignKeys(fks => fks.ForeignKey(""Customer"", ""CustomerId"", ""CustomerName""));

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_single_foreign_key_with_annotations()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.Name);
                        })
                .Key(e => e.Id);
            builder.Entity<Order>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.CustomerId);
                        })
                .Key(e => e.Id);
            builder.Entity<Order>()
                .ForeignKeys(fks => fks.ForeignKey<Customer>(e => e.CustomerId)
                    .Annotation("A1", "V1")
                    .Annotation("A2", "V2"));

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""Id"");
                ps.Property<string>(""Name"");
            })
    .Key(""Id"");

builder.Entity(""Order"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""CustomerId"");
                ps.Property<int>(""Id"");
            })
    .Key(""Id"");

builder.Entity(""Order"")
    .ForeignKeys(fks => fks.ForeignKey(""Customer"", ""CustomerId"")
        .Annotation(""A1"", ""V1"")
        .Annotation(""A2"", ""V2""));

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_multiple_foreign_keys()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.Name);
                        })
                .Key(e => new { e.Id, e.Name });
            builder.Entity<Order>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.CustomerId);
                            ps.Property(e => e.CustomerName);
                            ps.Property(e => e.ProductId);
                        })
                .Key(e => e.Id);
            builder.Entity<Product>()
                .Properties(ps => ps.Property(e => e.Id))
                .Key(e => e.Id);
            builder.Entity<Order>()
                .ForeignKeys(
                    fks =>
                        {
                            fks.ForeignKey<Customer>(e => new { e.CustomerId, e.CustomerName });
                            fks.ForeignKey<Product>(e => e.ProductId);
                        });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""Id"");
                ps.Property<string>(""Name"");
            })
    .Key(""Id"", ""Name"");

builder.Entity(""Order"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""CustomerId"");
                ps.Property<string>(""CustomerName"");
                ps.Property<int>(""Id"");
                ps.Property<int>(""ProductId"");
            })
    .Key(""Id"");

builder.Entity(""Product"")
    .Properties(ps => ps.Property<int>(""Id""))
    .Key(""Id"");

builder.Entity(""Order"")
    .ForeignKeys(
        fks =>
            {
                fks.ForeignKey(""Customer"", ""CustomerId"", ""CustomerName"");
                fks.ForeignKey(""Product"", ""ProductId"");
            });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_multiple_foreign_keys_with_annotations()
        {
            var builder = new ModelBuilder();
            builder.Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.Name);
                        })
                .Key(e => new { e.Id, e.Name });
            builder.Entity<Order>()
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.CustomerId);
                            ps.Property(e => e.CustomerName);
                            ps.Property(e => e.ProductId);
                        })
                .Key(e => e.Id);
            builder.Entity<Product>()
                .Properties(ps => ps.Property(e => e.Id))
                .Key(e => e.Id);
            builder.Entity<Order>()
                .ForeignKeys(
                    fks =>
                        {
                            fks.ForeignKey<Customer>(e => new { e.CustomerId, e.CustomerName })
                                .Annotation("A1", "V1")
                                .Annotation("A2", "V2");
                            fks.ForeignKey<Product>(e => e.ProductId)
                                .Annotation("A1", "V1")
                                .Annotation("A2", "V2");
                        });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity(""Customer"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""Id"");
                ps.Property<string>(""Name"");
            })
    .Key(""Id"", ""Name"");

builder.Entity(""Order"")
    .Properties(
        ps =>
            {
                ps.Property<int>(""CustomerId"");
                ps.Property<string>(""CustomerName"");
                ps.Property<int>(""Id"");
                ps.Property<int>(""ProductId"");
            })
    .Key(""Id"");

builder.Entity(""Product"")
    .Properties(ps => ps.Property<int>(""Id""))
    .Key(""Id"");

builder.Entity(""Order"")
    .ForeignKeys(
        fks =>
            {
                fks.ForeignKey(""Customer"", ""CustomerId"", ""CustomerName"")
                    .Annotation(""A1"", ""V1"")
                    .Annotation(""A2"", ""V2"");
                fks.ForeignKey(""Product"", ""ProductId"")
                    .Annotation(""A1"", ""V1"")
                    .Annotation(""A2"", ""V2"");
            });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_model_snapshot_class()
        {
            var model = new Metadata.Model();
            var entityType = new EntityType("Entity");

            entityType.SetKey(entityType.AddProperty("Id", typeof(int)));
            model.AddEntityType(entityType);

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().GenerateModelSnapshotClass("MyNamespace", "MyClass", model, stringBuilder);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using System;

namespace MyNamespace
{
    public class MyClass : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new ModelBuilder();
                builder.Entity(""Entity"")
                    .Properties(ps => ps.Property<int>(""Id""))
                    .Key(""Id"");
                
                return builder.Model;
            }
        }
    }
}",
                stringBuilder.ToString());
        }
    }
}
