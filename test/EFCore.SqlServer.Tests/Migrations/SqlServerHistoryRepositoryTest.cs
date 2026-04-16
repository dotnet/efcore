// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations;

public class SqlServerHistoryRepositoryTest
{
    [ConditionalFact]
    public void GetCreateScript_works()
    {
        var sql = CreateHistoryRepository().GetCreateScript();

        Assert.Equal(
            """
CREATE TABLE [__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetCreateScript_works_with_full_text_catalog()
    {
        // Inject a model finalizing convention that adds a full-text catalog to the model, simulating a scenario where
        // provider conventions add database-level annotations. Without filtering, the history table creation script would
        // include CREATE FULLTEXT CATALOG.
        var sql = CreateHistoryRepository(addFullTextCatalogConvention: true).GetCreateScript();

        Assert.Equal(
            """
CREATE TABLE [__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetCreateScript_works_with_schema()
    {
        var sql = CreateHistoryRepository("my").GetCreateScript();

        Assert.Equal(
            """
IF SCHEMA_ID(N'my') IS NULL EXEC(N'CREATE SCHEMA [my];');
CREATE TABLE [my].[__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetCreateIfNotExistsScript_works()
    {
        var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

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

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetCreateIfNotExistsScript_works_with_schema()
    {
        var sql = CreateHistoryRepository("my").GetCreateIfNotExistsScript();

        Assert.Equal(
            """
IF OBJECT_ID(N'[my].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'my') IS NULL EXEC(N'CREATE SCHEMA [my];');
    CREATE TABLE [my].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetDeleteScript_works()
    {
        var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

        Assert.Equal(
            """
DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'Migration1';

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetInsertScript_works()
    {
        var sql = CreateHistoryRepository().GetInsertScript(
            new HistoryRow("Migration1", "7.0.0"));

        Assert.Equal(
            """
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'Migration1', N'7.0.0');

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetBeginIfNotExistsScript_works()
    {
        var sql = CreateHistoryRepository().GetBeginIfNotExistsScript("Migration1");

        Assert.Equal(
            """
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'Migration1'
)
BEGIN
""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetBeginIfExistsScript_works()
    {
        var sql = CreateHistoryRepository().GetBeginIfExistsScript("Migration1");

        Assert.Equal(
            """
IF EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'Migration1'
)
BEGIN
""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetEndIfScript_works()
    {
        var sql = CreateHistoryRepository().GetEndIfScript();

        Assert.Equal(
            """
END;

""", sql, ignoreLineEndingDifferences: true);
    }

    private static IHistoryRepository CreateHistoryRepository(
        string schema = null,
        Action<ModelBuilder> configureModel = null,
        bool addFullTextCatalogConvention = false)
    {
        var serviceProvider = addFullTextCatalogConvention
            ? SqlServerTestHelpers.Instance.CreateServiceProvider(
                new ServiceCollection().AddSingleton<IConventionSetPlugin, FullTextCatalogConventionPlugin>())
            : SqlServerTestHelpers.Instance.CreateServiceProvider();

        return new TestDbContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(serviceProvider)
                    .UseSqlServer(
                        new SqlConnection("Database=DummyDatabase"),
                        b => b.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema))
                    .Options,
                configureModel)
            .GetService<IHistoryRepository>();
    }

    private class TestDbContext(DbContextOptions options, Action<ModelBuilder> configureModel = null) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; }

        [DbFunction("TableFunction")]
        public IQueryable<TableFunction> TableFunction()
            => FromExpression(() => TableFunction());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            configureModel?.Invoke(modelBuilder);
        }
    }

    /// <summary>
    ///     A convention plugin that adds a full-text catalog annotation to the model, simulating what a provider convention
    ///     might do. This allows testing that <see cref="SqlServerHistoryRepository.GetCreateCommands" /> properly filters
    ///     out the full-text catalog from the history table creation script.
    /// </summary>
    private class FullTextCatalogConventionPlugin : IConventionSetPlugin
    {
        public ConventionSet ModifyConventions(ConventionSet conventionSet)
        {
            conventionSet.ModelFinalizingConventions.Add(new FullTextCatalogAddingConvention());
            return conventionSet;
        }
    }

    private class FullTextCatalogAddingConvention : IModelFinalizingConvention
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        public void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
            => SqlServerFullTextCatalog.AddFullTextCatalog(
                (IMutableModel)modelBuilder.Metadata, "TestCatalog", ConfigurationSource.Convention);
#pragma warning restore EF1001 // Internal EF Core API usage.
    }

    private class Blog
    {
        public int Id { get; set; }
    }

    private class TableFunction
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
