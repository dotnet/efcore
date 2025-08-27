// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Migrations;

public class SqlServerMigrationsSqlGeneratorNullReferenceTest : MigrationsSqlGeneratorTestBase
{
    public SqlServerMigrationsSqlGeneratorNullReferenceTest() 
        : base(SqlServerTestHelpers.Instance)
    {
    }

    protected override string GetGeometryCollectionStoreType()
        => "geography";
}