// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Design.Tests
{
    public class MigrationScaffolderTest
    {
        [Fact]
        public void Scaffold_empty_migration()
        {
            using (var context = new Context(new Model()))
            {
                var scaffolder
                    = new MyMigrationScaffolder(
                        context.Configuration,
                        MockMigrationAssembly(context.Configuration),
                        new ModelDiffer(new DatabaseBuilder()),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateEmptyMigration,
                        ValidateEmptyModelSnapshot);

                scaffolder.ScaffoldMigration("MyMigration");
            }
        }

        [Fact]
        public void Scaffold_migration()
        {
            using (var context = new Context(CreateModel()))
            {
                var scaffolder
                    = new MyMigrationScaffolder(
                        context.Configuration,
                        MockMigrationAssembly(context.Configuration),
                        new ModelDiffer(new DatabaseBuilder()),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigration,
                        ValidateModelSnapshot);

                scaffolder.ScaffoldMigration("MyMigration");
            }
        }

        [Fact]
        public void Scaffold_migration_with_foreign_keys()
        {
            using (var context = new Context(CreateModelWithForeignKeys()))
            {
                var scaffolder
                    = new MyMigrationScaffolder(
                        context.Configuration,
                        MockMigrationAssembly(context.Configuration),
                        new ModelDiffer(new DatabaseBuilder()),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigrationWithForeignKeys,
                        ValidateModelWithForeignKeysSnapshot);

                scaffolder.ScaffoldMigration("MyMigration");
            }
        }

        [Fact]
        public void Scaffold_migration_with_composite_keys()
        {
            using (var context = new Context(CreateModelWithCompositeKeys()))
            {
                var scaffolder
                    = new MyMigrationScaffolder(
                        context.Configuration,
                        MockMigrationAssembly(context.Configuration),
                        new ModelDiffer(new DatabaseBuilder()),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigrationWithCompositeKeys,
                        ValidateModelWithCompositeKeysSnapshot);

                scaffolder.ScaffoldMigration("MyMigration");
            }
        }

        private static void ValidateEmptyMigration(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using System;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}",
                migrationClass);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                return builder.Model;
            }
        }
    }
}",
                migrationMetadataClass);
        }

        private static void ValidateEmptyModelSnapshot(string className, string modelSnapshotClass)
        {
            Assert.Equal("ContextModelSnapshot", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        private static void ValidateMigration(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(@"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using System;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(""dbo.MyTable"",
                c => new
                    {
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""MyPK"", t => t.Id);
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(""dbo.MyTable"");
        }
    }
}",
                migrationClass);

            Assert.Equal(@"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Entity"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(k => k.Properties(""Id"")
                            .KeyName(""MyPK""));
                        b.TableName(""MyTable"", ""dbo"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                migrationMetadataClass);
        }

        private static void ValidateModelSnapshot(string className, string modelSnapshotClass)
        {
            Assert.Equal("ContextModelSnapshot", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Entity"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(k => k.Properties(""Id"")
                            .KeyName(""MyPK""));
                        b.TableName(""MyTable"", ""dbo"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        private static void ValidateMigrationWithForeignKeys(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using System;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(""dbo.[Cus[\""om.er]]s]"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        HouseIdColumn = c.Int(name: ""House[\""Id]Column"", nullable: false)
                    })
                .PrimaryKey(""My[\""PK]"", t => t.Id);
            
            migrationBuilder.CreateTable(""[Ho!use[]]]"",
                c => new
                    {
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_Ho!use[]"", t => t.Id);
            
            migrationBuilder.CreateTable(""dbo.[Ord[\""e.r]]s]"",
                c => new
                    {
                        OrderId = c.Int(nullable: false),
                        CustomerId = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_dbo.Ord[\""e.r]s"", t => t.OrderId);
            
            migrationBuilder.AddForeignKey(""dbo.[Cus[\""om.er]]s]"", ""My_[\""FK]"", new[] { ""House[\""Id]Column"" }, ""[Ho!use[]]]"", new[] { ""Id"" }, cascadeDelete: false);
            
            migrationBuilder.AddForeignKey(""dbo.[Ord[\""e.r]]s]"", ""FK_dbo.Ord[\""e.r]s_dbo.Cus[\""om.er]s_CustomerId"", new[] { ""CustomerId"" }, ""dbo.[Cus[\""om.er]]s]"", new[] { ""Id"" }, cascadeDelete: false);
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(""dbo.[Cus[\""om.er]]s]"", ""My_[\""FK]"");
            
            migrationBuilder.DropForeignKey(""dbo.[Ord[\""e.r]]s]"", ""FK_dbo.Ord[\""e.r]s_dbo.Cus[\""om.er]s_CustomerId"");
            
            migrationBuilder.DropTable(""dbo.[Cus[\""om.er]]s]"");
            
            migrationBuilder.DropTable(""[Ho!use[]]]"");
            
            migrationBuilder.DropTable(""dbo.[Ord[\""e.r]]s]"");
        }
    }
}",
                migrationClass);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Customer"", b =>
                    {
                        b.Property<int>(""HouseId"")
                            .ColumnName(""House[\""Id]Column"");
                        b.Property<int>(""Id"");
                        b.Key(k => k.Properties(""Id"")
                            .KeyName(""My[\""PK]"")
                            .Annotation(""My\""PK\""Annotat!on"", ""\""Foo\""""));
                        b.ForeignKeys(fks => fks.ForeignKey(""Ho!use[]"", ""HouseId"")
                            .KeyName(""My_[\""FK]"")
                            .Annotation(""My\""FK\""Annotation"", ""\""Bar\""""));
                        b.TableName(""Cus[\""om.er]s"", ""dbo"");
                    });
                
                builder.Entity(""Ho!use[]"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"");
                    });
                
                builder.Entity(""Order"", b =>
                    {
                        b.Property<int>(""CustomerId"");
                        b.Property<int>(""OrderId"");
                        b.Key(""OrderId"");
                        b.ForeignKeys(fks => fks.ForeignKey(""Customer"", ""CustomerId""));
                        b.TableName(""Ord[\""e.r]s"", ""dbo"");
                        b.Annotation(""Random annotation"", ""42"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                migrationMetadataClass);
        }

        private static void ValidateModelWithForeignKeysSnapshot(string className, string modelSnapshotClass)
        {
            Assert.Equal("ContextModelSnapshot", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Customer"", b =>
                    {
                        b.Property<int>(""HouseId"")
                            .ColumnName(""House[\""Id]Column"");
                        b.Property<int>(""Id"");
                        b.Key(k => k.Properties(""Id"")
                            .KeyName(""My[\""PK]"")
                            .Annotation(""My\""PK\""Annotat!on"", ""\""Foo\""""));
                        b.ForeignKeys(fks => fks.ForeignKey(""Ho!use[]"", ""HouseId"")
                            .KeyName(""My_[\""FK]"")
                            .Annotation(""My\""FK\""Annotation"", ""\""Bar\""""));
                        b.TableName(""Cus[\""om.er]s"", ""dbo"");
                    });
                
                builder.Entity(""Ho!use[]"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"");
                    });
                
                builder.Entity(""Order"", b =>
                    {
                        b.Property<int>(""CustomerId"");
                        b.Property<int>(""OrderId"");
                        b.Key(""OrderId"");
                        b.ForeignKeys(fks => fks.ForeignKey(""Customer"", ""CustomerId""));
                        b.TableName(""Ord[\""e.r]s"", ""dbo"");
                        b.Annotation(""Random annotation"", ""42"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        private static void ValidateMigrationWithCompositeKeys(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using System;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(""EntityWithNamedKey"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Foo = c.Int(nullable: false)
                    })
                .PrimaryKey(""MyPK2"", t => new { t.Id, t.Foo });
            
            migrationBuilder.CreateTable(""EntityWithNamedKeyAndAnnotations"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Foo = c.Int(nullable: false)
                    })
                .PrimaryKey(""MyPK1"", t => new { t.Id, t.Foo });
            
            migrationBuilder.CreateTable(""EntityWithUnnamedKey"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Foo = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_EntityWithUnnamedKey"", t => new { t.Id, t.Foo });
            
            migrationBuilder.CreateTable(""EntityWithUnnamedKeyAndAnnotations"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Foo = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_EntityWithUnnamedKeyAndAnnotations"", t => new { t.Id, t.Foo });
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(""EntityWithNamedKey"");
            
            migrationBuilder.DropTable(""EntityWithNamedKeyAndAnnotations"");
            
            migrationBuilder.DropTable(""EntityWithUnnamedKey"");
            
            migrationBuilder.DropTable(""EntityWithUnnamedKeyAndAnnotations"");
        }
    }
}",
                migrationClass);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""EntityWithNamedKey"", b =>
                    {
                        b.Property<int>(""Foo"");
                        b.Property<int>(""Id"");
                        b.Key(k => k.Properties(""Id"", ""Foo"")
                            .KeyName(""MyPK2""));
                    });
                
                builder.Entity(""EntityWithNamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(k => k.Properties(""Id"", ""Foo"")
                            .KeyName(""MyPK1"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2""));
                    });
                
                builder.Entity(""EntityWithUnnamedKey"", b =>
                    {
                        b.Property<int>(""Foo"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"", ""Foo"");
                    });
                
                builder.Entity(""EntityWithUnnamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(k => k.Properties(""Id"", ""Foo"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2""));
                    });
                
                return builder.Model;
            }
        }
    }
}",
                migrationMetadataClass);
        }

        private static void ValidateModelWithCompositeKeysSnapshot(string className, string modelSnapshotClass)
        {
            Assert.Equal("ContextModelSnapshot", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""EntityWithNamedKey"", b =>
                    {
                        b.Property<int>(""Foo"");
                        b.Property<int>(""Id"");
                        b.Key(k => k.Properties(""Id"", ""Foo"")
                            .KeyName(""MyPK2""));
                    });
                
                builder.Entity(""EntityWithNamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(k => k.Properties(""Id"", ""Foo"")
                            .KeyName(""MyPK1"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2""));
                    });
                
                builder.Entity(""EntityWithUnnamedKey"", b =>
                    {
                        b.Property<int>(""Foo"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"", ""Foo"");
                    });
                
                builder.Entity(""EntityWithUnnamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(k => k.Properties(""Id"", ""Foo"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2""));
                    });
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        #region Fixture

        private static MigrationAssembly MockMigrationAssembly(DbContextConfiguration contextConfiguration)
        {
            var mock = new Mock<MigrationAssembly>(contextConfiguration);

            mock.SetupGet(ma => ma.Migrations).Returns(new IMigrationMetadata[0]);
            mock.SetupGet(ma => ma.Model).Returns((IModel)null);

            return mock.Object;
        }

        private static IModel CreateModel()
        {
            var model = new Model();
            var entityType = new EntityType("Entity");
            var property = entityType.AddProperty("Id", typeof(int));

            entityType.SetTableName("MyTable");
            entityType.SetSchema("dbo");
            entityType.SetKey(property);
            entityType.GetKey().SetKeyName("MyPK");
            model.AddEntityType(entityType);

            return model;
        }

        private static IModel CreateModelWithForeignKeys()
        {
            var model = new Model();

            var houseType = new EntityType("Ho!use[]");
            var houseId = houseType.AddProperty("Id", typeof(int));
            houseType.SetKey(houseId);
            model.AddEntityType(houseType);

            var customerType = new EntityType(@"Customer");
            var customerId = customerType.AddProperty("Id", typeof(int));
            var customerFkProperty = customerType.AddProperty("HouseId", typeof(int));
            customerFkProperty.SetColumnName(@"House[""Id]Column");
            customerType.SetSchema("dbo");
            customerType.SetTableName(@"Cus[""om.er]s");
            customerType.SetKey(customerId);
            customerType.GetKey().SetKeyName(@"My[""PK]");
            customerType.GetKey().Annotations.Add(new Annotation(@"My""PK""Annotat!on", @"""Foo"""));
            var customerFk = customerType.AddForeignKey(houseType.GetKey(), customerFkProperty);
            customerFk.SetKeyName(@"My_[""FK]");
            customerFk.Annotations.Add(new Annotation(@"My""FK""Annotation", @"""Bar"""));
            model.AddEntityType(customerType);

            var orderType = new EntityType(@"Order");
            var orderId = orderType.AddProperty(@"OrderId", typeof(int));
            var orderFK = orderType.AddProperty(@"CustomerId", typeof(int));
            orderType.SetSchema("dbo");
            orderType.SetKey(orderId);
            orderType.SetTableName(@"Ord[""e.r]s");
            orderType.AddForeignKey(customerType.GetKey(), orderFK);
            orderType.Annotations.Add(new Annotation("Random annotation", "42"));
            model.AddEntityType(orderType);

            return model;
        }

        private static IModel CreateModelWithCompositeKeys()
        {
            var model = new Model();
            var entity1 = new EntityType("EntityWithNamedKeyAndAnnotations");

            var id1 = entity1.AddProperty("Id", typeof(int));
            id1.Annotations.Add(new Annotation("Id_Annotation1", "Id1"));
            id1.Annotations.Add(new Annotation("Id_Annotation2", "Id2"));
            var foo1 = entity1.AddProperty("Foo", typeof(int));
            foo1.Annotations.Add(new Annotation("Foo_Annotation", "Foo"));

            entity1.SetKey(id1, foo1);
            entity1.GetKey().SetKeyName("MyPK1");
            entity1.GetKey().Annotations.Add(new Annotation("KeyAnnotation1", "Key1"));
            entity1.GetKey().Annotations.Add(new Annotation("KeyAnnotation2", "Key2"));
            model.AddEntityType(entity1);

            var entity2 = new EntityType("EntityWithUnnamedKeyAndAnnotations");

            var id2 = entity2.AddProperty("Id", typeof(int));
            id2.Annotations.Add(new Annotation("Id_Annotation1", "Id1"));
            id2.Annotations.Add(new Annotation("Id_Annotation2", "Id2"));
            var foo2 = entity2.AddProperty("Foo", typeof(int));
            foo2.Annotations.Add(new Annotation("Foo_Annotation", "Foo"));

            entity2.SetKey(id2, foo2);
            entity2.GetKey().Annotations.Add(new Annotation("KeyAnnotation1", "Key1"));
            entity2.GetKey().Annotations.Add(new Annotation("KeyAnnotation2", "Key2"));
            model.AddEntityType(entity2);

            var entity3 = new EntityType("EntityWithNamedKey");
            var id3 = entity3.AddProperty("Id", typeof(int));
            var foo3 = entity3.AddProperty("Foo", typeof(int));
            entity3.SetKey(id3, foo3);
            entity3.GetKey().SetKeyName("MyPK2");
            model.AddEntityType(entity3);

            var entity4 = new EntityType("EntityWithUnnamedKey");
            var id4 = entity4.AddProperty("Id", typeof(int));
            var foo4 = entity4.AddProperty("Foo", typeof(int));
            entity4.SetKey(id4, foo4);
            model.AddEntityType(entity4);

            return model;
        }

        public class Context : DbContext
        {
            private readonly IModel _model;

            public Context(IModel model)
            {
                _model = model;
            }

            protected override void OnConfiguring(DbContextOptions builder)
            {
                var contextOptionsExtensions = (IDbContextOptionsExtensions)builder;

                builder.UseModel(_model);
                contextOptionsExtensions.AddOrUpdateExtension<MyRelationalOptionsExtension>(x => x.ConnectionString = "ConnectionString");
                contextOptionsExtensions.AddOrUpdateExtension<MyRelationalOptionsExtension>(x => x.MigrationNamespace = "MyNamespace");
            }
        }

        public class MyRelationalOptionsExtension : RelationalOptionsExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }

        public class MyMigrationScaffolder : MigrationScaffolder
        {
            private readonly Action<string, string, string> _migrationValidation;
            private readonly Action<string, string> _modelValidation;

            public MyMigrationScaffolder(
                DbContextConfiguration contextConfiguration,
                MigrationAssembly migrationAssembly,
                ModelDiffer modelDiffer,
                MigrationCodeGenerator migrationCodeGenerator,
                Action<string, string, string> migrationValidation,
                Action<string, string> modelValidation)
                : base(
                    contextConfiguration,
                    migrationAssembly,
                    modelDiffer,
                    migrationCodeGenerator)
            {
                _migrationValidation = migrationValidation;
                _modelValidation = modelValidation;
            }

            protected override string CreateMigrationId(string migrationName)
            {
                return "000000000000001_" + migrationName;
            }

            public override ScaffoldedMigration ScaffoldMigration(string migrationName)
            {
                var scaffoldedMigration = base.ScaffoldMigration(migrationName);

                _migrationValidation(
                    scaffoldedMigration.MigrationClass,
                    scaffoldedMigration.MigrationCode,
                    scaffoldedMigration.MigrationMetadataCode);

                _modelValidation(
                    scaffoldedMigration.SnapshotModelClass,
                    scaffoldedMigration.SnapshotModelCode);

                return scaffoldedMigration;
            }
        }

        #endregion
    }
}
