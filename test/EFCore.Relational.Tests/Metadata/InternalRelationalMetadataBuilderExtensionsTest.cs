// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class InternalRelationalMetadataBuilderExtensionsTest
    {
        private InternalModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            builder.Relational(ConfigurationSource.Convention).GetOrAddSequence("Mine").IncrementBy = 77;

            Assert.Equal(77, builder.Metadata.Relational().FindSequence("Mine").IncrementBy);
        }

        [Fact]
        public void Can_set_table_name()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.True(typeBuilder.Relational(ConfigurationSource.Convention).ToTable("Splew"));
            Assert.Equal("Splew", typeBuilder.Metadata.Relational().TableName);

            Assert.True(typeBuilder.Relational(ConfigurationSource.DataAnnotation).ToTable("Splow"));
            Assert.Equal("Splow", typeBuilder.Metadata.Relational().TableName);

            Assert.False(typeBuilder.Relational(ConfigurationSource.Convention).ToTable("Splod"));
            Assert.Equal("Splow", typeBuilder.Metadata.Relational().TableName);
        }

        [Fact]
        public void Can_set_table_name_and_schema()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.True(typeBuilder.Relational(ConfigurationSource.Convention).ToTable("Splew", "1"));
            Assert.Equal("Splew", typeBuilder.Metadata.Relational().TableName);
            Assert.Equal("1", typeBuilder.Metadata.Relational().Schema);

            Assert.True(typeBuilder.Relational(ConfigurationSource.DataAnnotation).ToTable("Splow", "2"));
            Assert.Equal("Splow", typeBuilder.Metadata.Relational().TableName);
            Assert.Equal("2", typeBuilder.Metadata.Relational().Schema);

            Assert.False(typeBuilder.Relational(ConfigurationSource.Convention).ToTable("Splod", "3"));
            Assert.Equal("Splow", typeBuilder.Metadata.Relational().TableName);
            Assert.Equal("2", typeBuilder.Metadata.Relational().Schema);
        }

        [Fact]
        public void Can_override_existing_schema()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            typeBuilder.Metadata.Relational().Schema = "Explicit";

            Assert.False(typeBuilder.Relational(ConfigurationSource.DataAnnotation).ToTable("Splod", "2"));
            Assert.Equal("Splot", typeBuilder.Metadata.Relational().TableName);
            Assert.Equal("Explicit", typeBuilder.Metadata.Relational().Schema);

            Assert.True(typeBuilder.Relational(ConfigurationSource.DataAnnotation).ToTable("Splod", "Explicit"));
            Assert.Equal("Splod", typeBuilder.Metadata.Relational().TableName);
            Assert.Equal("Explicit", typeBuilder.Metadata.Relational().Schema);

            Assert.True(typeBuilder.Relational(ConfigurationSource.Explicit).ToTable("Splew", "1"));
            Assert.Equal("Splew", typeBuilder.Metadata.Relational().TableName);
            Assert.Equal("1", typeBuilder.Metadata.Relational().Schema);
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Id", typeof(int), ConfigurationSource.Convention);

            Assert.True(propertyBuilder.Relational(ConfigurationSource.Convention).IsFixedLength(true));
            Assert.True(propertyBuilder.Metadata.Relational().IsFixedLength);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.Convention).HasColumnName("Splew"));
            Assert.Equal("Splew", propertyBuilder.Metadata.Relational().ColumnName);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.Convention).HasColumnType("int"));
            Assert.Equal("int", propertyBuilder.Metadata.Relational().ColumnType);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.Convention).HasDefaultValue(1));
            Assert.Equal(1, propertyBuilder.Metadata.Relational().DefaultValue);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.Convention).HasDefaultValueSql("2"));
            Assert.Equal("2", propertyBuilder.Metadata.Relational().DefaultValueSql);
            Assert.Null(propertyBuilder.Metadata.Relational().DefaultValue);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.Convention).HasComputedColumnSql("3"));
            Assert.Equal("3", propertyBuilder.Metadata.Relational().ComputedColumnSql);
            Assert.Null(propertyBuilder.Metadata.Relational().DefaultValueSql);

            Assert.True(propertyBuilder.Relational(ConfigurationSource.DataAnnotation).IsFixedLength(false));
            Assert.False(propertyBuilder.Relational(ConfigurationSource.Convention).IsFixedLength(true));
            Assert.False(propertyBuilder.Metadata.Relational().IsFixedLength);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.DataAnnotation).HasColumnName("Splow"));
            Assert.False(propertyBuilder.Relational(ConfigurationSource.Convention).HasColumnName("Splod"));
            Assert.Equal("Splow", propertyBuilder.Metadata.Relational().ColumnName);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.DataAnnotation).HasColumnType("varchar"));
            Assert.False(propertyBuilder.Relational(ConfigurationSource.Convention).HasColumnType("int"));
            Assert.Equal("varchar", propertyBuilder.Metadata.Relational().ColumnType);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.DataAnnotation).HasDefaultValue(0));
            Assert.False(propertyBuilder.Relational(ConfigurationSource.Convention).HasDefaultValue(1));
            Assert.Equal(0, propertyBuilder.Metadata.Relational().DefaultValue);
            Assert.Null(propertyBuilder.Metadata.Relational().ComputedColumnSql);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.DataAnnotation).HasDefaultValueSql("NULL"));
            Assert.False(propertyBuilder.Relational(ConfigurationSource.Convention).HasDefaultValueSql("2"));
            Assert.Equal("NULL", propertyBuilder.Metadata.Relational().DefaultValueSql);
            Assert.True(propertyBuilder.Relational(ConfigurationSource.DataAnnotation).HasComputedColumnSql("runthis()"));
            Assert.False(propertyBuilder.Relational(ConfigurationSource.Convention).HasComputedColumnSql("3"));
            Assert.Equal("runthis()", propertyBuilder.Metadata.Relational().ComputedColumnSql);
        }

        [Fact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            var idProperty = entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).Metadata;
            var keyBuilder = entityTypeBuilder.HasKey(new[] { idProperty.Name }, ConfigurationSource.Convention);

            Assert.True(keyBuilder.Relational(ConfigurationSource.Convention).HasName("Splew"));
            Assert.Equal("Splew", keyBuilder.Metadata.Relational().Name);

            Assert.True(keyBuilder.Relational(ConfigurationSource.DataAnnotation).HasName("Splow"));
            Assert.Equal("Splow", keyBuilder.Metadata.Relational().Name);

            Assert.False(keyBuilder.Relational(ConfigurationSource.Convention).HasName("Splod"));
            Assert.Equal("Splow", keyBuilder.Metadata.Relational().Name);
        }

        [Fact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(indexBuilder.Relational(ConfigurationSource.Convention).HasName("Splew"));
            Assert.Equal("Splew", indexBuilder.Metadata.Relational().Name);

            Assert.True(indexBuilder.Relational(ConfigurationSource.DataAnnotation).HasName("Splow"));
            Assert.Equal("Splow", indexBuilder.Metadata.Relational().Name);

            Assert.False(indexBuilder.Relational(ConfigurationSource.Convention).HasName("Splod"));
            Assert.Equal("Splow", indexBuilder.Metadata.Relational().Name);
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var relationshipBuilder = entityTypeBuilder.HasForeignKey("Splot", new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(relationshipBuilder.Relational(ConfigurationSource.Convention).HasConstraintName("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.Relational().Name);

            Assert.True(relationshipBuilder.Relational(ConfigurationSource.DataAnnotation).HasConstraintName("Splow"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.Relational().Name);

            Assert.False(relationshipBuilder.Relational(ConfigurationSource.Convention).HasConstraintName("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.Relational().Name);
        }

        [Fact]
        public void Can_access_discriminator()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator());
            Assert.Equal("Discriminator", typeBuilder.Metadata.Relational().DiscriminatorProperty.Name);
            Assert.Equal(typeof(string), typeBuilder.Metadata.Relational().DiscriminatorProperty.ClrType);

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator((PropertyInfo)null));
            Assert.Null(typeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Equal(0, typeBuilder.Metadata.GetProperties().Count());

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator("Splod", typeof(int?)));
            Assert.Equal("Splod", typeBuilder.Metadata.Relational().DiscriminatorProperty.Name);
            Assert.Equal(typeof(int?), typeBuilder.Metadata.Relational().DiscriminatorProperty.ClrType);
            Assert.Equal("Splod", typeBuilder.Metadata.GetProperties().Single().Name);

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.DataAnnotation).HasDiscriminator(Splot.SplowedProperty));
            Assert.Equal(Splot.SplowedProperty.Name, typeBuilder.Metadata.Relational().DiscriminatorProperty.Name);
            Assert.Equal(typeof(int?), typeBuilder.Metadata.Relational().DiscriminatorProperty.ClrType);
            Assert.Equal(Splot.SplowedProperty.Name, typeBuilder.Metadata.GetProperties().Single().Name);

            Assert.Null(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator("Splew", typeof(int?)));
            Assert.Equal(Splot.SplowedProperty.Name, typeBuilder.Metadata.Relational().DiscriminatorProperty.Name);
            Assert.Equal(typeof(int?), typeBuilder.Metadata.Relational().DiscriminatorProperty.ClrType);

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.DataAnnotation).HasDiscriminator(typeof(int)));
            Assert.Null(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator(typeof(long)));
            Assert.Equal("Discriminator", typeBuilder.Metadata.Relational().DiscriminatorProperty.Name);
            Assert.Equal(typeof(int), typeBuilder.Metadata.Relational().DiscriminatorProperty.ClrType);

            Assert.Null(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator((PropertyInfo)null));
        }

        [Fact]
        public void Discriminator_is_not_set_if_ignored()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
            typeBuilder.Ignore("Splod", ConfigurationSource.Explicit);

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator("Splew", typeof(string)));
            Assert.Equal("Splew", typeBuilder.Metadata.Relational().DiscriminatorProperty.Name);
            Assert.Equal(typeof(string), typeBuilder.Metadata.Relational().DiscriminatorProperty.ClrType);

            Assert.Null(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator("Splod", typeof(int?)));
            Assert.Equal("Splew", typeBuilder.Metadata.Relational().DiscriminatorProperty.Name);
            Assert.Equal(typeof(string), typeBuilder.Metadata.Relational().DiscriminatorProperty.ClrType);
        }

        [Fact]
        public void Discriminator_is_not_set_if_default_ignored()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
            typeBuilder.Ignore("Discriminator", ConfigurationSource.Explicit);

            Assert.Null(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator());
            Assert.Equal(0, typeBuilder.Metadata.GetProperties().Count());
        }

        [Fact]
        public void Can_access_discriminator_value()
        {
            var typeBuilder = CreateBuilder().Entity("Splot", ConfigurationSource.Convention);
            var derivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention);
            derivedTypeBuilder.HasBaseType(typeBuilder.Metadata, ConfigurationSource.DataAnnotation);

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator());
            Assert.Equal(1, typeBuilder.Metadata.GetDeclaredProperties().Count());
            Assert.Equal(0, derivedTypeBuilder.Metadata.GetDeclaredProperties().Count());

            var discriminatorBuilder = typeBuilder.Relational(ConfigurationSource.Convention)
                .HasDiscriminator(Splot.SplowedProperty.Name, Splot.SplowedProperty.PropertyType);
            Assert.NotNull(discriminatorBuilder.HasValue("Splot", 1));
            Assert.NotNull(discriminatorBuilder.HasValue("Splow", 2));
            Assert.NotNull(discriminatorBuilder.HasValue("Splod", 3));
            Assert.Equal(1, typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                2, typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                3, typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Same(typeBuilder.Metadata, typeBuilder.ModelBuilder.Metadata.FindEntityType("Splow").BaseType);

            discriminatorBuilder = typeBuilder.Relational(ConfigurationSource.DataAnnotation).HasDiscriminator();
            Assert.NotNull(discriminatorBuilder.HasValue("Splot", 4));
            Assert.NotNull(discriminatorBuilder.HasValue("Splow", 5));
            Assert.NotNull(discriminatorBuilder.HasValue("Splod", 6));
            Assert.Equal(4, typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                5, typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                6, typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);

            discriminatorBuilder = typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator();
            Assert.Null(discriminatorBuilder.HasValue("Splot", 1));
            Assert.Null(discriminatorBuilder.HasValue("Splow", 2));
            Assert.Null(discriminatorBuilder.HasValue("Splod", 3));
            Assert.Equal(4, typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                5, typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                6, typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator((Type)null));
            Assert.Null(typeBuilder.Metadata.Relational().DiscriminatorProperty);
            Assert.Equal(4, typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Empty(typeBuilder.Metadata.GetProperties());
        }

        [Fact]
        public void Changing_discriminator_type_removes_values()
        {
            var typeBuilder = CreateBuilder().Entity("Splot", ConfigurationSource.Convention);
            var derivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention);
            derivedTypeBuilder.HasBaseType(typeBuilder.Metadata, ConfigurationSource.DataAnnotation);

            Assert.NotNull(typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator());
            Assert.Equal(1, typeBuilder.Metadata.GetDeclaredProperties().Count());
            Assert.Equal(0, derivedTypeBuilder.Metadata.GetDeclaredProperties().Count());

            var discriminatorBuilder = typeBuilder.Relational(ConfigurationSource.Convention)
                .HasDiscriminator("Splowed", typeof(int));
            Assert.NotNull(discriminatorBuilder.HasValue("Splot", 1));
            Assert.NotNull(discriminatorBuilder.HasValue("Splow", 2));
            Assert.NotNull(discriminatorBuilder.HasValue("Splod", 3));

            discriminatorBuilder = typeBuilder.Relational(ConfigurationSource.Convention)
                .HasDiscriminator("Splowed", typeof(string));
            Assert.Null(typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Null(
                typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Null(
                typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.NotNull(discriminatorBuilder.HasValue("Splot", "4"));
            Assert.NotNull(discriminatorBuilder.HasValue("Splow", "5"));
            Assert.NotNull(discriminatorBuilder.HasValue("Splod", "6"));

            discriminatorBuilder = typeBuilder.Relational(ConfigurationSource.Convention)
                .HasDiscriminator("Splotted", typeof(string));

            Assert.NotNull(discriminatorBuilder);
            Assert.Equal("4", typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                "5", typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                "6", typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);

            discriminatorBuilder = typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator(typeof(int));

            Assert.NotNull(discriminatorBuilder);
            Assert.Null(typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Null(
                typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Null(
                typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
        }

        [Fact]
        public void Can_access_discriminator_value_generic()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            var discriminatorBuilder = new DiscriminatorBuilder<int?>(
                typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator(Splot.SplowedProperty));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splot), 1));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splow), 2));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splod), 3));
            Assert.Equal(1, typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                2, typeBuilder.ModelBuilder.Entity(typeof(Splow), ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                3, typeBuilder.ModelBuilder.Entity(typeof(Splod), ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);

            discriminatorBuilder = new DiscriminatorBuilder<int?>(
                typeBuilder.Relational(ConfigurationSource.DataAnnotation).HasDiscriminator());
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splot), 4));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splow), 5));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splod), 6));
            Assert.Equal(4, typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                5, typeBuilder.ModelBuilder.Entity(typeof(Splow), ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                6, typeBuilder.ModelBuilder.Entity(typeof(Splod), ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);

            discriminatorBuilder = new DiscriminatorBuilder<int?>(
                typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator());
            Assert.Null(discriminatorBuilder.HasValue(typeof(Splot), 1));
            Assert.Null(discriminatorBuilder.HasValue(typeof(Splow), 2));
            Assert.Null(discriminatorBuilder.HasValue(typeof(Splod), 3));
            Assert.Equal(4, typeBuilder.Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                5, typeBuilder.ModelBuilder.Entity(typeof(Splow), ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
            Assert.Equal(
                6, typeBuilder.ModelBuilder.Entity(typeof(Splod), ConfigurationSource.Convention)
                    .Metadata.Relational().DiscriminatorValue);
        }

        [Fact]
        public void DiscriminatorValue_throws_if_base_cannot_be_set()
        {
            var modelBuilder = CreateBuilder();
            var typeBuilder = modelBuilder.Entity("Splot", ConfigurationSource.Convention);
            var nonDerivedTypeBuilder = modelBuilder.Entity("Splow", ConfigurationSource.Convention);
            nonDerivedTypeBuilder.HasBaseType(
                modelBuilder.Entity("Splod", ConfigurationSource.Convention).Metadata, ConfigurationSource.Explicit);

            var discriminatorBuilder = typeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator();
            Assert.Equal(
                RelationalStrings.DiscriminatorEntityTypeNotDerived("Splow", "Splot"),
                Assert.Throws<InvalidOperationException>(() => discriminatorBuilder.HasValue("Splow", "1")).Message);
        }

        private class Splot
        {
            public static readonly PropertyInfo SplowedProperty = typeof(Splot).GetProperty("Splowed");

            public int? Splowed { get; set; }
        }

        private class Splow : Splot
        {
        }

        private class Splod : Splow
        {
        }
    }
}
