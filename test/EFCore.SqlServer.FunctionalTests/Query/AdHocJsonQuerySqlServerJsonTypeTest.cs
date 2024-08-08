// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocJsonQuerySqlServerJsonTypeTest : AdHocJsonQuerySqlServerTestBase
{
    public override async Task Contains_on_nested_collection_with_init_only_navigation(bool async)
        // TODO:SQLJSON (See JsonTypeToFunction.cs)
        => Assert.Equal(
            "OpenJson support not yet supported for JSON native data type.",
            (await Assert.ThrowsAsync<SqlException>(
                () => base.Contains_on_nested_collection_with_init_only_navigation(async))).Message);

    protected override string StoreName
        => "AdHocJsonQueryJsonTypeTest";

    protected override string JsonColumnType
        => "json";
}
