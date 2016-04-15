// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Commands.Migrations
{
    [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "https://github.com/aspnet/EntityFramework/issues/4841")]
    public class CodeCompilationTest
    {
        [ConditionalFact]
        public void Migrations_compile()
        {
            var codeHelper = new CSharpHelper();
            var generator = new CSharpMigrationsGenerator(
                codeHelper,
                new CSharpMigrationOperationGenerator(codeHelper),
                new CSharpSnapshotGenerator(codeHelper));

            var migrationCode = generator.GenerateMigration(
                "MyNamespace",
                "MyMigration",
                new[]
                {
                    new SqlOperation
                    {
                        Sql = "-- TEST",
                        ["Some:EnumValue"] = RegexOptions.Multiline
                    }
                },
                new MigrationOperation[0]);
            Assert.Equal(
                @"using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.RegularExpressions;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(""-- TEST"")
                .Annotation(""Some:EnumValue"", RegexOptions.Multiline);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
                new Model { ["Some:EnumValue"] = RegexOptions.Multiline });
            Assert.Equal(
                @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Commands.Migrations;
using System.Text.RegularExpressions;

namespace MyNamespace
{
    [DbContext(typeof(CodeCompilationTest.MyContext))]
    [Migration(""20150511161616_MyMigration"")]
    partial class MyMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation(""Some:EnumValue"", RegexOptions.Multiline);
        }
    }
}
",
                migrationMetadataCode);

            var build = new BuildSource
            {
                References =
                {
#if NETSTANDARDAPP1_5
                    BuildReference.ByName("System.Text.RegularExpressions"),
#else
                    BuildReference.ByName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
#endif
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Commands.FunctionalTests", depContextAssembly: GetType().GetTypeInfo().Assembly),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
                },
                Sources = { migrationCode, migrationMetadataCode }
            };

            var assembly = build.BuildInMemory();

            var migrationType = assembly.GetType("MyNamespace.MyMigration", throwOnError: true, ignoreCase: false);

            var contextTypeAttribute = migrationType.GetTypeInfo().GetCustomAttribute<DbContextAttribute>();
            Assert.NotNull(contextTypeAttribute);
            Assert.Equal(typeof(MyContext), contextTypeAttribute.ContextType);

            var migration = (Migration)Activator.CreateInstance(migrationType);

            Assert.Equal("20150511161616_MyMigration", migration.GetId());

            Assert.Equal(1, migration.UpOperations.Count);
            Assert.Empty(migration.DownOperations);
            Assert.Empty(migration.TargetModel.GetEntityTypes());
        }

        [ConditionalFact]
        public void Snapshots_compile()
        {
            var codeHelper = new CSharpHelper();
            var generator = new CSharpMigrationsGenerator(
                codeHelper,
                new CSharpMigrationOperationGenerator(codeHelper),
                new CSharpSnapshotGenerator(codeHelper));

            var modelSnapshotCode = generator.GenerateSnapshot(
                "MyNamespace",
                typeof(MyContext),
                "MySnapshot",
                new Model { ["Some:EnumValue"] = RegexOptions.Multiline });
            Assert.Equal(@"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Commands.Migrations;
using System.Text.RegularExpressions;

namespace MyNamespace
{
    [DbContext(typeof(CodeCompilationTest.MyContext))]
    partial class MySnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation(""Some:EnumValue"", RegexOptions.Multiline);
        }
    }
}
", modelSnapshotCode);

            var build = new BuildSource
            {
                References =
                {
#if NETSTANDARDAPP1_5
                    BuildReference.ByName("System.Runtime"),
                    BuildReference.ByName("System.Text.RegularExpressions"),
#else
                    BuildReference.ByName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
#endif
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Commands.FunctionalTests", depContextAssembly: GetType().GetTypeInfo().Assembly),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
                },
                Sources = { modelSnapshotCode }
            };

            var assembly = build.BuildInMemory();

            var snapshotType = assembly.GetType("MyNamespace.MySnapshot", throwOnError: true, ignoreCase: false);

            var contextTypeAttribute = snapshotType.GetTypeInfo().GetCustomAttribute<DbContextAttribute>();
            Assert.NotNull(contextTypeAttribute);
            Assert.Equal(typeof(MyContext), contextTypeAttribute.ContextType);

            var snapshot = (ModelSnapshot)Activator.CreateInstance(snapshotType);
            Assert.Empty(snapshot.Model.GetEntityTypes());
        }

        public class MyContext
        {
        }
    }
}
