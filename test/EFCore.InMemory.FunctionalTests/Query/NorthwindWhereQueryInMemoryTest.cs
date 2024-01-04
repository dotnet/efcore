// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindWhereQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindWhereQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    public override async Task<string> Where_simple_closure(bool async)
    {
        var queryString = await base.Where_simple_closure(async);

        Assert.Equal(InMemoryStrings.NoQueryStrings, queryString);

        return null;
    }

    public override Task Like_with_non_string_column_using_double_cast(bool async)
        // Casting int to object to string is invalid for InMemory
        => Assert.ThrowsAsync<InvalidCastException>(() => base.Like_with_non_string_column_using_double_cast(async));

    public override Task ElementAt_over_custom_projection_compared_to_not_null(bool async)
        => Task.CompletedTask;

    public override Task ElementAtOrDefault_over_custom_projection_compared_to_null(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_constructed_equal(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_constructed_multi_value_equal(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_tuple_constructed_equal(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_tuple_constructed_multi_value_equal(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_tuple_create_constructed_equal(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
        => Task.CompletedTask;


}
