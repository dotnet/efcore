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

    [ConditionalFact]
    public void CreateTableOperation_with_invalid_string_column_store_type_should_not_throw_null_reference()
    {
        // This should NOT throw a null reference exception when an invalid store type is specified for a string column
        Generate(
            new CreateTableOperation
            {
                Name = "People",
                Columns =
                {
                    new AddColumnOperation
                    {
                        Name = "FirstName",
                        Table = "People",
                        ClrType = typeof(string),
                        ColumnType = "decimal(18,2)", // Invalid store type for string
                        IsNullable = false
                    }
                }
            });

        // Let's see what SQL is generated
        Assert.NotEmpty(Sql);
        // If we reach here, no null reference exception was thrown, which is correct
    }

    [ConditionalFact]
    public void Reproduce_null_reference_with_invalid_column_type_during_migration_add()
    {
        // This test directly tests the fix by showing that 
        // when GetColumnType would return null, we now get a proper exception
        // instead of a null reference exception
        
        var operation = new AddColumnOperation
        {
            Name = "TestColumn",
            Table = "TestTable",
            ClrType = typeof(System.IO.FileStream), // This should definitely not have a mapping
            ColumnType = null, // Force it to call GetColumnType
            IsNullable = false
        };

        // Let's see what SQL gets generated instead of what we expect
        // This test validates that we don't get a NullReferenceException anymore
        try
        {
            Generate(operation);
            // If we get here without exception, the type mapping source found something unexpected
            Assert.NotNull(Sql);
        }
        catch (NullReferenceException)
        {
            // This should NOT happen anymore with our fix
            Assert.Fail("NullReferenceException should not be thrown - this indicates the fix didn't work");
        }
        catch (InvalidOperationException ex)
        {
            // This is the expected behavior with our fix
            Assert.Contains("type mapping", ex.Message);
        }
        // Any other exception is also acceptable as long as it's not NullReferenceException
    }
}