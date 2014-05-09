// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationBuilderTest
    {
        [Fact]
        public void Model_is_null_if_not_set()
        {
            Assert.Same(null, new DbContextOptions().BuildConfiguration().Model);
        }

        [Fact]
        public void Model_can_be_set_explicitly_in_config()
        {
            var model = Mock.Of<IModel>();

            var configuration = new DbContextOptions()
                .UseModel(model)
                .BuildConfiguration();

            Assert.Same(model, configuration.Model);
        }

        [Fact]
        public void Build_actions_are_applied_to_configuration()
        {
            var configuration = new DbContextOptions()
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
            IDbContextOptionsConstruction configuration = new DbContextOptions().BuildConfiguration();

            Assert.Equal(
                Strings.FormatEntityConfigurationLocked("Model"),
                Assert.Throws<InvalidOperationException>(() => configuration.Model = Mock.Of<IModel>()).Message);
        }

        [Fact]
        public void Can_create_derived_EntityConfiguration()
        {
            var model = Mock.Of<IModel>();

            var configuration = new DbContextOptions()
                .UseModel(model)
                .BuildConfiguration(() => new UnkoolContextOptions());

            Assert.IsType<UnkoolContextOptions>(configuration);
            Assert.Same(model, configuration.Model);
        }

        private class UnkoolContextOptions : ImmutableDbContextOptions
        {
        }

        [Fact]
        public void Can_create_derived_and_extended_EntityConfiguration()
        {
            var model = Mock.Of<IModel>();

            var configuration = new DbContextOptions()
                .UseModel(model)
                .BuildConfiguration(() => new KoolContextOptions { KoolAid = "Red" });

            Assert.IsType<KoolContextOptions>(configuration);
            Assert.Same(model, configuration.Model);
            Assert.Equal("Red", configuration.KoolAid);
        }

        private class KoolContextOptions : ImmutableDbContextOptions
        {
            public virtual string KoolAid { get; set; }
        }

        [Fact]
        public void Can_create_derived_and_extended_EntityConfiguration_from_extended_builder()
        {
            var model = Mock.Of<IModel>();

            var configuration = new KoolDbContextOptions()
                .UseKoolAid("Blue")
                .UseModel(model)
                .BuildConfiguration();

            Assert.IsType<KoolContextOptions>(configuration);
            Assert.Same(model, configuration.Model);
            Assert.Equal("Blue", configuration.KoolAid);
        }

        private class KoolDbContextOptions : DbContextOptions
        {
            private string _koolAid;

            public KoolDbContextOptions UseKoolAid(string koolAid)
            {
                _koolAid = koolAid;

                return this;
            }

            public new KoolDbContextOptions UseModel(IModel model)
            {
                return (KoolDbContextOptions)base.UseModel(model);
            }

            public new KoolContextOptions BuildConfiguration()
            {
                return BuildConfiguration(() => new KoolContextOptions { KoolAid = _koolAid });
            }
        }
    }
}
