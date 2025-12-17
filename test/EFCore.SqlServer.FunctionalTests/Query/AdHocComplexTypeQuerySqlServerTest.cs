// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocComplexTypeQuerySqlServerTest(NonSharedFixture fixture) : AdHocComplexTypeQueryRelationalTestBase(fixture)
{
    public override async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        await base.Complex_type_equals_parameter_with_nested_types_with_property_of_same_name();

        AssertSql(
            """
@entity_equality_container_Id='1' (Nullable = true)
@entity_equality_container_Containee1_Id='2' (Nullable = true)
@entity_equality_container_Containee2_Id='3' (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ComplexContainer_Id], [e].[ComplexContainer_Containee1_Id], [e].[ComplexContainer_Containee2_Id]
FROM [EntityType] AS [e]
WHERE [e].[ComplexContainer_Id] = @entity_equality_container_Id AND [e].[ComplexContainer_Containee1_Id] = @entity_equality_container_Containee1_Id AND [e].[ComplexContainer_Containee2_Id] = @entity_equality_container_Containee2_Id
""");
    }

    public override async Task Projecting_complex_property_does_not_auto_include_owned_types()
    {
        await base.Projecting_complex_property_does_not_auto_include_owned_types();

        AssertSql(
            """
SELECT [e].[Complex_Name], [e].[Complex_Number]
FROM [EntityType] AS [e]
""");
    }

    #region 36837

    [ConditionalFact]
    public virtual async Task Complex_type_equality_with_non_default_type_mapping()
    {
        var contextFactory = await InitializeAsync<Context36837>(
            seed: context =>
            {
                context.AddRange(
                    new Context36837.EntityType
                    {
                        ComplexThing = new Context36837.ComplexThing { DateTime = new DateTime(2020, 1, 1) }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var count = await context.Set<Context36837.EntityType>()
            .CountAsync(b => b.ComplexThing == new Context36837.ComplexThing { DateTime = new DateTime(2020, 1, 1, 1, 1, 1, 999, 999) });
        Assert.Equal(0, count);

        AssertSql(
            """
SELECT COUNT(*)
FROM [EntityType] AS [e]
WHERE [e].[ComplexThing_DateTime] = '2020-01-01T01:01:01.999'
""");
    }

    private class Context36837(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>().ComplexProperty(b => b.ComplexThing);

        public class EntityType
        {
            public int Id { get; set; }
            public ComplexThing ComplexThing { get; set; } = null!;
        }

        public class ComplexThing
        {
            [Column(TypeName = "datetime")] // Non-default type mapping
            public DateTime DateTime { get; set; }
        }
    }

    #endregion 36837

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
