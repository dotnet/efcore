// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Identity30.Data;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.Migrations.MigrationsInfrastructureFixtureBase;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations
{
    [SqlServerCondition(SqlServerCondition.IsNotAzureSql | SqlServerCondition.IsNotCI)]
    public class MigrationsInfrastructureSqlServerTest(
        MigrationsInfrastructureSqlServerTest.MigrationsInfrastructureSqlServerFixture fixture)
        : MigrationsInfrastructureTestBase<MigrationsInfrastructureSqlServerTest.MigrationsInfrastructureSqlServerFixture>(fixture)
    {
        public override void Can_apply_range_of_migrations()
        {
            base.Can_apply_range_of_migrations();

            var sql = @"CREATE DATABASE TransactionSuppressed;
";
            Assert.Equal(
                RelationalResources.LogNonTransactionalMigrationOperationWarning(new TestLogger<TestRelationalLoggingDefinitions>())
                    .GenerateMessage(sql, "Migration3"),
                Fixture.TestSqlLoggerFactory.Log.Single(l => l.Id == RelationalEventId.NonTransactionalMigrationOperationWarning).Message);
        }

        public override async Task Can_generate_migration_from_initial_database_to_initial()
        {
            await base.Can_generate_migration_from_initial_database_to_initial();

            Assert.Equal(
                """
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO


""",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override async Task Can_generate_no_migration_script()
        {
            await base.Can_generate_no_migration_script();

            Assert.Equal(
                """
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO


""",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override async Task Can_generate_up_and_down_scripts()
        {
            await base.Can_generate_up_and_down_scripts();

            Assert.Equal(
                """
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Table1] (
    [Id] int NOT NULL,
    [Foo] int NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Table1] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000001_Migration1', N'7.0.0-test');

EXEC sp_rename N'[Table1].[Foo]', N'Bar', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

COMMIT;
GO

CREATE DATABASE TransactionSuppressed;
GO

DROP DATABASE TransactionSuppressed;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000003_Migration3', N'7.0.0-test');
GO

CREATE PROCEDURE [dbo].[GotoReproduction]
AS
BEGIN
    DECLARE @Counter int;
    SET @Counter = 1;
    WHILE @Counter < 10
    BEGIN
        SELECT @Counter
        SET @Counter = @Counter + 1
        IF @Counter = 4 GOTO Branch_One --Jumps to the first branch.
        IF @Counter = 5 GOTO Branch_Two --This will never execute.
    END
    Branch_One:
        SELECT 'Jumping To Branch One.'
        GOTO Branch_Three; --This will prevent Branch_Two from executing.'
    Branch_Two:
        SELECT 'Jumping To Branch Two.'
    Branch_Three:
        SELECT 'Jumping To Branch Three.'
END;

GO

SELECT GetDate();
--GO
SELECT GetDate()
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000004_Migration4', N'7.0.0-test');
GO

BEGIN TRANSACTION;
INSERT INTO Table1 (Id, Bar, Description) VALUES (-1, 3, 'Value With

Empty Lines')

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000005_Migration5', N'7.0.0-test');

INSERT INTO Table1 (Id, Bar, Description) VALUES (-2, 4, 'GO
Value With

Empty Lines')

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000006_Migration6', N'7.0.0-test');

INSERT INTO Table1 (Id, Bar, Description) VALUES (-3, 5, '--Start
GO
Value With

GO

Empty Lines;
GO
')

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000007_Migration7', N'7.0.0-test');

COMMIT;
GO

BEGIN TRANSACTION;
DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000007_Migration7';

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000006_Migration6';

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000005_Migration5';

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000004_Migration4';

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000003_Migration3';

EXEC sp_rename N'[Table1].[Bar]', N'Foo', 'COLUMN';

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000002_Migration2';

DROP TABLE [Table1];

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000001_Migration1';

COMMIT;
GO


""",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override async Task Can_generate_up_and_down_scripts_noTransactions()
        {
            await base.Can_generate_up_and_down_scripts_noTransactions();

            Assert.Equal(
                """
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
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
    [Foo] int NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Table1] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000001_Migration1', N'7.0.0-test');
GO

EXEC sp_rename N'[Table1].[Foo]', N'Bar', 'COLUMN';
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

CREATE PROCEDURE [dbo].[GotoReproduction]
AS
BEGIN
    DECLARE @Counter int;
    SET @Counter = 1;
    WHILE @Counter < 10
    BEGIN
        SELECT @Counter
        SET @Counter = @Counter + 1
        IF @Counter = 4 GOTO Branch_One --Jumps to the first branch.
        IF @Counter = 5 GOTO Branch_Two --This will never execute.
    END
    Branch_One:
        SELECT 'Jumping To Branch One.'
        GOTO Branch_Three; --This will prevent Branch_Two from executing.'
    Branch_Two:
        SELECT 'Jumping To Branch Two.'
    Branch_Three:
        SELECT 'Jumping To Branch Three.'
END;

GO

SELECT GetDate();
--GO
SELECT GetDate()
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000004_Migration4', N'7.0.0-test');
GO

INSERT INTO Table1 (Id, Bar, Description) VALUES (-1, 3, 'Value With

Empty Lines')
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000005_Migration5', N'7.0.0-test');
GO

INSERT INTO Table1 (Id, Bar, Description) VALUES (-2, 4, 'GO
Value With

Empty Lines')
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000006_Migration6', N'7.0.0-test');
GO

INSERT INTO Table1 (Id, Bar, Description) VALUES (-3, 5, '--Start
GO
Value With

GO

Empty Lines;
GO
')
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000007_Migration7', N'7.0.0-test');
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000007_Migration7';
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000006_Migration6';
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000005_Migration5';
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000004_Migration4';
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000003_Migration3';
GO

EXEC sp_rename N'[Table1].[Bar]', N'Foo', 'COLUMN';
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000002_Migration2';
GO

DROP TABLE [Table1];
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000001_Migration1';
GO


""",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override async Task Can_generate_one_up_and_down_script()
        {
            await base.Can_generate_one_up_and_down_script();

            Assert.Equal(
                """
BEGIN TRANSACTION;
EXEC sp_rename N'[Table1].[Foo]', N'Bar', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

COMMIT;
GO

BEGIN TRANSACTION;
EXEC sp_rename N'[Table1].[Bar]', N'Foo', 'COLUMN';

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000002_Migration2';

COMMIT;
GO


""",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override async Task Can_generate_up_and_down_script_using_names()
        {
            await base.Can_generate_up_and_down_script_using_names();

            Assert.Equal(
                """
BEGIN TRANSACTION;
EXEC sp_rename N'[Table1].[Foo]', N'Bar', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

COMMIT;
GO

BEGIN TRANSACTION;
EXEC sp_rename N'[Table1].[Bar]', N'Foo', 'COLUMN';

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'00000000000002_Migration2';

COMMIT;
GO


""",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override async Task Can_generate_idempotent_up_and_down_scripts()
        {
            await base.Can_generate_idempotent_up_and_down_scripts();

            Assert.Equal(
                """
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1'
)
BEGIN
    CREATE TABLE [Table1] (
        [Id] int NOT NULL,
        [Foo] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Table1] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'00000000000001_Migration1', N'7.0.0-test');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2'
)
BEGIN
    EXEC sp_rename N'[Table1].[Foo]', N'Bar', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'00000000000002_Migration2', N'7.0.0-test');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2'
)
BEGIN
    EXEC sp_rename N'[Table1].[Bar]', N'Foo', 'COLUMN';
END;

IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2'
)
BEGIN
    DELETE FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2';
END;

IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1'
)
BEGIN
    DROP TABLE [Table1];
END;

IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1'
)
BEGIN
    DELETE FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1';
END;

COMMIT;
GO


""",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override async Task Can_generate_idempotent_up_and_down_scripts_noTransactions()
        {
            await base.Can_generate_idempotent_up_and_down_scripts_noTransactions();

            Assert.Equal(
                """
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1'
)
BEGIN
    CREATE TABLE [Table1] (
        [Id] int NOT NULL,
        [Foo] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Table1] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'00000000000001_Migration1', N'7.0.0-test');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2'
)
BEGIN
    EXEC sp_rename N'[Table1].[Foo]', N'Bar', 'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'00000000000002_Migration2', N'7.0.0-test');
END;
GO

IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2'
)
BEGIN
    EXEC sp_rename N'[Table1].[Bar]', N'Foo', 'COLUMN';
END;
GO

IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2'
)
BEGIN
    DELETE FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000002_Migration2';
END;
GO

IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1'
)
BEGIN
    DROP TABLE [Table1];
END;
GO

IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1'
)
BEGIN
    DELETE FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'00000000000001_Migration1';
END;
GO


""",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_get_active_provider()
        {
            base.Can_get_active_provider();

            Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", ActiveProvider);
        }

        [ConditionalFact]
        public void Throws_when_no_migrations()
        {
            using var context = new DbContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)
                        .ConfigureWarnings(e => e.Throw(RelationalEventId.MigrationsNotFound))).Options);

            context.Database.EnsureDeleted();
            GiveMeSomeTime(context);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.MigrationsNotFound.ToString(),
                    RelationalResources.LogNoMigrationsFound(new TestLogger<TestRelationalLoggingDefinitions>())
                        .GenerateMessage(typeof(DbContext).Assembly.GetName().Name),
                    "RelationalEventId.MigrationsNotFound"),
                (Assert.Throws<InvalidOperationException>(context.Database.Migrate)).Message);
        }

        [ConditionalFact]
        public async Task Throws_when_no_migrations_async()
        {
            using var context = new DbContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)
                        .ConfigureWarnings(e => e.Throw(RelationalEventId.MigrationsNotFound))).Options);

            await context.Database.EnsureDeletedAsync();
            await GiveMeSomeTimeAsync(context);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.MigrationsNotFound.ToString(),
                    RelationalResources.LogNoMigrationsFound(new TestLogger<TestRelationalLoggingDefinitions>())
                        .GenerateMessage(typeof(DbContext).Assembly.GetName().Name),
                    "RelationalEventId.MigrationsNotFound"),
                (await Assert.ThrowsAsync<InvalidOperationException>(() => context.Database.MigrateAsync())).Message);
        }

        [ConditionalFact]
        public void Throws_when_no_snapshot()
        {
            using var context = new MigrationsContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)
                        .ConfigureWarnings(e => e.Throw(RelationalEventId.ModelSnapshotNotFound))).Options);

            context.Database.EnsureDeleted();
            GiveMeSomeTime(context);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.ModelSnapshotNotFound.ToString(),
                    RelationalResources.LogNoModelSnapshotFound(new TestLogger<TestRelationalLoggingDefinitions>())
                        .GenerateMessage(typeof(MigrationsContext).Assembly.GetName().Name),
                    "RelationalEventId.ModelSnapshotNotFound"),
                (Assert.Throws<InvalidOperationException>(context.Database.Migrate)).Message);
        }

        [ConditionalFact]
        public async Task Throws_when_no_snapshot_async()
        {
            using var context = new MigrationsContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)
                        .ConfigureWarnings(e => e.Throw(RelationalEventId.ModelSnapshotNotFound))).Options);

            await context.Database.EnsureDeletedAsync();
            await GiveMeSomeTimeAsync(context);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.ModelSnapshotNotFound.ToString(),
                    RelationalResources.LogNoModelSnapshotFound(new TestLogger<TestRelationalLoggingDefinitions>())
                        .GenerateMessage(typeof(MigrationsContext).Assembly.GetName().Name),
                    "RelationalEventId.ModelSnapshotNotFound"),
                (await Assert.ThrowsAsync<InvalidOperationException>(() => context.Database.MigrateAsync())).Message);
        }

        [ConditionalFact]
        public void Throws_for_nondeterministic_HasData()
        {
            using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options,
                randomData: true);

            context.Database.EnsureDeleted();
            GiveMeSomeTime(context);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.PendingModelChangesWarning.ToString(),
                    RelationalResources.LogNonDeterministicModel(new TestLogger<TestRelationalLoggingDefinitions>())
                        .GenerateMessage(nameof(BloggingContext)),
                    "RelationalEventId.PendingModelChangesWarning"),
                (Assert.Throws<InvalidOperationException>(context.Database.Migrate)).Message);
        }

        [ConditionalFact]
        public async Task Throws_for_nondeterministic_HasData_async()
        {
            using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options,
                randomData: true);

            await context.Database.EnsureDeletedAsync();
            await GiveMeSomeTimeAsync(context);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.PendingModelChangesWarning.ToString(),
                    RelationalResources.LogNonDeterministicModel(new TestLogger<TestRelationalLoggingDefinitions>())
                        .GenerateMessage(nameof(BloggingContext)),
                    "RelationalEventId.PendingModelChangesWarning"),
                (await Assert.ThrowsAsync<InvalidOperationException>(() => context.Database.MigrateAsync())).Message);
        }

        [ConditionalFact]
        public void Throws_for_pending_model_changes()
        {
            using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options,
                randomData: false);

            context.Database.EnsureDeleted();
            GiveMeSomeTime(context);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.PendingModelChangesWarning.ToString(),
                    RelationalResources.LogPendingModelChanges(new TestLogger<TestRelationalLoggingDefinitions>())
                        .GenerateMessage(nameof(BloggingContext)),
                    "RelationalEventId.PendingModelChangesWarning"),
                (Assert.Throws<InvalidOperationException>(context.Database.Migrate)).Message);
        }

        [ConditionalFact]
        public async Task Throws_for_pending_model_changes_async()
        {
            using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options,
                randomData: false);

            await context.Database.EnsureDeletedAsync();
            await GiveMeSomeTimeAsync(context);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    RelationalEventId.PendingModelChangesWarning.ToString(),
                    RelationalResources.LogPendingModelChanges(new TestLogger<TestRelationalLoggingDefinitions>())
                        .GenerateMessage(nameof(BloggingContext)),
                    "RelationalEventId.PendingModelChangesWarning"),
                (await Assert.ThrowsAsync<InvalidOperationException>(() => context.Database.MigrateAsync())).Message);
        }

        [ConditionalFact]
        public async Task Empty_Migration_Creates_Database()
        {
            using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                        new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options);

            context.Database.EnsureDeleted();
            GiveMeSomeTime(context);

            var creator = (SqlServerDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();
            creator.RetryTimeout = TimeSpan.FromMinutes(10);

            await context.Database.MigrateAsync("Empty");

            Assert.True(creator.Exists());
        }

        [ConditionalFact]
        public void Non_transactional_migration_is_retried()
        {
            using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                        new DbContextOptionsBuilder().EnableServiceProviderCaching(false))
                    .ConfigureWarnings(
                        e => e.Log(
                            RelationalEventId.PendingModelChangesWarning, RelationalEventId.NonTransactionalMigrationOperationWarning))
                    .UseLoggerFactory(Fixture.TestSqlLoggerFactory).Options);

            context.Database.EnsureDeleted();
            GiveMeSomeTime(context);

            Fixture.TestSqlLoggerFactory.Clear();

            var creator = (SqlServerDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();
            creator.RetryTimeout = TimeSpan.FromMinutes(10);

            context.Database.Migrate();

            Assert.Equal(
                """
CREATE DATABASE [MigrationsTest];

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [MigrationsTest] SET READ_COMMITTED_SNAPSHOT ON;
END;

SELECT 1

DECLARE @result int;
EXEC @result = sp_getapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session', @LockMode = 'Exclusive';
SELECT @result

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

SELECT 1

SELECT OBJECT_ID(N'[__EFMigrationsHistory]');

SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000000_Empty', N'7.0.0-test');

--Before

IF OBJECT_ID(N'Blogs', N'U') IS NULL
BEGIN
    CREATE TABLE [Blogs] (
        [Id] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Blogs] PRIMARY KEY ([Id])
    );

    THROW 65536, 'Test', 0;
END

DECLARE @result int;
EXEC @result = sp_releaseapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session';
SELECT @result

DECLARE @result int;
EXEC @result = sp_getapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session', @LockMode = 'Exclusive';
SELECT @result

SELECT 1

SELECT OBJECT_ID(N'[__EFMigrationsHistory]');

SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId];

IF OBJECT_ID(N'Blogs', N'U') IS NULL
BEGIN
    CREATE TABLE [Blogs] (
        [Id] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Blogs] PRIMARY KEY ([Id])
    );

    THROW 65536, 'Test', 0;
END

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000001_Migration1', N'7.0.0-test');

--After

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

DECLARE @result int;
EXEC @result = sp_releaseapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session';
SELECT @result
""",
                Fixture.TestSqlLoggerFactory.Sql.Replace(ProductInfo.GetVersion(), "7.0.0-test"),
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public async Task Non_transactional_migration_is_retried_async()
        {
            using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                        new DbContextOptionsBuilder().EnableServiceProviderCaching(false))
                    .ConfigureWarnings(
                        e => e.Log(
                            RelationalEventId.PendingModelChangesWarning, RelationalEventId.NonTransactionalMigrationOperationWarning))
                    .UseLoggerFactory(Fixture.TestSqlLoggerFactory).Options);

            context.Database.EnsureDeleted();
            GiveMeSomeTime(context);

            Fixture.TestSqlLoggerFactory.Clear();

            var creator = (SqlServerDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();
            creator.RetryTimeout = TimeSpan.FromMinutes(10);

            await context.Database.MigrateAsync();

            Assert.Equal(
                """
CREATE DATABASE [MigrationsTest];

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [MigrationsTest] SET READ_COMMITTED_SNAPSHOT ON;
END;

SELECT 1

DECLARE @result int;
EXEC @result = sp_getapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session', @LockMode = 'Exclusive';
SELECT @result

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

SELECT 1

SELECT OBJECT_ID(N'[__EFMigrationsHistory]');

SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000000_Empty', N'7.0.0-test');

--Before

IF OBJECT_ID(N'Blogs', N'U') IS NULL
BEGIN
    CREATE TABLE [Blogs] (
        [Id] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Blogs] PRIMARY KEY ([Id])
    );

    THROW 65536, 'Test', 0;
END

DECLARE @result int;
EXEC @result = sp_releaseapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session';
SELECT @result

DECLARE @result int;
EXEC @result = sp_getapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session', @LockMode = 'Exclusive';
SELECT @result

SELECT 1

SELECT OBJECT_ID(N'[__EFMigrationsHistory]');

SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId];

IF OBJECT_ID(N'Blogs', N'U') IS NULL
BEGIN
    CREATE TABLE [Blogs] (
        [Id] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Blogs] PRIMARY KEY ([Id])
    );

    THROW 65536, 'Test', 0;
END

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000001_Migration1', N'7.0.0-test');

--After

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

DECLARE @result int;
EXEC @result = sp_releaseapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session';
SELECT @result
""",
                Fixture.TestSqlLoggerFactory.Sql.Replace(ProductInfo.GetVersion(), "7.0.0-test"),
                ignoreLineEndingDifferences: true);
        }

        private class BloggingContext(DbContextOptions options, bool? randomData = null) : DbContext(options)
        {
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

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                if (randomData != null)
                {
                    modelBuilder.Entity<Blog>().HasData(
                        new Blog { Id = randomData.Value ? (int)new Random().NextInt64(int.MaxValue) : 1, Name = "HalfADonkey" });
                }
            }
        }

        [DbContext(typeof(BloggingContext))]
        partial class BloggingContextSnapshot : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "9.0.0")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128);

                SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

                modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.MigrationsInfrastructureSqlServerTest+BloggingContext+Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });
