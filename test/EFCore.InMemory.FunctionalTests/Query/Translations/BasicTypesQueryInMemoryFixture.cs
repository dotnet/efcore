// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class BasicTypesQueryInMemoryFixture : BasicTypesQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;
}
