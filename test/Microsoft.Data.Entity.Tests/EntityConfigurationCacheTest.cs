// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationCacheTest
    {
        [Fact]
        public void OnConfiguring_is_called_on_context_when_configuration_happens_and_configuration_is_cached()
        {
            EntityConfiguration config;
            using (var someContext = new SomeContext())
            {
                Assert.Equal(1, someContext.CallCount);
                config = someContext.Configuration;
            }

            using (var someContext = new SomeContext())
            {
                Assert.Equal(0, someContext.CallCount);
                Assert.Same(config, someContext.Configuration);
            }
        }

        private class SomeContext : EntityContext
        {
            public int CallCount { get; private set; }

            protected override void OnConfiguring(EntityConfigurationBuilder builder)
            {
                CallCount++;
            }
        }
    }
}
