// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public class SqlServerUpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
{
    protected override IUpdateSqlGenerator CreateSqlGenerator()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer("Database=Foo");

        return new SqlServerUpdateSqlGenerator(
            new UpdateSqlGeneratorDependencies(
                new SqlServerSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new SqlServerTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<SqlServerSingletonOptions>())));
    }

    protected override TestHelpers TestHelpers
        => SqlServerTestHelpers.Instance;

    [Fact]
    public void AppendBatchHeader_should_append_SET_NOCOUNT_ON()
    {
        var sb = new StringBuilder();

        CreateSqlGenerator().AppendBatchHeader(sb);

        Assert.Equal("SET NOCOUNT ON;" + Environment.NewLine, sb.ToString());
    }

    protected override void AppendInsertOperation_for_store_generated_columns_but_no_identity_verification(
        StringBuilder stringBuilder)
        => AssertBaseline(
            """
INSERT INTO [dbo].[Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken])
OUTPUT INSERTED.[Computed]
VALUES (@p0, @p1, @p2, @p3);
""",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_insert_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            """
INSERT INTO [dbo].[Ducks] ([Name], [Quacks], [ConcurrencyToken])
OUTPUT INSERTED.[Id], INSERTED.[Computed]
VALUES (@p0, @p1, @p2);
""",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_for_only_single_identity_columns_verification(
        StringBuilder stringBuilder)
        => AssertBaseline(
            """
INSERT INTO [dbo].[Ducks]
OUTPUT INSERTED.[Id]
DEFAULT VALUES;
""",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_for_only_identity_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            """
INSERT INTO [dbo].[Ducks] ([Name], [Quacks], [ConcurrencyToken])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);
""",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            """
INSERT INTO [dbo].[Ducks]
OUTPUT INSERTED.[Id], INSERTED.[Computed]
DEFAULT VALUES;
""",
            stringBuilder.ToString());

    [Fact]
    public void AppendBulkInsertOperation_appends_merge_if_store_generated_columns_exist()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand();

        var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
        var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, [command, command], 0);

        AssertBaseline(
            """
MERGE [dbo].[Ducks] USING (
VALUES (@p0, @p1, @p2, 0),
(@p0, @p1, @p2, 1)) AS i ([Name], [Quacks], [ConcurrencyToken], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Name], [Quacks], [ConcurrencyToken])
VALUES (i.[Name], i.[Quacks], i.[ConcurrencyToken])
OUTPUT INSERTED.[Id], INSERTED.[Computed], i._Position;
""",
            stringBuilder.ToString());
        Assert.Equal(ResultSetMapping.NotLastInResultSet | ResultSetMapping.IsPositionalResultMappingEnabled, grouping);
    }

    [Fact]
    public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(identityKey: false, isComputed: false);

        var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
        var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, [command, command], 0);

        AssertBaseline(
            """
INSERT INTO [dbo].[Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken])
VALUES (@p0, @p1, @p2, @p3),
(@p0, @p1, @p2, @p3);
""",
            stringBuilder.ToString());
        Assert.Equal(ResultSetMapping.NoResults, grouping);
    }

    [Fact]
    public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist_default_values_only()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(identityKey: true, isComputed: true, defaultsOnly: true);

        var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
        var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, [command, command], 0);

        AssertBaseline(
            """
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [dbo].[Ducks] ([Id])
OUTPUT INSERTED.[Id]
INTO @inserted0
VALUES (DEFAULT),
(DEFAULT);
SELECT [t].[Id], [t].[Computed] FROM [dbo].[Ducks] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);
""",
            stringBuilder.ToString());
        Assert.Equal(ResultSetMapping.NotLastInResultSet, grouping);
    }

    [Fact]
    public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist_default_values_only()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(identityKey: false, isComputed: false, defaultsOnly: true);

        var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
        var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, [command, command], 0);

        var expectedText =
            """
INSERT INTO [dbo].[Ducks] ([Computed])
VALUES (DEFAULT),
(DEFAULT);
""";
        AssertBaseline(expectedText, stringBuilder.ToString());
        Assert.Equal(ResultSetMapping.NoResults, grouping);
    }

    protected override void AppendUpdateOperation_for_computed_property_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            """
UPDATE [dbo].[Ducks] SET [Name] = @p0, [Quacks] = @p1, [ConcurrencyToken] = @p2
OUTPUT INSERTED.[Computed]
WHERE [Id] = @p3;
""",
            stringBuilder.ToString());

    protected override void AppendUpdateOperation_if_store_generated_columns_exist_verification(
        StringBuilder stringBuilder)
        => AssertBaseline(
            """
UPDATE [dbo].[Ducks] SET [Name] = @p0, [Quacks] = @p1, [ConcurrencyToken] = @p2
OUTPUT INSERTED.[Computed]
WHERE [Id] = @p3 AND [ConcurrencyToken] IS NULL;
""",
            stringBuilder.ToString());

    protected override void AppendUpdateOperation_if_store_generated_columns_dont_exist_verification(
        StringBuilder stringBuilder)
        => AssertBaseline(
            """
UPDATE [dbo].[Ducks] SET [Name] = @p0, [Quacks] = @p1, [ConcurrencyToken] = @p2
OUTPUT 1
WHERE [Id] = @p3;
""",
            stringBuilder.ToString());

    protected override void AppendUpdateOperation_appends_where_for_concurrency_token_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            """
UPDATE [dbo].[Ducks] SET [Name] = @p0, [Quacks] = @p1, [ConcurrencyToken] = @p2
OUTPUT 1
WHERE [Id] = @p3 AND [ConcurrencyToken] IS NULL;
""",
            stringBuilder.ToString());

    protected override void AppendDeleteOperation_creates_full_delete_command_text_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            """
DELETE FROM [dbo].[Ducks]
OUTPUT 1
WHERE [Id] = @p0;
""",
            stringBuilder.ToString());

    protected override void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check_verification(
        StringBuilder stringBuilder)
        => AssertBaseline(
            """
DELETE FROM [dbo].[Ducks]
OUTPUT 1
WHERE [Id] = @p0 AND [ConcurrencyToken] IS NULL;
""",
            stringBuilder.ToString());

    protected override string RowsAffected
        => "@@ROWCOUNT";

    protected override string Identity
        => throw new NotImplementedException();

    protected override string OpenDelimiter
        => "[";

    protected override string CloseDelimiter
        => "]";

    private void AssertBaseline(string expected, string actual)
        => Assert.Equal(expected, actual.TrimEnd(), ignoreLineEndingDifferences: true);

    [Fact]
    public void AppendOptionalFragmentUpsertOperation_generates_update_then_conditional_insert()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateOptionalFragmentCommand(EntityState.Modified, concurrencyToken: false);

        CreateSqlGenerator().AppendOptionalFragmentUpsertOperation(stringBuilder, command, 0, out var requiresTransaction);

        AssertBaseline(
            """
DECLARE @_fragmentRowsAffected0 int;

UPDATE [dbo].[CustomerDetails] SET [Description] = @p0, [Score] = @p1
WHERE [Id] = @p2;
SET @_fragmentRowsAffected0 = @@ROWCOUNT;

IF @_fragmentRowsAffected0 = 0
BEGIN
    IF (@p0 IS NOT NULL OR @p1 IS NOT NULL)
    BEGIN
        INSERT INTO [dbo].[CustomerDetails] ([Id], [Description], [Score])
        VALUES (@p2, @p0, @p1);
        SET @_fragmentRowsAffected0 = @@ROWCOUNT;
    END
    ELSE
    BEGIN
        SET @_fragmentRowsAffected0 = 1;
    END
END

SELECT @_fragmentRowsAffected0;
""",
            stringBuilder.ToString());
        Assert.True(requiresTransaction);
    }

    [Fact]
    public void AppendOptionalFragmentUpsertOperation_distinguishes_absence_from_conflict_with_concurrency_token()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateOptionalFragmentCommand(EntityState.Modified, concurrencyToken: true);

        CreateSqlGenerator().AppendOptionalFragmentUpsertOperation(stringBuilder, command, 0, out var requiresTransaction);

        AssertBaseline(
            """
DECLARE @_fragmentRowsAffected0 int;

UPDATE [dbo].[CustomerDetails] SET [Description] = @p0, [Score] = @p1
WHERE [Id] = @p2 AND [Version] IS NULL;
SET @_fragmentRowsAffected0 = @@ROWCOUNT;

IF @_fragmentRowsAffected0 = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerDetails] WHERE [Id] = @p2)
    BEGIN
        IF (@p0 IS NOT NULL OR @p1 IS NOT NULL)
        BEGIN
            INSERT INTO [dbo].[CustomerDetails] ([Id], [Description], [Score])
            VALUES (@p2, @p0, @p1);
            SET @_fragmentRowsAffected0 = @@ROWCOUNT;
        END
        ELSE
        BEGIN
            SET @_fragmentRowsAffected0 = 1;
        END
    END
END

SELECT @_fragmentRowsAffected0;
""",
            stringBuilder.ToString());
        Assert.True(requiresTransaction);
    }

    [Fact]
    public void AppendOptionalFragmentDeleteOperation_generates_tolerant_delete()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateOptionalFragmentCommand(EntityState.Deleted, concurrencyToken: false);

        CreateSqlGenerator().AppendOptionalFragmentDeleteOperation(stringBuilder, command, 0, out var requiresTransaction);

        AssertBaseline(
            """
DECLARE @_fragmentRowsAffected0 int;

DELETE FROM [dbo].[CustomerDetails]
WHERE [Id] = @p0;
SET @_fragmentRowsAffected0 = @@ROWCOUNT;

IF @_fragmentRowsAffected0 = 0
BEGIN
    SET @_fragmentRowsAffected0 = 1;
END

SELECT @_fragmentRowsAffected0;
""",
            stringBuilder.ToString());
        Assert.True(requiresTransaction);
    }

    [Fact]
    public void AppendOptionalFragmentDeleteOperation_distinguishes_absence_from_conflict_with_concurrency_token()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateOptionalFragmentCommand(EntityState.Deleted, concurrencyToken: true);

        CreateSqlGenerator().AppendOptionalFragmentDeleteOperation(stringBuilder, command, 0, out var requiresTransaction);

        AssertBaseline(
            """
DECLARE @_fragmentRowsAffected0 int;

DELETE FROM [dbo].[CustomerDetails]
WHERE [Id] = @p0 AND [Version] IS NULL;
SET @_fragmentRowsAffected0 = @@ROWCOUNT;

IF @_fragmentRowsAffected0 = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerDetails] WHERE [Id] = @p0)
    BEGIN
        SET @_fragmentRowsAffected0 = 1;
    END
END

SELECT @_fragmentRowsAffected0;
""",
            stringBuilder.ToString());
        Assert.True(requiresTransaction);
    }

    [Fact]
    public void AppendOptionalFragmentUpsertOperation_throws_for_generated_values()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateOptionalFragmentCommand(EntityState.Modified, concurrencyToken: false, generatedValue: true);

        Assert.Throws<NotSupportedException>(
            () => CreateSqlGenerator().AppendOptionalFragmentUpsertOperation(stringBuilder, command, 0, out _));
    }

    private IModificationCommand CreateOptionalFragmentCommand(
        EntityState entityState,
        bool concurrencyToken,
        bool generatedValue = false)
    {
        var model = GetOptionalFragmentModel();
        var stateManager = TestHelpers.CreateContextServices(model).GetRequiredService<IStateManager>();
        var entry = stateManager.GetOrCreateEntry(new OptionalFragmentDetail());
        entry.SetEntityState(entityState);
        var generator = new ParameterNameGenerator();

        var detailType = entry.EntityType;
        var idProperty = detailType.FindProperty(nameof(OptionalFragmentDetail.Id));
        var descriptionProperty = detailType.FindProperty(nameof(OptionalFragmentDetail.Description));
        var scoreProperty = detailType.FindProperty(nameof(OptionalFragmentDetail.Score));
        var versionProperty = detailType.FindProperty(nameof(OptionalFragmentDetail.Version));

        var columnModifications = new List<ColumnModificationParameters>
        {
            new(
                entry, idProperty, idProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                idProperty.GetTableColumnMappings().Single().TypeMapping, false, false, true, true, true)
        };

        if (entityState != EntityState.Deleted)
        {
            columnModifications.Add(
                new(
                    entry, descriptionProperty, descriptionProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    descriptionProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true));
            columnModifications.Add(
                new(
                    entry, scoreProperty, scoreProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    scoreProperty.GetTableColumnMappings().Single().TypeMapping, generatedValue, !generatedValue, false, false, true));
        }

        if (concurrencyToken)
        {
            columnModifications.Add(
                new(
                    entry, versionProperty, versionProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                    versionProperty.GetTableColumnMappings().Single().TypeMapping, false, false, false, true, true));
        }

        var modificationCommandParameters = new ModificationCommandParameters(
            entry.EntityType.GetTableMappings().Single().Table, sensitiveLoggingEnabled: false);
        var modificationCommand = CreateMutableModificationCommandFactory().CreateModificationCommand(modificationCommandParameters);

        modificationCommand.AddEntry(entry, mainEntry: true);

        foreach (var columnModification in columnModifications)
        {
            ((INonTrackedModificationCommand)modificationCommand).AddColumnModification(columnModification);
        }

        return modificationCommand;
    }

    private IModel GetOptionalFragmentModel()
    {
        var modelBuilder = TestHelpers.CreateConventionBuilder();
        modelBuilder.Entity<OptionalFragmentDetail>().ToTable("CustomerDetails", Schema)
            .Property(e => e.Id).ValueGeneratedNever();
        return modelBuilder.Model.FinalizeModel();
    }

    private class OptionalFragmentDetail
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int? Score { get; set; }
        public int? Version { get; set; }
    }
}
