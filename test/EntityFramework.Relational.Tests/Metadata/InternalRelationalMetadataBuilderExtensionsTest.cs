// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class InternalRelationalMetadataBuilderExtensionsTest
    {
        private InternalModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model(), new ConventionSet());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            builder.Relational(ConfigurationSource.Convention).GetOrAddSequence("Mine").IncrementBy = 77;

            Assert.Equal(77, builder.Metadata.Relational().FindSequence("Mine").IncrementBy);
        }

        [Fact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            typeBuilder.Relational(ConfigurationSource.Convention).TableName = "Splew";
            Assert.Equal("Splew", typeBuilder.Metadata.Relational().TableName);

            typeBuilder.Relational(ConfigurationSource.DataAnnotation).TableName = "Splow";
            Assert.Equal("Splow", typeBuilder.Metadata.Relational().TableName);

            typeBuilder.Relational(ConfigurationSource.Convention).TableName = "Splod";
            Assert.Equal("Splow", typeBuilder.Metadata.Relational().TableName);
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Id", typeof(int), ConfigurationSource.Convention);

            propertyBuilder.Relational(ConfigurationSource.Convention).ColumnName = "Splew";
            Assert.Equal("Splew", propertyBuilder.Metadata.Relational().ColumnName);

            propertyBuilder.Relational(ConfigurationSource.DataAnnotation).ColumnName = "Splow";
            Assert.Equal("Splow", propertyBuilder.Metadata.Relational().ColumnName);

            propertyBuilder.Relational(ConfigurationSource.Convention).ColumnName = "Splod";
            Assert.Equal("Splow", propertyBuilder.Metadata.Relational().ColumnName);
        }

        [Fact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            var property = entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).Metadata;
            var keyBuilder = entityTypeBuilder.Key(new[] { property }, ConfigurationSource.Convention);

            keyBuilder.Relational(ConfigurationSource.Convention).Name = "Splew";
            Assert.Equal("Splew", keyBuilder.Metadata.Relational().Name);

            keyBuilder.Relational(ConfigurationSource.DataAnnotation).Name = "Splow";
            Assert.Equal("Splow", keyBuilder.Metadata.Relational().Name);

            keyBuilder.Relational(ConfigurationSource.Convention).Name = "Splod";
            Assert.Equal("Splow", keyBuilder.Metadata.Relational().Name);
        }

        [Fact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var indexBuilder = entityTypeBuilder.Index(new[] { "Id" }, ConfigurationSource.Convention);

            indexBuilder.Relational(ConfigurationSource.Convention).Name = "Splew";
            Assert.Equal("Splew", indexBuilder.Metadata.Relational().Name);

            indexBuilder.Relational(ConfigurationSource.DataAnnotation).Name = "Splow";
            Assert.Equal("Splow", indexBuilder.Metadata.Relational().Name);

            indexBuilder.Relational(ConfigurationSource.Convention).Name = "Splod";
            Assert.Equal("Splow", indexBuilder.Metadata.Relational().Name);
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var relationshipBuilder = entityTypeBuilder.ForeignKey("Splot", new[] { "Id" }, ConfigurationSource.Convention);

            relationshipBuilder.Relational(ConfigurationSource.Convention).Name = "Splew";
            Assert.Equal("Splew", relationshipBuilder.Metadata.Relational().Name);

            relationshipBuilder.Relational(ConfigurationSource.DataAnnotation).Name = "Splow";
            Assert.Equal("Splow", relationshipBuilder.Metadata.Relational().Name);

            relationshipBuilder.Relational(ConfigurationSource.Convention).Name = "Splod";
            Assert.Equal("Splow", relationshipBuilder.Metadata.Relational().Name);
        }

        private class Splot
        {
        }
    }
}