#pragma warning restore 612, 618
            }
        }

        [DbContext(typeof(BloggingContext))]
        [Migration("00000000000000_Empty")]
        private class EmptyMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(BloggingContext))]
        [Migration("00000000000001_Migration1")]
        private class BloggingMigration1 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("--Before", suppressTransaction: true);
                migrationBuilder.Sql(
                    """
IF OBJECT_ID(N'Blogs', N'U') IS NULL
BEGIN
    CREATE TABLE [Blogs] (
        [Id] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Blogs] PRIMARY KEY ([Id])
    );

    THROW 65536, 'Test', 0;
END
""", suppressTransaction: true);
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(BloggingContext))]
        [Migration("00000000000002_Migration2")]
        private class BloggingMigration2 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
                => migrationBuilder.Sql("--After");

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }

        public override void Can_diff_against_2_2_model()
        {
            using var context = new ModelSnapshot22.BloggingContext();
            DiffSnapshot(new BloggingContextModelSnapshot22(), context);
        }

        public class BloggingContextModelSnapshot22 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                modelBuilder.Entity(
                    "ModelSnapshot22.Blog", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.Property<string>("Name");

                        b.HasKey("Id");

                        b.ToTable("Blogs");

                        b.HasData(
                            new { Id = 1, Name = "HalfADonkey" });
                    });

                modelBuilder.Entity(
                    "ModelSnapshot22.Post", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.Property<int?>("BlogId");

                        b.Property<string>("Content");

                        b.Property<DateTime>("EditDate");

                        b.Property<string>("Title");

                        b.HasKey("Id");

                        b.HasIndex("BlogId");

                        b.ToTable("Post");
                    });

                modelBuilder.Entity(
                    "ModelSnapshot22.Post", b =>
                    {
                        b.HasOne("ModelSnapshot22.Blog", "Blog")
                            .WithMany("Posts")
                            .HasForeignKey("BlogId");
                    });
