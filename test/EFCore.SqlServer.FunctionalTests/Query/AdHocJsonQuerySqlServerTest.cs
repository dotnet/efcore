// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocJsonQuerySqlServerTest(NonSharedFixture fixture) : AdHocJsonQuerySqlServerTestBase(fixture)
{
    public override async Task Read_enum_property_with_legacy_values(bool async)
    {
        var exception = await Assert.ThrowsAsync<SqlException>(() => base.Read_enum_property_with_legacy_values_core(async));

        // When using legacy nvarchar(max) to store JSON, we add a CAST() node to convert the text coming out of JSON_VALUE()
        // to the appropriate type (int in this case); if the format isn't correct as here (text instead of int),
        // we get: Conversion failed when converting the nvarchar value '...' to data type int
        Assert.Equal(245, exception.Number);
    }
}
