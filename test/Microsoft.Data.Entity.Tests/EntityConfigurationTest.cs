// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
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
                Strings.FormatEntityConfigurationLocked("AddExtension"),
                Assert.Throws<InvalidOperationException>(() => configuration.AddExtension(Mock.Of<EntityConfigurationExtension>())).Message);
        }

        [Fact]
        public void Adding_a_new_extension_replaces_existing_extension_of_the_same_type()
        {
            IEntityConfigurationConstruction configuration = new EntityConfiguration();

            var extension1 = Mock.Of<FakeEntityConfigurationExtension>();
            var extension2 = Mock.Of<FakeEntityConfigurationExtension>();

            configuration.AddExtension(extension1);
            configuration.AddExtension(extension2);

            Assert.Same(extension2, ((EntityConfiguration)configuration).Extensions.Single());
        }

        public abstract class FakeEntityConfigurationExtension : EntityConfigurationExtension
        {
        }
    }
}
