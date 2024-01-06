// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class PropertyValuesSqliteTest(PropertyValuesSqliteTest.PropertyValuesSqliteFixture fixture) : PropertyValuesTestBase<PropertyValuesSqliteTest.PropertyValuesSqliteFixture>(fixture)
{
    public class PropertyValuesSqliteFixture : PropertyValuesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
