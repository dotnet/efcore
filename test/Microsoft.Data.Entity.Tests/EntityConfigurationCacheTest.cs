// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationCacheTest
    {
        [Fact]
        public void OnConfiguring_is_called_on_context_when_configuration_happens()
        {
            using (var someContext = new SomeContext())
            {
                Assert.Equal(1, someContext.CallCount);
            }

            using (var someContext = new SomeContext())
            {
                Assert.Equal(0, someContext.CallCount);
            }
        }

        // Do not reuse this type in any other test
        private class SomeContext : EntityContext
        {
            public int CallCount { get; private set; }

            // This is protected _internal_ because of InternalsVisibleTo
            protected internal override void OnConfiguring(EntityConfigurationBuilder builder)
            {
                CallCount++;
            }
        }

        [Fact]
        public void Configuration_is_cached_by_context_type()
        {
            var cache = new EntityConfigurationCache();

            EntityConfiguration config;
            using (var someContext = new SomeContext2())
            {
                config = cache.GetOrAddConfiguration(someContext);
            }

            using (var someContext = new SomeContext2())
            {
                Assert.Same(config, cache.GetOrAddConfiguration(someContext));
            }
        }

        private class SomeContext2 : EntityContext
        {
        }
    }
}
