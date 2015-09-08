// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class ValueGeneratorFactoryTest
    {
        [Fact]
        public void Creates_an_instance_of_the_generic_type()
        {
            Assert.IsType<GuidValueGenerator>(
                new ValueGeneratorFactory<GuidValueGenerator>().Create(CreateProperty()));
        }

        private static Property CreateProperty()
        {
            var entityType = new Model().AddEntityType("Led");
            return entityType.GetOrAddProperty("Zeppelin", typeof(Guid), shadowProperty: true);
        }
    }
}
