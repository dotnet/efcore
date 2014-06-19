// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbContextOptionsTest
    {
        [Fact]
        public void Model_is_null_if_not_set()
        {
            Assert.Null(new DbContextOptions().Model);
        }

        [Fact]
        public void Model_can_be_set_explicitly_in_options()
        {
            var model = Mock.Of<IModel>();

            var options = new DbContextOptions().UseModel(model);

            Assert.Same(model, options.Model);
        }

        [Fact]
        public void Extensions_can_be_added_to_options()
        {
            IDbContextOptionsExtensions options = new DbContextOptions();

            options.AddOrUpdateExtension<FakeDbContextOptionsExtension1>(e => { });
            options.AddOrUpdateExtension<FakeDbContextOptionsExtension2>(e => { });

            Assert.Equal(2, options.Extensions.Count);
            Assert.IsType<FakeDbContextOptionsExtension1>(options.Extensions[0]);
            Assert.IsType<FakeDbContextOptionsExtension2>(options.Extensions[1]);
        }

        [Fact]
        public void Can_update_an_existing_extension()
        {
            IDbContextOptionsExtensions options = new DbContextOptions();

            options.AddOrUpdateExtension<FakeDbContextOptionsExtension1>(e => e.Something += "One");
            options.AddOrUpdateExtension<FakeDbContextOptionsExtension1>(e => e.Something += "Two");

            Assert.Equal("OneTwo", options.Extensions.OfType<FakeDbContextOptionsExtension1>().Single().Something);
        }

        private class FakeDbContextOptionsExtension1 : DbContextOptionsExtension
        {
            public string Something { get; set; }

            protected internal override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }

        private class FakeDbContextOptionsExtension2 : DbContextOptionsExtension
        {
            protected internal override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }

        [Fact]
        public void Can_lock_options()
        {
            var options = new DbContextOptions();

            Assert.False(options.IsLocked);

            options.Lock();

            Assert.True(options.IsLocked);

            Assert.Equal(
                Strings.FormatEntityConfigurationLocked("UseModel"),
                Assert.Throws<InvalidOperationException>(() => options.UseModel(Mock.Of<IModel>())).Message);

            Assert.Equal(
                Strings.FormatEntityConfigurationLocked("Can_lock_options"),
                Assert.Throws<InvalidOperationException>(
                    () => ((IDbContextOptionsExtensions)options).AddOrUpdateExtension<FakeDbContextOptionsExtension1>(e => { })).Message);
        }

        [Fact]
        public void UseModel_on_generic_options_returns_generic_options()
        {
            var model = Mock.Of<IModel>();

            var options = GenericCheck(new DbContextOptions<UnkoolContext>().UseModel(model));

            Assert.Same(model, options.Model);
        }

        private DbContextOptions<UnkoolContext> GenericCheck(DbContextOptions<UnkoolContext> options)
        {
            return options;
        }

        private class UnkoolContext : DbContext
        {
        }
    }
}
