// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class DbContextOptionsTest
    {
        [Fact]
        public void Warnings_can_be_configured()
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .ConfigureWarnings(c => c.Default(WarningBehavior.Throw));

            var warningConfiguration = optionsBuilder.Options.FindExtension<CoreOptionsExtension>().WarningsConfiguration;

            Assert.Equal(WarningBehavior.Throw, warningConfiguration.DefaultBehavior);
        }
        
        [Fact]
        public void Model_can_be_set_explicitly_in_options()
        {
            var model = new Model();

            var optionsBuilder = new DbContextOptionsBuilder().UseModel(model);

            Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
        }

        [Fact]
        public void Sensitive_data_logging_can_be_set_explicitly_in_options()
        {
            var model = new Model();

            var optionsBuilder = new DbContextOptionsBuilder().UseModel(model).EnableSensitiveDataLogging();

            Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
            Assert.True(optionsBuilder.Options.FindExtension<CoreOptionsExtension>().IsSensitiveDataLoggingEnabled);
        }

        [Fact]
        public void Extensions_can_be_added_to_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.Null(optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension1>());
            Assert.Empty(optionsBuilder.Options.Extensions);

            var extension1 = new FakeDbContextOptionsExtension1();
            var extension2 = new FakeDbContextOptionsExtension2();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension1);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension2);

            Assert.Equal(2, optionsBuilder.Options.Extensions.Count());
            Assert.Contains(extension1, optionsBuilder.Options.Extensions);
            Assert.Contains(extension2, optionsBuilder.Options.Extensions);

            Assert.Same(extension1, optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension1>());
            Assert.Same(extension2, optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension2>());
        }

        [Fact]
        public void Can_update_an_existing_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            var extension1 = new FakeDbContextOptionsExtension1 { Something = "One " };
            var extension2 = new FakeDbContextOptionsExtension1 { Something = "Two " };

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension1);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension2);

            Assert.Equal(1, optionsBuilder.Options.Extensions.Count());
            Assert.DoesNotContain(extension1, optionsBuilder.Options.Extensions);
            Assert.Contains(extension2, optionsBuilder.Options.Extensions);

            Assert.Same(extension2, optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension1>());
        }

        [Fact]
        public void IsConfigured_returns_true_if_any_extensions_have_been_added()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(optionsBuilder.IsConfigured);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new FakeDbContextOptionsExtension2());

            Assert.True(optionsBuilder.IsConfigured);
        }

        private class FakeDbContextOptionsExtension1 : IDbContextOptionsExtension
        {
            public string Something { get; set; }

            public virtual void ApplyServices(IServiceCollection services)
            {
            }
        }

        private class FakeDbContextOptionsExtension2 : IDbContextOptionsExtension
        {
            public virtual void ApplyServices(IServiceCollection services)
            {
            }
        }

        [Fact]
        public void UseModel_on_generic_builder_returns_generic_builder()
        {
            var model = new Model();

            var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseModel(model));

            Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
        }

        private DbContextOptionsBuilder<UnkoolContext> GenericCheck(DbContextOptionsBuilder<UnkoolContext> optionsBuilder) => optionsBuilder;

        [Fact]
        public void Generic_builder_returns_generic_options()
        {
            var builder = new DbContextOptionsBuilder<UnkoolContext>();
            Assert.Same(((DbContextOptionsBuilder)builder).Options, GenericCheck(builder.Options));
        }

        private DbContextOptions<UnkoolContext> GenericCheck(DbContextOptions<UnkoolContext> options) => options;

        private class UnkoolContext : DbContext
        {
        }
    }
}
