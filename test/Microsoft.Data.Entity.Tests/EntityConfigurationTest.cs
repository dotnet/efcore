// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationTest
    {
        [Fact]
        public void Mutating_methods_throw_when_configuratin_locked()
        {
            IEntityConfigurationConstruction configuration = new EntityConfiguration();
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
            IEntityConfigurationConstruction configuration = new EntityConfiguration();

            configuration.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => e.Something += "One");
            configuration.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => e.Something += "Two");

            Assert.Equal(
                "OneTwo", ((EntityConfiguration)configuration).Extensions.OfType<FakeEntityConfigurationExtension>().Single().Something);
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
