// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Design.Tests
{
    public class MigrationScaffolderTest
    {
        [Fact]
        public void Scaffold_empty_migration()
        {
            using (var context = new Context(new Metadata.Model()))
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
        string IMigrationMetadata.Name
        {
            get
            {
                return ""MyMigration"";
            }
        }
        
        string IMigrationMetadata.Timestamp
        {
            get
            {
                return ""Timestamp"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new ModelBuilder();
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
                var builder = new ModelBuilder();
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
        string IMigrationMetadata.Name
        {
            get
            {
                return ""MyMigration"";
            }
        }
        
        string IMigrationMetadata.Timestamp
        {
            get
            {
                return ""Timestamp"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new ModelBuilder();
                builder.Entity(""Entity"")
                    .Properties(ps => ps.Property<int>(""Id""))
                    .Key(k => k.Properties(""Id"")
                        .KeyName(""MyPK""))
                    .TableName(""MyTable"", ""dbo"");
                
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
                var builder = new ModelBuilder();
                builder.Entity(""Entity"")
                    .Properties(ps => ps.Property<int>(""Id""))
                    .Key(k => k.Properties(""Id"")
                        .KeyName(""MyPK""))
                    .TableName(""MyTable"", ""dbo"");
                
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
                        HouseIdColumn = c.Int(name: ""House[\""Id]Column"", nullable: false),
                        Id = c.Int(nullable: false)
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
                        CustomerId = c.Int(nullable: false),
                        OrderId = c.Int(nullable: false)
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
        string IMigrationMetadata.Name
        {
            get
            {
                return ""MyMigration"";
            }
        }
        
        string IMigrationMetadata.Timestamp
        {
            get
            {
                return ""Timestamp"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new ModelBuilder();
                builder.Entity(""Customer"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""HouseId"").ColumnName(""House[\""Id]Column"");
                                ps.Property<int>(""Id"");
                            })
                    .Key(k => k.Properties(""Id"")
                        .Annotation(""My\""PK\""Annotat!on"", ""\""Foo\"""")
                        .KeyName(""My[\""PK]""))
                    .TableName(""Cus[\""om.er]s"", ""dbo"");
                
                builder.Entity(""Ho!use[]"")
                    .Properties(ps => ps.Property<int>(""Id""))
                    .Key(""Id"");
                
                builder.Entity(""Order"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""CustomerId"");
                                ps.Property<int>(""OrderId"");
                            })
                    .Key(""OrderId"")
                    .Annotation(""Random annotation"", ""42"")
                    .TableName(""Ord[\""e.r]s"", ""dbo"");
                
                builder.Entity(""Customer"")
                    .ForeignKeys(fks => fks.ForeignKey(""Ho!use[]"", ""HouseId"")
                        .KeyName(""My_[\""FK]"")
                        .Annotation(""My\""FK\""Annotation"", ""\""Bar\""""));
                
                builder.Entity(""Order"")
                    .ForeignKeys(fks => fks.ForeignKey(""Customer"", ""CustomerId""));
                
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
using System;

namespace MyNamespace
{
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new ModelBuilder();
                builder.Entity(""Customer"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""HouseId"").ColumnName(""House[\""Id]Column"");
                                ps.Property<int>(""Id"");
                            })
                    .Key(k => k.Properties(""Id"")
                        .Annotation(""My\""PK\""Annotat!on"", ""\""Foo\"""")
                        .KeyName(""My[\""PK]""))
                    .TableName(""Cus[\""om.er]s"", ""dbo"");
                
                builder.Entity(""Ho!use[]"")
                    .Properties(ps => ps.Property<int>(""Id""))
                    .Key(""Id"");
                
                builder.Entity(""Order"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""CustomerId"");
                                ps.Property<int>(""OrderId"");
                            })
                    .Key(""OrderId"")
                    .Annotation(""Random annotation"", ""42"")
                    .TableName(""Ord[\""e.r]s"", ""dbo"");
                
                builder.Entity(""Customer"")
                    .ForeignKeys(fks => fks.ForeignKey(""Ho!use[]"", ""HouseId"")
                        .KeyName(""My_[\""FK]"")
                        .Annotation(""My\""FK\""Annotation"", ""\""Bar\""""));
                
                builder.Entity(""Order"")
                    .ForeignKeys(fks => fks.ForeignKey(""Customer"", ""CustomerId""));
                
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
                        Foo = c.Int(nullable: false),
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""MyPK2"",
                    t => new
                        {
                            Id = t.Id,
                            Foo = t.Foo
                        });
            
            migrationBuilder.CreateTable(""EntityWithNamedKeyAndAnnotations"",
                c => new
                    {
                        Foo = c.Int(nullable: false),
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""MyPK1"",
                    t => new
                        {
                            Id = t.Id,
                            Foo = t.Foo
                        });
            
            migrationBuilder.CreateTable(""EntityWithUnnamedKey"",
                c => new
                    {
                        Foo = c.Int(nullable: false),
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_EntityWithUnnamedKey"",
                    t => new
                        {
                            Id = t.Id,
                            Foo = t.Foo
                        });
            
            migrationBuilder.CreateTable(""EntityWithUnnamedKeyAndAnnotations"",
                c => new
                    {
                        Foo = c.Int(nullable: false),
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_EntityWithUnnamedKeyAndAnnotations"",
                    t => new
                        {
                            Id = t.Id,
                            Foo = t.Foo
                        });
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
        string IMigrationMetadata.Name
        {
            get
            {
                return ""MyMigration"";
            }
        }
        
        string IMigrationMetadata.Timestamp
        {
            get
            {
                return ""Timestamp"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new ModelBuilder();
                builder.Entity(""EntityWithNamedKey"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""Foo"");
                                ps.Property<int>(""Id"");
                            })
                    .Key(k => k.Properties(""Id"", ""Foo"")
                        .KeyName(""MyPK2""));
                
                builder.Entity(""EntityWithNamedKeyAndAnnotations"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""Foo"")
                                    .Annotation(""Foo_Annotation"", ""Foo"");
                                ps.Property<int>(""Id"")
                                    .Annotation(""Id_Annotation1"", ""Id1"")
                                    .Annotation(""Id_Annotation2"", ""Id2"");
                            })
                    .Key(k => k.Properties(""Id"", ""Foo"")
                        .Annotation(""KeyAnnotation1"", ""Key1"")
                        .Annotation(""KeyAnnotation2"", ""Key2"")
                        .KeyName(""MyPK1""));
                
                builder.Entity(""EntityWithUnnamedKey"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""Foo"");
                                ps.Property<int>(""Id"");
                            })
                    .Key(""Id"", ""Foo"");
                
                builder.Entity(""EntityWithUnnamedKeyAndAnnotations"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""Foo"")
                                    .Annotation(""Foo_Annotation"", ""Foo"");
                                ps.Property<int>(""Id"")
                                    .Annotation(""Id_Annotation1"", ""Id1"")
                                    .Annotation(""Id_Annotation2"", ""Id2"");
                            })
                    .Key(k => k.Properties(""Id"", ""Foo"")
                        .Annotation(""KeyAnnotation1"", ""Key1"")
                        .Annotation(""KeyAnnotation2"", ""Key2""));
                
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
using System;

namespace MyNamespace
{
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new ModelBuilder();
                builder.Entity(""EntityWithNamedKey"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""Foo"");
                                ps.Property<int>(""Id"");
                            })
                    .Key(k => k.Properties(""Id"", ""Foo"")
                        .KeyName(""MyPK2""));
                
                builder.Entity(""EntityWithNamedKeyAndAnnotations"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""Foo"")
                                    .Annotation(""Foo_Annotation"", ""Foo"");
                                ps.Property<int>(""Id"")
                                    .Annotation(""Id_Annotation1"", ""Id1"")
                                    .Annotation(""Id_Annotation2"", ""Id2"");
                            })
                    .Key(k => k.Properties(""Id"", ""Foo"")
                        .Annotation(""KeyAnnotation1"", ""Key1"")
                        .Annotation(""KeyAnnotation2"", ""Key2"")
                        .KeyName(""MyPK1""));
                
                builder.Entity(""EntityWithUnnamedKey"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""Foo"");
                                ps.Property<int>(""Id"");
                            })
                    .Key(""Id"", ""Foo"");
                
                builder.Entity(""EntityWithUnnamedKeyAndAnnotations"")
                    .Properties(
                        ps =>
                            {
                                ps.Property<int>(""Foo"")
                                    .Annotation(""Foo_Annotation"", ""Foo"");
                                ps.Property<int>(""Id"")
                                    .Annotation(""Id_Annotation1"", ""Id1"")
                                    .Annotation(""Id_Annotation2"", ""Id2"");
                            })
                    .Key(k => k.Properties(""Id"", ""Foo"")
                        .Annotation(""KeyAnnotation1"", ""Key1"")
                        .Annotation(""KeyAnnotation2"", ""Key2""));
                
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
            var model = new Metadata.Model();
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
            var model = new Metadata.Model();

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
            var model = new Metadata.Model();
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

            protected override string CreateMigrationTimestamp()
            {
                return "Timestamp";
            }

            protected override void OnMigrationScaffolded(string className, string migrationClass, string migrationMetadataClass)
            {
                _migrationValidation(className, migrationClass, migrationMetadataClass);
            }

            protected override void OnModelScaffolded(string className, string modelSnapshotClass)
            {
                _modelValidation(className, modelSnapshotClass);
            }
        }

        #endregion
    }
}
