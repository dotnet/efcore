// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Tests;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Tests.Migrations
{
    public class MigrationScaffolderTest
    {
        [Fact]
        public void Scaffold_empty_migration()
        {
            using (var context = new Context(new Model()))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var options = contextServices.GetRequiredService<DbContextService<IDbContextOptions>>();
                var model = contextServices.GetRequiredService<DbContextService<IModel>>().Service;

                var scaffolder
                    = new MyMigrationScaffolder(
                        context,
                        options.Service,
                        model,
                        new MigrationAssembly(new DbContextService<DbContext>(context), options),
                        CreateModelDiffer(),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateEmptyMigration,
                        ValidateEmptyModelSnapshot);

                scaffolder.ScaffoldMigration("MyMigration", "MyNamespace");
            }
        }

        [Fact]
        public void Scaffold_migration()
        {
            using (var context = new Context(CreateModel()))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var options = contextServices.GetRequiredService<DbContextService<IDbContextOptions>>();
                var model = contextServices.GetRequiredService<DbContextService<IModel>>().Service;

                var scaffolder
                    = new MyMigrationScaffolder(
                        context,
                        options.Service,
                        model,
                        new MigrationAssembly(new DbContextService<DbContext>(context), options),
                        CreateModelDiffer(),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigration,
                        ValidateModelSnapshot);

                scaffolder.ScaffoldMigration("MyMigration", "MyNamespace");
            }
        }

        [Fact]
        public void Scaffold_migration_with_foreign_keys()
        {
            using (var context = new Context(CreateModelWithForeignKeys()))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var options = contextServices.GetRequiredService<DbContextService<IDbContextOptions>>();
                var model = contextServices.GetRequiredService<DbContextService<IModel>>().Service;

                var scaffolder
                    = new MyMigrationScaffolder(
                        context,
                        options.Service,
                        model,
                        new MigrationAssembly(new DbContextService<DbContext>(context), options),
                        CreateModelDiffer(),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigrationWithForeignKeys,
                        ValidateModelWithForeignKeysSnapshot);

                scaffolder.ScaffoldMigration("MyMigration", "MyNamespace");
            }
        }

        [Fact]
        public void Scaffold_migration_with_composite_keys()
        {
            using (var context = new Context(CreateModelWithCompositeKeys()))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var options = contextServices.GetRequiredService<DbContextService<IDbContextOptions>>();
                var model = contextServices.GetRequiredService<DbContextService<IModel>>().Service;

                var scaffolder
                    = new MyMigrationScaffolder(
                        context,
                        options.Service,
                        model,
                        new MigrationAssembly(new DbContextService<DbContext>(context), options),
                        CreateModelDiffer(),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigrationWithCompositeKeys,
                        ValidateModelWithCompositeKeysSnapshot);

                scaffolder.ScaffoldMigration("MyMigration", "MyNamespace");
            }
        }

        [Fact]
        public void GetNamespace_returns_migration_namespace_when_set()
        {
            var migration = new Mock<Migration>();
            var scaffolder = new Mock<MigrationScaffolder> { CallBase = true }.Object;

            var result = scaffolder.GetNamespace(migration.Object, "Unicorn", Mock.Of<Type>());

            Assert.Equal("Castle.Proxies", result);
        }

        [Fact]
        public void GetNamespace_returns_default_when_snapshot_null()
        {
            var scaffolder = new Mock<MigrationScaffolder> { CallBase = true }.Object;

            var result = scaffolder.GetNamespace(default(ModelSnapshot), "Unicorn.Migrations");

            Assert.Equal("Unicorn.Migrations", result);
        }

        [Fact]
        public void GetNamespace_returns_snapshot_namespace_when_set()
        {
            var migration = new Mock<ModelSnapshot>();
            var scaffolder = new Mock<MigrationScaffolder> { CallBase = true }.Object;

            var result = scaffolder.GetNamespace(migration.Object, "Unicorn.Migrations");

            Assert.Equal("Castle.Proxies", result);
        }

        private static void ValidateEmptyMigration(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using System;

namespace MyNamespace.Migrations
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
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace.Migrations
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest.Context))]
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        string IMigrationMetadata.ProductVersion
        {
            get
            {
                return ""1.2.3.4"";
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
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace.Migrations
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest.Context))]
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

            Assert.Equal(@"using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using System;

namespace MyNamespace.Migrations
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

            Assert.Equal(
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace.Migrations
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest.Context))]
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        string IMigrationMetadata.ProductVersion
        {
            get
            {
                return ""1.2.3.4"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Entity"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"")
                            .ForRelational(rb => rb.Name(""MyPK""));
                        b.ForRelational().Table(""MyTable"", ""dbo"");
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
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace.Migrations
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest.Context))]
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Entity"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"")
                            .ForRelational(rb => rb.Name(""MyPK""));
                        b.ForRelational().Table(""MyTable"", ""dbo"");
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
                @"using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using System;

