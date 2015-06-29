// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class MigrationTest
    {
        private readonly DbContextOptionsBuilder _options;
        private readonly IServiceProvider _provider;
        private readonly SqliteTestStore _testStore;

        [Fact]
        public void DropColumn_rebuilds_table()
        {
            _testStore.ExecuteNonQuery(@"CREATE TABLE Table1 (Id INT PRIMARY KEY, Col1, Col2); 
INSERT INTO Table1 (Col1, Col2) Values('dropped value','preserved entry');");

            Migrate(up => { up.DropColumn("Col1", "Table1"); }, targetModel =>
                {
                    targetModel.Entity<UpdatedTableType>(b =>
                        {
                            b.Key(p => p.Id);
                            b.Property(p => p.Id)
                                .StoreGeneratedPattern(StoreGeneratedPattern.Identity);

                            b.Property(p => p.Col2);
                            b.ToSqliteTable("Table1");
                        });
                });

            AssertColumns("Table1", new List<string>
            {
                "Id", "Col2"
            });

            using (var context = CreateContext())
            {
                // Assert data moved
                var all = context.Set<UpdatedTableType>().ToList();
                Assert.Equal(1, all.Count);
                Assert.Equal(1, all[0].Id);
                Assert.Equal("preserved entry", all[0].Col2);
            }
        }

        public MigrationTest()
        {
            _testStore = SqliteTestStore.CreateScratch();

            _provider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .AddDbContext<TestContext>()
                .ServiceCollection()
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder(new DbContextOptions<TestContext>());
            _options.UseSqlite(_testStore.Connection);
        }

        private void AssertColumns(string name, List<string> columnNames)
        {
            var command = _testStore.Connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info({name});";
            var columns = new List<string>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(reader.GetFieldValue<string>(1));
                }
            }
            Assert.Equal(columnNames, columns);
        }

        private void Migrate(Action<MigrationBuilder> up, Action<ModelBuilder> targetModel)
        {
            var migration = new TestMigration(up, targetModel);
            var builder = new MigrationBuilder();
            migration.Up(builder);
            var operations = builder.Operations.ToList();

            using (var context = CreateContext())
            {
                var model = context.GetService<IMigrationModelFactory>().Create(migration.BuildTargetModel);
                var command = context.GetService<IMigrationSqlGenerator>().Generate(operations, model);

                using (var transaction = context.Database.GetDbConnection().BeginTransaction())
                {
                    context.GetService<ISqlStatementExecutor>()
                        .ExecuteNonQuery(context.Database.GetService<IRelationalConnection>(), transaction, command);
                    transaction.Commit();
                }
            }
        }

        private TestContext CreateContext() => new TestContext(_provider, _options.Options);

        public class TestContext : DbContext
        {
            public TestContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<UpdatedTableType>().ToSqliteTable("Table1");
            }
        }

        public class UpdatedTableType
        {
            public int Id { get; set; }
            public string Col2 { get; set; }
        }

        public class TestMigration : Migration
        {
            private readonly Action<ModelBuilder> _targetModel;
            private readonly Action<MigrationBuilder> _up;

            public TestMigration(Action<MigrationBuilder> up, Action<ModelBuilder> targetModel)
            {
                _up = up;
                _targetModel = targetModel;
            }

            public override string Id { get; } = "TestMigration";

            public override void Up(MigrationBuilder migrationBuilder)
            {
                _up?.Invoke(migrationBuilder);
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
            }

            public override void BuildTargetModel(ModelBuilder modelBuilder)
            {
                _targetModel?.Invoke(modelBuilder);
            }
        }
    }
}
