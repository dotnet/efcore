// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ValueGeneration
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
            return entityType.AddProperty("Zeppelin", typeof(Guid));
        }
    }
}
