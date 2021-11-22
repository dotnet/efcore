// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Update;

public class SqliteUpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
{
    protected override IUpdateSqlGenerator CreateSqlGenerator()
        => new SqliteUpdateSqlGenerator(
            new UpdateSqlGeneratorDependencies(
                new SqliteSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                TestServiceFactory.Instance.Create<SqliteTypeMappingSource>()));

    protected override TestHelpers TestHelpers
        => SqliteTestHelpers.Instance;

    protected override string RowsAffected
        => "changes()";

    protected override string Schema
        => null;

    protected override string GetIdentityWhereCondition(string columnName)
        => OpenDelimiter + "rowid" + CloseDelimiter + " = last_insert_rowid()";

    public override void GenerateNextSequenceValueOperation_correctly_handles_schemas()
    {
        var ex = Assert.Throws<NotSupportedException>(() => base.GenerateNextSequenceValueOperation_correctly_handles_schemas());
        Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
    }

    public override void GenerateNextSequenceValueOperation_returns_statement_with_sanitized_sequence()
    {
        var ex = Assert.Throws<NotSupportedException>(
            () => base.GenerateNextSequenceValueOperation_returns_statement_with_sanitized_sequence());
        Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
    }
}
