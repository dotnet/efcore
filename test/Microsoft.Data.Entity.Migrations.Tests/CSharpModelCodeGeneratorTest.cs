// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
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
            public int CustomerName { get; set; }
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
builder.Entity<Customer>()
    .Properties(ps => ps.Property(e => e.Id))
    .Key(e => e.Id);
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
builder.Entity<Customer>()
    .Properties(ps => ps.Property(e => e.Id)
        .Annotation(""A1"", ""V1"")
        .Annotation(""A2"", ""V2""))
    .Key(e => e.Id);
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
builder.Entity<Customer>()
    .Properties(
        ps =>
            {
                ps.Property(e => e.Id);
                ps.Property(e => e.Name);
            })
    .Key(e => e.Id);
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
builder.Entity<Customer>()
    .Properties(
        ps =>
            {
                ps.Property(e => e.Id)
                    .Annotation(""A1"", ""V1"")
                    .Annotation(""A2"", ""V2"");
                ps.Property(e => e.Name)
                    .Annotation(""A1"", ""V1"")
                    .Annotation(""A2"", ""V2"");
            })
    .Key(e => e.Id);
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
                .Key(e => new { e.Id, e.Name });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new ModelBuilder();
builder.Entity<Customer>()
    .Properties(
        ps =>
            {
                ps.Property(e => e.Id);
                ps.Property(e => e.Name);
            })
    .Key(e => new { e.Id, e.Name });
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
                ps.Property(e => e.CustomerId);
                ps.Property(e => e.Id);
            })
    .Key(e => e.Id);
builder.Entity<Order>()
    .ForeignKeys(fks => fks.ForeignKey<Customer>(e => e.CustomerId));
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
                ps.Property(e => e.CustomerId);
                ps.Property(e => e.CustomerName);
                ps.Property(e => e.Id);
            })
    .Key(e => e.Id);
builder.Entity<Order>()
    .ForeignKeys(fks => fks.ForeignKey<Customer>(e => new { e.CustomerId, e.CustomerName }));
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
                ps.Property(e => e.CustomerId);
                ps.Property(e => e.Id);
            })
    .Key(e => e.Id);
builder.Entity<Order>()
    .ForeignKeys(fks => fks.ForeignKey<Customer>(e => e.CustomerId)
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
                ps.Property(e => e.CustomerId);
                ps.Property(e => e.CustomerName);
                ps.Property(e => e.Id);
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
                ps.Property(e => e.CustomerId);
                ps.Property(e => e.CustomerName);
                ps.Property(e => e.Id);
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
                    .Annotation(""A1"", ""V1"")
                    .Annotation(""A2"", ""V2"");
                fks.ForeignKey<Product>(e => e.ProductId)
                    .Annotation(""A1"", ""V1"")
                    .Annotation(""A2"", ""V2"");
            });
return builder.Model;",
                stringBuilder.ToString());
        }
    }
}
