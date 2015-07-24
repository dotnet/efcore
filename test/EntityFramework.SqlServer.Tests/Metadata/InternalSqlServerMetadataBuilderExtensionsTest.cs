// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata
{
    public class InternalSqlServerMetadataBuilderExtensionsTest
    {
        private InternalModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model(), new ConventionSet());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            Assert.True(builder.SqlServer(ConfigurationSource.Convention).IdentityStrategy(SqlServerIdentityStrategy.SequenceHiLo));
            Assert.Equal(SqlServerIdentityStrategy.SequenceHiLo, builder.Metadata.SqlServer().IdentityStrategy);

            Assert.True(builder.SqlServer(ConfigurationSource.DataAnnotation).IdentityStrategy(SqlServerIdentityStrategy.IdentityColumn));
            Assert.Equal(SqlServerIdentityStrategy.IdentityColumn, builder.Metadata.SqlServer().IdentityStrategy);

            Assert.False(builder.SqlServer(ConfigurationSource.Convention).IdentityStrategy(SqlServerIdentityStrategy.SequenceHiLo));
            Assert.Equal(SqlServerIdentityStrategy.IdentityColumn, builder.Metadata.SqlServer().IdentityStrategy);

            Assert.Equal(1, builder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.True(typeBuilder.SqlServer(ConfigurationSource.Convention).ToTable("Splew"));
            Assert.Equal("Splew", typeBuilder.Metadata.SqlServer().TableName);

            Assert.True(typeBuilder.SqlServer(ConfigurationSource.DataAnnotation).ToTable("Splow"));
            Assert.Equal("Splow", typeBuilder.Metadata.SqlServer().TableName);

            Assert.False(typeBuilder.SqlServer(ConfigurationSource.Convention).ToTable("Splod"));
            Assert.Equal("Splow", typeBuilder.Metadata.SqlServer().TableName);

            Assert.Equal(1, typeBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Id", typeof(int), ConfigurationSource.Convention);

            Assert.True(propertyBuilder.SqlServer(ConfigurationSource.Convention).HiLoSequenceName("Splew"));
            Assert.Equal("Splew", propertyBuilder.Metadata.SqlServer().HiLoSequenceName);

            Assert.True(propertyBuilder.SqlServer(ConfigurationSource.DataAnnotation).HiLoSequenceName("Splow"));
            Assert.Equal("Splow", propertyBuilder.Metadata.SqlServer().HiLoSequenceName);

            Assert.False(propertyBuilder.SqlServer(ConfigurationSource.Convention).HiLoSequenceName("Splod"));
            Assert.Equal("Splow", propertyBuilder.Metadata.SqlServer().HiLoSequenceName);

            Assert.Equal(1, propertyBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            var property = entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).Metadata;
            var keyBuilder = entityTypeBuilder.Key(new[] { property }, ConfigurationSource.Convention);

            Assert.True(keyBuilder.SqlServer(ConfigurationSource.Convention).Clustered(true));
            Assert.True(keyBuilder.Metadata.SqlServer().IsClustered);

            Assert.True(keyBuilder.SqlServer(ConfigurationSource.DataAnnotation).Clustered(false));
            Assert.False(keyBuilder.Metadata.SqlServer().IsClustered);

            Assert.False(keyBuilder.SqlServer(ConfigurationSource.Convention).Clustered(true));
            Assert.False(keyBuilder.Metadata.SqlServer().IsClustered);

            Assert.Equal(1, keyBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var indexBuilder = entityTypeBuilder.Index(new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(indexBuilder.SqlServer(ConfigurationSource.Convention).Clustered(true));
            Assert.True(indexBuilder.Metadata.SqlServer().IsClustered);

            Assert.True(indexBuilder.SqlServer(ConfigurationSource.DataAnnotation).Clustered(false));
            Assert.False(indexBuilder.Metadata.SqlServer().IsClustered);

            Assert.False(indexBuilder.SqlServer(ConfigurationSource.Convention).Clustered(true));
            Assert.False(indexBuilder.Metadata.SqlServer().IsClustered);

            Assert.Equal(1, indexBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var relationshipBuilder = entityTypeBuilder.ForeignKey("Splot", new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(relationshipBuilder.SqlServer(ConfigurationSource.Convention).Name("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.SqlServer().Name);

            Assert.True(relationshipBuilder.SqlServer(ConfigurationSource.DataAnnotation).Name("Splow"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.SqlServer().Name);

            Assert.False(relationshipBuilder.SqlServer(ConfigurationSource.Convention).Name("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.SqlServer().Name);

            Assert.Equal(1, relationshipBuilder.Metadata.Annotations.Count(
                a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        private class Splot
        {
        }
    }
}
