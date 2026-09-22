// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindJoinQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindJoinQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    public override Task SelectMany_with_client_eval(bool async)
        // Joins between sources with client eval. Issue #21200.
        => Assert.ThrowsAsync<NotImplementedException>(() => base.SelectMany_with_client_eval(async));

    public override Task SelectMany_with_client_eval_with_collection_shaper(bool async)
        // Joins between sources with client eval. Issue #21200.
        => Assert.ThrowsAsync<NotImplementedException>(() => base.SelectMany_with_client_eval_with_collection_shaper(async));

    public override Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
        // Joins between sources with client eval. Issue #21200.
        => Assert.ThrowsAsync<NotImplementedException>(() => base.SelectMany_with_client_eval_with_collection_shaper_ignored(async));

    public override Task SelectMany_with_client_eval_with_constructor(bool async)
        // Joins between sources with client eval. Issue #21200.
        => Assert.ThrowsAsync<NotImplementedException>(() => base.SelectMany_with_client_eval_with_constructor(async));

    public override async Task Join_local_collection_int_closure_is_cached_correctly(bool async)
    {
        var ids = new uint[] { 1, 2 };

        await AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID));

        ids = [3];
        await AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID));
    }
}
