// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Moq;
using Moq.Protected;
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
        public void ScriptMigrations_with_single_initial_migration()
        {
            var migrator = MockMigrator(
                new IMigrationMetadata[0],
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
                new IMigrationMetadata[0],
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new CreateTableOperation(new Table("MyTable1"))
                                        }
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
                        new MigrationMetadata("000000000000001_Migration1")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation(new Table("MyTable2"))
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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

            var sqlStatements = migrator.ScriptMigrations("Migration3");

            Assert.Equal(0, sqlStatements.Count);
        }

        [Fact]
        public void ScriptMigrations_with_target_database_migration()
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
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string)))
                                        }
                            },
                        new MigrationMetadata("000000000000005_Migration5")
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2"),
                        new MigrationMetadata("000000000000004_Migration4")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2"),
                        new MigrationMetadata("000000000000004_Migration4")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationMetadata("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationMetadata("000000000000004_Migration4")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    };
            var localMigrations
                = new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropTableOperation("MyTable1")
                                        }
                            },
                        new MigrationMetadata("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
            var databaseMigrations = new MigrationMetadata[0];
            var localMigrations
                = new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations()).Message);
            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations("Migration1")).Message);

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
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations()).Message);
            Assert.Equal(
                Strings.FormatLocalMigrationNotFound("000000000000002_Migration2"),
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations("Migration1")).Message);
        }

        [Fact]
        public void ScriptMigrations_throws_if_target_migration_not_found()
        {
            var migrator = MockMigrator(
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1")
                    },
                new[]
                    {
                        new MigrationMetadata("000000000000001_Migration1"),
                        new MigrationMetadata("000000000000002_Migration2")
                    }
                );

            Assert.Equal(
                Strings.FormatTargetMigrationNotFound("Foo"),
                Assert.Throws<InvalidOperationException>(() => migrator.ScriptMigrations("Foo")).Message);
        }

        [Fact]
        public void ApplyMigrations_creates_database_and_executes_upgrade_statements()
        {
            var databaseMigrations = new IMigrationMetadata[0];
            var localMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1")
                              {
                                  UpgradeOperations = new[] { new SqlOperation("SomeSql") }
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
            var databaseMigrations = new IMigrationMetadata[0];
            var localMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1")
                              {
                                  UpgradeOperations = new[] { new SqlOperation("SomeSql") }
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
            var databaseMigrations = new IMigrationMetadata[0];
            var localMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1")
                              {
                                  UpgradeOperations = new[] { new SqlOperation("SomeSql") }
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
                          new MigrationMetadata("000000000000001_Migration1"),
                          new MigrationMetadata("000000000000001_Migration2")
                      };
            var localMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1"),
                          new MigrationMetadata("000000000000001_Migration2")
                              {
                                  DowngradeOperations = new[] { new SqlOperation("SomeSql") },
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
                          new MigrationMetadata("000000000000001_Migration1"),
                      };
            var localMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1")
                              {
                                  DowngradeOperations = new[] { new SqlOperation("SomeSql") },
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
                    new MigrationMetadata[0],
                    new MigrationMetadata[0],
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
            var databaseMigrations = new IMigrationMetadata[0];
            var localMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1")
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
                                            }
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
            var databaseMigrations = new IMigrationMetadata[0];
            var localMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1")
                              {
                                  UpgradeOperations = new[] { new SqlOperation("SomeSql") }
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
    .Append("RelationalDatabase Information ")
    .AppendLine(GetString("FormatRelationalLoggerCreatingDatabase", "MyDatabase"))
    .Append("MigratorProxy Information ")
    .AppendLine(Strings.MigratorLoggerCreatingHistoryTable)
    .Append("FakeRelationalConnection Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerBeginningTransaction", "Serializable"))
    .Append("SqlStatementExecutorProxy Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerOpeningConnection", "MyConnectionString"))
    .AppendLine("SqlStatementExecutorProxy Verbose Create__MigrationHistorySql")
    .Append("SqlStatementExecutorProxy Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerClosingConnection", "MyConnectionString"))
    .Append("FakeRelationalConnection Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerCommittingTransaction"))
    .Append("MigratorProxy Information ")
    .AppendLine(Strings.FormatMigratorLoggerApplyingMigration("000000000000001_Migration1"))
    .Append("FakeRelationalConnection Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerBeginningTransaction", "Serializable"))
    .Append("SqlStatementExecutorProxy Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerOpeningConnection", "MyConnectionString"))
    .AppendLine("SqlStatementExecutorProxy Verbose SomeSql")
    .AppendLine("SqlStatementExecutorProxy Verbose Migration1InsertSql")
    .Append("SqlStatementExecutorProxy Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerClosingConnection", "MyConnectionString"))
    .Append("FakeRelationalConnection Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerCommittingTransaction"))
    .ToString(),
                loggerFactory.LogContent);
        }

        [Fact]
        public void ApplyMigrations_logs_downgrades()
        {
            var databaseMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1"),
                      };
            var localMigrations
                = new[]
                      {
                          new MigrationMetadata("000000000000001_Migration1")
                              {
                                  DowngradeOperations = new[] { new SqlOperation("SomeSql") },
                                  TargetModel = new Metadata.Model()
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
    .Append("MigratorProxy Information ")
    .AppendLine(Strings.FormatMigratorLoggerRevertingMigration("000000000000001_Migration1"))
    .Append("FakeRelationalConnection Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerBeginningTransaction", "Serializable"))
    .Append("SqlStatementExecutorProxy Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerOpeningConnection", "MyConnectionString"))
    .AppendLine("SqlStatementExecutorProxy Verbose SomeSql")
    .AppendLine("SqlStatementExecutorProxy Verbose Migration1DeleteSql")
    .Append("SqlStatementExecutorProxy Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerClosingConnection", "MyConnectionString"))
    .Append("FakeRelationalConnection Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerCommittingTransaction"))
    .Append("MigratorProxy Information ")
    .AppendLine(Strings.MigratorLoggerDroppingHistoryTable)
    .Append("FakeRelationalConnection Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerBeginningTransaction", "Serializable"))
    .Append("SqlStatementExecutorProxy Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerOpeningConnection", "MyConnectionString"))
    .AppendLine("SqlStatementExecutorProxy Verbose Drop__MigrationHistorySql")
    .Append("SqlStatementExecutorProxy Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerClosingConnection", "MyConnectionString"))
    .Append("FakeRelationalConnection Verbose ")
    .AppendLine(GetString("FormatRelationalLoggerCommittingTransaction"))
    .ToString(),
                loggerFactory.LogContent);
        }

        private static string GetString(string stringName, params object[] parameters)
        {
            var strings = typeof(SqlStatement).GetTypeInfo().Assembly.GetType(typeof(SqlStatement).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, parameters);
        }

        #region Fixture

        private static Migrator MockMigrator(
            IReadOnlyList<IMigrationMetadata> databaseMigrations,
            IReadOnlyList<IMigrationMetadata> localMigrations,
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
            IReadOnlyList<IMigrationMetadata> databaseMigrations,
            IReadOnlyList<IMigrationMetadata> localMigrations,            
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
                loggerFactory = new NullLoggerFactory();
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
                    contextConfiguration,
                    MockHistoryRepository(contextConfiguration, databaseMigrations, historyRepositoryExists).Object,
                    MockMigrationAssembly(contextConfiguration, localMigrations).Object,
                    new ModelDiffer(new DatabaseBuilder()),
                    MockMigrationOperationSqlGeneratorFactory().Object,
                    new Mock<SqlGenerator>().Object,
                    sqlStatementExecutorMock.Object)
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
                .AddRelational()
                .ServiceCollection
                .AddScoped<DataStoreSource, FakeDataStoreSource>()
                .AddScoped<DataStoreSelector>()
                .AddScoped<FakeRelationalDataStoreServices>()
                .AddScoped<FakeRelationalOptionsExtension>()
                .AddScoped<FakeRelationalConnection>()
                .AddInstance(dbCreator)
                .AddInstance(loggerFactory);

            var serviceProvider = services.BuildServiceProvider();

            var contextOptions = new DbContextOptions();

            ((IDbContextOptionsExtensions)contextOptions)
                .AddOrUpdateExtension<FakeRelationalOptionsExtension>(
                    x => { x.Connection = dbConnection; });

            var context = new DbContext(serviceProvider, contextOptions);

            return context.Configuration;
        }

        private static Mock<HistoryRepository> MockHistoryRepository(
            DbContextConfiguration contextConfiguration, IReadOnlyList<IMigrationMetadata> migrations,
            bool historyRepositoryExists = true)
        {
            var mock = new Mock<HistoryRepository>(contextConfiguration) { CallBase = true };

            if (historyRepositoryExists)
            {
                mock.SetupGet(hr => hr.Migrations).Returns(migrations.ToArray());
            }
            else
            {
                var dbException = new Mock<DbException>();

                mock.SetupGet(hr => hr.Migrations).Throws(dbException.Object);
            }

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
            private readonly RelationalDatabase _database;

            public FakeRelationalDataStoreServices(RelationalDataStoreCreator creator,
                FakeRelationalConnection connection, RelationalDatabase database)
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

        private class FakeRelationalOptionsExtension : RelationalOptionsExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
                throw new NotImplementedException();
            }
        }

        private class FakeRelationalConnection : RelationalConnection
        {
            public FakeRelationalConnection(
                DbContextConfiguration configuration, ConnectionStringResolver connectionStringResolver)
                : base(configuration, connectionStringResolver)
            {
            }

            protected override DbConnection CreateDbConnection()
            {
                throw new NotImplementedException();
            }
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
                builder.Append("Create").Append(operation.Table.Name).Append("Sql");
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

            public void Write(TraceType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                _builder
                    .Append(_name)
                    .Append(" ")
                    .Append(eventType.ToString("G"))
                    .Append(" ")
                    .AppendLine(formatter(state, exception));
            }

            public bool IsEnabled(TraceType eventType)
            {
                return true;
            }

            public IDisposable BeginScope(object state)
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
