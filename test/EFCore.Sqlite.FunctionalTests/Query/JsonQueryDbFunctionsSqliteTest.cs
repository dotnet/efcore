// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class JsonQueryDbFunctionsSqliteTest : JsonQueryDbFunctionsRelationalTestBase<JsonQuerySqliteFixture>
{
    public JsonQueryDbFunctionsSqliteTest(JsonQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }


    public override async Task JsonExists_With_ConstantValue(bool async)
    {
        await base.JsonExists_With_ConstantValue(async);
        // TODO: AssertSql
    }

    public override async Task JsonExists_With_StringJsonProperty(bool async)
    {
        await base.JsonExists_With_StringConversionJsonProperty(async);
        // TODO: AssertSql
    }

    public override async Task JsonExists_With_StringConversionJsonProperty(bool async)
    {
        await base.JsonExists_With_StringConversionJsonProperty(async);
        // TODO: AssertSql
    }

    public override async Task JsonExists_With_OwnedJsonProperty(bool async)
    {
        await base.JsonExists_With_OwnedJsonProperty(async);
        // TODO: AssertSql
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
