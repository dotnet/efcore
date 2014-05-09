// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationTest
    {
        [Fact]
        public void Mutating_methods_throw_when_configuratin_locked()
        {
            IDbContextOptionsConstruction configuration = new ImmutableDbContextOptions();
            configuration.Lock();

            Assert.Equal(
                Strings.FormatEntityConfigurationLocked("Model"),
                Assert.Throws<InvalidOperationException>(() => configuration.Model = Mock.Of<IModel>()).Message);

            Assert.Equal(
                Strings.FormatEntityConfigurationLocked("AddOrUpdateExtension"),
                Assert.Throws<InvalidOperationException>(() => configuration.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => { })).Message);
        }

        [Fact]
        public void Can_update_an_existing_extension()
        {
            IDbContextOptionsConstruction configuration = new ImmutableDbContextOptions();

            configuration.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => e.Something += "One");
            configuration.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => e.Something += "Two");

            Assert.Equal(
                "OneTwo", ((ImmutableDbContextOptions)configuration).Extensions.OfType<FakeEntityConfigurationExtension>().Single().Something);
        }

        private class FakeEntityConfigurationExtension : EntityConfigurationExtension
        {
            public string Something { get; set; }

            protected internal override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }
    }
}
