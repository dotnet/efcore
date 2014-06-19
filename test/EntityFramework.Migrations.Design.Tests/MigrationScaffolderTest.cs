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
                builder.Entity(""Entity"")
                    .Properties(ps => ps.Property<int>(""Id""))
                    .Key(""Id"");
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
                    .Key(""Id"");
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

            entityType.StorageName = "dbo.MyTable";
            entityType.SetKey(property);
            entityType.GetKey().StorageName = "MyPK";
            model.AddEntityType(entityType);

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
