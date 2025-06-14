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

        AssertSql("""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE JSON_TYPE('{"Name": "Test"}', '$.Name') IS NOT NULL
""");
    }

    public override async Task JsonExists_With_StringJsonProperty(bool async)
    {
        await base.JsonExists_With_StringJsonProperty(async);

        AssertSql("""
SELECT "j"."Id", "j"."CollectionRoot", "j"."Name", "j"."ReferenceRoot", "j"."StringJsonValue"
FROM "JsonEntitiesStringConversion" AS "j"
WHERE CASE
    WHEN "j"."StringJsonValue" IS NOT NULL THEN JSON_TYPE("j"."StringJsonValue", '$.Name') IS NOT NULL
END
""");
    }

    public override async Task JsonExists_With_StringConversionJsonProperty(bool async)
    {
        await base.JsonExists_With_StringConversionJsonProperty(async);

        AssertSql("""
SELECT "j"."Id", "j"."CollectionRoot", "j"."Name", "j"."ReferenceRoot", "j"."StringJsonValue"
FROM "JsonEntitiesStringConversion" AS "j"
WHERE CASE
    WHEN "j"."ReferenceRoot" IS NOT NULL THEN JSON_TYPE("j"."ReferenceRoot", '$.Name') IS NOT NULL
END
""");
    }

    public override async Task JsonExists_With_OwnedJsonProperty(bool async)
    {
        await base.JsonExists_With_OwnedJsonProperty(async);
        // TODO: AssertSql

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
