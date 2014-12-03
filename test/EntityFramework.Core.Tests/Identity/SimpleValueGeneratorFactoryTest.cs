// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class SimpleValueGeneratorFactoryTest
    {
        [Fact]
        public void Creates_an_instance_of_the_generic_type()
        {
            Assert.IsType<TemporaryValueGenerator>(
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>().Create(CreateProperty()));
        }

        [Fact]
        public void Returns_pool_size_of_one()
        {
            Assert.Equal(1, new SimpleValueGeneratorFactory<TemporaryValueGenerator>().GetPoolSize(CreateProperty()));
        }

        [Fact]
        public void Uses_property_name_to_form_cache_key()
        {
            Assert.Equal("Led.Zeppelin", new SimpleValueGeneratorFactory<TemporaryValueGenerator>().GetCacheKey(CreateProperty()));
        }

        private static Property CreateProperty()
        {
            var entityType = new Model().AddEntityType("Led");
            return entityType.GetOrAddProperty("Zeppelin", typeof(int), shadowProperty: true);
        }
    }
}
