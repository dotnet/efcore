// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Tests.Migrations
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

            GenerateAndValidateCode(builder.Model);
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

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .ConcurrencyToken();
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .ConcurrencyToken();
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<string>(""Name"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
        b.Property<string>(""Name"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<string>(""Name"");
        b.Key(""Id"", ""Name"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<string>(""Name"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"");
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<string>(""Name"");
        b.Key(""Id"", ""Name"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"");
        b.Property<string>(""CustomerName"");
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"", ""CustomerName"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<string>(""Name"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"");
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<string>(""Name"");
        b.Key(""Id"", ""Name"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"");
        b.Property<string>(""CustomerName"");
        b.Property<int>(""Id"");
        b.Property<int>(""ProductId"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Product"", b =>
    {
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Product"", ""ProductId"");
        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"", ""CustomerName"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
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

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<string>(""Name"");
        b.Key(""Id"", ""Name"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.Property<int>(""CustomerId"");
        b.Property<string>(""CustomerName"");
        b.Property<int>(""Id"");
        b.Property<int>(""ProductId"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Product"", b =>
    {
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Order"", b =>
    {
        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Product"", ""ProductId"")
            .Annotation(""A3"", ""V3"")
            .Annotation(""A4"", ""V4"");
        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", ""CustomerId"", ""CustomerName"")
            .Annotation(""A1"", ""V1"")
            .Annotation(""A2"", ""V2"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
        }

        [Fact]
        public void Generate_outputs_property_value_generation_settings()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.Id).GenerateValueOnAdd();
                });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"")
            .GenerateValueOnAdd();
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());
        }

        [Fact]
        public void Generate_entity_type_with_nullable_property()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
            {
                b.Property(e => e.Id);
                b.Property(e => e.ZipCode);
                b.Key(e => e.Id);
            });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<int?>(""ZipCode"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
        }

        [Fact]
        public void Generate_entity_type_with_enum_property()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
            {
                b.Property(e => e.Id);
                b.Property(e => e.Day);
                b.Key(e => e.Id);
            });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<byte>(""Day"");
        b.Property<int>(""Id"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
        }

        [Fact]
        public void Generate_entity_type_with_nullable_enum_property()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<Customer>(b =>
            {
                b.Property(e => e.Id);
                b.Property(e => e.OptionalDay);
                b.Key(e => e.Id);
            });

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator().Generate(builder.Model, stringBuilder);

            Assert.Equal(
                @"var builder = new BasicModelBuilder();

builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest+Customer"", b =>
    {
        b.Property<int>(""Id"");
        b.Property<byte?>(""OptionalDay"");
        b.Key(""Id"");
    });

return builder.Model;",
                stringBuilder.ToString());

            GenerateAndValidateCode(builder.Model);
        }

        [Fact]
        public void Generate_model_snapshot_class()
        {
            var model = new Model();
            var entityType = model.AddEntityType("Entity");

            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            var stringBuilder = new IndentedStringBuilder();
            new CSharpModelCodeGenerator()
                .GenerateModelSnapshotClass("MyNamespace", "MyClass", model, typeof(MyContext), stringBuilder);

            Assert.Equal(
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.CSharpModelCodeGeneratorTest.MyContext))]
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

        #region Helper methods

        private void GenerateAndValidateCode(IModel model)
        {
            var @namespace = GetType().Namespace + ".DynamicallyCompiled";
            var className = "ModelSnapshot" + Guid.NewGuid().ToString("N");

            var generator = new CSharpModelCodeGenerator();
            var modelSnapshotBuilder = new IndentedStringBuilder();

            generator.GenerateModelSnapshotClass(@namespace, className, model, typeof(DbContext), modelSnapshotBuilder);

            var modelSnapshotSource = modelSnapshotBuilder.ToString();

            var compiledAssembly = CodeGeneratorTestHelper.Compile(
                @namespace + ".dll",
                new[] { modelSnapshotSource },
                new[]
                    {
                        "mscorlib",
                        "System.Runtime",
                        "EntityFramework.Core",
                        "EntityFramework.Relational"
                    });
            var compiledModelSnapshot = (ModelSnapshot)
                compiledAssembly.CreateInstance(@namespace + "." + className);

            Assert.NotNull(compiledModelSnapshot);

            var compiledModel = (IModel)compiledModelSnapshot.GetType().GetProperty("Model").GetValue(compiledModelSnapshot);

            Assert.NotNull(compiledModel);

            generator = new CSharpModelCodeGenerator();
            modelSnapshotBuilder = new IndentedStringBuilder();

            generator.GenerateModelSnapshotClass(@namespace, className, compiledModel, typeof(DbContext), modelSnapshotBuilder);

            Assert.Equal(modelSnapshotSource, modelSnapshotBuilder.ToString());
        }

        #endregion

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? ZipCode { get; set; }
            public Days Day { get; set; }
            public Days? OptionalDay { get; set; }
        }

        public enum Days : byte
        {
            Sun,
            Mon,
            Tue,
            Wed,
            Thu,
            Fri,
            Sat
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

        public class MyContext : DbContext
        {
        }
    }
}
