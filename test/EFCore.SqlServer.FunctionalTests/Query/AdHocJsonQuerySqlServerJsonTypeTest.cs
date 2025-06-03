// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

[SqlServerCondition(SqlServerCondition.SupportsJsonType)]
public class AdHocJsonQuerySqlServerJsonTypeTest(NonSharedFixture fixture) : AdHocJsonQuerySqlServerTestBase(fixture)
{
    public override async Task Missing_navigation_works_with_deduplication(bool async)
    {
        // TODO:SQLJSON Returns empty (invalid) JSON (See BadJson.cs)
        if (async)
        {
            Assert.Equal(
                "Unable to cast object of type 'System.DBNull' to type 'System.String'.",
                (await Assert.ThrowsAsync<InvalidCastException>(() => base.Missing_navigation_works_with_deduplication(true))).Message);
        }
        else
        {
            Assert.Equal(
                RelationalStrings.JsonEmptyString,
                (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Missing_navigation_works_with_deduplication(false)))
                .Message);
        }
    }

    public override async Task Contains_on_nested_collection_with_init_only_navigation()
        // TODO:SQLJSON (See JsonTypeToFunction.cs)
        => Assert.Equal(
            "OpenJson support not yet supported for JSON native data type.",
            (await Assert.ThrowsAsync<SqlException>(
                () => base.Contains_on_nested_collection_with_init_only_navigation())).Message);

    protected override string StoreName
        => "AdHocJsonQueryJsonTypeTest";

    protected override string JsonColumnType
        => "json";
}
