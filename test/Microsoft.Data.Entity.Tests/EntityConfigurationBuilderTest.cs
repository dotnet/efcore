// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationBuilderTest
    {
        [Fact]
        public void Model_is_null_if_not_set()
        {
            Assert.Same(null, new EntityConfigurationBuilder().BuildConfiguration().Model);
        }

        [Fact]
        public void Model_can_be_set_explicitly_in_config()
        {
            var model = Mock.Of<IModel>();

            var configuration = new EntityConfigurationBuilder()
                .UseModel(model)
                .BuildConfiguration();

            Assert.Same(model, configuration.Model);
        }

        [Fact]
        public void Build_actions_are_applied_to_configuration()
        {
            var extension1 = Mock.Of<FakeEntityConfigurationExtension1>();
            var extension2 = Mock.Of<FakeEntityConfigurationExtension2>();

            var configuration = new EntityConfigurationBuilder()
                .AddBuildAction(c => c.AddExtension(extension1))
                .AddBuildAction(c => c.AddExtension(extension2))
                .BuildConfiguration();

            Assert.Equal(2, configuration.Extensions.Count);
            Assert.Same(extension1, configuration.Extensions[0]);
            Assert.Same(extension2, configuration.Extensions[1]);
        }

        public abstract class FakeEntityConfigurationExtension1 : EntityConfigurationExtension
        {
        }

        public abstract class FakeEntityConfigurationExtension2 : EntityConfigurationExtension
        {
        }

        [Fact]
        public void Build_locks_configuration()
        {
            IEntityConfigurationConstruction configuration = new EntityConfigurationBuilder().BuildConfiguration();

            Assert.Equal(
                Strings.FormatEntityConfigurationLocked("Model"),
                Assert.Throws<InvalidOperationException>(() => configuration.Model = Mock.Of<IModel>()).Message);
        }

        [Fact]
        public void Can_create_derived_EntityConfiguration()
        {
            var model = Mock.Of<IModel>();

            var configuration = new EntityConfigurationBuilder()
                .UseModel(model)
                .BuildConfiguration(() => new UnkoolEntityConfiguration());

            Assert.IsType<UnkoolEntityConfiguration>(configuration);
            Assert.Same(model, configuration.Model);
        }

        private class UnkoolEntityConfiguration : EntityConfiguration
        {
        }

        [Fact]
        public void Can_create_derived_and_extended_EntityConfiguration()
        {
            var model = Mock.Of<IModel>();

            var configuration = new EntityConfigurationBuilder()
                .UseModel(model)
                .BuildConfiguration(() => new KoolEntityConfiguration { KoolAid = "Red" });

            Assert.IsType<KoolEntityConfiguration>(configuration);
            Assert.Same(model, configuration.Model);
            Assert.Equal("Red", configuration.KoolAid);
        }

        private class KoolEntityConfiguration : EntityConfiguration
        {
            public virtual string KoolAid { get; set; }
        }

        [Fact]
        public void Can_create_derived_and_extended_EntityConfiguration_from_extended_builder()
        {
            var model = Mock.Of<IModel>();

            var configuration = new KoolEntityConfigurationBuilder()
                .UseKoolAid("Blue")
                .UseModel(model)
                .BuildConfiguration();

            Assert.IsType<KoolEntityConfiguration>(configuration);
            Assert.Same(model, configuration.Model);
            Assert.Equal("Blue", configuration.KoolAid);
        }

        private class KoolEntityConfigurationBuilder : EntityConfigurationBuilder
        {
            private string _koolAid;

            public KoolEntityConfigurationBuilder UseKoolAid(string koolAid)
            {
                _koolAid = koolAid;

                return this;
            }

            public new KoolEntityConfigurationBuilder UseModel(IModel model)
            {
                return (KoolEntityConfigurationBuilder)base.UseModel(model);
            }

            public new KoolEntityConfiguration BuildConfiguration()
            {
                return BuildConfiguration(() => new KoolEntityConfiguration { KoolAid = _koolAid });
            }
        }
    }
}
