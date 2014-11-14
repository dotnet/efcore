// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using RelationalStrings = Microsoft.Data.Entity.Relational.Strings;

namespace Microsoft.Data.Entity.Migrations.Tests.Infrastructure
{
    public class MigratorTest
    {
        [Fact]
        public void Get_database_migrations()
        {
            var localMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    };
            var databaseMigrations = localMigrations;

            var migrator = MockMigrator(databaseMigrations, localMigrations);
            var migrations = migrator.GetDatabaseMigrations();

            Assert.Equal(databaseMigrations.Length, migrations.Count);

            for (var i = 0; i < databaseMigrations.Length; i++)
            {
                Assert.Equal(databaseMigrations[i].MigrationId, migrations[i].GetMigrationId());
            }
        }

        [Fact]
        public void Get_local_migrations()
        {
            var localMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    };

            var migrator = MockMigrator(new MigrationInfo[0], localMigrations);
            var migrations = migrator.GetLocalMigrations();

            Assert.Equal(localMigrations.Length, migrations.Count);

            for (var i = 0; i < localMigrations.Length; i++)
            {
                Assert.Equal(localMigrations[i].MigrationId, migrations[i].GetMigrationId());
            }
        }

        [Fact]
        public void Get_pending_migrations()
        {
            var databaseMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_M1"),
                        new MigrationInfo("000000000000003_M3")
                    };

            var localMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_M1"),
                        new MigrationInfo("000000000000002_M2"),
                        new MigrationInfo("000000000000003_M3"),
                        new MigrationInfo("000000000000004_M4")
                    };

            var migrator = MockMigrator(databaseMigrations, localMigrations);
            var migrations = migrator.GetPendingMigrations();

            Assert.Equal(2, migrations.Count);

            Assert.Equal(localMigrations[1].MigrationId, migrations[0].GetMigrationId());
            Assert.Equal(localMigrations[3].MigrationId, migrations[1].GetMigrationId());
        }

        [Fact]
        public void ScriptMigrations_with_single_initial_migration()
        {
            var migrator = MockMigrator(
                new MigrationInfo[0],
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
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

            var sqlStatements = migrator.ScriptMigrations();

            Assert.Equal(2, sqlStatements.Count);
            Assert.Equal("CreateMyTable1Sql", sqlStatements[0].Sql);
            Assert.Equal("Migration1InsertSql", sqlStatements[1].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_multiple_initial_migrations()
        {
            var migrator = MockMigrator(
                new MigrationInfo[0],
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new CreateTableOperation(new Table("MyTable1"))
                                        }
                            },
                        new MigrationInfo("000000000000002_Migration2")
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

            var sqlStatements = migrator.ScriptMigrations();

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("CreateMyTable1Sql", sqlStatements[0].Sql);
            Assert.Equal("Migration1InsertSql", sqlStatements[1].Sql);
            Assert.Equal("AddColumnOperationSql", sqlStatements[2].Sql);
            Assert.Equal("CreateMyTable2Sql", sqlStatements[3].Sql);
            Assert.Equal("Migration2InsertSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_single_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
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

            var sqlStatements = migrator.ScriptMigrations();

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("CreateMyTable2Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration2InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_multiple_migrations()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo")
                                        }
                            }
                    }
                );

            var sqlStatements = migrator.ScriptMigrations();

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("CreateMyTable2Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[2].Sql);
            Assert.Equal("DropColumnOperationSql", sqlStatements[3].Sql);
            Assert.Equal("Migration4InsertSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_target_last_local_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo")
                                        }
                            }
                    }
                );

            var sqlStatements = migrator.ScriptMigrations("Migration4");

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("CreateMyTable2Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[2].Sql);
            Assert.Equal("DropColumnOperationSql", sqlStatements[3].Sql);
            Assert.Equal("Migration4InsertSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_target_local_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo")
                                        }
                            }
                    }
                );

            var sqlStatements = migrator.ScriptMigrations("Migration3");

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("CreateMyTable2Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_target_last_database_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2"),
                        new MigrationInfo("000000000000003_Migration3")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2"),
                        new MigrationInfo("000000000000003_Migration3"),
                        new MigrationInfo("000000000000004_Migration4")
                    }
                );

            var sqlStatements = migrator.ScriptMigrations("Migration3");

            Assert.Equal(0, sqlStatements.Count);
        }

        [Fact]
        public void ScriptMigrations_with_target_database_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2"),
                        new MigrationInfo("000000000000003_Migration3"),
                        new MigrationInfo("000000000000004_Migration4")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string)))
                                        }
                            },
                        new MigrationInfo("000000000000005_Migration5")
                    }
                );

            var sqlStatements = migrator.ScriptMigrations("Migration2");

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("Migration4DeleteSql", sqlStatements[1].Sql);
            Assert.Equal("DropColumnOperationSql", sqlStatements[2].Sql);
            Assert.Equal("DropMyTable2Sql", sqlStatements[3].Sql);
            Assert.Equal("Migration3DeleteSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_local_migration_before_last_database_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2"),
                        new MigrationInfo("000000000000004_Migration4")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
                    }
                );

            var sqlStatements = migrator.ScriptMigrations();

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("DropColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("DropMyTable2Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_target_local_migration_before_last_database_migration()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2"),
                        new MigrationInfo("000000000000004_Migration4")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
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

            var sqlStatements = migrator.ScriptMigrations("Migration3");

            Assert.Equal(5, sqlStatements.Count);
            Assert.Equal("AddColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("Migration4DeleteSql", sqlStatements[1].Sql);
            Assert.Equal("DropColumnOperationSql", sqlStatements[2].Sql);
            Assert.Equal("DropMyTable2Sql", sqlStatements[3].Sql);
            Assert.Equal("Migration3InsertSql", sqlStatements[4].Sql);
        }

        [Fact]
        public void ScriptMigrations_with_InitialDatabase()
        {
            var databaseMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    };
            var localMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropTableOperation("MyTable1")
                                        }
                            },
                        new MigrationInfo("000000000000002_Migration2")
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

            var migrator = MockMigrator(databaseMigrations, localMigrations);

            var sqlStatements = migrator.ScriptMigrations(Migrator.InitialDatabase);

            Assert.Equal(6, sqlStatements.Count);
            Assert.Equal("DropColumnOperationSql", sqlStatements[0].Sql);
            Assert.Equal("DropMyTable2Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration2DeleteSql", sqlStatements[2].Sql);
            Assert.Equal("DropMyTable1Sql", sqlStatements[3].Sql);
            Assert.Equal("Migration1DeleteSql", sqlStatements[4].Sql);
            Assert.Equal("Drop__MigrationHistorySql", sqlStatements[5].Sql);
        }

        [Fact]
        public void ScriptMigrations_when_history_repository_does_not_exist()
        {
            var databaseMigrations = new MigrationInfo[0];
            var localMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new CreateTableOperation(new Table("MyTable1"))
                                        }
                            }
                    };

            var migrator = MockMigrator(databaseMigrations, localMigrations, databaseExists: true, historyRepositoryExists: false);

            var sqlStatements = migrator.ScriptMigrations();

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("Create__MigrationHistorySql", sqlStatements[0].Sql);
            Assert.Equal("CreateMyTable1Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration1InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void ScriptMigrations_throws_if_local_migrations_do_not_include_all_database_migrations()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                    }
                );

            Assert.Equal(
                Strings.LocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations()).Message);
            Assert.Equal(
                Strings.LocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations("Migration1")).Message);

            migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000003_Migration3"),
                        new MigrationInfo("000000000000004_Migration4")
                    }
                );

            Assert.Equal(
                Strings.LocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations()).Message);
            Assert.Equal(
                Strings.LocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations("Migration1")).Message);
        }

        [Fact]
        public void ScriptMigrations_throws_if_target_migration_not_found()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    }
                );

            Assert.Equal(
                Strings.TargetMigrationNotFound("Foo"),
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations("Foo")).Message);
        }

        [Fact]
        public void ApplyMigrations_creates_database_and_executes_upgrade_statements()
        {
            var databaseMigrations = new MigrationInfo[0];
            var localMigrations
                = new[]
                      {
                          new MigrationInfo("000000000000001_Migration1")
                              {
                                  UpgradeOperations = new[] { new SqlOperation("SomeSql") },
                                  TargetModel = new Metadata.Model()
                              }
                      };

            Mock<RelationalDataStoreCreator> dbCreatorMock;
            Mock<DbConnection> dbConnectionMock;
            Mock<DbTransaction> dbTransactionMock;
            Mock<SqlStatementExecutor> sqlStatementExecutorMock;

            var migrator
                = MockMigrator(
                    databaseMigrations,
                    localMigrations,
                    /*databaseExists*/ false,
                    /*historyTableExists*/ false,
                    out dbCreatorMock,
                    out dbConnectionMock,
                    out dbTransactionMock,
                    out sqlStatementExecutorMock);

            var callCount = 0;

            sqlStatementExecutorMock.Setup(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()))
                .Callback<DbConnection, DbTransaction, IEnumerable<SqlStatement>>(
                    (_, __, statements) =>
                        {
                            switch (++callCount)
                            {
                                case 1:
                                    Assert.Equal(new[] { "Create__MigrationHistorySql" }, statements.Select(s => s.Sql));
                                    break;
                                case 2:
                                    Assert.Equal(new[] { "SomeSql", "Migration1InsertSql" }, statements.Select(s => s.Sql));
                                    break;
                                default:
                                    Assert.False(true, "Unexpected call count.");
                                    break;
                            }
                        });

            migrator.ApplyMigrations();

            dbCreatorMock.Verify(m => m.Exists(), Times.Once);
            dbCreatorMock.Verify(m => m.Create(), Times.Once);
            dbConnectionMock.Protected().Verify("BeginDbTransaction", Times.Exactly(2), IsolationLevel.Serializable);
            dbTransactionMock.Verify(m => m.Commit(), Times.Exactly(2));
            sqlStatementExecutorMock.Verify(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()), Times.Exactly(2));
        }

        [Fact]
        public void ApplyMigrations_creates_history_table_and_executes_upgrade_statements_if_database_exists()
        {
            var databaseMigrations = new MigrationInfo[0];
            var localMigrations
                = new[]
                      {
                          new MigrationInfo("000000000000001_Migration1")
                              {
                                  UpgradeOperations = new[] { new SqlOperation("SomeSql") },
                                  TargetModel = new Metadata.Model()
                              }
                      };

            Mock<RelationalDataStoreCreator> dbCreatorMock;
            Mock<DbConnection> dbConnectionMock;
            Mock<DbTransaction> dbTransactionMock;
            Mock<SqlStatementExecutor> sqlStatementExecutorMock;

            var migrator
                = MockMigrator(
                    databaseMigrations,
                    localMigrations,
                    /*databaseExists*/ true,
                    /*historyTableExists*/ false,
                    out dbCreatorMock,
                    out dbConnectionMock,
                    out dbTransactionMock,
                    out sqlStatementExecutorMock);

            var callCount = 0;

            sqlStatementExecutorMock.Setup(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()))
                .Callback<DbConnection, DbTransaction, IEnumerable<SqlStatement>>(
                    (_, __, statements) =>
                        {
                            switch (++callCount)
                            {
                                case 1:
                                    Assert.Equal(new[] { "Create__MigrationHistorySql" }, statements.Select(s => s.Sql));
                                    break;
                                case 2:
                                    Assert.Equal(new[] { "SomeSql", "Migration1InsertSql" }, statements.Select(s => s.Sql));
                                    break;
                                default:
                                    Assert.False(true, "Unexpected call count.");
                                    break;
                            }
                        });

            migrator.ApplyMigrations();

            dbCreatorMock.Verify(m => m.Exists(), Times.Once);
            dbCreatorMock.Verify(m => m.Create(), Times.Never);
            dbConnectionMock.Protected().Verify("BeginDbTransaction", Times.Exactly(2), IsolationLevel.Serializable);
            dbTransactionMock.Verify(m => m.Commit(), Times.Exactly(2));
            sqlStatementExecutorMock.Verify(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()), Times.Exactly(2));
        }

        [Fact]
        public void ApplyMigrations_executes_upgrade_statements_if_database_and_history_table_exist()
        {
            var databaseMigrations = new MigrationInfo[0];
            var localMigrations
                = new[]
                      {
                          new MigrationInfo("000000000000001_Migration1")
                              {
                                  UpgradeOperations = new[] { new SqlOperation("SomeSql") },
                                  TargetModel = new Metadata.Model()
                              }
                      };

            Mock<RelationalDataStoreCreator> dbCreatorMock;
            Mock<DbConnection> dbConnectionMock;
            Mock<DbTransaction> dbTransactionMock;
            Mock<SqlStatementExecutor> sqlStatementExecutorMock;

            var migrator
                = MockMigrator(
                    databaseMigrations,
                    localMigrations,
                    /*databaseExists*/ true,
                    /*historyTableExists*/ true,
                    out dbCreatorMock,
                    out dbConnectionMock,
                    out dbTransactionMock,
                    out sqlStatementExecutorMock);

            var expectedSql = new[] { "SomeSql", "Migration1InsertSql" };

            sqlStatementExecutorMock.Setup(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()))
                .Callback<DbConnection, DbTransaction, IEnumerable<SqlStatement>>(
                    (_, __, statements) => Assert.Equal(expectedSql, statements.Select(s => s.Sql)));

            migrator.ApplyMigrations();

            dbCreatorMock.Verify(m => m.Exists(), Times.Once);
            dbCreatorMock.Verify(m => m.Create(), Times.Never);
            dbConnectionMock.Protected().Verify("BeginDbTransaction", Times.Once(), IsolationLevel.Serializable);
            dbTransactionMock.Verify(m => m.Commit(), Times.Once);
            sqlStatementExecutorMock.Verify(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()), Times.Once);
        }

        [Fact]
        public void ApplyMigrations_executes_downgrade_statements()
        {
            var databaseMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000001_Migration2")
                    };
            var localMigrations
                = new[]
                      {
                          new MigrationInfo("000000000000001_Migration1")
                              {
                                  TargetModel = new Metadata.Model()
                              },
                          new MigrationInfo("000000000000001_Migration2")
                              {
                                  DowngradeOperations = new[] { new SqlOperation("SomeSql") },
                              }
                      };

            Mock<RelationalDataStoreCreator> dbCreatorMock;
            Mock<DbConnection> dbConnectionMock;
            Mock<DbTransaction> dbTransactionMock;
            Mock<SqlStatementExecutor> sqlStatementExecutorMock;

            var migrator
                = MockMigrator(
                    databaseMigrations,
                    localMigrations,
                    /*databaseExists*/ true,
                    /*historyTableExists*/ true,
                    out dbCreatorMock,
                    out dbConnectionMock,
                    out dbTransactionMock,
                    out sqlStatementExecutorMock);

            var expectedSql = new[] { "SomeSql", "Migration2DeleteSql" };

            sqlStatementExecutorMock.Setup(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()))
                .Callback<DbConnection, DbTransaction, IEnumerable<SqlStatement>>(
                    (_, __, statements) => Assert.Equal(expectedSql, statements.Select(s => s.Sql)));

            migrator.ApplyMigrations("Migration1");

            dbCreatorMock.Verify(m => m.Exists(), Times.Once);
            dbCreatorMock.Verify(m => m.Create(), Times.Never);
            dbConnectionMock.Protected().Verify("BeginDbTransaction", Times.Once(), IsolationLevel.Serializable);
            dbTransactionMock.Verify(m => m.Commit(), Times.Once);
            sqlStatementExecutorMock.Verify(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()), Times.Once);
        }

        [Fact]
        public void ApplyMigrations_executes_downgrade_statements_and_removes_history_table()
        {
            var databaseMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                    };
            var localMigrations
                = new[]
                      {
                          new MigrationInfo("000000000000001_Migration1")
                              {
                                  DowngradeOperations = new[] { new SqlOperation("SomeSql") },
                              }
                      };

            Mock<RelationalDataStoreCreator> dbCreatorMock;
            Mock<DbConnection> dbConnectionMock;
            Mock<DbTransaction> dbTransactionMock;
            Mock<SqlStatementExecutor> sqlStatementExecutorMock;

            var migrator
                = MockMigrator(
                    databaseMigrations,
                    localMigrations,
                    /*databaseExists*/ true,
                    /*historyTableExists*/ true,
                    out dbCreatorMock,
                    out dbConnectionMock,
                    out dbTransactionMock,
                    out sqlStatementExecutorMock);

            var callCount = 0;

            sqlStatementExecutorMock.Setup(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()))
                .Callback<DbConnection, DbTransaction, IEnumerable<SqlStatement>>(
                    (_, __, statements) =>
                        {
                            switch (++callCount)
                            {
                                case 1:
                                    Assert.Equal(new[] { "SomeSql", "Migration1DeleteSql" }, statements.Select(s => s.Sql));
                                    break;
                                case 2:
                                    Assert.Equal(new[] { "Drop__MigrationHistorySql" }, statements.Select(s => s.Sql));
                                    break;
                                default:
                                    Assert.False(true, "Unexpected call count.");
                                    break;
                            }
                        });

            migrator.ApplyMigrations(Migrator.InitialDatabase);

            dbCreatorMock.Verify(m => m.Exists(), Times.Once);
            dbCreatorMock.Verify(m => m.Create(), Times.Never);
            dbConnectionMock.Protected().Verify("BeginDbTransaction", Times.Exactly(2), IsolationLevel.Serializable);
            dbTransactionMock.Verify(m => m.Commit(), Times.Exactly(2));
            sqlStatementExecutorMock.Verify(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()), Times.Exactly(2));
        }

        [Fact]
        public void ApplyMigrations_creates_database_if_no_migrations()
        {
            Mock<RelationalDataStoreCreator> dbCreatorMock;
            Mock<DbConnection> dbConnectionMock;
            Mock<DbTransaction> dbTransactionMock;
            Mock<SqlStatementExecutor> sqlStatementExecutorMock;

            var migrator
                = MockMigrator(
                    new MigrationInfo[0],
                    new MigrationInfo[0],
                    /*databaseExists*/ false,
                    /*historyTableExists*/ false,
                    out dbCreatorMock,
                    out dbConnectionMock,
                    out dbTransactionMock,
                    out sqlStatementExecutorMock);

            migrator.ApplyMigrations();

            dbCreatorMock.Verify(m => m.Exists(), Times.Once);
            dbCreatorMock.Verify(m => m.Create(), Times.Once);

            migrator.ApplyMigrations(Migrator.InitialDatabase);

            dbCreatorMock.Verify(m => m.Exists(), Times.Exactly(2));
            dbCreatorMock.Verify(m => m.Create(), Times.Exactly(2));

            sqlStatementExecutorMock.Verify(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()), Times.Never);
        }

        [Fact]
        public void Sql_statements_with_suppress_transaction_true_trigger_commit_of_current_transaction()
        {
            var databaseMigrations = new MigrationInfo[0];
            var localMigrations
                = new[]
                      {
                          new MigrationInfo("000000000000001_Migration1")
                              {
                                  UpgradeOperations
                                      = new[]
                                            {
                                                new SqlOperation("1") { SuppressTransaction = true },
                                                new SqlOperation("2"),
                                                new SqlOperation("3"),
                                                new SqlOperation("4") { SuppressTransaction = true },
                                                new SqlOperation("5"),
                                                new SqlOperation("6"),
                                                new SqlOperation("7") { SuppressTransaction = true }
                                            },
                                  TargetModel = new Metadata.Model()
                              }
                      };

            Mock<RelationalDataStoreCreator> dbCreatorMock;
            Mock<DbConnection> dbConnectionMock;
            Mock<DbTransaction> dbTransactionMock;
            Mock<SqlStatementExecutor> sqlStatementExecutorMock;

            var migrator
                = MockMigrator(
                    databaseMigrations,
                    localMigrations,
                    /*databaseExists*/ true,
                    /*historyTableExists*/ true,
                    out dbCreatorMock,
                    out dbConnectionMock,
                    out dbTransactionMock,
                    out sqlStatementExecutorMock);

            var callCount = 0;

            sqlStatementExecutorMock.Setup(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()))
                .Callback<DbConnection, DbTransaction, IEnumerable<SqlStatement>>(
                    (connection, transaction, statements) =>
                        {
                            switch (++callCount)
                            {
                                case 1:
                                    Assert.Null(transaction);
                                    Assert.Equal(new[] { "1" }, statements.Select(s => s.Sql));
                                    break;
                                case 2:
                                    Assert.Equal(new[] { "2", "3" }, statements.Select(s => s.Sql));
                                    break;
                                case 3:
                                    Assert.Null(transaction);
                                    Assert.Equal(new[] { "4" }, statements.Select(s => s.Sql));
                                    break;
                                case 4:
                                    Assert.NotNull(transaction);
                                    Assert.Equal(new[] { "5", "6" }, statements.Select(s => s.Sql));
                                    break;
                                case 5:
                                    Assert.Null(transaction);
                                    Assert.Equal(new[] { "7" }, statements.Select(s => s.Sql));
                                    break;
                                case 6:
                                    Assert.NotNull(transaction);
                                    Assert.Equal(new[] { "Migration1InsertSql" }, statements.Select(s => s.Sql));
                                    break;
                                default:
                                    Assert.False(true, "Unexpected call count.");
                                    break;
                            }
                        });

            migrator.ApplyMigrations();

            dbCreatorMock.Verify(m => m.Exists(), Times.Once);
            dbCreatorMock.Verify(m => m.Create(), Times.Never);
            dbConnectionMock.Protected().Verify("BeginDbTransaction", Times.Exactly(3), IsolationLevel.Serializable);
            dbTransactionMock.Verify(m => m.Commit(), Times.Exactly(3));
            sqlStatementExecutorMock.Verify(m => m.ExecuteNonQuery(It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<IEnumerable<SqlStatement>>()), Times.Exactly(6));
        }

        [Fact]
        public void ApplyMigrations_logs_upgrades()
        {
            var databaseMigrations = new MigrationInfo[0];
            var localMigrations
                = new[]
                      {
                          new MigrationInfo("000000000000001_Migration1")
                              {
                                  UpgradeOperations = new[] { new SqlOperation("SomeSql") },
                                  TargetModel = new Metadata.Model()
                              }
                      };

            var loggerFactory = new TestLoggerFactory();
            var migrator
                = MockMigrator(
                    databaseMigrations,
                    localMigrations,
                    /*databaseExists*/ false,
                    /*historyTableExists*/ false,
                    loggerFactory);

            migrator.ApplyMigrations();

            Assert.Equal(
                new StringBuilder()
                    .Append(typeof(Migrator).FullName).Append(" Information ")
                    .AppendLine(Strings.MigratorLoggerCreatingHistoryTable)
                    .Append(typeof(DataStoreConnection).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerBeginningTransaction("Serializable"))
                    .Append(typeof(SqlStatementExecutor).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerOpeningConnection("MyConnectionString"))
                    .Append(typeof(SqlStatementExecutor).FullName).AppendLine(" Verbose Create__MigrationHistorySql")
                    .Append(typeof(SqlStatementExecutor).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerClosingConnection("MyConnectionString"))
                    .Append(typeof(DataStoreConnection).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerCommittingTransaction)
                    .Append(typeof(Migrator).FullName).Append(" Information ")
                    .AppendLine(Strings.MigratorLoggerApplyingMigration("000000000000001_Migration1"))
                    .Append(typeof(DataStoreConnection).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerBeginningTransaction("Serializable"))
                    .Append(typeof(SqlStatementExecutor).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerOpeningConnection("MyConnectionString"))
                    .Append(typeof(SqlStatementExecutor).FullName).AppendLine(" Verbose SomeSql")
                    .Append(typeof(SqlStatementExecutor).FullName).AppendLine(" Verbose Migration1InsertSql")
                    .Append(typeof(SqlStatementExecutor).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerClosingConnection("MyConnectionString"))
                    .Append(typeof(DataStoreConnection).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerCommittingTransaction)
                    .ToString(),
                loggerFactory.LogContent);
        }

        [Fact]
        public void ApplyMigrations_logs_downgrades()
        {
            var databaseMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                    };
            var localMigrations
                = new[]
                      {
                          new MigrationInfo("000000000000001_Migration1")
                              {
                                  DowngradeOperations = new[] { new SqlOperation("SomeSql") },
                              }
                      };

            var loggerFactory = new TestLoggerFactory();
            var migrator
                = MockMigrator(
                    databaseMigrations,
                    localMigrations,
                    /*databaseExists*/ true,
                    /*historyTableExists*/ true,
                    loggerFactory);

            migrator.ApplyMigrations(Migrator.InitialDatabase);

            Assert.Equal(
                new StringBuilder()
                    .Append(typeof(Migrator).FullName).Append(" Information ")
                    .AppendLine(Strings.MigratorLoggerRevertingMigration("000000000000001_Migration1"))
                    .Append(typeof(DataStoreConnection).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerBeginningTransaction("Serializable"))
                    .Append(typeof(SqlStatementExecutor).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerOpeningConnection("MyConnectionString"))
                    .Append(typeof(SqlStatementExecutor).FullName).AppendLine(" Verbose SomeSql")
                    .Append(typeof(SqlStatementExecutor).FullName).AppendLine(" Verbose Migration1DeleteSql")
                    .Append(typeof(SqlStatementExecutor).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerClosingConnection("MyConnectionString"))
                    .Append(typeof(DataStoreConnection).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerCommittingTransaction)
                    .Append(typeof(Migrator).FullName).Append(" Information ")
                    .AppendLine(Strings.MigratorLoggerDroppingHistoryTable)
                    .Append(typeof(DataStoreConnection).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerBeginningTransaction("Serializable"))
                    .Append(typeof(SqlStatementExecutor).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerOpeningConnection("MyConnectionString"))
                    .Append(typeof(SqlStatementExecutor).FullName).AppendLine(" Verbose Drop__MigrationHistorySql")
                    .Append(typeof(SqlStatementExecutor).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerClosingConnection("MyConnectionString"))
                    .Append(typeof(DataStoreConnection).FullName).Append(" Verbose ")
                    .AppendLine(RelationalStrings.RelationalLoggerCommittingTransaction)
                    .ToString(),
                loggerFactory.LogContent);
        }

        #region Fixture

        private static Migrator MockMigrator(
            IReadOnlyList<MigrationInfo> databaseMigrations,
            IReadOnlyList<MigrationInfo> localMigrations,
            bool databaseExists = true,
            bool historyRepositoryExists = true,
            ILoggerFactory loggerFactory = null)
        {
            Mock<RelationalDataStoreCreator> dbCreatorMock;
            Mock<DbConnection> dbConnectionMock;
            Mock<DbTransaction> dbTransactionMock;
            Mock<SqlStatementExecutor> sqlStatementExecutorMock;

            return MockMigrator(
                databaseMigrations,
                localMigrations,
                databaseExists,
                historyRepositoryExists,
                out dbCreatorMock,
                out dbConnectionMock,
                out dbTransactionMock,
                out sqlStatementExecutorMock,
                loggerFactory);
        }

        private static Migrator MockMigrator(
            IReadOnlyList<MigrationInfo> databaseMigrations,
            IReadOnlyList<MigrationInfo> localMigrations,
            bool databaseExists,
            bool historyRepositoryExists,
            out Mock<RelationalDataStoreCreator> dbCreatorMock,
            out Mock<DbConnection> dbConnectionMock,
            out Mock<DbTransaction> dbTransactionMock,
            out Mock<SqlStatementExecutor> sqlStatementExecutorMock,
            ILoggerFactory loggerFactory = null)
        {
            if (loggerFactory == null)
            {
                loggerFactory = new LoggerFactory();
            }

            dbCreatorMock = new Mock<RelationalDataStoreCreator>();
            dbConnectionMock = new Mock<DbConnection>();
            dbTransactionMock = new Mock<DbTransaction>();
            sqlStatementExecutorMock = new Mock<SqlStatementExecutor>(loggerFactory) { CallBase = true };

            dbCreatorMock.Setup(m => m.Exists()).Returns(databaseExists);
            dbConnectionMock.SetupGet(m => m.Database).Returns("MyDatabase");
            dbConnectionMock.SetupGet(m => m.ConnectionString).Returns("MyConnectionString");
            dbConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", ItExpr.IsAny<IsolationLevel>()).Returns(dbTransactionMock.Object);
            dbTransactionMock.Protected().SetupGet<DbConnection>("DbConnection").Returns(dbConnectionMock.Object);
            sqlStatementExecutorMock.Protected()
                .Setup<DbCommand>("CreateCommand", ItExpr.IsAny<DbConnection>(), ItExpr.IsAny<DbTransaction>(), ItExpr.IsAny<SqlStatement>())
                .Returns(new Mock<DbCommand>().Object);

            var contextConfiguration = CreateFakeContextConfiguration(dbCreatorMock.Object, dbConnectionMock.Object, loggerFactory);

            return
                new Mock<Migrator>(
                    MockHistoryRepository(contextConfiguration, databaseMigrations, historyRepositoryExists).Object,
                    MockMigrationAssembly(contextConfiguration, localMigrations).Object,
                    new TestModelDiffer(),
                    MockMigrationOperationSqlGeneratorFactory().Object,
                    new Mock<SqlGenerator>().Object,
                    sqlStatementExecutorMock.Object,
                    dbCreatorMock.Object,
                    contextConfiguration.ScopedServiceProvider.GetRequiredService<FakeRelationalConnection>(),
                    loggerFactory)
                {
                    CallBase = true
                }
                    .Object;
        }

        private static DbContextConfiguration CreateFakeContextConfiguration(RelationalDataStoreCreator dbCreator,
            DbConnection dbConnection, ILoggerFactory loggerFactory)
        {
            var services = new ServiceCollection()
                .AddEntityFramework()
                .AddMigrations()
                .ServiceCollection
                .AddScoped<DataStoreSource, FakeDataStoreSource>()
                .AddScoped<DataStoreSelector>()
                .AddScoped<FakeRelationalDataStoreServices>()
                .AddScoped<FakeDatabase>()
                .AddScoped<FakeRelationalOptionsExtension>()
                .AddScoped<FakeRelationalConnection>()
                .AddScoped<FakeMigrator>()
                .AddInstance(dbCreator)
                .AddInstance(loggerFactory);

            var serviceProvider = services.BuildServiceProvider();

            var contextOptions = new DbContextOptions();

            ((IDbContextOptions)contextOptions)
                .AddOrUpdateExtension<FakeRelationalOptionsExtension>(
                    x => { x.Connection = dbConnection; });

            var context = new DbContext(serviceProvider, contextOptions);

            return context.Configuration;
        }

        private static Mock<HistoryRepository> MockHistoryRepository(
            DbContextConfiguration contextConfiguration, IReadOnlyList<MigrationInfo> migrations,
            bool historyRepositoryExists = true)
        {
            var mock = new Mock<HistoryRepository>(
                        contextConfiguration.ScopedServiceProvider,
                        new LazyRef<IDbContextOptions>(new DbContextOptions()),
                        new LazyRef<DbContext>(() => null))
            { CallBase = true };

            if (historyRepositoryExists)
            {
                mock.Protected().Setup<IReadOnlyList<HistoryRow>>("GetRows")
                    .Returns(migrations.Select(m => new HistoryRow { MigrationId = m.MigrationId }).ToArray());
            }
            else
            {
                var dbException = new Mock<DbException>();

                mock.Protected().Setup<IReadOnlyList<HistoryRow>>("GetRows").Throws(dbException.Object);
            }

            mock.Setup(hr => hr.GenerateInsertMigrationSql(It.IsAny<IMigrationMetadata>(), It.IsAny<SqlGenerator>()))
                .Returns<IMigrationMetadata, SqlGenerator>((m, sg) => new[] { new SqlStatement(m.GetMigrationName() + "InsertSql") });

            mock.Setup(hr => hr.GenerateDeleteMigrationSql(It.IsAny<IMigrationMetadata>(), It.IsAny<SqlGenerator>()))
                .Returns<IMigrationMetadata, SqlGenerator>((m, sg) => new[] { new SqlStatement(m.GetMigrationName() + "DeleteSql") });

            return mock;
        }

        private static Mock<MigrationAssembly> MockMigrationAssembly(
            DbContextConfiguration contextConfiguration, IReadOnlyList<MigrationInfo> migrations)
        {
            var mock = new Mock<MigrationAssembly>(contextConfiguration);

            mock.SetupGet(ma => ma.Migrations).Returns(migrations.Select(m => new FakeMigration(m)).ToArray());

            return mock;
        }

        private static Mock<IMigrationOperationSqlGeneratorFactory> MockMigrationOperationSqlGeneratorFactory()
        {
            var mock = new Mock<IMigrationOperationSqlGeneratorFactory>();

            mock.Setup(mosgf => mosgf.Create())
                .Returns(MockMigrationOperationSqlGenerator().Object);

            mock.Setup(mosgf => mosgf.Create(It.IsAny<DatabaseModel>()))
                .Returns(MockMigrationOperationSqlGenerator().Object);

            return mock;
        }

        private static Mock<MigrationOperationSqlGenerator> MockMigrationOperationSqlGenerator()
        {
            var mock = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());

            mock.Setup(mosg => mosg.Generate(It.IsAny<IReadOnlyList<MigrationOperation>>()))
                .Returns<IReadOnlyList<MigrationOperation>>(
                    operations => FakeSqlGenerator.Instance.Generate(operations));

            return mock;
        }

        private class FakeDataStoreSource : DataStoreSource<FakeRelationalDataStoreServices, FakeRelationalOptionsExtension>
        {
            public FakeDataStoreSource(DbContextConfiguration configuration)
                : base(configuration)
            {
            }

            public override string Name
            {
                get { return GetType().Name; }
            }
        }

        private class FakeRelationalDataStoreServices : DataStoreServices
        {
            private readonly RelationalDataStoreCreator _creator;
            private readonly FakeRelationalConnection _connection;
            private readonly FakeDatabase _database;

            public FakeRelationalDataStoreServices(RelationalDataStoreCreator creator,
                FakeRelationalConnection connection, FakeDatabase database)
            {
                _creator = creator;
                _connection = connection;
                _database = database;
            }

            public override DataStore Store
            {
                get { throw new NotImplementedException(); }
            }

            public override DataStoreCreator Creator
            {
                get { return _creator; }
            }

            public override DataStoreConnection Connection
            {
                get { return _connection; }
            }

            public override ValueGeneratorCache ValueGeneratorCache
            {
                get { throw new NotImplementedException(); }
            }

            public override Database Database
            {
                get { return _database; }
            }

            public override IModelBuilderFactory ModelBuilderFactory
            {
                get { throw new NotImplementedException(); }
            }
        }

        private class FakeDatabase : MigrationsEnabledDatabase
        {
            public FakeDatabase(
                LazyRef<IModel> model,
                RelationalDataStoreCreator dataStoreCreator,
                FakeRelationalConnection connection,
                FakeMigrator migrator,
                ILoggerFactory loggerFactory)
                : base(model, dataStoreCreator, connection, migrator, loggerFactory)
            {
            }
        }

        private class FakeRelationalOptionsExtension : RelationalOptionsExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
                throw new NotImplementedException();
            }
        }

        private class FakeRelationalConnection : RelationalConnection
        {
            public FakeRelationalConnection(LazyRef<IDbContextOptions> options, ILoggerFactory loggerFactory)
                : base(options, loggerFactory)
            {
            }

            protected override DbConnection CreateDbConnection()
            {
                throw new NotImplementedException();
            }
        }

        private class FakeMigrator : Migrator
        {
        }

        private class FakeSqlGenerator : MigrationOperationVisitor<IndentedStringBuilder>
        {
            public static readonly FakeSqlGenerator Instance = new FakeSqlGenerator();

            public virtual IEnumerable<SqlStatement> Generate(IEnumerable<MigrationOperation> operations)
            {
                return operations.Select(
                    o =>
                        {
                            var sqlOperation = o as SqlOperation;
                            var builder = new IndentedStringBuilder();

                            o.Accept(this, builder);

                            return
                                new SqlStatement(builder.ToString())
                                {
                                    SuppressTransaction = sqlOperation != null && sqlOperation.SuppressTransaction
                                };
                        });
            }

            public override void Visit(CreateTableOperation operation, IndentedStringBuilder builder)
            {
                builder.Append("Create").Append(operation.TableName).Append("Sql");
            }

            public override void Visit(DropTableOperation operation, IndentedStringBuilder builder)
            {
                builder.Append("Drop").Append(operation.TableName).Append("Sql");
            }

            public override void Visit(SqlOperation operation, IndentedStringBuilder builder)
            {
                builder.Append(operation.Sql);
            }

            protected override void VisitDefault(MigrationOperation operation, IndentedStringBuilder builder)
            {
                builder.Append(operation.GetType().Name).Append("Sql");
            }
        }

        private class FakeMigration : Migration, IMigrationMetadata
        {
            private readonly MigrationInfo _migration;

            public FakeMigration(MigrationInfo migration)
            {
                _migration = migration;
            }

            public override void Up(MigrationBuilder migrationBuilder)
            {
                foreach (var operation in _migration.UpgradeOperations)
                {
                    migrationBuilder.AddOperation(operation);
                }
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
                foreach (var operation in _migration.DowngradeOperations)
                {
                    migrationBuilder.AddOperation(operation);
                }
            }

            public string MigrationId
            {
                get { return _migration.MigrationId; }
            }

            public string ProductVersion
            {
                get { return _migration.ProductVersion; }
            }

            public IModel TargetModel
            {
                get { return _migration.TargetModel; }
            }
        }

        private class TestLoggerFactory : ILoggerFactory
        {
            private readonly StringBuilder _builder = new StringBuilder();

            public string LogContent
            {
                get { return _builder.ToString(); }
            }

            public ILogger Create(string name)
            {
                return new TestLogger(name, _builder);
            }

            public void AddProvider(ILoggerProvider provider)
            {
                throw new NotImplementedException();
            }
        }

        private class TestLogger : ILogger
        {
            private readonly string _name;
            private readonly StringBuilder _builder;

            public TestLogger(string name, StringBuilder builder)
            {
                _name = name;
                _builder = builder;
            }

            public void Write(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                _builder
                    .Append(_name)
                    .Append(" ")
                    .Append(logLevel.ToString("G"))
                    .Append(" ")
                    .AppendLine(formatter(state, exception));
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope(object state)
            {
                throw new NotImplementedException();
            }
        }

        private class TestDatabaseBuilder : DatabaseBuilder
        {
            public TestDatabaseBuilder()
                : base(new RelationalTypeMapper())
            {
            }

            protected override Sequence BuildSequence(IProperty property)
            {
                return null;
            }
        }

        private class TestModelDiffer : ModelDiffer
        {
            public TestModelDiffer()
                : base(new TestDatabaseBuilder())
            {
            }

            protected override string GetSequenceName(Column column)
            {
                return null;
            }
        }

        #endregion
    }
}
