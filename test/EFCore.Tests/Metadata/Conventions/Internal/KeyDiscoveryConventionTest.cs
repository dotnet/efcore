// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class KeyDiscoveryConventionTest
    {
        [Fact]
        public void Primary_key_is_not_set_when_zero_key_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();

            Assert.Same(entityBuilder, CreateKeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.Null(key);
        }

        [Fact]
        public void Primary_key_is_set_when_shadow_property_not_defined_by_convention_matches()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();
            var propertyBuilder = entityBuilder.Property("Id", typeof(int), ConfigurationSource.DataAnnotation);

            Assert.Same(propertyBuilder, CreateKeyDiscoveryConvention().Apply(propertyBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        [Fact]
        public void Primary_key_is_not_set_when_shadow_property_defined_by_convention_matches()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();
            var propertyBuilder = entityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);

            Assert.Same(propertyBuilder, CreateKeyDiscoveryConvention().Apply(propertyBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.Null(key);
        }

        private class EntityWithId
        {
            public int Id { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_discovers_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithId>();

            Assert.Same(entityBuilder, CreateKeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithTypeId
        {
            public int EntityWithTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_discovers_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithTypeId>();

            Assert.Same(entityBuilder, CreateKeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "EntityWithTypeIdId" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithIdAndTypeId
        {
            public int Id { get; set; }
            public int EntityWithIdAndTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_prefers_id_over_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithIdAndTypeId>();

            Assert.Same(entityBuilder, CreateKeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithMultipleIds
        {
            public int ID { get; set; }
            public int Id { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_does_not_discover_key_when_multiple_ids()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithMultipleIds>();

            Assert.Same(entityBuilder, CreateKeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.FindPrimaryKey();
            Assert.Null(key);

            Assert.Equal(1, Log.Count);
            Assert.Equal(LogLevel.Information, Log[0].Level);
            Assert.Equal(CoreStrings.LogMultiplePrimaryKeyCandidates.GenerateMessage(
                nameof(EntityWithMultipleIds.ID), nameof(EntityWithMultipleIds.Id), nameof(EntityWithMultipleIds)), Log[0].Message);
        }

        public List<(LogLevel Level, EventId Id, string Message)> Log { get; }
            = new List<(LogLevel, EventId, string)>();

        private KeyDiscoveryConvention CreateKeyDiscoveryConvention() => new KeyDiscoveryConvention(CreateLogger());

        private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
            var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
                new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Name),
                options,
                new DiagnosticListener("Fake"));
            return modelLogger;
        }

        private static InternalEntityTypeBuilder CreateInternalEntityBuilder<T>()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.Convention);

            new PropertyDiscoveryConvention(
                TestServiceFactory.Instance.Create<CoreTypeMapper>()).Apply(entityBuilder);

            return entityBuilder;
        }

        private class EntityWithNoId
        {
            public string Name { get; set; }
            public DateTime ModifiedDate { get; set; }
        }
    }
}
