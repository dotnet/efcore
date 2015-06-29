// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Data.Entity.Commands.TestUtilities;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class CodeCompilationTest
    {
        [Fact]
        public void Migrations_compile()
        {
            var codeHelper = new CSharpHelper();
            var generator = new CSharpMigrationGenerator(
                codeHelper,
                new CSharpMigrationOperationGenerator(codeHelper),
                new CSharpModelGenerator(codeHelper));

            var migrationCode = generator.Generate(
                "MyNamespace",
                "MyMigration",
                new MigrationOperation[0],
                new MigrationOperation[0]);
            Assert.Equal(
                @"using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Operations;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migration)
        {
        }

        public override void Down(MigrationBuilder migration)
        {
        }
    }
}
",
                migrationCode);

            var migrationMetadataCode = generator.GenerateMetadata(
                "MyNamespace",
                typeof(MyContext),
                "MyMigration",
                "20150511161616_MyMigration",
                "7.0.0",
                new Model());
            Assert.Equal(
                @"using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Commands.Migrations;

namespace MyNamespace
{
    [ContextType(typeof(CodeCompilationTest.MyContext))]
    partial class MyMigration
    {
        public override string Id
        {
            get { return ""20150511161616_MyMigration""; }
        }

        public override string ProductVersion
        {
            get { return ""7.0.0""; }
        }

        public override void BuildTargetModel(ModelBuilder builder)
        {
        }
    }
}
",
                migrationMetadataCode);

            var build = new BuildSource
            {
                References =
                {
                    BuildReference.ByName(typeof(CodeCompilationTest).Assembly.GetName().Name),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                    BuildReference.ByName("EntityFramework.Core"),
                    BuildReference.ByName("EntityFramework.Relational")
                },
                Sources = { migrationCode, migrationMetadataCode }
            };

            var assembly = build.BuildInMemory();

            var migrationType = assembly.GetType("MyNamespace.MyMigration", throwOnError: true);

            var contextTypeAttribute = migrationType.GetCustomAttribute<ContextTypeAttribute>();
            Assert.NotNull(contextTypeAttribute);
            Assert.Equal(typeof(MyContext), contextTypeAttribute.ContextType);

            var migration = (Migration)Activator.CreateInstance(migrationType);

            Assert.Equal("20150511161616_MyMigration", migration.Id);
            Assert.Equal("7.0.0", migration.ProductVersion);

            var migrationBuilder = new MigrationBuilder();
            migration.Up(migrationBuilder);
            Assert.Empty(migrationBuilder.Operations);

            migrationBuilder = new MigrationBuilder();
            migration.Down(migrationBuilder);
            Assert.Empty(migrationBuilder.Operations);

            var conventions = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventions);
            migration.BuildTargetModel(modelBuilder);
            Assert.Empty(modelBuilder.Model.EntityTypes);
        }

        [Fact]
        public void Snapshots_compile()
        {
            var codeHelper = new CSharpHelper();
            var generator = new CSharpMigrationGenerator(
                codeHelper,
                new CSharpMigrationOperationGenerator(codeHelper),
                new CSharpModelGenerator(codeHelper));

            var modelSnapshotCode = generator.GenerateSnapshot("MyNamespace", typeof(MyContext), "MySnapshot", new Model());
            Assert.Equal(@"using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Commands.Migrations;

namespace MyNamespace
{
    [ContextType(typeof(CodeCompilationTest.MyContext))]
    partial class MySnapshot : ModelSnapshot
    {
        public override void BuildModel(ModelBuilder builder)
        {
        }
    }
}
", modelSnapshotCode);

            var build = new BuildSource
            {
                References =
                {
                    BuildReference.ByName(typeof(CodeCompilationTest).Assembly.GetName().Name),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                    BuildReference.ByName("EntityFramework.Core"),
                    BuildReference.ByName("EntityFramework.Relational")
                },
                Sources = { modelSnapshotCode }
            };

            var assembly = build.BuildInMemory();

            var snapshotType = assembly.GetType("MyNamespace.MySnapshot", throwOnError: true);

            var contextTypeAttribute = snapshotType.GetCustomAttribute<ContextTypeAttribute>();
            Assert.NotNull(contextTypeAttribute);
            Assert.Equal(typeof(MyContext), contextTypeAttribute.ContextType);

            var snapshot = (ModelSnapshot)Activator.CreateInstance(snapshotType);

            var conventions = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventions);
            snapshot.BuildModel(modelBuilder);
            Assert.Empty(modelBuilder.Model.EntityTypes);
        }

        public class MyContext
        {
        }
    }
}
