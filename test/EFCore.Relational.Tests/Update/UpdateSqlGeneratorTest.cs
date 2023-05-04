// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Update;

public class UpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
{
    protected override IUpdateSqlGenerator CreateSqlGenerator()
        => new FakeSqlGenerator(
            new UpdateSqlGeneratorDependencies(
                new RelationalSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));

    protected override void AppendDeleteOperation_creates_full_delete_command_text_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            @"DELETE FROM ""dbo"".""Ducks""
WHERE ""Id"" = @p0
RETURNING 1;
",
            stringBuilder.ToString());

    protected override void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check_verification(
        StringBuilder stringBuilder)
        => AssertBaseline(
            @"DELETE FROM ""dbo"".""Ducks""
WHERE ""Id"" = @p0 AND ""ConcurrencyToken"" IS NULL
RETURNING 1;
",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_insert_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            @"INSERT INTO ""dbo"".""Ducks"" (""Name"", ""Quacks"", ""ConcurrencyToken"")
VALUES (@p0, @p1, @p2)
RETURNING ""Id"", ""Computed"";
",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_for_store_generated_columns_but_no_identity_verification(
        StringBuilder stringBuilder)
        => AssertBaseline(
            @"INSERT INTO ""dbo"".""Ducks"" (""Id"", ""Name"", ""Quacks"", ""ConcurrencyToken"")
VALUES (@p0, @p1, @p2, @p3)
RETURNING ""Computed"";
",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_for_only_identity_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            @"INSERT INTO ""dbo"".""Ducks"" (""Name"", ""Quacks"", ""ConcurrencyToken"")
VALUES (@p0, @p1, @p2)
RETURNING ""Id"";
",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            @"INSERT INTO ""dbo"".""Ducks""
DEFAULT VALUES
RETURNING ""Id"", ""Computed"";
",
            stringBuilder.ToString());

    protected override void AppendInsertOperation_for_only_single_identity_columns_verification(
        StringBuilder stringBuilder)
        => AssertBaseline(
            @"INSERT INTO ""dbo"".""Ducks""
DEFAULT VALUES
RETURNING ""Id"";
",
            stringBuilder.ToString());

    protected override void AppendUpdateOperation_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            @"UPDATE ""dbo"".""Ducks"" SET ""Name"" = @p0, ""Quacks"" = @p1, ""ConcurrencyToken"" = @p2
WHERE ""Id"" = @p3 AND ""ConcurrencyToken"" IS NULL
RETURNING ""Computed"";
",
            stringBuilder.ToString());

    protected override void AppendUpdateOperation_if_store_generated_columns_dont_exist_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            @"UPDATE ""dbo"".""Ducks"" SET ""Name"" = @p0, ""Quacks"" = @p1, ""ConcurrencyToken"" = @p2
WHERE ""Id"" = @p3
RETURNING 1;
",
            stringBuilder.ToString());

    protected override void AppendUpdateOperation_appends_where_for_concurrency_token_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            @"UPDATE ""dbo"".""Ducks"" SET ""Name"" = @p0, ""Quacks"" = @p1, ""ConcurrencyToken"" = @p2
WHERE ""Id"" = @p3 AND ""ConcurrencyToken"" IS NULL
RETURNING 1;
",
            stringBuilder.ToString());

    protected override void AppendUpdateOperation_for_computed_property_verification(StringBuilder stringBuilder)
        => AssertBaseline(
            @"UPDATE ""dbo"".""Ducks"" SET ""Name"" = @p0, ""Quacks"" = @p1, ""ConcurrencyToken"" = @p2
WHERE ""Id"" = @p3
RETURNING ""Computed"";
",
            stringBuilder.ToString());

    protected override TestHelpers TestHelpers
        => FakeRelationalTestHelpers.Instance;

    protected override string RowsAffected
        => "provider_specific_rowcount()";

    protected override string Identity
        => "provider_specific_identity()";

    private void AssertBaseline(string expected, string actual)
        => Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
}
