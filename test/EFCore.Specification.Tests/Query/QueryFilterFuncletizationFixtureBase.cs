// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryFilterFuncletizationFixtureBase : SharedStoreFixtureBase<QueryFilterFuncletizationContext>
    {
        protected override string StoreName { get; } = "QueryFilterFuncletizationTest";

        protected override bool UsePooling => false;

        protected override void Seed(QueryFilterFuncletizationContext context)
            => QueryFilterFuncletizationContext.SeedData(context);
    }
}
