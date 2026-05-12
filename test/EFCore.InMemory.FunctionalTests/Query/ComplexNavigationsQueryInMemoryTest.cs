// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsQueryInMemoryTest(ComplexNavigationsQueryInMemoryFixture fixture)
    : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryInMemoryFixture>(fixture)
{
    public override Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
        // Expression cannot be used for return type. Issue #23302.
        => Assert.ThrowsAsync<ArgumentException>(
            () => base.Join_with_result_selector_returning_queryable_throws_validation_error(async));

    public override Task Correlated_projection_with_first(bool async)
        => Task.CompletedTask;
}
