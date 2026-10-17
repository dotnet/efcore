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

    // When using the SQL Server json type, we use JSON_VALUE() with the RETURNING clause to parse the JSON property value
    // into the correct type (int in this case); since we (currently) use lax mode, if the format isn't correct as here
    // (text instead of int), we get NULL out (strict mode would throw, see #36802 about possibly switching to that).
    // And since the enum properties are required in the model, this causes "Nullable object must have a value" to be thrown.
    public override Task Read_enum_property_with_legacy_values(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Read_enum_property_with_legacy_values_core(async));

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public Task Json_collection_Any_with_nested_scalar_predicate_AzureSql(bool complex, bool async)
        => Json_collection_Any_with_nested_scalar_predicate(complex, azureSql: true, async);

    [ConditionalTheory, SqlServerCondition(SqlServerCondition.IsNotAzureSql)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public Task Json_collection_Any_with_nested_scalar_predicate_SqlServer(bool complex, bool async)
        => Json_collection_Any_with_nested_scalar_predicate(complex, azureSql: false, async);

    private async Task Json_collection_Any_with_nested_scalar_predicate(bool complex, bool azureSql, bool async)
    {
        var contextFactory = await InitializeAsync<Context39277>(
            onConfiguring: b => b.UseSqlServerCompatibilityLevel(170),
            onModelCreating: modelBuilder =>
            {
                var entity = modelBuilder.Entity<Context39277.Entity>();
                entity.Property(e => e.Id).ValueGeneratedNever();

                if (complex)
                {
                    entity.ComplexProperty(e => e.Document, document =>
                    {
                        document.ToJson().HasColumnType("json");
                        document.ComplexCollection(d => d.Entries, entry => entry.ComplexProperty(e => e.Metadata));
                    });
                }
                else
                {
                    entity.OwnsOne(e => e.Document, document =>
                    {
                        document.ToJson().HasColumnType("json");
                        document.OwnsMany(d => d.Entries, entry => entry.OwnsOne(e => e.Metadata));
                    });
                }
            },
            seed: context =>
            {
                context.AddRange(
                    new Context39277.Entity
                    {
                        Id = 1,
                        Document = new()
                        {
                            Entries = [new() { Metadata = new() { Name = new string('x', 4001) + "Driver", Number = 7 } }]
                        }
                    },
                    new Context39277.Entity
                    {
                        Id = 2,
                        Document = new() { Entries = [new() { Metadata = new() { Name = "Driver", Number = 8 } }] }
                    },
                    new Context39277.Entity
                    {
                        Id = 3,
                        Document = new() { Entries = [new() { Metadata = new() { Name = "Passenger", Number = 7 } }] }
                    });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();
        var optionsBuilder = new DbContextOptionsBuilder()
            .UseModel(context.Model)
            .UseLoggerFactory(TestSqlLoggerFactory);

        if (azureSql)
        {
            optionsBuilder.UseAzureSql(context.Database.GetDbConnection(), b => b.UseCompatibilityLevel(170));
        }
        else
        {
            optionsBuilder.UseSqlServer(context.Database.GetDbConnection(), b => b.UseCompatibilityLevel(170));
        }

        await using var queryContext = new Context39277(optionsBuilder.Options);
        var query = queryContext.Set<Context39277.Entity>()
            .Where(e => e.Document.Entries.Any(
                entry => entry.Metadata.Name.ToLower().Contains("driver") && entry.Metadata.Number == 7))
            .Select(e => e.Id);

        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal([1], result);

        AssertSql(
            azureSql
                ? """
SELECT [e].[Id]
FROM [Entity] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([e].[Document], '$.Entries') WITH ([Metadata] nvarchar(max) '$.Metadata' AS JSON) AS [e0]
    WHERE LOWER(JSON_VALUE(CAST([e0].[Metadata] AS json), '$.Name' RETURNING nvarchar(max))) LIKE N'%driver%' AND JSON_VALUE(CAST([e0].[Metadata] AS json), '$.Number' RETURNING int) = 7)
"""
                : """
SELECT [e].[Id]
FROM [Entity] AS [e]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([e].[Document], '$.Entries') WITH ([Metadata] json '$.Metadata' AS JSON) AS [e0]
    WHERE LOWER(JSON_VALUE([e0].[Metadata], '$.Name' RETURNING nvarchar(max))) LIKE N'%driver%' AND JSON_VALUE([e0].[Metadata], '$.Number' RETURNING int) = 7)
""");
    }

    protected class Context39277(DbContextOptions options) : DbContext(options)
    {
        public class Entity
        {
            public int Id { get; set; }
            public Document Document { get; set; }
        }

        public class Document
        {
            public List<JsonEntry> Entries { get; set; }
        }

        public class JsonEntry
        {
            public Metadata Metadata { get; set; }
        }

        public class Metadata
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }
    }

    protected override string StoreName
        => "AdHocJsonQueryJsonTypeTest";

    protected override string JsonColumnType
        => "json";
}
