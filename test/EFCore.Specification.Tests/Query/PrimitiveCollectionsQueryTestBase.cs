// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class PrimitiveCollectionsQueryTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : PrimitiveCollectionsQueryTestBase<TFixture>.PrimitiveCollectionsQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_of_ints_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 10, 999 }.Contains(c.Int)));

    // TODO: The following no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with optional
    // parameter, not supported in expression trees). #35547 is tracking on the EF side.
    //
    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Inline_collection_of_nullable_ints_Contains(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new int?[] { 10, 999 }.Contains(c.NullableInt)));
    //
    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Inline_collection_of_nullable_ints_Contains_null(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new int?[] { null, 999 }.Contains(c.NullableInt)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_zero_values(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once UseArrayEmptyMethod
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new int[0].Count(i => i > c.Id) == 1),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_one_value(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2 }.Count(i => i > c.Id) == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_two_values(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999 }.Count(i => i > c.Id) == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_three_values(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999, 1000 }.Count(i => i > c.Id) == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_zero_values(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once UseArrayEmptyMethod
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new int[0].Contains(c.Id)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_one_value(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2 }.Contains(c.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_two_values(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999 }.Contains(c.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_three_values(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999, 1000 }.Contains(c.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_all_parameters(bool async)
    {
        var (i, j) = (2, 999);

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { i, j }.Contains(c.Id)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_constant_and_parameter(bool async)
    {
        var j = 999;

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, j }.Contains(c.Id)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_Contains_with_mixed_value_types(bool async)
    {
        // Note: see many nullability-related variations on this in NullSemanticsQueryTestBase

        var i = 11;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 999, i, c.Id, c.Id + c.Int }.Contains(c.Int)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_List_Contains_with_mixed_value_types(bool async)
    {
        var i = 11;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => new List<int>
                {
                    999,
                    i,
                    c.Id,
                    c.Id + c.Int
                }.Contains(c.Int)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_as_Any_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999 }.Any(i => i == c.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_negated_Contains_as_All(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999 }.All(i => i != c.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_Min_with_two_values(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 30, c.Int }.Min() == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_List_Min_with_two_values(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new List<int> { 30, c.Int }.Min() == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_Max_with_two_values(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 30, c.Int }.Max() == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_List_Max_with_two_values(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new List<int> { 30, c.Int }.Max() == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_Min_with_three_values(bool async)
    {
        var i = 25;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 30, c.Int, i }.Min() == 25));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_List_Min_with_three_values(bool async)
    {
        var i = 25;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => new List<int>
                    {
                        30,
                        c.Int,
                        i
                    }.Min()
                    == 25));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_Max_with_three_values(bool async)
    {
        var i = 35;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 30, c.Int, i }.Max() == 35));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_List_Max_with_three_values(bool async)
    {
        var i = 35;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => new List<int>
                    {
                        30,
                        c.Int,
                        i
                    }.Max()
                    == 35));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_of_nullable_value_type_Min(bool async)
    {
        int? i = 25;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 30, c.Int, i }.Min() == 25));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_of_nullable_value_type_Max(bool async)
    {
        int? i = 35;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 30, c.Int, i }.Max() == 35));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_of_nullable_value_type_with_null_Min(bool async)
    {
        int? i = null;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 30, c.NullableInt, i }.Min() == 30));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_of_nullable_value_type_with_null_Max(bool async)
    {
        int? i = null;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 30, c.NullableInt, i }.Max() == 30));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_with_single_parameter_element_Contains(bool async)
    {
        var i = 2;

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { i }.Contains(c.Id)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { i }.Contains(c.Id)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_with_single_parameter_element_Count(bool async)
    {
        var i = 2;

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { i }.Count(i => i > c.Id) == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { i }.Count(i => i > c.Id) == 1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_EF_Parameter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => EF.Parameter(new[] { 2, 999, 1000 }).Contains(c.Id)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999, 1000 }.Contains(c.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_column_predicate_with_EF_Parameter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => EF.Parameter(new[] { 2, 999, 1000 }).Count(i => i > c.Id) == 2),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999, 1000 }.Count(i => i > c.Id) == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_Count(bool async)
    {
        var ids = new[] { 2, 999 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ids.Count(i => i > c.Id) == 1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_ints_Contains_int(bool async)
    {
        var ints = new[] { 10, 999 };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Contains(c.Int)));
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !ints.Contains(c.Int)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_HashSet_of_ints_Contains_int(bool async)
    {
        var ints = new HashSet<int> { 10, 999 };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Contains(c.Int)));
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !ints.Contains(c.Int)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_ImmutableArray_of_ints_Contains_int(bool async)
    {
        var ints = ImmutableArray.Create([10, 999]);

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Contains(c.Int)));
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !ints.Contains(c.Int)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_ints_Contains_nullable_int(bool async)
    {
        var ints = new[] { 10, 999 };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Contains(c.NullableInt!.Value)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableInt != null && ints.Contains(c.NullableInt!.Value)));
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !ints.Contains(c.NullableInt!.Value)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableInt == null || !ints.Contains(c.NullableInt!.Value)));
    }

    // TODO: The following no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with optional
    // parameter, not supported in expression trees). #35547 is tracking on the EF side.
    //
    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual async Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
    // {
    //     var nullableInts = new int?[] { 10, 999 };
    //
    //     await AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => nullableInts.Contains(c.Int)));
    //     await AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !nullableInts.Contains(c.Int)));
    // }
    //
    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual async Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
    // {
    //     var nullableInts = new int?[] { null, 999 };
    //
    //     await AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => nullableInts.Contains(c.NullableInt)));
    //     await AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !nullableInts.Contains(c.NullableInt)));
    // }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_structs_Contains_struct(bool async)
    {
        var values = new List<WrappedId> { new(22), new(33) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => values.Contains(c.WrappedId)));

        values = new List<WrappedId> { new(11), new(44) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !values.Contains(c.WrappedId)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_structs_Contains_nullable_struct(bool async)
    {
        var values = new List<WrappedId> { new(22), new(33) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => values.Contains(c.NullableWrappedId!.Value)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableWrappedId != null && values.Contains(c.NullableWrappedId.Value)));

        values = new List<WrappedId> { new(11), new(44) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !values.Contains(c.NullableWrappedId!.Value)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableWrappedId == null || !values.Contains(c.NullableWrappedId!.Value)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))] // Issue #35117
    public virtual async Task Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        var values = new List<WrappedId> { new(22), new(33) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => values.Contains(c.NullableWrappedIdWithNullableComparer!.Value)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => c.NullableWrappedIdWithNullableComparer != null && values.Contains(c.NullableWrappedIdWithNullableComparer.Value)));

        values = new List<WrappedId> { new(11), new(44) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !values.Contains(c.NullableWrappedId!.Value)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => c.NullableWrappedIdWithNullableComparer == null || !values.Contains(c.NullableWrappedIdWithNullableComparer!.Value)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_nullable_structs_Contains_struct(bool async)
    {
        var values = new List<WrappedId?> { null, new(22) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => values.Contains(c.WrappedId)));

        values = new List<WrappedId?> { new(11), new(44) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !values.Contains(c.WrappedId)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct(bool async)
    {
        var values = new List<WrappedId?> { null, new(22) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => values.Contains(c.NullableWrappedId)));

        values = new List<WrappedId?> { new(11), new(44) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !values.Contains(c.NullableWrappedId)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        var values = new List<WrappedId?> { null, new(22) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => values.Contains(c.NullableWrappedIdWithNullableComparer)));

        values = new List<WrappedId?> { new(11), new(44) };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !values.Contains(c.NullableWrappedIdWithNullableComparer)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_strings_Contains_string(bool async)
    {
        var strings = new[] { "10", "999" };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => strings.Contains(c.String)));
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !strings.Contains(c.String)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
    {
        string?[] strings = ["10", "999"];

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => strings.Contains(c.NullableString)));
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !strings.Contains(c.NullableString)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_nullable_strings_Contains_string(bool async)
    {
        var strings = new[] { "10", null };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => strings.Contains(c.String)));
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !strings.Contains(c.String)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_of_nullable_strings_Contains_nullable_string(bool async)
    {
        var strings = new[] { "999", null };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => strings.Contains(c.NullableString)));
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !strings.Contains(c.NullableString)));
    }

    // See more nullability-related tests in NullSemanticsQueryTestBase

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        var dateTimes = new[]
        {
            new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc), new DateTime(9999, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => dateTimes.Contains(c.DateTime)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_bools_Contains(bool async)
    {
        var bools = new[] { true };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => bools.Contains(c.Bool)));
    }

    // TODO: The following no longer compiles since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with optional
    // parameter, not supported in expression trees). #35547 is tracking on the EF side.
    //
    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Parameter_collection_of_enums_Contains(bool async)
    // {
    //     var enums = new[] { MyEnum.Value1, MyEnum.Value4 };
    //
    //     return AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => enums.Contains(c.Enum)));
    // }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_null_Contains(bool async)
    {
        int[]? ints = null;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints!.Contains(c.Int)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => false),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_Contains_with_EF_Constant(bool async)
    {
        var ids = new[] { 2, 999, 1000 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => EF.Constant(ids).Contains(c.Id)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ids.Contains(c.Id)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_Where_with_EF_Constant_Where_Any(bool async)
    {
        var ids = new[] { 2, 999, 1000 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => EF.Constant(ids).Where(x => x > 0).Any()),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ids.Where(x => x > 0).Any()));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_Count_with_column_predicate_with_EF_Constant(bool async)
    {
        var ids = new[] { 2, 999, 1000 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => EF.Constant(ids).Count(i => i > c.Id) == 2),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ids.Count(i => i > c.Id) == 2));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_of_ints_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Contains(10)));

    // TODO: The following no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with optional
    // parameter, not supported in expression trees). #35547 is tracking on the EF side.
    //
    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Column_collection_of_nullable_ints_Contains(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableInts.Contains(10)));
    //
    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Column_collection_of_nullable_ints_Contains_null(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableInts.Contains(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_of_strings_contains_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ((string?[])c.Strings).Contains(null)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_of_nullable_strings_contains_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableStrings.Contains(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_of_bools_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Bools.Contains(true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Count_method(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once UseCollectionCountProperty
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Count() == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Length == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Count_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Count(i => i > 1) == 2));

    [ConditionalTheory] // #33932
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Where_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Where(i => i > 1).Count() == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_index_int(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints[1] == 10),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Ints.Length >= 2 ? c.Ints[1] : -1) == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_index_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Strings[1] == "10"),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Strings.Length >= 2 ? c.Strings[1] : "-1") == "10"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_index_datetime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => c.DateTimes[1] == new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => (c.DateTimes.Length >= 2 ? c.DateTimes[1] : default) == new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_index_beyond_end(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints[999] == 10),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => false),
            assertEmpty: true);

    // TODO: This test is incorrect, see #33784
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nullable_reference_column_collection_index_equals_nullable_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => c.NullableStrings[2] == c.NullableString),
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => (c.NullableStrings.Length > 2 ? c.NullableStrings[2] : default) == c.NullableString));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Non_nullable_reference_column_collection_index_equals_nullable_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => c.Strings.Any() && c.Strings[1] == c.NullableString),
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => c.Strings.Any() && (c.Strings.Length > 1 ? c.Strings[1] : default) == c.NullableString));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_index_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 1, 2, 3 }[c.Int] == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Int <= 2 ? new[] { 1, 2, 3 }[c.Int] : -1) == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_value_index_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 1, c.Int, 3 }[c.Int] == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Int <= 2 ? new[] { 1, c.Int, 3 }[c.Int] : -1) == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_List_value_index_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => new List<int>
                    {
                        1,
                        c.Int,
                        3
                    }[c.Int]
                    == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => (c.Int <= 2
                        ? new List<int>
                        {
                            1,
                            c.Int,
                            3
                        }[c.Int]
                        : -1)
                    == 1));

    // The JsonScalarExpression (ints[c.Int]) should get inferred from the column on the other side (c.Int), and that should propagate to
    // ints
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_index_Column_equal_Column(bool async)
    {
        var ints = new[] { 0, 2, 3 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints[c.Int] == c.Int),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Int <= 2 ? ints[c.Int] : -1) == c.Int));
    }

    // Since the JsonScalarExpression (ints[c.Int]) is being compared to a constant, there's nothing to infer the type mapping from.
    // ints should get the default type mapping for based on its CLR type.
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        var ints = new[] { 1, 2, 3 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints[c.Int] == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Int <= 2 ? ints[c.Int] : -1) == 1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.ElementAt(1) == 10),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Ints.Length >= 2 ? c.Ints.ElementAt(1) : -1) == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_First(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.First() == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Ints.Length >= 1 ? c.Ints.First() : -1) == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.FirstOrDefault() == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Single(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Single() == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Ints.Length >= 1 ? c.Ints.First() : -1) == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_SingleOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.SingleOrDefault() == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Ints.Length >= 1 ? c.Ints[0] : -1) == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Skip(1).Count() == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Take(2).Contains(11)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Skip(1).Take(2).Contains(11)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Where_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Where(i => i > 1).Skip(1).Count() == 3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Where_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Where(i => i > 1).Take(2).Count() == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Where_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Where(i => i > 1).Skip(1).Take(2).Count() == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Contains_over_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Where(i => i > 1).Contains(11)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_OrderByDescending_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => c.Ints.OrderByDescending(i => i).ElementAt(0) == 111),
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => c.Ints.Length > 0 && c.Ints.OrderByDescending(i => i).ElementAt(0) == 111));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Where_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => c.Ints.Where(i => i > 1).ElementAt(0) == 11),
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => c.Ints.Where(i => i > 1).FirstOrDefault(0) == 11));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Any(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Any()));

    // If this test is failing because of DistinctAfterOrderByWithoutRowLimitingOperatorWarning, this is because EF warns/errors by
    // default for Distinct after OrderBy (without Skip/Take); but you likely have a naturally-ordered JSON collection, where the
    // ordering has been added by the provider as part of the collection translation.
    // Consider overriding RelationalQueryableMethodTranslatingExpressionVisitor.IsNaturallyOrdered() to identify such naturally-ordered
    // collections, exempting them from the warning.
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Distinct().Count() == 3));

    [ConditionalTheory] // #32505
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().SelectMany(c => c.Ints));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_SelectMany_with_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().SelectMany(c => c.Ints.Where(i => i > 1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_SelectMany_with_Select_to_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().SelectMany(c => c.Ints.Select(i => new { Original = i, Incremented = i + 1 })));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_projection_from_top_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(c => c.Id).Select(c => c.Ints),
            elementAsserter: (a, b) => Assert.Equivalent(a, b),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Join_parameter_collection(bool async)
    {
        var ints = new[] { 11, 111 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => c.Ints.Join(ints, i => i, j => j, (i, j) => new { I = i, J = j }).Count() == 2));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Join_ordered_column_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Where(c => new[] { 11, 111 }.Join(c.Ints, i => i, j => j, (i, j) => new { I = i, J = j }).Count() == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_Concat_column_collection(bool async)
    {
        int[] ints = [11, 111];

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Concat(c.Ints).Count() == 2));
    }

    [ConditionalTheory] // #33582
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_with_type_inference_for_JsonScalarExpression(bool async)
    {
        string[] values = ["one", "two"];

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Select(c => c.Id != 0 ? values[c.Int % 2] : "foo"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Union_parameter_collection(bool async)
    {
        var ints = new[] { 11, 111 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Union(ints).Count() == 2));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Intersect_inline_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Intersect(new[] { 11, 111 }).Count() == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Except_column_collection(bool async)
        // Note that in relational, since the VALUES is on the left side of the set operation, it must assign column names, otherwise the
        // column coming out of the set operation has undetermined naming.
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => new[] { 11, 111 }.Except(c.Ints).Count(i => i % 2 == 1) == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Where_Union(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Where(i => i > 100).Union(new[] { 50 }).Count() == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_equality_parameter_collection(bool async)
    {
        var ints = new[] { 1, 10 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints == ints),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.SequenceEqual(ints)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Column_collection_Concat_parameter_collection_equality_inline_collection(bool async)
    {
        var ints = new[] { 1, 10 };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Concat(ints) == new[] { 1, 11, 111, 1, 10 }),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Concat(ints).SequenceEqual(new[] { 1, 11, 111, 1, 10 })));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_equality_inline_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints == new[] { 1, 10 }),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.SequenceEqual(new[] { 1, 10 })));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Column_collection_equality_inline_collection_with_parameters(bool async)
    {
        var (i, j) = (1, 10);

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints == new[] { i, j }),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.SequenceEqual(new[] { i, j })));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Where_equality_inline_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Where(i => i != 11) == new[] { 1, 111 }),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Where(i => i != 11).SequenceEqual(new[] { 1, 111 })));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
    {
        // The Skip causes a pushdown into a subquery before the Union, and so the projection on the left side of the union points to the
        // subquery as its table, and not directly to the parameter's table.
        // This creates an initially untyped ColumnExpression referencing the pushed-down subquery; it must also be inferred.
        // Note that this must be a compiled query, since with normal queries the Skip(1) gets client-evaluated.
        var compiledQuery = EF.CompileQuery(
            (PrimitiveCollectionsContext context, int[] ints)
                => context.Set<PrimitiveCollectionsEntity>().Where(p => ints.Skip(1).Count(i => i > p.Id) == 1).Count());

        await using var context = Fixture.CreateContext();
        var ints = new[] { 10, 111 };

        // TODO: Complete
        var results = compiledQuery(context, ints);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
    {
        // The Skip causes a pushdown into a subquery before the Union, and so the projection on the left side of the union points to the
        // subquery as its table, and not directly to the parameter's table.
        // This creates an initially untyped ColumnExpression referencing the pushed-down subquery; it must also be inferred.
        // Note that this must be a compiled query, since with normal queries the Skip(1) gets client-evaluated.
        var compiledQuery = EF.CompileQuery(
            (PrimitiveCollectionsContext context, int[] ints)
                => context.Set<PrimitiveCollectionsEntity>().Where(p => ints.Skip(1).Union(p.Ints).Count() == 3));

        await using var context = Fixture.CreateContext();
        var ints = new[] { 10, 111 };

        // TODO: Complete
        var results = compiledQuery(context, ints).ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_in_subquery_Union_column_collection(bool async)
    {
        var ints = new[] { 10, 111 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(p => ints.Skip(1).Union(p.Ints).Count() == 3));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_in_subquery_Union_column_collection_nested(bool async)
    {
        var ints = new[] { 10, 111 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                p => ints.Skip(1).Union(p.Ints.OrderBy(x => x).Skip(1).Distinct().OrderByDescending(x => x).Take(20)).Count() == 3));
    }

    [ConditionalFact]
    public virtual void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        var query = EF.CompileQuery(
            (PrimitiveCollectionsContext context, object[] parameters)
                => context.Set<PrimitiveCollectionsEntity>().Where(p => p.String == (string)parameters[0]));

        using var context = Fixture.CreateContext();

        _ = query(context, ["foo"]).ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
    {
        var compiledQuery = EF.CompileQuery(
            (PrimitiveCollectionsContext context, int[] ints1, int[] ints2)
                => context.Set<PrimitiveCollectionsEntity>().Where(p => ints1.Skip(1).Union(ints2).Count() == 3));

        await using var context = Fixture.CreateContext();
        var ints1 = new[] { 10, 111 };
        var ints2 = new[] { 7, 42 };

        _ = compiledQuery(context, ints1, ints2).ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        var ints = new[] { 10, 111 };

        // The Skip causes a pushdown into a subquery before the Union. This creates an initially untyped ColumnExpression referencing the
        // pushed-down subquery; it must also be inferred
        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Skip(1).Union(ints).Count() == 3));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_ints_simple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.Ints),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_ints_ordered(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.Ints.OrderByDescending(xx => xx).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_datetimes_filtered(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.DateTimes.Where(xx => xx.Day != 1).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_nullable_ints_with_paging(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.NullableInts.Take(20).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_nullable_ints_with_paging2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.NullableInts.OrderBy(x => x).Skip(1).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_nullable_ints_with_paging3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.NullableInts.Skip(2).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_ints_with_distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.Ints.Distinct().ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory(Skip = "issue #31277")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_nullable_ints_with_distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.NullableInts.Distinct().ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_of_ints_with_ToList_and_FirstOrDefault(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(x => x.Ints.ToList()),
            asserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(
                x => new
                {
                    Empty = x.NullableInts.Where(x => false).ToList(), OnlyNull = x.NullableInts.Where(x => x == null).ToList(),
                }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.Empty, a.Empty, ordered: true);
                AssertCollection(e.OnlyNull, a.OnlyNull, ordered: true);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_multiple_collections(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(
                x => new
                {
                    Ints = x.Ints.ToList(),
                    OrderedInts = x.Ints.OrderByDescending(xx => xx).ToList(),
                    FilteredDateTimes = x.DateTimes.Where(xx => xx.Day != 1).ToList(),
                    FilteredDateTimes2 = x.DateTimes.Where(xx => xx > new DateTime(2000, 1, 1)).ToList()
                }),
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.Ints, a.Ints, ordered: true);
                AssertCollection(e.OrderedInts, a.OrderedInts, ordered: true);
                AssertCollection(e.FilteredDateTimes, a.FilteredDateTimes, elementSorter: ee => ee);
                AssertCollection(e.FilteredDateTimes2, a.FilteredDateTimes2, elementSorter: ee => ee);
            },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_primitive_collections_element(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(x => x.Id < 4).OrderBy(x => x.Id).Select(
                x => new
                {
                    Indexer = x.Ints[0],
                    EnumerableElementAt = x.DateTimes.ElementAt(0),
                    QueryableElementAt = x.Strings.AsQueryable().ElementAt(1)
                }),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Indexer, a.Indexer);
                AssertEqual(e.EnumerableElementAt, a.EnumerableElementAt);
                AssertEqual(e.QueryableElementAt, a.QueryableElementAt);
            },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_inline_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Select(x => new[] { x.String, "foo" }),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_inline_collection_with_Union(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Select(
                    x => new
                    {
                        x.Id,
                        Values = new[] { x.String }
                            .Union(ss.Set<PrimitiveCollectionsEntity>().OrderBy(xx => xx.Id).Select(xx => xx.String)).ToList()
                    })
                .OrderBy(x => x.Id),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.Values, a.Values, ordered: false);
            },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_inline_collection_with_Concat(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>()
                .Select(
                    x => new
                    {
                        x.Id,
                        Values = new[] { x.String }
                            .Concat(ss.Set<PrimitiveCollectionsEntity>().OrderBy(xx => xx.Id).Select(xx => xx.String)).ToList()
                    })
                .OrderBy(x => x.Id),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.Values, a.Values, ordered: false);
            },
            assertOrder: true);

    [ConditionalTheory] // #32208, #32215
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_contains_with_Lists_and_no_inferred_type_mapping(bool async)
    {
        var ints = new List<int>
        {
            1,
            2,
            3
        };
        var strings = new List<string>
        {
            "one",
            "two",
            "three"
        };

        // Note that in this query, the outer Contains really has no type mapping, neither for its source (collection parameter), nor
        // for its item (the conditional expression returns constants). The default type mapping must be applied.
        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => strings.Contains(ints.Contains(e.Int) ? "one" : "two")));
    }

    [ConditionalTheory] // #32208, #32215
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_contains_with_arrays_and_no_inferred_type_mapping(bool async)
    {
        var ints = new[] { 1, 2, 3 };
        var strings = new[] { "one", "two", "three" };

        // Note that in this query, the outer Contains really has no type mapping, neither for its source (collection parameter), nor
        // for its item (the conditional expression returns constants). The default type mapping must be applied.
        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => strings.Contains(ints.Contains(e.Int) ? "one" : "two")));
    }

    public abstract class PrimitiveCollectionsQueryFixtureBase : SharedStoreFixtureBase<PrimitiveCollectionsContext>, IQueryFixtureBase
    {
        private PrimitiveCollectionsData? _expectedData;

        protected override string StoreName
            => "PrimitiveCollectionsTest";

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => modelBuilder.Entity<PrimitiveCollectionsEntity>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.WrappedId).HasConversion<WrappedIdConverter>();
                    b.Property(e => e.NullableWrappedId).HasConversion<WrappedIdConverter>();
                    b.Property(e => e.NullableWrappedIdWithNullableComparer).HasConversion<NullableWrappedIdConverter>();
                });

        protected class WrappedIdConverter() : ValueConverter<WrappedId, int>(v => v.Value, v => new(v));

        // Note that value comparers over nullable value types are not a good idea, unless the comparer is handling nulls itself.
        protected class NullableWrappedIdConverter() : ValueConverter<WrappedId?, int?>(
            id => id == null ? null : id.Value.Value,
            value => value == null ? null : new WrappedId(value.Value));

        protected override Task SeedAsync(PrimitiveCollectionsContext context)
        {
            context.AddRange(new PrimitiveCollectionsData().PrimitiveArrayEntities);
            return context.SaveChangesAsync();
        }

        public virtual ISetSource GetExpectedData()
            => _expectedData ??= new PrimitiveCollectionsData();

        public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
        {
            { typeof(PrimitiveCollectionsEntity), e => ((PrimitiveCollectionsEntity?)e)?.Id }
        }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object?, object?>>
        {
            {
                typeof(PrimitiveCollectionsEntity), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (PrimitiveCollectionsEntity)e!;
                        var aa = (PrimitiveCollectionsEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equivalent(ee.Ints, aa.Ints, strict: true);
                        Assert.Equivalent(ee.Strings, aa.Strings, strict: true);
                        Assert.Equivalent(ee.DateTimes, aa.DateTimes, strict: true);
                        Assert.Equivalent(ee.Bools, aa.Bools, strict: true);
                        // TODO: Complete
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    public class PrimitiveCollectionsContext(DbContextOptions options) : PoolableDbContext(options);

    public class PrimitiveCollectionsEntity
    {
        public int Id { get; set; }

        public required string String { get; set; }
        public int Int { get; set; }
        public DateTime DateTime { get; set; }
        public bool Bool { get; set; }
        public WrappedId WrappedId { get; set; }
        public MyEnum Enum { get; set; }
        public int? NullableInt { get; set; }
        public string? NullableString { get; set; }
        public WrappedId? NullableWrappedId { get; set; }
        public WrappedId? NullableWrappedIdWithNullableComparer { get; set; }

        public required string[] Strings { get; set; }
        public required int[] Ints { get; set; }
        public required DateTime[] DateTimes { get; set; }
        public required bool[] Bools { get; set; }
        public required MyEnum[] Enums { get; set; }
        public required int?[] NullableInts { get; set; }
        public required string?[] NullableStrings { get; set; }
    }

    public readonly record struct WrappedId(int Value);

    public enum MyEnum { Value1, Value2, Value3, Value4 }

    public class PrimitiveCollectionsData : ISetSource
    {
        public IReadOnlyList<PrimitiveCollectionsEntity> PrimitiveArrayEntities { get; }

        public PrimitiveCollectionsData(PrimitiveCollectionsContext? context = null)
            => PrimitiveArrayEntities = CreatePrimitiveArrayEntities();

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(PrimitiveCollectionsEntity))
            {
                return (IQueryable<TEntity>)PrimitiveArrayEntities.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        private static IReadOnlyList<PrimitiveCollectionsEntity> CreatePrimitiveArrayEntities()
            => new List<PrimitiveCollectionsEntity>
            {
                new()
                {
                    Id = 1,
                    Int = 10,
                    String = "10",
                    DateTime = new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc),
                    Bool = true,
                    Enum = MyEnum.Value1,
                    WrappedId = new(22),
                    NullableInt = 10,
                    NullableString = "10",
                    NullableWrappedId = new(22),
                    NullableWrappedIdWithNullableComparer = new(22),
                    Ints = [1, 10],
                    Strings = ["1", "10"],
                    DateTimes =
                    [
                        new DateTime(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc), new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc)
                    ],
                    Bools = [true, false],
                    Enums = [MyEnum.Value1, MyEnum.Value2],
                    NullableInts = [1, 10],
                    NullableStrings = ["1", "10"],
                },
                new()
                {
                    Id = 2,
                    Int = 11,
                    String = "11",
                    DateTime = new DateTime(2020, 1, 11, 12, 30, 0, DateTimeKind.Utc),
                    Bool = false,
                    Enum = MyEnum.Value2,
                    WrappedId = new(22),
                    NullableInt = null,
                    NullableString = null,
                    NullableWrappedId = null,
                    NullableWrappedIdWithNullableComparer = null,
                    Ints = [1, 11, 111],
                    Strings = ["1", "11", "111"],
                    DateTimes =
                    [
                        new DateTime(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 11, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 31, 12, 30, 0, DateTimeKind.Utc)
                    ],
                    Bools = [false],
                    Enums = [MyEnum.Value2, MyEnum.Value3],
                    NullableInts = [1, 11, null],
                    NullableStrings = ["1", "11", null]
                },
                new()
                {
                    Id = 3,
                    Int = 20,
                    String = "20",
                    DateTime = new DateTime(2022, 1, 10, 12, 30, 0, DateTimeKind.Utc),
                    Bool = true,
                    Enum = MyEnum.Value1,
                    WrappedId = new(22),
                    NullableInt = 20,
                    NullableString = "20",
                    NullableWrappedId = new(22),
                    NullableWrappedIdWithNullableComparer = new(22),
                    Ints = [1, 1, 10, 10, 10, 1, 10],
                    Strings = ["1", "10", "10", "1", "1"],
                    DateTimes =
                    [
                        new DateTime(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc)
                    ],
                    Bools = [true, false],
                    Enums = [MyEnum.Value1, MyEnum.Value2],
                    NullableInts = [1, 1, 10, 10, null, 1],
                    NullableStrings = ["1", "1", "10", "10", null, "1"]
                },
                new()
                {
                    Id = 4,
                    Int = 41,
                    String = "41",
                    DateTime = new DateTime(2024, 1, 11, 12, 30, 0, DateTimeKind.Utc),
                    Bool = false,
                    Enum = MyEnum.Value2,
                    WrappedId = new(22),
                    NullableInt = null,
                    NullableString = null,
                    NullableWrappedId = null,
                    NullableWrappedIdWithNullableComparer = null,
                    Ints = [1, 1, 111, 11, 1, 111],
                    Strings = ["1", "11", "111", "11"],
                    DateTimes =
                    [
                        new DateTime(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 11, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 11, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 31, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 31, 12, 30, 0, DateTimeKind.Utc),
                        new DateTime(2020, 1, 31, 12, 30, 0, DateTimeKind.Utc)
                    ],
                    Bools = [false],
                    Enums = [MyEnum.Value2, MyEnum.Value3],
                    NullableInts = [null, null],
                    NullableStrings = [null, null],
                },
                new()
                {
                    Id = 5,
                    Int = 0,
                    String = "",
                    DateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Bool = false,
                    Enum = MyEnum.Value1,
                    WrappedId = new(22),
                    NullableWrappedIdWithNullableComparer = null,
                    NullableInt = null,
                    NullableString = null,
                    NullableWrappedId = null,
                    Ints = [],
                    Strings = [],
                    DateTimes = [],
                    Bools = [],
                    Enums = [],
                    NullableInts = [],
                    NullableStrings = []
                }
            };
    }
}
