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
using Microsoft.Data.Entity.Storage;
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
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
        public void Get_pending_migrations()
        {
            var databaseMigrations
                = new IMigrationMetadata[]
                    {
                        new MigrationMetadata("000000000000001_M1"),
                        new MigrationMetadata("000000000000003_M3")
                    };

            var localMigrations
                = new IMigrationMetadata[]
                    {
                        new MigrationMetadata("000000000000001_M1"),
                        new MigrationMetadata("000000000000002_M2"),
                        new MigrationMetadata("000000000000003_M3"),
                        new MigrationMetadata("000000000000004_M4")
                    };

            var migrator = MockMigrator(databaseMigrations, localMigrations);
            var migrations = migrator.GetPendingMigrations();

            Assert.Equal(2, migrations.Count);

            Assert.Equal(localMigrations[1], migrations[0]);
            Assert.Equal(localMigrations[3], migrations[1]);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_with_single_initial_migration()
        {
            var migrator = MockMigrator(
                new IMigrationMetadata[0],
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
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
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new CreateTableOperation(new Table("MyTable1"))
                                        }
                            },
                        new MigrationMetadata("000000000000002_Migration2")
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
                        new MigrationMetadata("000000000000001_Migration1")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2"),
                        new MigrationMetadata("000000000000003_Migration3")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2"),
                        new MigrationMetadata("000000000000003_Migration3"),
                        new MigrationMetadata("000000000000004_Migration4")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2"),
                        new MigrationMetadata("000000000000003_Migration3"),
                        new MigrationMetadata("000000000000004_Migration4")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string)))
                                        }
                            },
                        new MigrationMetadata("000000000000005_Migration5")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2"),
                        new MigrationMetadata("000000000000004_Migration4")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2"),
                        new MigrationMetadata("000000000000004_Migration4")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
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
        public void GenerateUpdateDatabaseSql_with_InitialDatabase()
        {
            var databaseMigrations
                = new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    };
            var localMigrations
                = new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropTableOperation("MyTable1")
                                        }
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            }
                    };

            var contextConfiguration = new Mock<DbContextConfiguration>().Object;
            var historyRepository = MockHistoryRepository(contextConfiguration, databaseMigrations);
            historyRepository.CallBase = true;

            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());

            sqlGenerator.Setup(mosg => mosg.Generate(It.IsAny<IReadOnlyList<MigrationOperation>>()))
                .Returns<IReadOnlyList<MigrationOperation>>(
                    operations => operations.Select(
                        op => new SqlStatement(
                            (op is DropTableOperation
                                ? "Drop" + ((DropTableOperation)op).TableName.Name
                                : op.GetType().Name)
                            + "Sql")));

            var sqlGeneratorFactory = new Mock<IMigrationOperationSqlGeneratorFactory>();

            sqlGeneratorFactory.Setup(mosgf => mosgf.Create(It.IsAny<DatabaseModel>()))
                .Returns(sqlGenerator.Object);

            var migrator
                = new Mock<DbMigrator>(
                    contextConfiguration,
                    historyRepository.Object,
                    MockMigrationAssembly(contextConfiguration, localMigrations).Object,
                    new ModelDiffer(new DatabaseBuilder()),
                    sqlGeneratorFactory.Object,
                    new Mock<SqlGenerator>().Object,
                    new Mock<SqlStatementExecutor>().Object)
                    {
                        CallBase = true
                    }
                    .Object;

            var sqlStatements = migrator.GenerateUpdateDatabaseSql(DbMigrator.InitialDatabase);

            Assert.Equal(6, sqlStatements.Count);
            Assert.Equal("DropColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("DropMyTable2Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration2DeleteSql", sqlStatements[2].Sql);
            Assert.Equal("DropMyTable1Sql", sqlStatements[3].Sql);
            Assert.Equal("Migration1DeleteSql", sqlStatements[4].Sql);
            Assert.Equal("Drop__MigrationHistorySql", sqlStatements[5].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_when_history_repository_does_not_exist()
        {
            var localMigrations
                = new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new CreateTableOperation(new Table("MyTable1"))
                                        }
                            }
                    };

            var contextConfiguration = new Mock<DbContextConfiguration>().Object;
            var historyRepository = new Mock<HistoryRepository>(contextConfiguration) { CallBase = true };
            var dbException = new Mock<DataStoreException>();

            historyRepository.SetupGet(hr => hr.Migrations).Throws(dbException.Object);
            historyRepository.Setup(hr => hr.GenerateInsertMigrationSql(It.IsAny<IMigrationMetadata>(), It.IsAny<SqlGenerator>()))
                .Returns<IMigrationMetadata, SqlGenerator>((m, sg) => new[] { new SqlStatement(m.GetMigrationName() + "InsertSql") });

            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());

            sqlGenerator.Setup(mosg => mosg.Generate(It.IsAny<IReadOnlyList<MigrationOperation>>()))
                .Returns<IReadOnlyList<MigrationOperation>>(
                    operations => operations.Select(op => new SqlStatement("Create" + ((CreateTableOperation)op).Table.Name + "Sql")));

            var sqlGeneratorFactory = new Mock<IMigrationOperationSqlGeneratorFactory>();

            sqlGeneratorFactory.Setup(mosgf => mosgf.Create())
                .Returns(sqlGenerator.Object);

            sqlGeneratorFactory.Setup(mosgf => mosgf.Create(It.IsAny<DatabaseModel>()))
                .Returns(sqlGenerator.Object);

            var migrator
                = new Mock<DbMigrator>(
                    contextConfiguration,
                    historyRepository.Object,
                    MockMigrationAssembly(contextConfiguration, localMigrations).Object,
                    new ModelDiffer(new DatabaseBuilder()),
                    sqlGeneratorFactory.Object,
                    new Mock<SqlGenerator>().Object,
                    new Mock<SqlStatementExecutor>().Object)
                    {
                        CallBase = true
                    }
                    .Object;

            var sqlStatements = migrator.GenerateUpdateDatabaseSql();

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("Create__MigrationHistorySql", sqlStatements[0].Sql);
            Assert.Equal("CreateMyTable1Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration1InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_throws_if_local_migrations_do_not_include_all_database_migrations()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                    }
                );

            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql()).Message);
            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql("Migration1")).Message);

            migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000003_Migration3"),
                        new MigrationMetadata("000000000000004_Migration4")
                    }
                );

            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql()).Message);
            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.GenerateUpdateDatabaseSql("Migration1")).Message);
        }

        [Fact]
        public void GenerateUpdateDatabaseSql_throws_if_target_migration_not_found()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1"),
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
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
            sqlStatementExecutorMock.Setup(sse => sse.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()))
                .Callback<DbConnection, DbTransaction, IEnumerable<SqlStatement>>(AssertCallback);

            var migratorMock
                = new Mock<DbMigrator>(
                    contextConfigurationMock.Object,
                    MockHistoryRepository(contextConfigurationMock.Object, new IMigrationMetadata[0]).Object,
                    MockMigrationAssembly(contextConfigurationMock.Object, new IMigrationMetadata[0]).Object,
                    new ModelDiffer(new DatabaseBuilder()),
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

        private static void AssertCallback(DbConnection _, DbTransaction __, IEnumerable<SqlStatement> statements)
        {
            Assert.Equal("GeneratedUpdateDatabaseSql", statements.First().Sql);
        }

        #region Fixture

        private static DbMigrator MockMigrator(
            IReadOnlyList<IMigrationMetadata> databaseMigrations,
            IReadOnlyList<IMigrationMetadata> localMigrations)
        {
            var contextConfiguration = new Mock<DbContextConfiguration>().Object;
            return
                new Mock<DbMigrator>(
                    contextConfiguration,
                    MockHistoryRepository(contextConfiguration, databaseMigrations).Object,
                    MockMigrationAssembly(contextConfiguration, localMigrations).Object,
                    new ModelDiffer(new DatabaseBuilder()),
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
                .Returns<IMigrationMetadata, SqlGenerator>((m, sg) => new[] { new SqlStatement(m.GetMigrationName() + "InsertSql") });

            mock.Setup(hr => hr.GenerateDeleteMigrationSql(It.IsAny<IMigrationMetadata>(), It.IsAny<SqlGenerator>()))
                .Returns<IMigrationMetadata, SqlGenerator>((m, sg) => new[] { new SqlStatement(m.GetMigrationName() + "DeleteSql") });

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

            mock.Setup(mosg => mosg.Generate(It.IsAny<IReadOnlyList<MigrationOperation>>()))
                .Returns<IReadOnlyList<MigrationOperation>>(
                    operations => operations.Select(op => new SqlStatement(op.GetType().Name + "Sql")));

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
