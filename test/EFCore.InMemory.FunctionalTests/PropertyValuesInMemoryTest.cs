// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class PropertyValuesInMemoryTest : PropertyValuesTestBase<PropertyValuesInMemoryTest.PropertyValuesInMemoryFixture>
{
    public PropertyValuesInMemoryTest(PropertyValuesInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class PropertyValuesInMemoryFixture : PropertyValuesFixtureBase
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging(false);

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
