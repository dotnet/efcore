// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

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
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));
    }

    protected override TestHelpers TestHelpers
        => SqlServerTestHelpers.Instance;

    [ConditionalFact]
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

    [ConditionalFact]
    public void AppendBulkInsertOperation_appends_merge_if_store_generated_columns_exist()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand();

        var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
        var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

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

    [ConditionalFact]
    public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(identityKey: false, isComputed: false);

        var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
        var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

        AssertBaseline(
            """
INSERT INTO [dbo].[Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken])
VALUES (@p0, @p1, @p2, @p3),
(@p0, @p1, @p2, @p3);
""",
            stringBuilder.ToString());
        Assert.Equal(ResultSetMapping.NoResults, grouping);
    }

    [ConditionalFact]
    public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist_default_values_only()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(identityKey: true, isComputed: true, defaultsOnly: true);

        var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
        var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

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

    [ConditionalFact]
    public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist_default_values_only()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(identityKey: false, isComputed: false, defaultsOnly: true);

        var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
        var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

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
}
