// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class PropertyValuesInMemoryTest(PropertyValuesInMemoryTest.PropertyValuesInMemoryFixture fixture) : PropertyValuesTestBase<PropertyValuesInMemoryTest.PropertyValuesInMemoryFixture>(fixture)
{
    public class PropertyValuesInMemoryFixture : PropertyValuesFixtureBase
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging(false);

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
