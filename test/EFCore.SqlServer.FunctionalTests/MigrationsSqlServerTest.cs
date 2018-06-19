// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
    public class MigrationsSqlServerTest : MigrationsTestBase<MigrationsSqlServerFixture>
    {
        public MigrationsSqlServerTest(MigrationsSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public override void Can_generate_migration_from_initial_database_to_initial()
        {
            base.Can_generate_migration_from_initial_database_to_initial();

            Assert.Equal(
                @"IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_no_migration_script()
        {
            base.Can_generate_no_migration_script();

            Assert.Equal(
                @"IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_up_scripts()
        {
            base.Can_generate_up_scripts();

            Assert.Equal(
                @"IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [Table1] (
    [Id] int NOT NULL,
    CONSTRAINT [PK_Table1] PRIMARY KEY ([Id])
);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000001_Migration1', N'7.0.0-test');

GO

EXEC sp_rename N'[Table1]', N'Table2';

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

GO

CREATE DATABASE TransactionSuppressed;

GO

DROP DATABASE TransactionSuppressed;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000003_Migration3', N'7.0.0-test');

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_one_up_script()
        {
            base.Can_generate_one_up_script();

            Assert.Equal(
                @"EXEC sp_rename N'[Table1]', N'Table2';

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_up_script_using_names()
        {
            base.Can_generate_up_script_using_names();

            Assert.Equal(
                @"EXEC sp_rename N'[Table1]', N'Table2';

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_idempotent_up_scripts()
        {
            base.Can_generate_idempotent_up_scripts();

            Assert.Equal(
                @"IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000001_Migration1')
BEGIN
    CREATE TABLE [Table1] (
        [Id] int NOT NULL,
        CONSTRAINT [PK_Table1] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000001_Migration1')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'00000000000001_Migration1', N'7.0.0-test');
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000002_Migration2')
BEGIN
    EXEC sp_rename N'[Table1]', N'Table2';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000002_Migration2')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'00000000000002_Migration2', N'7.0.0-test');
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000003_Migration3')
BEGIN
    CREATE DATABASE TransactionSuppressed;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000003_Migration3')
BEGIN
    DROP DATABASE TransactionSuppressed;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000003_Migration3')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'00000000000003_Migration3', N'7.0.0-test');
END;

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_down_scripts()
        {
            base.Can_generate_down_scripts();

            Assert.Equal(
                @"EXEC sp_rename N'[Table2]', N'Table1';

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000002_Migration2';

GO

DROP TABLE [Table1];

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000001_Migration1';

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_idempotent_down_scripts()
        {
            base.Can_generate_idempotent_down_scripts();

            Assert.Equal(
                @"IF EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000002_Migration2')
BEGIN
    EXEC sp_rename N'[Table2]', N'Table1';
END;

GO

IF EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000002_Migration2')
BEGIN
    DELETE FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2';
END;

GO

IF EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000001_Migration1')
BEGIN
    DROP TABLE [Table1];
END;

GO

IF EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'00000000000001_Migration1')
BEGIN
    DELETE FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1';
END;

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_one_down_script()
        {
            base.Can_generate_one_down_script();

            Assert.Equal(
                @"EXEC sp_rename N'[Table2]', N'Table1';

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000002_Migration2';

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_down_script_using_names()
        {
            base.Can_generate_down_script_using_names();

            Assert.Equal(
                @"EXEC sp_rename N'[Table2]', N'Table1';

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000002_Migration2';

GO

",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_get_active_provider()
        {
            base.Can_get_active_provider();

            Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", ActiveProvider);
        }

        protected override async Task AssertFirstMigrationAsync(DbConnection connection)
        {
            var sql = await GetDatabaseSchemaAsync(connection);
            Assert.Equal(
                @"
CreatedTable
    Id int NOT NULL
    ColumnWithDefaultToDrop int NULL DEFAULT ((0))
    ColumnWithDefaultToAlter int NULL DEFAULT ((1))
",
                sql,
                ignoreLineEndingDifferences: true);
        }

        protected override async Task AssertSecondMigrationAsync(DbConnection connection)
        {
            var sql = await GetDatabaseSchemaAsync(connection);
            Assert.Equal(
                @"
CreatedTable
    Id int NOT NULL
    ColumnWithDefaultToAlter int NULL
",
                sql,
                ignoreLineEndingDifferences: true);
        }

        private async Task<string> GetDatabaseSchemaAsync(DbConnection connection)
        {
            var builder = new IndentedStringBuilder();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT
                    t.name,
                    c.Name,
                    TYPE_NAME(c.user_type_id),
                    c.is_nullable,
                    d.Definition
                FROM sys.objects t
                LEFT JOIN sys.columns c ON c.object_id = t.object_id
                LEFT JOIN sys.default_constraints d ON d.parent_column_id = c.column_id AND d.parent_object_id = t.object_id
                WHERE t.type = 'U'
                ORDER BY t.name, c.column_id;";

            using (var reader = await command.ExecuteReaderAsync())
            {
                var first = true;
                string lastTable = null;
                while (await reader.ReadAsync())
                {
                    var currentTable = reader.GetString(0);
                    if (currentTable != lastTable)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            builder.DecrementIndent();
                        }

                        builder
                            .AppendLine()
                            .AppendLine(currentTable)
                            .IncrementIndent();

                        lastTable = currentTable;
                    }

                    builder
                        .Append(reader[1]) // Name
                        .Append(" ")
                        .Append(reader[2]) // Type
                        .Append(" ")
                        .Append(reader.GetBoolean(3) ? "NULL" : "NOT NULL");

                    if (!await reader.IsDBNullAsync(4))
                    {
                        builder
                            .Append(" DEFAULT ")
                            .Append(reader[4]);
                    }

                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        [ConditionalFact]
        public async Task Empty_Migration_Creates_Database()
        {
            using (var context = new BloggingContext(Fixture.TestStore.AddProviderOptions(new DbContextOptionsBuilder()).Options))
            {
                var creator = (SqlServerDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();
                creator.RetryTimeout = TimeSpan.FromMinutes(10);

                await context.Database.MigrateAsync();

                Assert.True(creator.Exists());
            }
        }

        private class BloggingContext : DbContext
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
            {
            }

            // ReSharper disable once UnusedMember.Local
            public DbSet<Blog> Blogs { get; set; }

            // ReSharper disable once ClassNeverInstantiated.Local
            public class Blog
            {
                // ReSharper disable UnusedMember.Local
                public int Id { get; set; }

                public string Name { get; set; }
                // ReSharper restore UnusedMember.Local
            }
        }

        [DbContext(typeof(BloggingContext))]
        [Migration("00000000000000_Empty")]
        public class EmptyMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
