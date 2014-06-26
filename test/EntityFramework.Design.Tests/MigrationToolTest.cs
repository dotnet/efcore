// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.ConfigurationModel;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Design.Tests
{
    public class MigrationToolTest
    {
        [Fact]
        public void CommitConfiguration()
        {
            var toolMock = new Mock<MyMigrationTool>() { CallBase = true };
            var tool = toolMock.Object;
            var args
                = new[]
                      {
                          "--ConfigFile=MyConfig.ini",
                          "--ContextAssembly=EntityFramework.Design.Tests.dll",
                          "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext",
                          "--MigrationAssembly=EntityFramework.Design.Tests.dll",
                          "--MigrationNamespace=MyNamespace",
                          "--MigrationDirectory=MyDirectory",
                          "--References=Ref1;Ref2;Ref3"
                      };

            var configSourceMock = new Mock<IniFileConfigurationSource>("Foo") { CallBase = true };

            configSourceMock
                .Setup(m => m.Load());

            configSourceMock
                .Setup(m => m.Commit())
                .Callback(
                    () =>
                        {
                            Assert.Equal(configSourceMock.Object.Data["ContextAssembly"], "EntityFramework.Design.Tests.dll");
                            Assert.Equal(configSourceMock.Object.Data["ContextType"], "Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext");
                            Assert.Equal(configSourceMock.Object.Data["MigrationAssembly"], "EntityFramework.Design.Tests.dll");
                            Assert.Equal(configSourceMock.Object.Data["MigrationNamespace"], "MyNamespace");
                            Assert.Equal(configSourceMock.Object.Data["MigrationDirectory"], "MyDirectory");
                            Assert.Equal(configSourceMock.Object.Data["References"], "Ref1;Ref2;Ref3");
                        });

            toolMock.Protected()
                .Setup<IniFileConfigurationSource>("CreateIniFileConfigurationSource", ItExpr.IsAny<string>())
                .Callback<string>(
                    configFile =>
                        {
                            Assert.True(configFile.EndsWith("MyConfig.ini"));
                            Assert.True(Path.IsPathRooted(configFile));
                        })
                .Returns(configSourceMock.Object);

            tool.Configuration.AddCommandLine(args);

            tool.CommitConfiguration();

            configSourceMock.Verify(m => m.Commit(), Times.Once);
            toolMock.Protected().Verify<IniFileConfigurationSource>("CreateIniFileConfigurationSource", Times.Once(), ItExpr.IsAny<string>());
        }

        [Fact]
        public void CommitConfiguration_with_default_config_file()
        {
            var toolMock = new Mock<MyMigrationTool>() { CallBase = true };
            var tool = toolMock.Object;
            var args
                = new[]
                      {
                          "--ContextAssembly=EntityFramework.Design.Tests.dll",
                      };

            var configSourceMock = new Mock<IniFileConfigurationSource>("Foo") { CallBase = true };

            configSourceMock.Setup(m => m.Load());
            configSourceMock.Setup(m => m.Commit());

            toolMock.Protected()
                .Setup<IniFileConfigurationSource>("CreateIniFileConfigurationSource", ItExpr.IsAny<string>())
                .Callback<string>(
                    configFile =>
                        {
                            Assert.True(configFile.EndsWith("migration.ini"));
                            Assert.True(Path.IsPathRooted(configFile));
                        })
                .Returns(configSourceMock.Object);

            tool.Configuration.AddCommandLine(args);

            tool.CommitConfiguration();

            toolMock.Protected().Verify<IniFileConfigurationSource>("CreateIniFileConfigurationSource", Times.Once(), ItExpr.IsAny<string>());            
        }

        [Fact]
        public void CreateMigration()
        {
            var tool = new MyMigrationTool();
            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--MigrationName=MyMigration", 
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext", 
                        "--MigrationAssembly=EntityFramework.Design.Tests.dll", 
                        "--MigrationNamespace=MyNamespace", 
                        "--MigrationDirectory=C:\\MyDirectory"
                    });

            var scaffoldedMigration = tool.CreateMigration();

            Assert.Equal("MyNamespace", scaffoldedMigration.MigrationNamespace);
            Assert.Equal("MyMigration", scaffoldedMigration.MigrationClass);
            Assert.Equal("MyContextModelSnapshot", scaffoldedMigration.SnapshotModelClass);
            Assert.Equal("C:\\MyDirectory\\MyMigration.cs", scaffoldedMigration.MigrationFile);
            Assert.Equal("C:\\MyDirectory\\MyMigration.Designer.cs", scaffoldedMigration.MigrationMetadataFile);
            Assert.Equal("C:\\MyDirectory\\MyContextModelSnapshot.cs", scaffoldedMigration.SnapshotModelFile);
        }

        [Fact]
        public void CreateMigration_with_default_migration_assembly_and_namespace()
        {
            var tool = new MyMigrationTool();
            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--MigrationName=MyMigration", 
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext", 
                        "--MigrationDirectory=MyDirectory"
                    });

            var scaffoldedMigration = tool.CreateMigration();

            Assert.Equal("Microsoft.Data.Entity.Design.Tests.MyContextMigrations", scaffoldedMigration.MigrationNamespace);
            Assert.Equal("MyMigration", scaffoldedMigration.MigrationClass);
            Assert.Equal("MyContextModelSnapshot", scaffoldedMigration.SnapshotModelClass);
            Assert.True(scaffoldedMigration.MigrationFile.EndsWith("MyDirectory\\MyMigration.cs"));
            Assert.True(scaffoldedMigration.MigrationMetadataFile.EndsWith("MyDirectory\\MyMigration.Designer.cs"));
            Assert.True(scaffoldedMigration.SnapshotModelFile.EndsWith("MyDirectory\\MyContextModelSnapshot.cs"));
            Assert.True(Path.IsPathRooted(scaffoldedMigration.MigrationFile));
            Assert.True(Path.IsPathRooted(scaffoldedMigration.MigrationMetadataFile));
            Assert.True(Path.IsPathRooted(scaffoldedMigration.SnapshotModelFile));
        }

        [Fact]
        public void CreateMigration_with_default_migration_directory()
        {
            var tool = new MyMigrationTool();
            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--MigrationName=MyMigration", 
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext", 
                        "--MigrationAssembly=EntityFramework.Design.Tests.dll", 
                        "--MigrationNamespace=MyNamespace"
                    });

            var scaffoldedMigration = tool.CreateMigration();

            Assert.Equal("MyNamespace", scaffoldedMigration.MigrationNamespace);
            Assert.Equal("MyMigration", scaffoldedMigration.MigrationClass);
            Assert.Equal("MyContextModelSnapshot", scaffoldedMigration.SnapshotModelClass);
            Assert.True(scaffoldedMigration.MigrationFile.EndsWith("MyMigration.cs"));
            Assert.True(scaffoldedMigration.MigrationMetadataFile.EndsWith("MyMigration.Designer.cs"));
            Assert.True(scaffoldedMigration.SnapshotModelFile.EndsWith("MyContextModelSnapshot.cs"));
            Assert.True(Path.IsPathRooted(scaffoldedMigration.MigrationFile));
            Assert.True(Path.IsPathRooted(scaffoldedMigration.MigrationMetadataFile));
            Assert.True(Path.IsPathRooted(scaffoldedMigration.SnapshotModelFile));
        }

        [Fact]
        public void CreateMigration_throws_if_migration_name_not_specified()
        {
            var tool = new MyMigrationTool();

            Assert.Equal(
                Strings.MigrationNameNotSpecified,
                Assert.Throws<InvalidOperationException>(() => tool.CreateMigration()).Message);
        }

        [Fact]
        public void GetMigrations_with_source_database()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;
            var migratorMock = new Mock<Migrator>();

            migratorMock.Setup(m => m.GetDatabaseMigrations()).Returns(
                new[]
                    {
                        new MigrationMetadata("M1", "T1")
                    });

            toolMock.Protected()
                .Setup<Migrator>("GetMigrator", ItExpr.IsAny<DbContextConfiguration>())
                .Returns(migratorMock.Object);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext", 
                        "--MigrationSource=Database"
                    });

            var migrations = tool.GetMigrations();

            Assert.Equal(1, migrations.Count);
            Assert.Equal("M1", migrations[0].Name);
            Assert.Equal("T1", migrations[0].Timestamp);
        }

        [Fact]
        public void GetMigrations_with_default_source()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;
            var migratorMock = new Mock<Migrator>();

            migratorMock.Setup(m => m.GetDatabaseMigrations()).Returns(
                new[]
                    {
                        new MigrationMetadata("M1", "T1")
                    });

            toolMock.Protected()
                .Setup<Migrator>("GetMigrator", ItExpr.IsAny<DbContextConfiguration>())
                .Returns(migratorMock.Object);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext"
                    });

            var migrations = tool.GetMigrations();

            Assert.Equal(1, migrations.Count);
            Assert.Equal("M1", migrations[0].Name);
            Assert.Equal("T1", migrations[0].Timestamp);
        }

        [Fact]
        public void GetMigrations_with_source_local()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;
            var migratorMock = new Mock<Migrator>();

            migratorMock.Setup(m => m.GetLocalMigrations()).Returns(
                new[]
                    {
                        new MigrationMetadata("M1", "T1"),
                        new MigrationMetadata("M2", "T2")
                    });

            toolMock.Protected()
                .Setup<Migrator>("GetMigrator", ItExpr.IsAny<DbContextConfiguration>())
                .Returns(migratorMock.Object);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext", 
                        "--MigrationSource=Local"
                    });

            var migrations = tool.GetMigrations();

            Assert.Equal(2, migrations.Count);
            Assert.Equal("M1", migrations[0].Name);
            Assert.Equal("M2", migrations[1].Name);
            Assert.Equal("T1", migrations[0].Timestamp);
            Assert.Equal("T2", migrations[1].Timestamp);
        }

        [Fact]
        public void GetMigrations_with_source_pending()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;
            var migratorMock = new Mock<Migrator>();

            migratorMock.Setup(m => m.GetPendingMigrations()).Returns(
                new[]
                    {
                        new MigrationMetadata("M2", "T2")
                    });

            toolMock.Protected()
                .Setup<Migrator>("GetMigrator", ItExpr.IsAny<DbContextConfiguration>())
                .Returns(migratorMock.Object);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext", 
                        "--MigrationSource=Pending"
                    });

            var migrations = tool.GetMigrations();

            Assert.Equal(1, migrations.Count);
            Assert.Equal("M2", migrations[0].Name);
            Assert.Equal("T2", migrations[0].Timestamp);
        }

        [Fact]
        public void GetMigrations_throws_if_invalid_source()
        {
            var tool = new MyMigrationTool();

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext", 
                        "--MigrationSource=Foo"
                    });

            Assert.Equal(
                Strings.InvalidMigrationSource,
                Assert.Throws<InvalidOperationException>(() => tool.GetMigrations()).Message);
        }

        [Fact]
        public void GenerateScript()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;
            var migratorMock = new Mock<Migrator>();

            migratorMock.Setup(m => m.GenerateUpdateDatabaseSql()).Returns(
                new[]
                    {
                        new SqlStatement("Script")
                    });

            toolMock.Protected()
                .Setup<Migrator>("GetMigrator", ItExpr.IsAny<DbContextConfiguration>())
                .Returns(migratorMock.Object);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext"
                    });

            var statements = tool.GenerateScript();

            migratorMock.Verify(m => m.GenerateUpdateDatabaseSql(), Times.Once);
            Assert.Equal(1, statements.Count);
            Assert.Equal("Script", statements[0].Sql);
        }

        [Fact]
        public void GenerateScript_with_target_migration()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;
            var migratorMock = new Mock<Migrator>();

            migratorMock.Setup(m => m.GenerateUpdateDatabaseSql(It.IsAny<string>())).Returns(
                new[]
                    {
                        new SqlStatement("Script")
                    });

            toolMock.Protected()
                .Setup<Migrator>("GetMigrator", ItExpr.IsAny<DbContextConfiguration>())
                .Returns(migratorMock.Object);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--TargetMigration=MyMigrationName",
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext"
                    });

            var statements = tool.GenerateScript();

            migratorMock.Verify(m => m.GenerateUpdateDatabaseSql("MyMigrationName"), Times.Once);
            Assert.Equal(1, statements.Count);
            Assert.Equal("Script", statements[0].Sql);
        }

        [Fact]
        public void UpdateDatabase()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;
            var migratorMock = new Mock<Migrator>();

            toolMock.Protected()
                .Setup<Migrator>("GetMigrator", ItExpr.IsAny<DbContextConfiguration>())
                .Returns(migratorMock.Object);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext"
                    });

            tool.UpdateDatabase();

            migratorMock.Verify(m => m.UpdateDatabase(), Times.Once);
        }

        [Fact]
        public void UpdateDatabase_with_target_migration()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;
            var migratorMock = new Mock<Migrator>();

            toolMock.Protected()
                .Setup<Migrator>("GetMigrator", ItExpr.IsAny<DbContextConfiguration>())
                .Returns(migratorMock.Object);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--TargetMigration=MyMigrationName",
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext"
                    });

            tool.UpdateDatabase();

            migratorMock.Verify(m => m.UpdateDatabase("MyMigrationName"), Times.Once);
        }

        [Fact]
        public void LoadContext_loads_references_before_instantiating_context_type()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;

            toolMock.Protected()
                .Setup<DbContext>("CreateContext", ItExpr.IsAny<Type>())
                .Callback(() => toolMock.Protected()
                    .Verify<Assembly>("LoadAssembly", Times.Exactly(4), ItExpr.IsAny<string>()));

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+MyContext",
                        "--References=Ref1;Ref2;Ref3"
                    });

            tool.LoadContext();
        }

        [Fact]
        public void LoadContext_throws_if_context_assembly_not_specified()
        {
            var tool = new MyMigrationTool();

            Assert.Equal(
                Strings.ContextAssemblyNotSpecified,
                Assert.Throws<InvalidOperationException>(() => tool.LoadContext()).Message);
        }

        [Fact]
        public void LoadContext_throws_if_context_type_not_found()
        {
            var tool = new MyMigrationTool();
            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.Vuvuzelas"
                    });

            Assert.Equal(
                Strings.FormatAssemblyDoesNotContainType(
                    Assembly.GetExecutingAssembly().FullName, 
                    "Microsoft.Data.Entity.Design.Tests.Vuvuzelas"),
                Assert.Throws<InvalidOperationException>(() => tool.LoadContext()).Message);
        }

        [Fact]
        public void LoadContext_throws_if_context_type_is_not_DbContext()
        {
            var tool = new MyMigrationTool();
            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll", 
                        "--ContextType=Microsoft.Data.Entity.Design.Tests.MigrationToolTest+NotAContext"
                    });

            Assert.Equal(
                Strings.FormatTypeIsNotDbContext("Microsoft.Data.Entity.Design.Tests.MigrationToolTest+NotAContext"),
                Assert.Throws<InvalidOperationException>(() => tool.LoadContext()).Message);
        }

        [Fact]
        public void LoadContext_throws_if_context_type_not_specified_and_no_DbContext_found_in_assembly()
        {
            var toolMock = new Mock<MyMigrationTool> { CallBase = true };
            var tool = toolMock.Object;

            toolMock.Protected().Setup<IReadOnlyList<Type>>("GetContextTypes", ItExpr.IsAny<Assembly>()).Returns(new Type[0]);

            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll"
                    });

            Assert.Equal(
                Strings.FormatAssemblyDoesNotContainDbContext(Assembly.GetExecutingAssembly().FullName),
                Assert.Throws<InvalidOperationException>(() => tool.LoadContext()).Message);
        }

        [Fact]
        public void LoadContext_throws_if_context_type_not_specified_and_multiple_DbContext_found_in_assembly()
        {
            var tool = new MyMigrationTool();
            tool.Configuration.AddCommandLine(
                new[]
                    {
                        "--ContextAssembly=EntityFramework.Design.Tests.dll"
                    });

            Assert.Equal(
                Strings.FormatAssemblyContainsMultipleDbContext(Assembly.GetExecutingAssembly().FullName),
                Assert.Throws<InvalidOperationException>(() => tool.LoadContext()).Message);
        }

        public class MyContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(new SqlConnection());
            }
        }

        public class MyContext2 : DbContext
        {
        }

        public class NotAContext
        {            
        }

        public class MyMigrationTool : MigrationTool
        {
            protected override Assembly LoadAssembly(string assemblyFile)
            {
                return Assembly.GetExecutingAssembly();
            }

            protected override void WriteFile(string path, string content, bool overwrite)
            {
            }
        }
    }
}
