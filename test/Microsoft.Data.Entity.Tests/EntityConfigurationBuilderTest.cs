// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using Microsoft.AspNet.DependencyInjection;
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
            var configuration = new EntityConfigurationBuilder()
                .AddBuildAction(c => c.AddOrUpdateExtension<FakeEntityConfigurationExtension1>(e => { }))
                .AddBuildAction(c => c.AddOrUpdateExtension<FakeEntityConfigurationExtension2>(e => { }))
                .BuildConfiguration();

            Assert.Equal(2, configuration.Extensions.Count);
            Assert.IsType<FakeEntityConfigurationExtension1>(configuration.Extensions[0]);
            Assert.IsType<FakeEntityConfigurationExtension2>(configuration.Extensions[1]);
        }

        private class FakeEntityConfigurationExtension1 : EntityConfigurationExtension
        {
            protected internal override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }

        private class FakeEntityConfigurationExtension2 : EntityConfigurationExtension
        {
            protected internal override void ApplyServices(EntityServicesBuilder builder)
            {
            }
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