#pragma warning restore 612, 618
            }
        }

        public override void Can_diff_against_2_1_ASP_NET_Identity_model()
        {
            using var context = new ApplicationDbContext();
            DiffSnapshot(new AspNetIdentity21ModelSnapshot(), context);
        }

        public class AspNetIdentity21ModelSnapshot : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "2.1.0")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRole", b =>
                    {
                        b.Property<string>("Id")
                            .ValueGeneratedOnAdd();

                        b.Property<string>("ConcurrencyStamp")
                            .IsConcurrencyToken();

                        b.Property<string>("Name")
                            .HasMaxLength(256);

                        b.Property<string>("NormalizedName")
                            .HasMaxLength(256);

                        b.HasKey("Id");

                        b.HasIndex("NormalizedName")
                            .IsUnique()
                            .HasName("RoleNameIndex") // Don't change to HasDatabaseName
                            .HasFilter("[NormalizedName] IS NOT NULL");

                        b.ToTable("AspNetRoles");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.Property<string>("ClaimType");

                        b.Property<string>("ClaimValue");

                        b.Property<string>("RoleId")
                            .IsRequired();

                        b.HasKey("Id");

                        b.HasIndex("RoleId");

                        b.ToTable("AspNetRoleClaims");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUser", b =>
                    {
                        b.Property<string>("Id")
                            .ValueGeneratedOnAdd();

                        b.Property<int>("AccessFailedCount");

                        b.Property<string>("ConcurrencyStamp")
                            .IsConcurrencyToken();

                        b.Property<string>("Email")
                            .HasMaxLength(256);

                        b.Property<bool>("EmailConfirmed");

                        b.Property<bool>("LockoutEnabled");

                        b.Property<DateTimeOffset?>("LockoutEnd");

                        b.Property<string>("NormalizedEmail")
                            .HasMaxLength(256);

                        b.Property<string>("NormalizedUserName")
                            .HasMaxLength(256);

                        b.Property<string>("PasswordHash");

                        b.Property<string>("PhoneNumber");

                        b.Property<bool>("PhoneNumberConfirmed");

                        b.Property<string>("SecurityStamp");

                        b.Property<bool>("TwoFactorEnabled");

                        b.Property<string>("UserName")
                            .HasMaxLength(256);

                        b.HasKey("Id");

                        b.HasIndex("NormalizedEmail")
                            .HasName("EmailIndex");

                        b.HasIndex("NormalizedUserName")
                            .IsUnique()
                            .HasName("UserNameIndex")
                            .HasFilter("[NormalizedUserName] IS NOT NULL");

                        b.ToTable("AspNetUsers");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.Property<string>("ClaimType");

                        b.Property<string>("ClaimValue");

                        b.Property<string>("UserId")
                            .IsRequired();

                        b.HasKey("Id");

                        b.HasIndex("UserId");

                        b.ToTable("AspNetUserClaims");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                    {
                        b.Property<string>("LoginProvider")
                            .HasMaxLength(128);

                        b.Property<string>("ProviderKey")
                            .HasMaxLength(128);

                        b.Property<string>("ProviderDisplayName");

                        b.Property<string>("UserId")
                            .IsRequired();

                        b.HasKey("LoginProvider", "ProviderKey");

                        b.HasIndex("UserId");

                        b.ToTable("AspNetUserLogins");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                    {
                        b.Property<string>("UserId");

                        b.Property<string>("RoleId");

                        b.HasKey("UserId", "RoleId");

                        b.HasIndex("RoleId");

                        b.ToTable("AspNetUserRoles");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                    {
                        b.Property<string>("UserId");

                        b.Property<string>("LoginProvider")
                            .HasMaxLength(128);

                        b.Property<string>("Name")
                            .HasMaxLength(128);

                        b.Property<string>("Value");

                        b.HasKey("UserId", "LoginProvider", "Name");

                        b.ToTable("AspNetUserTokens");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade);

                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });
#pragma warning restore 612, 618
            }
        }

        public override void Can_diff_against_2_2_ASP_NET_Identity_model()
        {
            using var context = new ApplicationDbContext();
            DiffSnapshot(new AspNetIdentity22ModelSnapshot(), context);
        }

        public class AspNetIdentity22ModelSnapshot : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "2.2.0-preview1")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRole", b =>
                    {
                        b.Property<string>("Id")
                            .ValueGeneratedOnAdd();

                        b.Property<string>("ConcurrencyStamp")
                            .IsConcurrencyToken();

                        b.Property<string>("Name")
                            .HasMaxLength(256);

                        b.Property<string>("NormalizedName")
                            .HasMaxLength(256);

                        b.HasKey("Id");

                        b.HasIndex("NormalizedName")
                            .IsUnique()
                            .HasName("RoleNameIndex")
                            .HasFilter("[NormalizedName] IS NOT NULL");

                        b.ToTable("AspNetRoles");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.Property<string>("ClaimType");

                        b.Property<string>("ClaimValue");

                        b.Property<string>("RoleId")
                            .IsRequired();

                        b.HasKey("Id");

                        b.HasIndex("RoleId");

                        b.ToTable("AspNetRoleClaims");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUser", b =>
                    {
                        b.Property<string>("Id")
                            .ValueGeneratedOnAdd();

                        b.Property<int>("AccessFailedCount");

                        b.Property<string>("ConcurrencyStamp")
                            .IsConcurrencyToken();

                        b.Property<string>("Email")
                            .HasMaxLength(256);

                        b.Property<bool>("EmailConfirmed");

                        b.Property<bool>("LockoutEnabled");

                        b.Property<DateTimeOffset?>("LockoutEnd");

                        b.Property<string>("NormalizedEmail")
                            .HasMaxLength(256);

                        b.Property<string>("NormalizedUserName")
                            .HasMaxLength(256);

                        b.Property<string>("PasswordHash");

                        b.Property<string>("PhoneNumber");

                        b.Property<bool>("PhoneNumberConfirmed");

                        b.Property<string>("SecurityStamp");

                        b.Property<bool>("TwoFactorEnabled");

                        b.Property<string>("UserName")
                            .HasMaxLength(256);

                        b.HasKey("Id");

                        b.HasIndex("NormalizedEmail")
                            .HasName("EmailIndex");

                        b.HasIndex("NormalizedUserName")
                            .IsUnique()
                            .HasName("UserNameIndex")
                            .HasFilter("[NormalizedUserName] IS NOT NULL");

                        b.ToTable("AspNetUsers");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.Property<string>("ClaimType");

                        b.Property<string>("ClaimValue");

                        b.Property<string>("UserId")
                            .IsRequired();

                        b.HasKey("Id");

                        b.HasIndex("UserId");

                        b.ToTable("AspNetUserClaims");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                    {
                        b.Property<string>("LoginProvider")
                            .HasMaxLength(128);

                        b.Property<string>("ProviderKey")
                            .HasMaxLength(128);

                        b.Property<string>("ProviderDisplayName");

                        b.Property<string>("UserId")
                            .IsRequired();

                        b.HasKey("LoginProvider", "ProviderKey");

                        b.HasIndex("UserId");

                        b.ToTable("AspNetUserLogins");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                    {
                        b.Property<string>("UserId");

                        b.Property<string>("RoleId");

                        b.HasKey("UserId", "RoleId");

                        b.HasIndex("RoleId");

                        b.ToTable("AspNetUserRoles");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                    {
                        b.Property<string>("UserId");

                        b.Property<string>("LoginProvider")
                            .HasMaxLength(128);

                        b.Property<string>("Name")
                            .HasMaxLength(128);

                        b.Property<string>("Value");

                        b.HasKey("UserId", "LoginProvider", "Name");

                        b.ToTable("AspNetUserTokens");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade);

                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade);
                    });
#pragma warning restore 612, 618
            }
        }

        public override void Can_diff_against_3_0_ASP_NET_Identity_model()
        {
            using var context = new ApplicationDbContext();
            DiffSnapshot(new AspNetIdentity30ModelSnapshot(), context);
        }

        protected override Task ExecuteSqlAsync(string value)
        {
            ((SqlServerTestStore)Fixture.TestStore).ExecuteScript(value);
            return Task.CompletedTask;
        }

        public class AspNetIdentity30ModelSnapshot : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "3.0.0")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRole", b =>
                    {
                        b.Property<string>("Id")
                            .HasColumnType("nvarchar(450)");

                        b.Property<string>("ConcurrencyStamp")
                            .IsConcurrencyToken()
                            .HasColumnType("nvarchar(max)");

                        b.Property<string>("Name")
                            .HasColumnType("nvarchar(256)")
                            .HasMaxLength(256);

                        b.Property<string>("NormalizedName")
                            .HasColumnType("nvarchar(256)")
                            .HasMaxLength(256);

                        b.HasKey("Id");

                        b.HasIndex("NormalizedName")
                            .IsUnique()
                            .HasName("RoleNameIndex")
                            .HasFilter("[NormalizedName] IS NOT NULL");

                        b.ToTable("AspNetRoles");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.Property<string>("ClaimType")
                            .HasColumnType("nvarchar(max)");

                        b.Property<string>("ClaimValue")
                            .HasColumnType("nvarchar(max)");

                        b.Property<string>("RoleId")
                            .IsRequired()
                            .HasColumnType("nvarchar(450)");

                        b.HasKey("Id");

                        b.HasIndex("RoleId");

                        b.ToTable("AspNetRoleClaims");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUser", b =>
                    {
                        b.Property<string>("Id")
                            .HasColumnType("nvarchar(450)");

                        b.Property<int>("AccessFailedCount")
                            .HasColumnType("int");

                        b.Property<string>("ConcurrencyStamp")
                            .IsConcurrencyToken()
                            .HasColumnType("nvarchar(max)");

                        b.Property<string>("Email")
                            .HasColumnType("nvarchar(256)")
                            .HasMaxLength(256);

                        b.Property<bool>("EmailConfirmed")
                            .HasColumnType("bit");

                        b.Property<bool>("LockoutEnabled")
                            .HasColumnType("bit");

                        b.Property<DateTimeOffset?>("LockoutEnd")
                            .HasColumnType("datetimeoffset");

                        b.Property<string>("NormalizedEmail")
                            .HasColumnType("nvarchar(256)")
                            .HasMaxLength(256);

                        b.Property<string>("NormalizedUserName")
                            .HasColumnType("nvarchar(256)")
                            .HasMaxLength(256);

                        b.Property<string>("PasswordHash")
                            .HasColumnType("nvarchar(max)");

                        b.Property<string>("PhoneNumber")
                            .HasColumnType("nvarchar(max)");

                        b.Property<bool>("PhoneNumberConfirmed")
                            .HasColumnType("bit");

                        b.Property<string>("SecurityStamp")
                            .HasColumnType("nvarchar(max)");

                        b.Property<bool>("TwoFactorEnabled")
                            .HasColumnType("bit");

                        b.Property<string>("UserName")
                            .HasColumnType("nvarchar(256)")
                            .HasMaxLength(256);

                        b.HasKey("Id");

                        b.HasIndex("NormalizedEmail")
                            .HasName("EmailIndex");

                        b.HasIndex("NormalizedUserName")
                            .IsUnique()
                            .HasName("UserNameIndex")
                            .HasFilter("[NormalizedUserName] IS NOT NULL");

                        b.ToTable("AspNetUsers");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.Property<string>("ClaimType")
                            .HasColumnType("nvarchar(max)");

                        b.Property<string>("ClaimValue")
                            .HasColumnType("nvarchar(max)");

                        b.Property<string>("UserId")
                            .IsRequired()
                            .HasColumnType("nvarchar(450)");

                        b.HasKey("Id");

                        b.HasIndex("UserId");

                        b.ToTable("AspNetUserClaims");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                    {
                        b.Property<string>("LoginProvider")
                            .HasColumnType("nvarchar(128)")
                            .HasMaxLength(128);

                        b.Property<string>("ProviderKey")
                            .HasColumnType("nvarchar(128)")
                            .HasMaxLength(128);

                        b.Property<string>("ProviderDisplayName")
                            .HasColumnType("nvarchar(max)");

                        b.Property<string>("UserId")
                            .IsRequired()
                            .HasColumnType("nvarchar(450)");

                        b.HasKey("LoginProvider", "ProviderKey");

                        b.HasIndex("UserId");

                        b.ToTable("AspNetUserLogins");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                    {
                        b.Property<string>("UserId")
                            .HasColumnType("nvarchar(450)");

                        b.Property<string>("RoleId")
                            .HasColumnType("nvarchar(450)");

                        b.HasKey("UserId", "RoleId");

                        b.HasIndex("RoleId");

                        b.ToTable("AspNetUserRoles");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                    {
                        b.Property<string>("UserId")
                            .HasColumnType("nvarchar(450)");

                        b.Property<string>("LoginProvider")
                            .HasColumnType("nvarchar(128)")
                            .HasMaxLength(128);

                        b.Property<string>("Name")
                            .HasColumnType("nvarchar(128)")
                            .HasMaxLength(128);

                        b.Property<string>("Value")
                            .HasColumnType("nvarchar(max)");

                        b.HasKey("UserId", "LoginProvider", "Name");

                        b.ToTable("AspNetUserTokens");
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();

                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();
                    });

                modelBuilder.Entity(
                    "Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                    {
                        b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();
                    });
#pragma warning restore 612, 618
            }
        }

        public class MigrationsInfrastructureSqlServerFixture : MigrationsInfrastructureFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            public override async Task InitializeAsync()
            {
                await base.InitializeAsync();
                await ((SqlServerTestStore)TestStore).ExecuteNonQueryAsync(
                    @"USE master
IF EXISTS(select * from sys.databases where name='TransactionSuppressed')
DROP DATABASE TransactionSuppressed");
            }

            public override MigrationsContext CreateContext()
            {
                var options = AddOptions(TestStore.AddProviderOptions(new DbContextOptionsBuilder()))
                    .UseSqlServer(TestStore.ConnectionString, b => b
                        .ApplyConfiguration())
                    .UseInternalServiceProvider(ServiceProvider)
                    .Options;
                return new MigrationsContext(options);
            }

            protected override bool ShouldLogCategory(string logCategory)
                => base.ShouldLogCategory(logCategory);
        }
    }
}

namespace ModelSnapshot22
{
    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Post> Posts { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime EditDate { get; set; }

        public Blog Blog { get; set; }
    }

    public class BloggingContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0");

        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog>().HasData(
                new Blog { Id = 1, Name = "HalfADonkey" });
    }
}

namespace Identity30.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0");

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>(
                b =>
                {
                    b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
                    b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
                    b.ToTable("AspNetUsers");
                });

            builder.Entity<IdentityUserClaim<string>>(
                b =>
                {
                    b.ToTable("AspNetUserClaims");
                });

            builder.Entity<IdentityUserLogin<string>>(
                b =>
                {
                    b.ToTable("AspNetUserLogins");
                });

            builder.Entity<IdentityUserToken<string>>(
                b =>
                {
                    b.ToTable("AspNetUserTokens");
                });

            builder.Entity<IdentityRole>(
                b =>
                {
                    b.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();
                    b.ToTable("AspNetRoles");
                });

            builder.Entity<IdentityRoleClaim<string>>(
                b =>
                {
                    b.ToTable("AspNetRoleClaims");
                });

            builder.Entity<IdentityUserRole<string>>(
                b =>
                {
                    b.ToTable("AspNetUserRoles");
                });
        }
    }
}
