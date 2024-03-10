// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class FindSqlServerTest : FindTestBase<FindSqlServerTest.FindSqlServerFixture>
{
    protected FindSqlServerTest(FindSqlServerFixture fixture)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

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

SELECT TOP(1) [i].[Id], [i].[Foo]
FROM [IntKey] AS [i]
WHERE [i].[Id] = @__p_0
""");
    }

    public override void Returns_null_for_int_key_not_in_store()
    {
        base.Returns_null_for_int_key_not_in_store();

        AssertSql(
            """
@__p_0='99'

SELECT TOP(1) [i].[Id], [i].[Foo]
FROM [IntKey] AS [i]
WHERE [i].[Id] = @__p_0
""");
    }

    public override void Find_nullable_int_key_tracked()
    {
        base.Find_int_key_tracked();

        Assert.Equal("", Sql);
    }

    public override void Find_nullable_int_key_from_store()
    {
        base.Find_int_key_from_store();

        AssertSql(
            """
@__p_0='77'

SELECT TOP(1) [i].[Id], [i].[Foo]
FROM [IntKey] AS [i]
WHERE [i].[Id] = @__p_0
""");
    }

    public override void Returns_null_for_nullable_int_key_not_in_store()
    {
        base.Returns_null_for_int_key_not_in_store();

        AssertSql(
            """
@__p_0='99'

SELECT TOP(1) [i].[Id], [i].[Foo]
FROM [IntKey] AS [i]
WHERE [i].[Id] = @__p_0
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
