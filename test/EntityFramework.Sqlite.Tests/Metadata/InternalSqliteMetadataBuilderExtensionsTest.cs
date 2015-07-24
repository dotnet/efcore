// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Sqlite.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class InternalSqliteMetadataBuilderExtensionsTest
    {
        private InternalModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model(), new ConventionSet());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            builder.Sqlite(ConfigurationSource.Convention).GetOrAddSequence("Mine").IncrementBy = 77;

            Assert.Equal(77, builder.Metadata.Sqlite().FindSequence("Mine").IncrementBy);

            Assert.Equal(1, builder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqliteAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.True(typeBuilder.Sqlite(ConfigurationSource.Convention).ToTable("Splew"));
            Assert.Equal("Splew", typeBuilder.Metadata.Sqlite().TableName);

            Assert.True(typeBuilder.Sqlite(ConfigurationSource.DataAnnotation).ToTable("Splow"));
            Assert.Equal("Splow", typeBuilder.Metadata.Sqlite().TableName);

            Assert.False(typeBuilder.Sqlite(ConfigurationSource.Convention).ToTable("Splod"));
            Assert.Equal("Splow", typeBuilder.Metadata.Sqlite().TableName);

            Assert.Equal(1, typeBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqliteAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Id", typeof(int), ConfigurationSource.Convention);

            Assert.True(propertyBuilder.Sqlite(ConfigurationSource.Convention).ColumnName("Splew"));
            Assert.Equal("Splew", propertyBuilder.Metadata.Sqlite().ColumnName);

            Assert.True(propertyBuilder.Sqlite(ConfigurationSource.DataAnnotation).ColumnName("Splow"));
            Assert.Equal("Splow", propertyBuilder.Metadata.Sqlite().ColumnName);

            Assert.False(propertyBuilder.Sqlite(ConfigurationSource.Convention).ColumnName("Splod"));
            Assert.Equal("Splow", propertyBuilder.Metadata.Sqlite().ColumnName);

            Assert.Equal(1, propertyBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqliteAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            var property = entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).Metadata;
            var keyBuilder = entityTypeBuilder.Key(new[] { property }, ConfigurationSource.Convention);

            Assert.True(keyBuilder.Sqlite(ConfigurationSource.Convention).Name("Splew"));
            Assert.Equal("Splew", keyBuilder.Metadata.Sqlite().Name);

            Assert.True(keyBuilder.Sqlite(ConfigurationSource.DataAnnotation).Name("Splow"));
            Assert.Equal("Splow", keyBuilder.Metadata.Sqlite().Name);

            Assert.False(keyBuilder.Sqlite(ConfigurationSource.Convention).Name("Splod"));
            Assert.Equal("Splow", keyBuilder.Metadata.Sqlite().Name);

            Assert.Equal(1, keyBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqliteAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var indexBuilder = entityTypeBuilder.Index(new[] { "Id" }, ConfigurationSource.Convention);

            indexBuilder.Sqlite(ConfigurationSource.Convention).Name("Splew");
            Assert.Equal("Splew", indexBuilder.Metadata.Sqlite().Name);

            indexBuilder.Sqlite(ConfigurationSource.DataAnnotation).Name("Splow");
            Assert.Equal("Splow", indexBuilder.Metadata.Sqlite().Name);

            indexBuilder.Sqlite(ConfigurationSource.Convention).Name("Splod");
            Assert.Equal("Splow", indexBuilder.Metadata.Sqlite().Name);

            Assert.Equal(1, indexBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqliteAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var relationshipBuilder = entityTypeBuilder.ForeignKey("Splot", new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(relationshipBuilder.Sqlite(ConfigurationSource.Convention).Name("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.Sqlite().Name);

            Assert.True(relationshipBuilder.Sqlite(ConfigurationSource.DataAnnotation).Name("Splow"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.Sqlite().Name);

            Assert.False(relationshipBuilder.Sqlite(ConfigurationSource.Convention).Name("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.Sqlite().Name);

            Assert.Equal(1, relationshipBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqliteAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        private class Splot
        {
        }
    }
}
