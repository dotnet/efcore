// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Utilities;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.Infrastructure
{
    public class MigrationAssemblyTest
    {
        [Fact]
        public void Create_migration_assembly()
        {
            using (var context = new Context())
            {
                var migrationAssembly = ((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<MigrationAssembly>();

                Assert.Equal("EntityFramework.Relational.Tests", migrationAssembly.Assembly.GetName().Name);
            }
        }

        [Fact]
        public void Configure_assembly_and_namespace()
        {
            using (var context = new Context
            {
                MigrationAssembly = new MockAssembly()
            })
            {
                var migrationAssembly = ((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<MigrationAssembly>();

                Assert.Equal("MockAssembly", migrationAssembly.Assembly.FullName);
            }
        }

        [Fact]
        public void Load_and_cache_migrations()
        {
            using (var context = new Context())
            {
                var migrationAssembly = ((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<MigrationAssembly>();

                var migrations1 = migrationAssembly.Migrations;
                var migrations2 = migrationAssembly.Migrations;

                Assert.Same(migrations1, migrations2);
                Assert.Equal(2, migrations1.Count);
                Assert.Equal("000000000000001_Migration1", migrations1[0].GetMigrationId());
                Assert.Equal("000000000000002_Migration2", migrations1[1].GetMigrationId());
            }
        }

        [Fact]
        public void Loads_and_cache_model_snapshot()
        {
            using (var context = new Context())
            {
                var migrationAssembly = ((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<MigrationAssembly>();

                var model1 = migrationAssembly.ModelSnapshot;
                var model2 = migrationAssembly.ModelSnapshot;

                Assert.Same(model1, model2);
            }
        }

        #region Fixture

        public class Context : DbContext
        {
            internal Assembly MigrationAssembly { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                ((IDbContextOptions)options).AddOrUpdateExtension<MyRelationalOptionsExtension>(x => x.ConnectionString = "ConnectionString");

                if (MigrationAssembly != null)
                {
                    options.UseMigrationAssembly(MigrationAssembly);
                }
            }
        }

        public class MyRelationalOptionsExtension : RelationalOptionsExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
                builder.AddMigrations();
            }
        }

        public class MockAssembly : Assembly
        {
            public override string FullName
            {
                get { return "MockAssembly"; }
            }
        }

        #endregion
    }

    #region Fixture

    namespace Migrations
    {
        [ContextType(typeof(MigrationAssemblyTest.Context))]
        public class Migration2 : Migration, IMigrationMetadata
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("UpSql");
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("DownSql");
            }

            string IMigrationMetadata.MigrationId
            {
                get { return "000000000000002_Migration2"; }
            }

            public string ProductVersion
            {
                get { return "1.2.3.4"; }
            }

            public IModel TargetModel
            {
                get { return new Entity.Metadata.Model(); }
            }
        }

        [ContextType(typeof(MigrationAssemblyTest.Context))]
        public class Migration1 : Migration, IMigrationMetadata
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("UpSql");
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("DownSql");
            }

            string IMigrationMetadata.MigrationId
            {
                get { return "000000000000001_Migration1"; }
            }

            public string ProductVersion
            {
                get { return "1.2.3.4"; }
            }

            public IModel TargetModel
            {
                get { return new Entity.Metadata.Model(); }
            }
        }

        [ContextType(typeof(MigrationAssemblyTest.Context))]
        public class ContextModelSnapshot : ModelSnapshot
        {
            public override IModel Model
            {
                get { return new Entity.Metadata.Model { StorageName = "ContextModelSnapshot" }; }
            }
        }
    }

    #endregion
}
