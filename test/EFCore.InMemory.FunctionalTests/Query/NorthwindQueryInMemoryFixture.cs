// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindQueryInMemoryFixture<TModelCustomizer> : NorthwindQueryFixtureBase<TModelCustomizer>
    where TModelCustomizer : IModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    protected override Type ContextType
        => typeof(NorthwindInMemoryContext);
}
