// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityOptionsTest
    {
        [Fact]
        public void Model_can_be_set_explicitly_in_options()
        {
            var model = new Model();

            var optionsBuilder = new EntityOptionsBuilder().UseModel(model);

            Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
        }

        [Fact]
        public void Extensions_can_be_added_to_options()
        {
            var optionsBuilder = new EntityOptionsBuilder();

            Assert.Null(optionsBuilder.Options.FindExtension<FakeEntityOptionsExtension1>());
            Assert.Empty(optionsBuilder.Options.Extensions);

            var extension1 = new FakeEntityOptionsExtension1();
            var extension2 = new FakeEntityOptionsExtension2();

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension1);
            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension2);

            Assert.Equal(2, optionsBuilder.Options.Extensions.Count());
            Assert.Contains(extension1, optionsBuilder.Options.Extensions);
            Assert.Contains(extension2, optionsBuilder.Options.Extensions);

            Assert.Same(extension1, optionsBuilder.Options.FindExtension<FakeEntityOptionsExtension1>());
            Assert.Same(extension2, optionsBuilder.Options.FindExtension<FakeEntityOptionsExtension2>());
        }

        [Fact]
        public void Can_update_an_existing_extension()
        {
            var optionsBuilder = new EntityOptionsBuilder();

            var extension1 = new FakeEntityOptionsExtension1 { Something = "One " };
            var extension2 = new FakeEntityOptionsExtension1 { Something = "Two " };

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension1);
            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension2);

            Assert.Equal(1, optionsBuilder.Options.Extensions.Count());
            Assert.DoesNotContain(extension1, optionsBuilder.Options.Extensions);
            Assert.Contains(extension2, optionsBuilder.Options.Extensions);

            Assert.Same(extension2, optionsBuilder.Options.FindExtension<FakeEntityOptionsExtension1>());
        }

        [Fact]
        public void IsConfigured_returns_true_if_any_extensions_have_been_added()
        {
            var optionsBuilder = new EntityOptionsBuilder();

            Assert.False(optionsBuilder.IsConfigured);

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(new FakeEntityOptionsExtension2());

            Assert.True(optionsBuilder.IsConfigured);
        }

        private class FakeEntityOptionsExtension1 : IEntityOptionsExtension
        {
            public string Something { get; set; }

            public virtual void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
            }
        }

        private class FakeEntityOptionsExtension2 : IEntityOptionsExtension
        {
            public virtual void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
            }
        }

        [Fact]
        public void UseModel_on_generic_builder_returns_generic_builder()
        {
            var model = new Model();

            var optionsBuilder = GenericCheck(new EntityOptionsBuilder<UnkoolContext>().UseModel(model));

            Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
        }

        private EntityOptionsBuilder<UnkoolContext> GenericCheck(EntityOptionsBuilder<UnkoolContext> optionsBuilder) => optionsBuilder;

        [Fact]
        public void Generic_builder_returns_generic_options()
        {
            var builder = new EntityOptionsBuilder<UnkoolContext>();
            Assert.Same(((EntityOptionsBuilder)builder).Options, GenericCheck(builder.Options));
        }

        private EntityOptions<UnkoolContext> GenericCheck(EntityOptions<UnkoolContext> options) => options;

        private class UnkoolContext : DbContext
        {
        }
    }
}