namespace MyNamespace.Migrations
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(""[Ho!use[]]]"",
                c => new
                    {
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_Ho!use[]"", t => t.Id);
            
            migrationBuilder.CreateTable(""dbo.[Cus[\""om.er]]s]"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        HouseIdColumn = c.Int(name: ""House[\""Id]Column"", nullable: false)
                    })
                .PrimaryKey(""My[\""PK]"", t => t.Id);
            
            migrationBuilder.CreateTable(""dbo.[Ord[\""e.r]]s]"",
                c => new
                    {
                        OrderId = c.Int(nullable: false),
                        CustomerId = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_dbo.Ord[\""e.r]s"", t => t.OrderId);
            
            migrationBuilder.AddForeignKey(
                ""dbo.[Cus[\""om.er]]s]"",
                ""My_[\""FK]"",
                new[] { ""House[\""Id]Column"" },
                ""[Ho!use[]]]"",
                new[] { ""Id"" },
                cascadeDelete: false);
            
            migrationBuilder.AddForeignKey(
                ""dbo.[Ord[\""e.r]]s]"",
                ""FK_dbo.Ord[\""e.r]s_dbo.Cus[\""om.er]s_CustomerId"",
                new[] { ""CustomerId"" },
                ""dbo.[Cus[\""om.er]]s]"",
                new[] { ""Id"" },
                cascadeDelete: false);
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(""[Ho!use[]]]"");
            
            migrationBuilder.DropTable(""dbo.[Cus[\""om.er]]s]"");
            
            migrationBuilder.DropTable(""dbo.[Ord[\""e.r]]s]"");
        }
    }
}",
                migrationClass);

            Assert.Equal(
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace.Migrations
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest.Context))]
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        string IMigrationMetadata.ProductVersion
        {
            get
            {
                return ""1.2.3.4"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Ho!use[]"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", b =>
                    {
                        b.Property<int>(""HouseId"")
                            .ColumnName(""House[\""Id]Column"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"")
                            .ForRelational(rb => rb.Name(""My[\""PK]""))
                            .Annotation(""My\""PK\""Annotat!on"", ""\""Foo\"""");
                        b.ForRelational().Table(""Cus[\""om.er]s"", ""dbo"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Order"", b =>
                    {
                        b.Property<int>(""CustomerId"");
                        b.Property<int>(""OrderId"");
                        b.Key(""OrderId"");
                        b.ForRelational().Table(""Ord[\""e.r]s"", ""dbo"");
                        b.Annotation(""Random annotation"", ""42"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", b =>
                    {
                        b.ForeignKey(""Ho!use[]"", ""HouseId"")
                            .ForRelational(rb => rb.Name(""My_[\""FK]""))
                            .Annotation(""My\""FK\""Annotation"", ""\""Bar\"""");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Order"", b =>
                    {
                        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", ""CustomerId"");
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
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace.Migrations
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest.Context))]
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Ho!use[]"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", b =>
                    {
                        b.Property<int>(""HouseId"")
                            .ColumnName(""House[\""Id]Column"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"")
                            .ForRelational(rb => rb.Name(""My[\""PK]""))
                            .Annotation(""My\""PK\""Annotat!on"", ""\""Foo\"""");
                        b.ForRelational().Table(""Cus[\""om.er]s"", ""dbo"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Order"", b =>
                    {
                        b.Property<int>(""CustomerId"");
                        b.Property<int>(""OrderId"");
                        b.Key(""OrderId"");
                        b.ForRelational().Table(""Ord[\""e.r]s"", ""dbo"");
                        b.Annotation(""Random annotation"", ""42"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", b =>
                    {
                        b.ForeignKey(""Ho!use[]"", ""HouseId"")
                            .ForRelational(rb => rb.Name(""My_[\""FK]""))
                            .Annotation(""My\""FK\""Annotation"", ""\""Bar\"""");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Order"", b =>
                    {
                        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", ""CustomerId"");
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
                @"using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using System;

namespace MyNamespace.Migrations
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
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace.Migrations
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest.Context))]
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        string IMigrationMetadata.ProductVersion
        {
            get
            {
                return ""1.2.3.4"";
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
                        b.Key(""Id"", ""Foo"")
                            .ForRelational(rb => rb.Name(""MyPK2""));
                    });
                
                builder.Entity(""EntityWithNamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(""Id"", ""Foo"")
                            .ForRelational(rb => rb.Name(""MyPK1""))
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2"");
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
                        b.Key(""Id"", ""Foo"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2"");
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
                @"using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using System;

namespace MyNamespace.Migrations
{
    [ContextType(typeof(Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest.Context))]
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
                        b.Key(""Id"", ""Foo"")
                            .ForRelational(rb => rb.Name(""MyPK2""));
                    });
                
                builder.Entity(""EntityWithNamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(""Id"", ""Foo"")
                            .ForRelational(rb => rb.Name(""MyPK1""))
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2"");
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
                        b.Key(""Id"", ""Foo"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        private static IModel CreateModel()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Entity));
            var property = entityType.GetOrAddProperty("Id", typeof(int));

            entityType.Relational().Table = "MyTable";
            entityType.Relational().Schema = "dbo";
            entityType.GetOrSetPrimaryKey(property);
            entityType.GetPrimaryKey().Relational().Name = "MyPK";

            return model;
        }

        private static IModel CreateModelWithForeignKeys()
        {
            var model = new Model();

            var houseType = model.AddEntityType("Ho!use[]");
            var houseId = houseType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            houseType.GetOrSetPrimaryKey(houseId);

            var customerType = model.AddEntityType(typeof(Customer));
            var customerId = customerType.GetOrAddProperty("Id", typeof(int));
            var customerFkProperty = customerType.GetOrAddProperty("HouseId", typeof(int));
            customerFkProperty.Relational().Column = @"House[""Id]Column";
            customerType.Relational().Schema = "dbo";
            customerType.Relational().Table = @"Cus[""om.er]s";
            customerType.GetOrSetPrimaryKey(customerId);
            customerType.GetPrimaryKey().Relational().Name = @"My[""PK]";
            customerType.GetPrimaryKey()[@"My""PK""Annotat!on"] = @"""Foo""";
            var customerFk = customerType.GetOrAddForeignKey(customerFkProperty, houseType.GetPrimaryKey());
            customerFk.Relational().Name = @"My_[""FK]";
            customerFk[@"My""FK""Annotation"] = @"""Bar""";

            var orderType = model.AddEntityType(typeof(Order));
            var orderId = orderType.GetOrAddProperty(@"OrderId", typeof(int));
            var orderFK = orderType.GetOrAddProperty(@"CustomerId", typeof(int));
            orderType.Relational().Schema = "dbo";
            orderType.GetOrSetPrimaryKey(orderId);
            orderType.Relational().Table = @"Ord[""e.r]s";
            orderType.GetOrAddForeignKey(orderFK, customerType.GetPrimaryKey());
            orderType["Random annotation"] = "42";

            return model;
        }

        private static IModel CreateModelWithCompositeKeys()
        {
            var model = new Model();
            var entity1 = model.AddEntityType("EntityWithNamedKeyAndAnnotations");

            var id1 = entity1.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            id1["Id_Annotation1"] = "Id1";
            id1["Id_Annotation2"] = "Id2";
            var foo1 = entity1.GetOrAddProperty("Foo", typeof(int), shadowProperty: true);
            foo1["Foo_Annotation"] = "Foo";

            entity1.GetOrSetPrimaryKey(new[] { id1, foo1 });
            entity1.GetPrimaryKey().Relational().Name = "MyPK1";
            entity1.GetPrimaryKey()["KeyAnnotation1"] = "Key1";
            entity1.GetPrimaryKey()["KeyAnnotation2"] = "Key2";

            var entity2 = model.AddEntityType("EntityWithUnnamedKeyAndAnnotations");

            var id2 = entity2.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            id2["Id_Annotation1"] = "Id1";
            id2["Id_Annotation2"] = "Id2";
            var foo2 = entity2.GetOrAddProperty("Foo", typeof(int), shadowProperty: true);
            foo2["Foo_Annotation"] = "Foo";

            entity2.GetOrSetPrimaryKey(new[] { id2, foo2 });
            entity2.GetPrimaryKey()["KeyAnnotation1"] = "Key1";
            entity2.GetPrimaryKey()["KeyAnnotation2"] = "Key2";

            var entity3 = model.AddEntityType("EntityWithNamedKey");
            var id3 = entity3.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var foo3 = entity3.GetOrAddProperty("Foo", typeof(int), shadowProperty: true);
            entity3.GetOrSetPrimaryKey(new[] { id3, foo3 });
            entity3.GetPrimaryKey().Relational().Name = "MyPK2";

            var entity4 = model.AddEntityType("EntityWithUnnamedKey");
            var id4 = entity4.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var foo4 = entity4.GetOrAddProperty("Foo", typeof(int), shadowProperty: true);
            entity4.GetOrSetPrimaryKey(new[] { id4, foo4 });

            return model;
        }

        private class Entity
        {
            public int Id { get; set; }
        }

        private class Customer
        {
            public int Id { get; set; }
            public int HouseId { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }

        public class Context : DbContext
        {
            private readonly IModel _model;

            public Context(IModel model)
            {
                _model = model;
            }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseModel(_model);
                ((IDbContextOptions)options).AddOrUpdateExtension<MyRelationalOptionsExtension>(x => x.ConnectionString = "ConnectionString");
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
                DbContext context,
                IDbContextOptions options,
                IModel model,
                MigrationAssembly migrationAssembly,
                ModelDiffer modelDiffer,
                MigrationCodeGenerator migrationCodeGenerator,
                Action<string, string, string> migrationValidation,
                Action<string, string> modelValidation)
                : base(
                    context,
                    options,
                    model,
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

            protected virtual string GetMigrationName(string migrationId)
            {
                return migrationId.Substring(16);
            }

            protected override MigrationInfo CreateMigration(string migrationName)
            {
                var migration = base.CreateMigration(migrationName);

                return
                    new MigrationInfo(migration.MigrationId, "1.2.3.4")
                    {
                        TargetModel = migration.TargetModel,
                        UpgradeOperations = migration.UpgradeOperations,
                        DowngradeOperations = migration.DowngradeOperations
                    };
            }

            public override ScaffoldedMigration ScaffoldMigration(string migrationName, string rootNamespace)
            {
                var scaffoldedMigration = base.ScaffoldMigration(migrationName, rootNamespace);

                _migrationValidation(
                    GetMigrationName(scaffoldedMigration.MigrationId),
                    scaffoldedMigration.MigrationCode,
                    scaffoldedMigration.MigrationMetadataCode);

                _modelValidation(
                    scaffoldedMigration.SnapshotModelClass,
                    scaffoldedMigration.SnapshotModelCode);

                return scaffoldedMigration;
            }
        }

        private static ModelDiffer CreateModelDiffer()
        {
            var extensionProvider = RelationalTestHelpers.ExtensionProvider();
            var typeMapper = new RelationalTypeMapper();
            var operationFactory = new MigrationOperationFactory(extensionProvider);
            var operationProcessor = new MigrationOperationProcessor(extensionProvider, typeMapper, operationFactory);

            return new ModelDiffer(extensionProvider, typeMapper, operationFactory, operationProcessor);
        }
    }
}
