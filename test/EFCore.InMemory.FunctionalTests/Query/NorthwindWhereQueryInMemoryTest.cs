// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindWhereQueryInMemoryTest : NorthwindWhereQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
{
    public NorthwindWhereQueryInMemoryTest(
        NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
        ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    public override async Task<string> Where_simple_closure(bool async)
    {
        var queryString = await base.Where_simple_closure(async);

        Assert.Equal(InMemoryStrings.NoQueryStrings, queryString);

        return null;
    }

    // Casting int to object to string is invalid for InMemory
    public override Task Like_with_non_string_column_using_double_cast(bool async)
        => Task.CompletedTask;
}
