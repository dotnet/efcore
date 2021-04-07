// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
