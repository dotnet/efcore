// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class QueryFilterFuncletizationFixtureBase : SharedStoreFixtureBase<QueryFilterFuncletizationContext>
{
    protected override string StoreName
        => "QueryFilterFuncletizationTest";

    protected override bool UsePooling
        => false;

    protected override Task SeedAsync(QueryFilterFuncletizationContext context)
        => QueryFilterFuncletizationContext.SeedDataAsync(context);
}
