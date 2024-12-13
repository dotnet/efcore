// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class EnumTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    #region Equality

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Equality_to_constant(bool async)
         => AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => b.Enum == BasicEnum.One));

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Equality_to_parameter(bool async)
     {
         var basicEnum = BasicEnum.One;

         return AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => b.Enum == basicEnum));
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Equality_nullable_enum_to_constant(bool async)
         => AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == BasicEnum.One));

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Equality_nullable_enum_to_parameter(bool async)
     {
         var basicEnum = BasicEnum.One;

         return AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == basicEnum));
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Equality_nullable_enum_to_null_constant(bool async)
         => AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == null));

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Equality_nullable_enum_to_null_parameter(bool async)
     {
         BasicEnum? basicEnum = null;

         return AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == basicEnum));
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Equality_nullable_enum_to_nullable_parameter(bool async)
     {
         BasicEnum? basicEnum = BasicEnum.One;

         return AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Enum == basicEnum));
     }

     #endregion Equality

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual async Task Bitwise_and_enum_constant(bool async)
     {
         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(g => (g.FlagsEnum & BasicFlagsEnum.One) > 0));

         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(g => (g.FlagsEnum & BasicFlagsEnum.One) == BasicFlagsEnum.One));
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual async Task Bitwise_and_integral_constant(bool async)
     {
         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(g => ((int)g.FlagsEnum & 8) == 8));

         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(g => ((long)g.FlagsEnum & 8L) == 8L));

         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(g => ((short)g.FlagsEnum & 8) == 8));
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Bitwise_and_nullable_enum_with_constant(bool async)
         => AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & BasicFlagsEnum.Eight) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_and_nullable_enum_with_null_constant(bool async)
    {
        return AssertQuery(
            async,
#pragma warning disable CS0458 // The result of the expression is always 'null'
            ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & null) > 0),
#pragma warning restore CS0458 // The result of the expression is always 'null'
            assertEmpty: true);
    }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool async)
     {
         var flagsEnum = BasicFlagsEnum.Eight;

         return AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & flagsEnum) > 0));
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool async)
     {
         BasicFlagsEnum? flagsEnum = BasicFlagsEnum.Eight;

         await AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & flagsEnum) > 0));

         flagsEnum = null;

         await AssertQuery(
             async,
             ss => ss.Set<NullableBasicTypesEntity>().Where(w => (w.FlagsEnum & flagsEnum) > 0),
             assertEmpty: true);
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Bitwise_or(bool async)
         => AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(g => (g.FlagsEnum | BasicFlagsEnum.Eight) > 0));

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task Bitwise_projects_values_in_select(bool async)
         => AssertFirst(
             async,
             ss => ss.Set<BasicTypesEntity>()
                 .Where(g => (g.FlagsEnum & BasicFlagsEnum.Eight) == BasicFlagsEnum.Eight)
                 .Select(
                     b => new
                     {
                         BitwiseTrue = (b.FlagsEnum & BasicFlagsEnum.Eight) == BasicFlagsEnum.Eight,
                         // ReSharper disable once NonConstantEqualityExpressionHasConstantResult
                         BitwiseFalse = (b.FlagsEnum & BasicFlagsEnum.Eight) == BasicFlagsEnum.Four,
                         BitwiseValue = b.FlagsEnum & BasicFlagsEnum.Eight
                     }));

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual async Task HasFlag(bool async)
     {
         // Constant
         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag(BasicFlagsEnum.Eight)));

         // Expression
         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag(BasicFlagsEnum.Eight | BasicFlagsEnum.Four)),
             assertEmpty: true);

         // Casting
         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag((BasicFlagsEnum)8)));

         // Casting to nullable
         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag((BasicFlagsEnum?)8)));

         // QuerySource
         await AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => BasicFlagsEnum.Eight.HasFlag(b.FlagsEnum)));

         // Project out
         await AssertFirst(
             async,
             ss => ss.Set<BasicTypesEntity>()
                 .Where(b => b.FlagsEnum.HasFlag(BasicFlagsEnum.Eight))
                 .Select(
                     b => new
                     {
                         hasFlagTrue = b.FlagsEnum.HasFlag(BasicFlagsEnum.Eight),
                         hasFlagFalse = b.FlagsEnum.HasFlag(BasicFlagsEnum.Four)
                     }));
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task HasFlag_with_non_nullable_parameter(bool async)
     {
         var flagsEnum = BasicFlagsEnum.Eight;

         return AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag(flagsEnum)));
     }

     [ConditionalTheory]
     [MemberData(nameof(IsAsyncData))]
     public virtual Task HasFlag_with_nullable_parameter(bool async)
     {
         BasicFlagsEnum? flagsEnum = BasicFlagsEnum.Eight;

         return AssertQuery(
             async,
             ss => ss.Set<BasicTypesEntity>().Where(b => b.FlagsEnum.HasFlag(flagsEnum)));
     }

    protected BasicTypesContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }
}
