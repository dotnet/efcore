// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class SimpleValueGeneratorFactoryTest
    {
        [Fact]
        public void Creates_an_instance_of_the_generic_type()
        {
            Assert.IsType<GuidValueGenerator>(
                new SimpleValueGeneratorFactory<GuidValueGenerator>().Create(CreateProperty()));
        }

        [Fact]
        public void Returns_pool_size_of_one()
        {
            Assert.Equal(1, new SimpleValueGeneratorFactory<GuidValueGenerator>().GetPoolSize(CreateProperty()));
        }

        [Fact]
        public void Uses_property_name_to_form_cache_key()
        {
            Assert.Equal("Led.Zeppelin", new SimpleValueGeneratorFactory<GuidValueGenerator>().GetCacheKey(CreateProperty()));
        }

        private static Property CreateProperty()
        {
            var entityType = new Model().AddEntityType("Led");
            return entityType.GetOrAddProperty("Zeppelin", typeof(Guid), shadowProperty: true);
        }
    }
}
