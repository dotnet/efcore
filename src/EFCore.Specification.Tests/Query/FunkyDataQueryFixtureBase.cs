// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class FunkyDataQueryFixtureBase : SharedStoreFixtureBase<FunkyDataContext>
    {
        protected override string StoreName { get; } = "FunkyDataQueryTest";

        protected override void Seed(FunkyDataContext context) => FunkyDataContext.Seed(context);
    }
}
