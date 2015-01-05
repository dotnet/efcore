// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Relational.Migrations.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;
using RelationalStrings = Microsoft.Data.Entity.Relational.Strings;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.Infrastructure
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

            var migrator = CreateMigrator(databaseMigrations, localMigrations);

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

            var migrator = CreateMigrator(new MigrationInfo[0], localMigrations);

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

            var migrator = CreateMigrator(databaseMigrations, localMigrations);

            var migrations = migrator.GetPendingMigrations();

            Assert.Equal(2, migrations.Count);

            Assert.Equal(localMigrations[1].MigrationId, migrations[0].GetMigrationId());
            Assert.Equal(localMigrations[3].MigrationId, migrations[1].GetMigrationId());
        }

        [Fact]
        public void ScriptMigrations_with_single_initial_migration()
        {
            var migrator = CreateMigrator(
                new MigrationInfo[0],
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new CreateTableOperation("MyTable1")
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
            var migrator = CreateMigrator(
                new MigrationInfo[0],
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new CreateTableOperation("MyTable1")
                                        }
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation("MyTable2")
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
            var migrator = CreateMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation("MyTable2")
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
            var migrator = CreateMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation("MyTable2")
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
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
            var migrator = CreateMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation("MyTable2")
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
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
            var migrator = CreateMigrator(
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    },
                new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new AddColumnOperation("MyTable1", new Column("Foo", typeof(string))),
                                            new CreateTableOperation("MyTable2")
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
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
            var migrator = CreateMigrator(
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
            var migrator = CreateMigrator(
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
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
            var migrator = CreateMigrator(
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
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
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
            var migrator = CreateMigrator(
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
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000002_Migration2")
                            {
                                TargetModel = new Entity.Metadata.Model()
                            },
                        new MigrationInfo("000000000000003_Migration3")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropColumnOperation("MyTable1", "Foo"),
                                            new DropTableOperation("MyTable2")
                                        }
                            },
                        new MigrationInfo("000000000000004_Migration4")
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
                        new MigrationInfo("000000000000001_Migration1"),
                        new MigrationInfo("000000000000002_Migration2")
                    };
            var localMigrations
                = new[]
                    {
                        new MigrationInfo("000000000000001_Migration1")
                            {
                                TargetModel = new Entity.Metadata.Model(),
                                DowngradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new DropTableOperation("MyTable1")
                                        }
                            },
                        new MigrationInfo("000000000000002_Migration2")
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

            var migrator = CreateMigrator(databaseMigrations, localMigrations);

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
                                TargetModel = new Entity.Metadata.Model(),
                                UpgradeOperations
                                    = new MigrationOperation[]
                                        {
                                            new CreateTableOperation("MyTable1")
                                        }
                            }
                    };

            var migrator = CreateMigrator(databaseMigrations, localMigrations, historyRepositoryExists: false);

            var sqlStatements = migrator.ScriptMigrations();

            Assert.Equal(3, sqlStatements.Count);
            Assert.Equal("Create__MigrationHistorySql", sqlStatements[0].Sql);
            Assert.Equal("CreateMyTable1Sql", sqlStatements[1].Sql);
            Assert.Equal("Migration1InsertSql", sqlStatements[2].Sql);
        }

        [Fact]
        public void ScriptMigrations_throws_if_local_migrations_do_not_include_all_database_migrations()
        {
            var migrator = CreateMigrator(
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

            migrator = CreateMigrator(
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
            var migrator = CreateMigrator(
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
                                TargetModel = new Entity.Metadata.Model()
                            }
                    };

            var contextServices = CreateContextServices(databaseMigrations, localMigrations, historyRepositoryExists: false);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var creator = contextServices.GetRequiredService<RecordingDataStoreCreator>();
            var executor = (RecordingSqlStatementExecutor)contextServices.GetRequiredService<SqlStatementExecutor>();

            creator.ExistsState = false;

            migrator.ApplyMigrations();

            Assert.Equal(
                new[]
                    {
                        "Create__MigrationHistorySql",
                        "SomeSql",
                        "Migration1InsertSql",
                    },
                executor.NonQueries.SelectMany(t => t.Item3));

            Assert.Equal(
                new[]
                    {
                        true,
                        true
                    },
                executor.NonQueries.Select(t => ((FakeDbTransaction)t.Item2).Committed));

            Assert.True(creator.Created);
            Assert.True(creator.ExistsState);
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
                                  TargetModel = new Entity.Metadata.Model()
                              }
                      };

            var contextServices = CreateContextServices(databaseMigrations, localMigrations, historyRepositoryExists: false);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var creator = contextServices.GetRequiredService<RecordingDataStoreCreator>();
            var executor = (RecordingSqlStatementExecutor)contextServices.GetRequiredService<SqlStatementExecutor>();

            migrator.ApplyMigrations();

            Assert.Equal(
                new[]
                    {
                        "Create__MigrationHistorySql",
                        "SomeSql",
                        "Migration1InsertSql",
                    },
                executor.NonQueries.SelectMany(t => t.Item3));

            Assert.Equal(
                new[]
                    {
                        true,
                        true
                    },
                executor.NonQueries.Select(t => ((FakeDbTransaction)t.Item2).Committed));

            Assert.False(creator.Created);
            Assert.True(creator.ExistsState);
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
                                  TargetModel = new Entity.Metadata.Model()
                              }
                      };

            var contextServices = CreateContextServices(databaseMigrations, localMigrations);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var creator = contextServices.GetRequiredService<RecordingDataStoreCreator>();
            var executor = (RecordingSqlStatementExecutor)contextServices.GetRequiredService<SqlStatementExecutor>();

            migrator.ApplyMigrations();

            Assert.Equal(
                new[]
                    {
                        "SomeSql",
                        "Migration1InsertSql",
                    },
                executor.NonQueries.SelectMany(t => t.Item3));

            Assert.Equal(
                new[]
                    {
                        true
                    },
                executor.NonQueries.Select(t => ((FakeDbTransaction)t.Item2).Committed));

            Assert.False(creator.Created);
            Assert.True(creator.ExistsState);
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
                                  TargetModel = new Entity.Metadata.Model()
                              },
                          new MigrationInfo("000000000000001_Migration2")
                              {
                                  DowngradeOperations = new[] { new SqlOperation("SomeSql") },
                              }
                      };

            var contextServices = CreateContextServices(databaseMigrations, localMigrations);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var creator = contextServices.GetRequiredService<RecordingDataStoreCreator>();
            var executor = (RecordingSqlStatementExecutor)contextServices.GetRequiredService<SqlStatementExecutor>();

            migrator.ApplyMigrations("Migration1");

            Assert.Equal(
                new[]
                    {
                        "SomeSql",
                        "Migration2DeleteSql",
                    },
                executor.NonQueries.SelectMany(t => t.Item3));

            Assert.Equal(
                new[]
                    {
                        true
                    },
                executor.NonQueries.Select(t => ((FakeDbTransaction)t.Item2).Committed));

            Assert.False(creator.Created);
            Assert.True(creator.ExistsState);
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

            var contextServices = CreateContextServices(databaseMigrations, localMigrations);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var creator = contextServices.GetRequiredService<RecordingDataStoreCreator>();
            var executor = (RecordingSqlStatementExecutor)contextServices.GetRequiredService<SqlStatementExecutor>();

            migrator.ApplyMigrations(Migrator.InitialDatabase);

            Assert.Equal(
                new[]
                    {
                        "SomeSql",
                        "Migration1DeleteSql",
                        "Drop__MigrationHistorySql"
                    },
                executor.NonQueries.SelectMany(t => t.Item3));

            Assert.Equal(
                new[]
                    {
                        true,
                        true
                    },
                executor.NonQueries.Select(t => ((FakeDbTransaction)t.Item2).Committed));

            Assert.False(creator.Created);
            Assert.True(creator.ExistsState);
        }

        [Fact]
        public void ApplyMigrations_creates_database_if_no_migrations()
        {
            var contextServices = CreateContextServices(new MigrationInfo[0], new MigrationInfo[0], historyRepositoryExists: false);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var creator = contextServices.GetRequiredService<RecordingDataStoreCreator>();
            var executor = (RecordingSqlStatementExecutor)contextServices.GetRequiredService<SqlStatementExecutor>();

            creator.ExistsState = false;

            migrator.ApplyMigrations();

            Assert.Empty(executor.NonQueries);
            Assert.True(creator.Created);
            Assert.True(creator.ExistsState);

            creator.ExistsState = false;
            creator.Created = false;

            migrator.ApplyMigrations(Migrator.InitialDatabase);

            Assert.Empty(executor.NonQueries);
            Assert.True(creator.Created);
            Assert.True(creator.ExistsState);
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
                                  TargetModel = new Entity.Metadata.Model()
                              }
                      };

            var contextServices = CreateContextServices(databaseMigrations, localMigrations);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var creator = contextServices.GetRequiredService<RecordingDataStoreCreator>();
            var executor = (RecordingSqlStatementExecutor)contextServices.GetRequiredService<SqlStatementExecutor>();

            migrator.ApplyMigrations();

            Assert.Equal(
                new[]
                    {
                        new [] {  "1" },
                        new [] {  "2", "3" },
                        new [] {  "4" },
                        new [] {  "5", "6" },
                        new [] {  "7" },
                        new [] {  "Migration1InsertSql" }
                    },
                executor.NonQueries.Select(t => t.Item3).ToArray());

            var transactions = executor.NonQueries.Select(t => ((FakeDbTransaction)t.Item2)).ToList();

            Assert.Null(transactions[0]);
            Assert.True(transactions[1].Committed);
            Assert.Null(transactions[2]);
            Assert.True(transactions[3].Committed);
            Assert.Null(transactions[4]);
            Assert.True(transactions[5].Committed);

            Assert.False(creator.Created);
            Assert.True(creator.ExistsState);
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
                                  TargetModel = new Entity.Metadata.Model()
                              }
                      };

            var contextServices = CreateContextServices(databaseMigrations, localMigrations, historyRepositoryExists: false);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var creator = contextServices.GetRequiredService<RecordingDataStoreCreator>();
            var loggerFactory = (RecordingLoggerFactory)contextServices.GetRequiredService<ILoggerFactory>();

            creator.ExistsState = false;

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

            var contextServices = CreateContextServices(databaseMigrations, localMigrations);
            var migrator = contextServices.GetRequiredService<TestMigrator>();
            var loggerFactory = (RecordingLoggerFactory)contextServices.GetRequiredService<ILoggerFactory>();

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

        private static TestMigrator CreateMigrator(
            IReadOnlyList<MigrationInfo> databaseMigrations,
            IReadOnlyList<MigrationInfo> localMigrations,
            bool historyRepositoryExists = true)
        {
            return CreateContextServices(databaseMigrations, localMigrations, historyRepositoryExists).GetRequiredService<TestMigrator>();
        }

        private static IServiceProvider CreateContextServices(IReadOnlyList<MigrationInfo> databaseMigrations, IReadOnlyList<MigrationInfo> localMigrations, bool historyRepositoryExists = true)
        {
            var customServices = new ServiceCollection()
                .AddInstance<HistoryRepository>(new FakeHistoryRepository(databaseMigrations, historyRepositoryExists))
                .AddInstance<MigrationAssembly>(new FakeMigrationAssembly(localMigrations));

            var contextServices = TestHelpers.CreateContextServices(customServices);
            return contextServices;
        }

        private class FakeHistoryRepository : HistoryRepository
        {
            private readonly IReadOnlyList<MigrationInfo> _migrations;
            private readonly bool _historyRepositoryExists;

            public FakeHistoryRepository(IReadOnlyList<MigrationInfo> migrations, bool historyRepositoryExists = true)
            {
                _migrations = migrations;
                _historyRepositoryExists = historyRepositoryExists;
            }

            protected override IReadOnlyList<HistoryRow> GetRows()
            {
                if (_historyRepositoryExists)
                {
                    return _migrations.Select(m => new HistoryRow { MigrationId = m.MigrationId }).ToArray();
                }
                throw new FakeDbException();
            }

            public override IReadOnlyList<SqlBatch> GenerateInsertMigrationSql(IMigrationMetadata migration, SqlGenerator sqlGenerator)
            {
                return new[] { new SqlBatch(migration.GetMigrationName() + "InsertSql") };
            }

            public override IReadOnlyList<SqlBatch> GenerateDeleteMigrationSql(IMigrationMetadata migration, SqlGenerator sqlGenerator)
            {
                return new[] { new SqlBatch(migration.GetMigrationName() + "DeleteSql") };
            }
        }

        private class FakeMigrationAssembly : MigrationAssembly
        {
            private readonly IReadOnlyList<MigrationInfo> _migrations;

            public FakeMigrationAssembly(IReadOnlyList<MigrationInfo> migrations)
            {
                _migrations = migrations;
            }

            public override IReadOnlyList<Migration> Migrations
            {
                get
                {
                    return _migrations.Select(m => new FakeMigration(m)).ToArray();
                }
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
    }
}
