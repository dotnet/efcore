// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.Data.SqlClient;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

[SqlServerCondition(SqlServerCondition.SupportsJsonType)]
public class AdHocJsonQuerySqlServerJsonTypeTest(NonSharedFixture fixture) : AdHocJsonQuerySqlServerTestBase(fixture)
{
    #region BadJsonProperties

    // When using the SQL Server JSON data type, insertion of the bad data fails thanks to SQL Server validation,
    // unlike with tests mapping to nvarchar(max) where the bad JSON data is inserted correctly and then read.

    public override Task Bad_json_properties_duplicated_navigations(bool noTracking)
        => Task.CompletedTask;

    public override Task Bad_json_properties_duplicated_scalars(bool noTracking)
        => Task.CompletedTask;

    public override Task Bad_json_properties_empty_navigations(bool noTracking)
        => Task.CompletedTask;

    public override Task Bad_json_properties_empty_scalars(bool noTracking)
        => Task.CompletedTask;

    public override Task Bad_json_properties_null_navigations(bool noTracking)
        => Task.CompletedTask;

    public override Task Bad_json_properties_null_scalars(bool noTracking)
        => Task.CompletedTask;

    #endregion BadJsonProperties

    // SQL Server 2025 (CTP 2.1) does not support casting JSON scalar strings to json
    // (CAST('8' AS json) and CAST('null' AS json) fail with "JSON text is not properly formatted").
    public override Task Project_entity_with_json_null_values()
        => Assert.ThrowsAsync<SqlException>(base.Project_entity_with_json_null_values);

    // SQL Server 2025 (CTP 2.1) does not support casting JSON scalar strings to json
    // (CAST('8' AS json) and CAST('null' AS json) fail with "JSON text is not properly formatted").
    // The base implementation expects a different exception.
    public override Task Try_project_collection_but_JSON_is_entity()
        => Assert.ThrowsAsync<ThrowsException>(base.Try_project_collection_but_JSON_is_entity);

    // SQL Server 2025 (CTP 2.1) does not support casting JSON scalar strings to json
    // (CAST('8' AS json) and CAST('null' AS json) fail with "JSON text is not properly formatted").
    // The base implementation expects a different exception.
    public override Task Try_project_reference_but_JSON_is_collection()
        => Assert.ThrowsAsync<ThrowsException>(base.Try_project_reference_but_JSON_is_collection);

    protected override string StoreName
        => "AdHocJsonQueryJsonTypeTest";

    protected override string JsonColumnType
        => "json";
}
