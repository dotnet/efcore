// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata.ModelConventions
{
    class SqlServerKeyConventionTest
    {
        public class SampleEntity
        {
            public int Id { get; set; }
            public int SampleEntityId { get; set; }
            public int Number { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void Identity_annotation_is_set_for_primary_key()
        {
            var entityBuilder = CreateInternalEntityBuilder<SampleEntity>();

            var keyBuilder = entityBuilder.PrimaryKey(new List<string>() { "Id" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerKeyConvention().Apply(keyBuilder));

            var property = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).Metadata;

            Assert.Equal(1, property.Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Identity.ToString(), property[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration]);
        }

        [Fact]
        public void Identity_annotation_is_not_set_for_non_primary_key()
        {
            var entityBuilder = CreateInternalEntityBuilder<SampleEntity>();

            var keyBuilder = entityBuilder.Key(new List<string>() { "Id" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerKeyConvention().Apply(keyBuilder));

            Assert.Equal(0, entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).Metadata.Annotations.Count());
        }

        [Fact]
        public void DiscoverIdentityProperty_discovers_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<SampleEntity>();

            var keyBuilder = entityBuilder.PrimaryKey(new List<string>() { "Id", "Name" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerKeyConvention().Apply(keyBuilder));

            var property = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).Metadata;

            Assert.Equal(1, property.Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Identity.ToString(), property[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration]);
        }

        [Fact]
        public void DiscoverIdentityProperty_discovers_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<SampleEntity>();

            var keyBuilder = entityBuilder.PrimaryKey(new List<string>() { "SampleEntityId", "Name" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerKeyConvention().Apply(keyBuilder));

            var property = entityBuilder.Property(typeof(int), "SampleEntityId", ConfigurationSource.Convention).Metadata;

            Assert.Equal(1, property.Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Identity.ToString(), property[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration]);
        }

        [Fact]
        public void DiscoverIdentityProperty_prefers_id_over_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<SampleEntity>();

            var keyBuilder = entityBuilder.PrimaryKey(new List<string>() { "Id", "SampleEntityId", "Name" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerKeyConvention().Apply(keyBuilder));

            var property = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).Metadata;

            Assert.Equal(1, property.Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Identity.ToString(), property[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration]);
        }

        [Fact]
        public void DiscoverIdentityProperty_chooses_first_integer_property_when_no_id_found()
        {
            var entityBuilder = CreateInternalEntityBuilder<SampleEntity>();

            var keyBuilder = entityBuilder.PrimaryKey(new List<string>() { "Number", "Name" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerKeyConvention().Apply(keyBuilder));

            var property = entityBuilder.Property(typeof(int), "Number", ConfigurationSource.Convention).Metadata;

            Assert.Equal(1, property.Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Identity.ToString(), property[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration]);
        }

        [Fact]
        public void No_annotation_set_when_no_integer_property_found_in_key()
        {
            var entityBuilder = CreateInternalEntityBuilder<SampleEntity>();

            var keyBuilder = entityBuilder.PrimaryKey(new List<string>() { "Name" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerKeyConvention().Apply(keyBuilder));

            var property = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Convention).Metadata;

            Assert.Equal(0, property.Annotations.Count());
        }

        private static InternalEntityBuilder CreateInternalEntityBuilder<T>()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.Convention);

            new PropertiesConvention().Apply(entityBuilder);

            return entityBuilder;
        }
    }
}
