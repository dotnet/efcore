// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonMiscellaneousSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonMiscellaneousRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate], '$.Int') AS int) = 8
""");
        }
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[OptionalAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[OptionalAssociate], '$.Int') AS int) = 8
""");
        }
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate], '$.RequiredNestedAssociate.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate], '$.RequiredNestedAssociate.Int') AS int) = 8
""");
        }
    }

    #endregion Simple filters

    #region Value types

    public override async Task Where_property_on_non_nullable_value_type()
    {
        await base.Where_property_on_non_nullable_value_type();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE JSON_VALUE([v].[RequiredAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE CAST(JSON_VALUE([v].[RequiredAssociate], '$.Int') AS int) = 8
""");
        }
    }

    public override async Task Where_property_on_nullable_value_type_Value()
    {
        await base.Where_property_on_nullable_value_type_Value();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE JSON_VALUE([v].[OptionalAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE CAST(JSON_VALUE([v].[OptionalAssociate], '$.Int') AS int) = 8
""");
        }
    }

    public override async Task Where_HasValue_on_nullable_value_type()
    {
        await base.Where_HasValue_on_nullable_value_type();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalAssociate] IS NOT NULL
""");
    }

    #endregion Value types

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate], [m].[RequiredAssociate]
FROM (
    SELECT * FROM [RootEntity]
) AS [m]
""");
    }

    public override async Task FromSql_on_root_composed(bool async)
    {
        await base.FromSql_on_root_composed(async);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate], [m].[RequiredAssociate]
FROM (
    SELECT * FROM "RootEntity"
) AS [m]
WHERE JSON_VALUE([m].[RequiredAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate], [m].[RequiredAssociate]
FROM (
    SELECT * FROM "RootEntity"
) AS [m]
WHERE CAST(JSON_VALUE([m].[RequiredAssociate], '$.Int') AS int) = 8
""");
        }
    }

    #region Left join

    public override async Task Where_on_associate_scalar_property_over_left_join()
    {
        await base.Where_on_associate_scalar_property_over_left_join();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE JSON_VALUE([r0].[RequiredAssociate], '$.Int' RETURNING int) <> 8 OR JSON_VALUE([r0].[RequiredAssociate], '$.Int' RETURNING int) IS NULL
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE CAST(JSON_VALUE([r0].[RequiredAssociate], '$.Int') AS int) <> 8 OR CAST(JSON_VALUE([r0].[RequiredAssociate], '$.Int') AS int) IS NULL
""");
        }
    }

    public override async Task Where_on_nested_associate_scalar_property_over_left_join()
    {
        await base.Where_on_nested_associate_scalar_property_over_left_join();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE JSON_VALUE([r0].[RequiredAssociate], '$.RequiredNestedAssociate.Int' RETURNING int) <> 8 OR JSON_VALUE([r0].[RequiredAssociate], '$.RequiredNestedAssociate.Int' RETURNING int) IS NULL
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE CAST(JSON_VALUE([r0].[RequiredAssociate], '$.RequiredNestedAssociate.Int') AS int) <> 8 OR CAST(JSON_VALUE([r0].[RequiredAssociate], '$.RequiredNestedAssociate.Int') AS int) IS NULL
""");
        }
    }

    public override async Task Where_on_associate_scalar_property_over_left_join_subquery()
    {
        await base.Where_on_associate_scalar_property_over_left_join_subquery();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@p='5'

SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT TOP(@p) [r0].[Id], [r0].[RequiredAssociate] AS [c1]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1] ON [r].[Id] + 1 = [r1].[Id]
WHERE JSON_VALUE([r1].[c1], '$.Int' RETURNING int) <> 8 OR JSON_VALUE([r1].[c1], '$.Int' RETURNING int) IS NULL
""");
        }
        else
        {
            AssertSql(
                """
@p='5'

SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT TOP(@p) [r0].[Id], [r0].[RequiredAssociate] AS [c1]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1] ON [r].[Id] + 1 = [r1].[Id]
WHERE CAST(JSON_VALUE([r1].[c1], '$.Int') AS int) <> 8 OR CAST(JSON_VALUE([r1].[c1], '$.Int') AS int) IS NULL
""");
        }
    }

    #endregion Left join

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
