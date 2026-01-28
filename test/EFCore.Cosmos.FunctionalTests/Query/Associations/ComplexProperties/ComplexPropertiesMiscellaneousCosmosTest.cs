// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public class ComplexPropertiesMiscellaneousCosmosTest
    : ComplexPropertiesMiscellaneousTestBase<ComplexPropertiesCosmosFixture>
{
    public ComplexPropertiesMiscellaneousCosmosTest(ComplexPropertiesCosmosFixture fixture, ITestOutputHelper outputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(outputHelper);
    }

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["Int"] = 8)
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["OptionalAssociate"]["Int"] = 8)
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["RequiredNestedAssociate"]["Int"] = 8)
""");
    }

    #region Value types

    public override async Task Where_property_on_non_nullable_value_type()
    {
        await base.Where_property_on_non_nullable_value_type();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["Int"] = 8)
""");
    }

    public override async Task Where_property_on_nullable_value_type_Value()
    {
        await base.Where_property_on_nullable_value_type_Value();

        AssertSql("""
SELECT VALUE c
FROM root c
WHERE (c["OptionalAssociate"]["Int"] = 8)
""");
    }

    public override async Task Where_HasValue_on_nullable_value_type()
    {
        // @TODO: Structural equality.
        await base.Where_HasValue_on_nullable_value_type();

        AssertSql("""
SELECT VALUE c
FROM root c
WHERE (c["OptionalAssociate"] != null)
""");
        //var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_HasValue_on_nullable_value_type());
        //Assert.Equal(CoreStrings.EntityEqualityOnKeylessEntityNotSupported("!=", "ValueRootEntity.OptionalAssociate#ValueAssociateType"), ex.Message);
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
