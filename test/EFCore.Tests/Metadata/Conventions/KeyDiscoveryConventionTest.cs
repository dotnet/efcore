// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class KeyDiscoveryConventionTest
    {
        [ConditionalFact]
        public void Primary_key_is_not_set_when_zero_key_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();

            RunConvention(entityBuilder);

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.Null(key);
        }

        [ConditionalFact]
        public void Primary_key_is_set_when_shadow_property_not_defined_by_convention_matches()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();
            var propertyBuilder = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.DataAnnotation);

            RunConvention(propertyBuilder);

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public void Primary_key_is_not_set_when_shadow_property_defined_by_convention_matches()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();
            var propertyBuilder = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.Null(key);
        }

        private class EntityWithId
        {
            public int Id { get; set; }
        }

        [ConditionalFact]
        public void DiscoverKeyProperties_discovers_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithId>();

            RunConvention(entityBuilder);

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithTypeId
        {
            public int EntityWithTypeIdId { get; set; }
        }

        [ConditionalFact]
        public void DiscoverKeyProperties_discovers_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithTypeId>();

            RunConvention(entityBuilder);

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "EntityWithTypeIdId" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithIdAndTypeId
        {
            public int Id { get; set; }
            public int EntityWithIdAndTypeIdId { get; set; }
        }

        [ConditionalFact]
        public void DiscoverKeyProperties_prefers_id_over_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithIdAndTypeId>();

            RunConvention(entityBuilder);

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithMultipleIds
        {
            public int ID { get; set; }
            public int Id { get; set; }
        }

        [ConditionalFact]
        public void DiscoverKeyProperties_does_not_discover_key_when_multiple_ids()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithMultipleIds>();

            RunConvention(entityBuilder);

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.Null(key);

            var logEntry = ListLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Debug, logEntry.Level);
            Assert.Equal(
                CoreResources.LogMultiplePrimaryKeyCandidates(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(EntityWithMultipleIds.ID), nameof(EntityWithMultipleIds.Id), nameof(EntityWithMultipleIds)), logEntry.Message);
        }

        public ListLoggerFactory ListLoggerFactory { get; }
            = new ListLoggerFactory(l => l == DbLoggerCategory.Model.Name);

        private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var context = new ConventionContext<IConventionEntityTypeBuilder>(
                entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            CreateKeyDiscoveryConvention().ProcessEntityTypeAdded(entityTypeBuilder, context);
        }

        private void RunConvention(InternalPropertyBuilder propertyBuilder)
        {
            var context = new ConventionContext<IConventionPropertyBuilder>(
                propertyBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

            CreateKeyDiscoveryConvention().ProcessPropertyAdded(propertyBuilder, context);
        }

        private KeyDiscoveryConvention CreateKeyDiscoveryConvention()
            => new KeyDiscoveryConvention(CreateDependencies());

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>()
                .With(CreateLogger());

        private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
        {
            ListLoggerFactory.Clear();
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
            var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
                ListLoggerFactory,
                options,
                new DiagnosticListener("Fake"),
                new TestLoggingDefinitions(),
                new NullDbContextLogger());
            return modelLogger;
        }

        private InternalEntityTypeBuilder CreateInternalEntityBuilder<T>()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.Convention);

            var context = new ConventionContext<IConventionEntityTypeBuilder>(modelBuilder.Metadata.ConventionDispatcher);
            new PropertyDiscoveryConvention(CreateDependencies())
                .ProcessEntityTypeAdded(entityBuilder, context);

            return entityBuilder;
        }

        private class EntityWithNoId
        {
            public string Name { get; set; }
            public DateTime ModifiedDate { get; set; }
        }
    }
}
