// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class EnumTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    #region Equality

    [ConditionalFact]
    public virtual Task Equality_to_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Enum == BasicEnum.One));

    [ConditionalFact]
    public virtual Task Equality_to_parameter()
    {
        var basicEnum = BasicEnum.One;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Enum == basicEnum));
    }

    [ConditionalFact]
    public virtual Task Equality_nullable_enum_to_constant()
        => AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == BasicEnum.One));

    [ConditionalFact]
    public virtual Task Equality_nullable_enum_to_parameter()
    {
        var basicEnum = BasicEnum.One;

        return AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == basicEnum));
    }

    [ConditionalFact]
    public virtual Task Equality_nullable_enum_to_null_constant()
        => AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == null));

    [ConditionalFact]
    public virtual Task Equality_nullable_enum_to_null_parameter()
    {
        BasicEnum? basicEnum = null;

        return AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == basicEnum));
    }

    [ConditionalFact]
    public virtual Task Equality_nullable_enum_to_nullable_parameter()
    {
        BasicEnum? basicEnum = BasicEnum.One;

        return AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == basicEnum));
    }

    #endregion Equality

    [ConditionalFact]
    public virtual async Task Bitwise_and_enum_constant()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(g => (g.FlagsEnum & BasicFlagsEnum.One) > 0));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(g => (g.FlagsEnum & BasicFlagsEnum.One) == BasicFlagsEnum.One));
    }

    [ConditionalFact]
    public virtual async Task Bitwise_and_integral_constant()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(g => ((int)g.FlagsEnum & 8) == 8));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(g => ((long)g.FlagsEnum & 8L) == 8L));

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(g => ((short)g.FlagsEnum & 8) == 8));
    }

    [ConditionalFact]
    public virtual Task Bitwise_and_nullable_enum_with_constant()
        => AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & BasicFlagsEnum.Eight) > 0));

    [ConditionalFact]
    public virtual Task Where_bitwise_and_nullable_enum_with_null_constant()
    {
        return AssertQuery(
#pragma warning disable CS0458 // The result of the expression is always 'null'
            ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & null) > 0),
#pragma warning restore CS0458 // The result of the expression is always 'null'
            assertEmpty: true);
    }

    [ConditionalFact]
    public virtual Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
    {
        var flagsEnum = BasicFlagsEnum.Eight;

        return AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & flagsEnum) > 0));
    }

    [ConditionalFact]
    public virtual async Task Where_bitwise_and_nullable_enum_with_nullable_parameter()
    {
        BasicFlagsEnum? flagsEnum = BasicFlagsEnum.Eight;

        await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & flagsEnum) > 0));

        flagsEnum = null;

        await AssertQuery(
            ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & flagsEnum) > 0),
            assertEmpty: true);
    }

    [ConditionalFact]
    public virtual Task Bitwise_or()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(g => (g.FlagsEnum | BasicFlagsEnum.Eight) > 0));

    [ConditionalFact]
    public virtual Task Bitwise_projects_values_in_select()
        => AssertFirst(ss => ss.Set<BasicTypesEntity>()
            .Where(g => (g.FlagsEnum & BasicFlagsEnum.Eight) == BasicFlagsEnum.Eight)
            .Select(b => new
            {
                BitwiseTrue = (b.FlagsEnum & BasicFlagsEnum.Eight) == BasicFlagsEnum.Eight,
                // ReSharper disable once NonConstantEqualityExpressionHasConstantResult
                BitwiseFalse = (b.FlagsEnum & BasicFlagsEnum.Eight) == BasicFlagsEnum.Four,
                BitwiseValue = b.FlagsEnum & BasicFlagsEnum.Eight
            }));

    [ConditionalFact]
    public virtual async Task HasFlag()
    {
        // Constant
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag(BasicFlagsEnum.Eight)));

        // Expression
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag(BasicFlagsEnum.Eight | BasicFlagsEnum.Four)),
            assertEmpty: true);

        // Casting
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag((BasicFlagsEnum)8)));

        // Casting to nullable
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag((BasicFlagsEnum?)8)));

        // QuerySource
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => BasicFlagsEnum.Eight.HasFlag(b.FlagsEnum)));

        // Project out
        await AssertFirst(ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.FlagsEnum.HasFlag(BasicFlagsEnum.Eight))
            .Select(b => new
            {
                hasFlagTrue = b.FlagsEnum.HasFlag(BasicFlagsEnum.Eight), hasFlagFalse = b.FlagsEnum.HasFlag(BasicFlagsEnum.Four)
            }));
    }

    [ConditionalFact]
    public virtual Task HasFlag_with_non_nullable_parameter()
    {
        var flagsEnum = BasicFlagsEnum.Eight;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag(flagsEnum)));
    }

    [ConditionalFact]
    public virtual Task HasFlag_with_nullable_parameter()
    {
        BasicFlagsEnum? flagsEnum = BasicFlagsEnum.Eight;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag(flagsEnum)));
    }

    protected BasicTypesContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }
}
