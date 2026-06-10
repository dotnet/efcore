// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class FindSqlServerTest : FindTestBase<FindSqlServerTest.FindSqlServerFixture>
{
    protected FindSqlServerTest(FindSqlServerFixture fixture)
        : base(fixture)
        => fixture.TestSqlLoggerFactory.Clear();

    public class FindSqlServerTestSet(FindSqlServerFixture fixture) : FindSqlServerTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindSqlServerTestContext(FindSqlServerFixture fixture) : FindSqlServerTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindSqlServerTestNonGeneric(FindSqlServerFixture fixture) : FindSqlServerTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public override void Find_int_key_tracked()
    {
        base.Find_int_key_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_int_key_from_store()
    {
        base.Find_int_key_from_store();

        AssertSql(
            """
@__p_0='77'

SELECT [i3].[Id], [i3].[Foo], [s].[IntKeyId], [s].[Id], [s].[Prop], [s].[NestedOwned_Prop], [s].[Owned1IntKeyId], [s].[Owned1Id], [s].[Id0], [s].[Prop0], [i3].[OwnedReference_Prop], [i3].[OwnedReference_NestedOwned_Prop], [i2].[Owned1IntKeyId], [i2].[Id], [i2].[Prop]
FROM (
    SELECT TOP(1) [i].[Id], [i].[Foo], [i].[OwnedReference_Prop], [i].[OwnedReference_NestedOwned_Prop]
    FROM [IntKey] AS [i]
    WHERE [i].[Id] = @__p_0
) AS [i3]
LEFT JOIN (
    SELECT [i0].[IntKeyId], [i0].[Id], [i0].[Prop], [i0].[NestedOwned_Prop], [i1].[Owned1IntKeyId], [i1].[Owned1Id], [i1].[Id] AS [Id0], [i1].[Prop] AS [Prop0]
    FROM [IntKey_OwnedCollection] AS [i0]
    LEFT JOIN [IntKey_OwnedCollection_NestedOwnedCollection] AS [i1] ON [i0].[IntKeyId] = [i1].[Owned1IntKeyId] AND [i0].[Id] = [i1].[Owned1Id]
) AS [s] ON [i3].[Id] = [s].[IntKeyId]
LEFT JOIN [IntKey_NestedOwnedCollection] AS [i2] ON CASE
    WHEN [i3].[OwnedReference_Prop] IS NOT NULL THEN [i3].[Id]
END = [i2].[Owned1IntKeyId]
ORDER BY [i3].[Id], [s].[IntKeyId], [s].[Id], [s].[Owned1IntKeyId], [s].[Owned1Id], [s].[Id0], [i2].[Owned1IntKeyId]
""");
    }

    public override void Returns_null_for_int_key_not_in_store()
    {
        base.Returns_null_for_int_key_not_in_store();

        AssertSql(
            """
@__p_0='99'

SELECT [i3].[Id], [i3].[Foo], [s].[IntKeyId], [s].[Id], [s].[Prop], [s].[NestedOwned_Prop], [s].[Owned1IntKeyId], [s].[Owned1Id], [s].[Id0], [s].[Prop0], [i3].[OwnedReference_Prop], [i3].[OwnedReference_NestedOwned_Prop], [i2].[Owned1IntKeyId], [i2].[Id], [i2].[Prop]
FROM (
    SELECT TOP(1) [i].[Id], [i].[Foo], [i].[OwnedReference_Prop], [i].[OwnedReference_NestedOwned_Prop]
    FROM [IntKey] AS [i]
    WHERE [i].[Id] = @__p_0
) AS [i3]
LEFT JOIN (
    SELECT [i0].[IntKeyId], [i0].[Id], [i0].[Prop], [i0].[NestedOwned_Prop], [i1].[Owned1IntKeyId], [i1].[Owned1Id], [i1].[Id] AS [Id0], [i1].[Prop] AS [Prop0]
    FROM [IntKey_OwnedCollection] AS [i0]
    LEFT JOIN [IntKey_OwnedCollection_NestedOwnedCollection] AS [i1] ON [i0].[IntKeyId] = [i1].[Owned1IntKeyId] AND [i0].[Id] = [i1].[Owned1Id]
) AS [s] ON [i3].[Id] = [s].[IntKeyId]
LEFT JOIN [IntKey_NestedOwnedCollection] AS [i2] ON CASE
    WHEN [i3].[OwnedReference_Prop] IS NOT NULL THEN [i3].[Id]
END = [i2].[Owned1IntKeyId]
ORDER BY [i3].[Id], [s].[IntKeyId], [s].[Id], [s].[Owned1IntKeyId], [s].[Owned1Id], [s].[Id0], [i2].[Owned1IntKeyId]
""");
    }

    public override void Find_nullable_int_key_tracked()
    {
        base.Find_int_key_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_nullable_int_key_from_store()
    {
        base.Find_nullable_int_key_from_store();

        AssertSql(
            """
@__p_0='77' (Nullable = true)

SELECT TOP(1) [n].[Id], [n].[Foo]
FROM [NullableIntKey] AS [n]
WHERE [n].[Id] = @__p_0
""");
    }

    public override void Returns_null_for_nullable_int_key_not_in_store()
    {
        base.Returns_null_for_nullable_int_key_not_in_store();

        AssertSql(
            """
@__p_0='99' (Nullable = true)

SELECT TOP(1) [n].[Id], [n].[Foo]
FROM [NullableIntKey] AS [n]
WHERE [n].[Id] = @__p_0
""");
    }

    public override void Find_string_key_tracked()
    {
        base.Find_string_key_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_string_key_from_store()
    {
        base.Find_string_key_from_store();

        AssertSql(
            """
@__p_0='Cat' (Size = 450)

SELECT TOP(1) [s].[Id], [s].[Foo]
FROM [StringKey] AS [s]
WHERE [s].[Id] = @__p_0
""");
    }

    public override void Returns_null_for_string_key_not_in_store()
    {
        base.Returns_null_for_string_key_not_in_store();

        AssertSql(
            """
@__p_0='Fox' (Size = 450)

SELECT TOP(1) [s].[Id], [s].[Foo]
FROM [StringKey] AS [s]
WHERE [s].[Id] = @__p_0
""");
    }

    public override void Find_composite_key_tracked()
    {
        base.Find_composite_key_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_composite_key_from_store()
    {
        base.Find_composite_key_from_store();

        AssertSql(
            """
@__p_0='77'
@__p_1='Dog' (Size = 450)

SELECT TOP(1) [c].[Id1], [c].[Id2], [c].[Foo]
FROM [CompositeKey] AS [c]
WHERE [c].[Id1] = @__p_0 AND [c].[Id2] = @__p_1
""");
    }

    public override void Returns_null_for_composite_key_not_in_store()
    {
        base.Returns_null_for_composite_key_not_in_store();

        AssertSql(
            """
@__p_0='77'
@__p_1='Fox' (Size = 450)

SELECT TOP(1) [c].[Id1], [c].[Id2], [c].[Foo]
FROM [CompositeKey] AS [c]
WHERE [c].[Id1] = @__p_0 AND [c].[Id2] = @__p_1
""");
    }

    public override void Find_base_type_tracked()
    {
        base.Find_base_type_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_base_type_from_store()
    {
        base.Find_base_type_from_store();

        AssertSql(
            """
@__p_0='77'

SELECT TOP(1) [b].[Id], [b].[Discriminator], [b].[Foo], [b].[Boo]
FROM [BaseType] AS [b]
WHERE [b].[Id] = @__p_0
""");
    }

    public override void Returns_null_for_base_type_not_in_store()
    {
        base.Returns_null_for_base_type_not_in_store();

        AssertSql(
            """
@__p_0='99'

SELECT TOP(1) [b].[Id], [b].[Discriminator], [b].[Foo], [b].[Boo]
FROM [BaseType] AS [b]
WHERE [b].[Id] = @__p_0
""");
    }

    public override void Find_derived_type_tracked()
    {
        base.Find_derived_type_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_derived_type_from_store()
    {
        base.Find_derived_type_from_store();

        AssertSql(
            """
@__p_0='78'

SELECT TOP(1) [b].[Id], [b].[Discriminator], [b].[Foo], [b].[Boo]
FROM [BaseType] AS [b]
WHERE [b].[Discriminator] = N'DerivedType' AND [b].[Id] = @__p_0
""");
    }

    public override void Returns_null_for_derived_type_not_in_store()
    {
        base.Returns_null_for_derived_type_not_in_store();

        AssertSql(
            """
@__p_0='99'

SELECT TOP(1) [b].[Id], [b].[Discriminator], [b].[Foo], [b].[Boo]
FROM [BaseType] AS [b]
WHERE [b].[Discriminator] = N'DerivedType' AND [b].[Id] = @__p_0
""");
    }

    public override void Find_base_type_using_derived_set_tracked()
    {
        base.Find_base_type_using_derived_set_tracked();

        AssertSql(
            """
@__p_0='88'

SELECT TOP(1) [b].[Id], [b].[Discriminator], [b].[Foo], [b].[Boo]
FROM [BaseType] AS [b]
WHERE [b].[Discriminator] = N'DerivedType' AND [b].[Id] = @__p_0
""");
    }

    public override void Find_base_type_using_derived_set_from_store()
    {
        base.Find_base_type_using_derived_set_from_store();

        AssertSql(
            """
@__p_0='77'

SELECT TOP(1) [b].[Id], [b].[Discriminator], [b].[Foo], [b].[Boo]
FROM [BaseType] AS [b]
WHERE [b].[Discriminator] = N'DerivedType' AND [b].[Id] = @__p_0
""");
    }

    public override void Find_derived_type_using_base_set_tracked()
    {
        base.Find_derived_type_using_base_set_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_derived_using_base_set_type_from_store()
    {
        base.Find_derived_using_base_set_type_from_store();

        AssertSql(
            """
@__p_0='78'

SELECT TOP(1) [b].[Id], [b].[Discriminator], [b].[Foo], [b].[Boo]
FROM [BaseType] AS [b]
WHERE [b].[Id] = @__p_0
""");
    }

    public override void Find_shadow_key_tracked()
    {
        base.Find_shadow_key_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_shadow_key_from_store()
    {
        base.Find_shadow_key_from_store();

        AssertSql(
            """
@__p_0='77'

SELECT TOP(1) [s].[Id], [s].[Foo]
FROM [ShadowKey] AS [s]
WHERE [s].[Id] = @__p_0
""");
    }

    public override void Returns_null_for_shadow_key_not_in_store()
    {
        base.Returns_null_for_shadow_key_not_in_store();

        AssertSql(
            """
@__p_0='99'

SELECT TOP(1) [s].[Id], [s].[Foo]
FROM [ShadowKey] AS [s]
WHERE [s].[Id] = @__p_0
""");
    }

    private string Sql
        => Fixture.TestSqlLoggerFactory.Sql;

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FindSqlServerFixture : FindFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
