// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Infrastructure
{
    public class MigratorTest
    {
        [Fact]
        public void Get_database_migrations()
        {
            var databaseMigrations
                = new IMigrationMetadata[]
                      {
                          new MigrationMetadata("Migration1", "Timestamp1"),
                          new MigrationMetadata("Migration2", "Timestamp1")
                      };

            var migrator = MockMigrator(databaseMigrations, new IMigrationMetadata[0]);
            var migrations = migrator.GetDatabaseMigrations();

            Assert.Equal(databaseMigrations.Length, migrations.Count);

            for (var i = 0; i < databaseMigrations.Length; i++)
            {
                Assert.Equal(databaseMigrations[i], migrations[i]);
            }
        }

        [Fact]
        public void Get_local_migrations()
        {
            var localMigrations
                = new IMigrationMetadata[]
                      {
                          new MigrationMetadata("Migration1", "Timestamp1"),
                          new MigrationMetadata("Migration2", "Timestamp1")
                      };

            var migrator = MockMigrator(new IMigrationMetadata[0], localMigrations);
            var migrations = migrator.GetLocalMigrations();

            Assert.Equal(localMigrations.Length, migrations.Count);

            for (var i = 0; i < localMigrations.Length; i++)
            {
                Assert.Equal(localMigrations[i], migrations[i]);
            }
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_single_initial_migration()
        {
            var migrator = MockMigrator(
                new IMigrationMetadata[0],
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new CreateTableOperation(new Table("MyTable1"))
                                          }
                            }
                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql();

            Assert.Equal(2, sqlStatements.Count);
            Assert.Equal("CreateTableOperationSql", sqlStatements[0].Sql);
            Assert.Equal("Migration1InsertSql", sqlStatements[1].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_multiple_initial_migrations()
        {
            var migrator = MockMigrator(
                new IMigrationMetadata[0],
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new CreateTableOperation(new Table("MyTable1"))
                                          }
                            },
                        new MigrationMetadata("Migration2", "Timestamp2")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                              new CreateTableOperation(new Table("MyTable2"))
                                          }
                            }

                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql();

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("CreateTableOperationSql", sqlStatements[0].Sql);
            Assert.Equal("Migration1InsertSql", sqlStatements[1].Sql);
            Assert.Equal("AddColumnOperationSql", sqlStatements[2].Sql);
            Assert.Equal("CreateTableOperationSql", sqlStatements[3].Sql);            
            Assert.Equal("Migration2InsertSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_single_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                              new CreateTableOperation(new Table("MyTable2"))
                                          }
                            }

                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql();

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("CreateTableOperationSql", sqlStatements[1].Sql);
            Assert.Equal("Migration2InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_multiple_migrations()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                              new CreateTableOperation(new Table("MyTable2"))
                                          }
                            },
                        new MigrationMetadata("Migration4", "Timestamp4")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new DropColumnOperation("MyTable1", "Foo"),
                                          }
                            }
                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql();

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("CreateTableOperationSql", sqlStatements[1].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[2].Sql);
            Assert.Equal("DropColumnOperationSql", sqlStatements[3].Sql);
            Assert.Equal("Migration4InsertSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_target_last_local_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                              new CreateTableOperation(new Table("MyTable2"))
                                          }
                            },
                        new MigrationMetadata("Migration4", "Timestamp4")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new DropColumnOperation("MyTable1", "Foo"),
                                          }
                            }
                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql("Migration4");

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("CreateTableOperationSql", sqlStatements[1].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[2].Sql);
            Assert.Equal("DropColumnOperationSql", sqlStatements[3].Sql);
            Assert.Equal("Migration4InsertSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_target_local_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                              new CreateTableOperation(new Table("MyTable2"))
                                          }
                            },
                        new MigrationMetadata("Migration4", "Timestamp4")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new DropColumnOperation("MyTable1", "Foo"),
                                          }
                            }
                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql("Migration3");

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("CreateTableOperationSql", sqlStatements[1].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_target_last_database_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3"),
                        new MigrationMetadata("Migration4", "Timestamp4")
                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql("Migration3");

            Assert.Equal(0, sqlStatements.Count);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_target_database_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3"),
                        new MigrationMetadata("Migration4", "Timestamp4")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new DropColumnOperation("MyTable1", "Foo"),
                                              new DropTableOperation("MyTable2")
                                          }
                            },
                        new MigrationMetadata("Migration4", "Timestamp4")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new AddColumnOperation("MyTable1", new Column("Foo", typeof(string)))
                                          }
                            },
                        new MigrationMetadata("Migration5", "Timestamp5")
                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql("Migration2");

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("Migration4DeleteSql", sqlStatements[1].Sql);
            Assert.Equal("DropColumnOperationSql", sqlStatements[2].Sql);
            Assert.Equal("DropTableOperationSql", sqlStatements[3].Sql);
            Assert.Equal("Migration3DeleteSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_local_migration_before_last_database_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration4", "Timestamp4")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new DropColumnOperation("MyTable1", "Foo"),
                                              new DropTableOperation("MyTable2")
                                          }
                            },
                        new MigrationMetadata("Migration4", "Timestamp4")
                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql();

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("DropColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("DropTableOperationSql", sqlStatements[1].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_target_local_migration_before_last_database_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration4", "Timestamp4")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2"),
                        new MigrationMetadata("Migration3", "Timestamp3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new DropColumnOperation("MyTable1", "Foo"),
                                              new DropTableOperation("MyTable2")
                                          }
                            },
                        new MigrationMetadata("Migration4", "Timestamp4")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                          {
                                              new AddColumnOperation("MyTable1", new Column("Foo", typeof(string)))
                                          }
                            }

                    }
                );

            var sqlStatements = migrator.GenerateUpdateDatabaseSql("Migration3");

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("Migration4DeleteSql", sqlStatements[1].Sql);
            Assert.Equal("DropColumnOperationSql", sqlStatements[2].Sql);
            Assert.Equal("DropTableOperationSql", sqlStatements[3].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_throws_if_local_migrations_do_not_include_all_database_migrations()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1")
                    }
                );

            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("Migration2", "Timestamp2"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql()).Message);
            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("Migration2", "Timestamp2"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql("Migration1")).Message);

            migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2")
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration3", "Timestamp3"),
                        new MigrationMetadata("Migration4", "Timestamp4")
                    }
                );

            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("Migration2", "Timestamp2"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql()).Message);
            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("Migration2", "Timestamp2"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql("Migration1")).Message);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_throws_if_target_migration_not_found()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                    },
                new[]
                    {
                        new MigrationMetadata("Migration1", "Timestamp1"),
                        new MigrationMetadata("Migration2", "Timestamp2")
                    }
                );

            Assert.Equal(
                Strings.FormatTargetMigrationNotFound("Foo"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql("Foo")).Message);
        }

        [Fact]
        public void UpdateDatabase_executes_generated_sql_statements()
        {
            var contextConfigurationMock = new Mock<DbContextConfiguration>();
            contextConfigurationMock.SetupGet(cc => cc.Connection).Returns(new Mock<RelationalConnection>().Object);

            var sqlStatementExecutorMock = new Mock<SqlStatementExecutor>();
            sqlStatementExecutorMock.Setup(sse => sse.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<IEnumerable<SqlStatement>>()))
                .Callback<DbConnection, IEnumerable<SqlStatement>>(
                    (connection, statements) => Assert.Equal("GeneratedUpdateDatabaseSql", statements.First().Sql));

            var migratorMock
                = new Mock<Migrator>(
                    contextConfigurationMock.Object,
                    MockHistoryRepository(contextConfigurationMock.Object, new IMigrationMetadata[0]).Object,
                    MockMigrationAssembly(contextConfigurationMock.Object, new IMigrationMetadata[0]).Object,
                    new DatabaseBuilder(),
                    MockMigrationOperationSqlGeneratorFactory().Object,
                    new Mock<SqlGenerator>().Object,
                    sqlStatementExecutorMock.Object)
                      {
                          CallBase = true
                      };
            migratorMock.Setup(m => m.GenerateUpdateDatabaseSql()).Returns(new[] { new SqlStatement("GeneratedUpdateDatabaseSql") });
            migratorMock.Setup(m => m.GenerateUpdateDatabaseSql(It.IsAny<string>())).Returns(new[] { new SqlStatement("GeneratedUpdateDatabaseSql") });

            migratorMock.Object.UpdateDatabase();
            migratorMock.Object.UpdateDatabase("TargetMigrationName");
        }

        #region Fixture

        private static Migrator MockMigrator(
            IReadOnlyList<IMigrationMetadata> databaseMigrations,
            IReadOnlyList<IMigrationMetadata> localMigrations)
        {
            var contextConfiguration = new Mock<DbContextConfiguration>().Object;
            return
                new Mock<Migrator>(
                    contextConfiguration,
                    MockHistoryRepository(contextConfiguration, databaseMigrations).Object,
                    MockMigrationAssembly(contextConfiguration, localMigrations).Object,
                    new DatabaseBuilder(),
                    MockMigrationOperationSqlGeneratorFactory().Object,
                    new Mock<SqlGenerator>().Object,
                    new Mock<SqlStatementExecutor>().Object)
                    {
                        CallBase = true
                    }
                    .Object;
        }

        private static Mock<HistoryRepository> MockHistoryRepository(
            DbContextConfiguration contextConfiguration, IReadOnlyList<IMigrationMetadata> migrations)
        {
            var mock = new Mock<HistoryRepository>(contextConfiguration);

            mock.SetupGet(hr => hr.Migrations).Returns(migrations.ToArray());

            mock.Setup(hr => hr.GenerateInsertMigrationSql(It.IsAny<IMigrationMetadata>(), It.IsAny<SqlGenerator>()))
                .Returns<IMigrationMetadata, SqlGenerator>((m, sg) => new[] { new SqlStatement(m.Name + "InsertSql") });

            mock.Setup(hr => hr.GenerateDeleteMigrationSql(It.IsAny<IMigrationMetadata>(), It.IsAny<SqlGenerator>()))
                .Returns<IMigrationMetadata, SqlGenerator>((m, sg) => new[] { new SqlStatement(m.Name + "DeleteSql") });

            return mock;
        }

        private static Mock<MigrationAssembly> MockMigrationAssembly(
            DbContextConfiguration contextConfiguration, IReadOnlyList<IMigrationMetadata> migrations)
        {
            var mock = new Mock<MigrationAssembly>(contextConfiguration);

            mock.SetupGet(ma => ma.Migrations).Returns(migrations);

            return mock;
        }

        private static Mock<MigrationOperationSqlGenerator> MockMigrationOperationSqlGenerator()
        {
            var mock = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());

            mock.Setup(mosg => mosg.Generate(It.IsAny<IReadOnlyList<MigrationOperation>>(), It.IsAny<bool>()))
                .Returns<IReadOnlyList<MigrationOperation>, bool>(
                    (operations, idempotent) => operations.Select(op => new SqlStatement(op.GetType().Name + "Sql")));

            return mock;
        }

        private static Mock<IMigrationOperationSqlGeneratorFactory> MockMigrationOperationSqlGeneratorFactory()
        {
            var mock = new Mock<IMigrationOperationSqlGeneratorFactory>();

            mock.Setup(mosgf => mosgf.Create(It.IsAny<DatabaseModel>()))
                .Returns(MockMigrationOperationSqlGenerator().Object);

            return mock;
        }

        #endregion
    }
}
