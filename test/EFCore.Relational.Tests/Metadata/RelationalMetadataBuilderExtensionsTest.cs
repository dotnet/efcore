// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalMetadataBuilderExtensionsTest
    {
        private InternalModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            ((IMutableModel)builder.Metadata).AddSequence("Mine").IncrementBy = 77;

            Assert.Equal(77, ((IMutableModel)builder.Metadata).FindSequence("Mine").IncrementBy);
        }

        [Fact]
        public void Can_set_table_name()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.NotNull(typeBuilder.ToTable("Splew"));
            Assert.Equal("Splew", typeBuilder.Metadata.GetTableName());

            Assert.NotNull(typeBuilder.ToTable("Splow", fromDataAnnotation: true));
            Assert.Equal("Splow", typeBuilder.Metadata.GetTableName());

            Assert.Null(typeBuilder.ToTable("Splod"));
            Assert.Equal("Splow", typeBuilder.Metadata.GetTableName());
        }

        [Fact]
        public void Can_set_table_name_and_schema()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.NotNull(typeBuilder.ToTable("Splew", "1"));
            Assert.Equal("Splew", typeBuilder.Metadata.GetTableName());
            Assert.Equal("1", typeBuilder.Metadata.GetSchema());

            Assert.NotNull(typeBuilder.ToTable("Splow", "2", fromDataAnnotation: true));
            Assert.Equal("Splow", typeBuilder.Metadata.GetTableName());
            Assert.Equal("2", typeBuilder.Metadata.GetSchema());

            Assert.Null(typeBuilder.ToTable("Splod", "3"));
            Assert.Equal("Splow", typeBuilder.Metadata.GetTableName());
            Assert.Equal("2", typeBuilder.Metadata.GetSchema());
        }

        [Fact]
        public void Can_override_existing_schema()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            typeBuilder.Metadata.SetSchema("Explicit");

            Assert.Null(typeBuilder.ToTable("Splod", "2", fromDataAnnotation: true));
            Assert.Equal("Splot", typeBuilder.Metadata.GetTableName());
            Assert.Equal("Explicit", typeBuilder.Metadata.GetSchema());

            Assert.NotNull(typeBuilder.ToTable("Splod", "Explicit", fromDataAnnotation: true));
            Assert.Equal("Splod", typeBuilder.Metadata.GetTableName());
            Assert.Equal("Explicit", typeBuilder.Metadata.GetSchema());

            Assert.NotNull(new EntityTypeBuilder(typeBuilder.Metadata).ToTable("Splew", "1"));
            Assert.Equal("Splew", typeBuilder.Metadata.GetTableName());
            Assert.Equal("1", typeBuilder.Metadata.GetSchema());
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property(typeof(int), "Id", ConfigurationSource.Convention);

            Assert.NotNull(propertyBuilder.IsFixedLength(true));
            Assert.True(propertyBuilder.Metadata.IsFixedLength());
            Assert.NotNull(propertyBuilder.HasColumnName("Splew"));
            Assert.Equal("Splew", propertyBuilder.Metadata.GetColumnName());
            Assert.NotNull(propertyBuilder.HasColumnType("int"));
            Assert.Equal("int", propertyBuilder.Metadata.GetColumnType());
            Assert.NotNull(propertyBuilder.HasDefaultValue(1));
            Assert.Equal(1, propertyBuilder.Metadata.GetDefaultValue());
            Assert.NotNull(propertyBuilder.HasDefaultValueSql("2"));
            Assert.Equal("2", propertyBuilder.Metadata.GetDefaultValueSql());
            Assert.Equal(1, propertyBuilder.Metadata.GetDefaultValue());
            Assert.NotNull(propertyBuilder.HasComputedColumnSql("3"));
            Assert.Equal("3", propertyBuilder.Metadata.GetComputedColumnSql());
            Assert.Equal("2", propertyBuilder.Metadata.GetDefaultValueSql());

            Assert.NotNull(propertyBuilder.IsFixedLength(false, fromDataAnnotation: true));
            Assert.Null(propertyBuilder.IsFixedLength(true));
            Assert.False(propertyBuilder.Metadata.IsFixedLength());
            Assert.NotNull(propertyBuilder.HasColumnName("Splow", fromDataAnnotation: true));
            Assert.Null(propertyBuilder.HasColumnName("Splod"));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetColumnName());
            Assert.NotNull(propertyBuilder.HasColumnType("varchar", fromDataAnnotation: true));
            Assert.Null(propertyBuilder.HasColumnType("int"));
            Assert.Equal("varchar", propertyBuilder.Metadata.GetColumnType());
            Assert.NotNull(propertyBuilder.HasDefaultValue(0, fromDataAnnotation: true));
            Assert.Null(propertyBuilder.HasDefaultValue(1));
            Assert.Equal(0, propertyBuilder.Metadata.GetDefaultValue());
            Assert.NotNull(propertyBuilder.HasDefaultValueSql("NULL", fromDataAnnotation: true));
            Assert.Null(propertyBuilder.HasDefaultValueSql("2"));
            Assert.Equal("NULL", propertyBuilder.Metadata.GetDefaultValueSql());
            Assert.NotNull(propertyBuilder.HasComputedColumnSql("runthis()", fromDataAnnotation: true));
            Assert.Null(propertyBuilder.HasComputedColumnSql("3"));
            Assert.Equal("runthis()", propertyBuilder.Metadata.GetComputedColumnSql());
        }

        [Fact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            var idProperty = entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).Metadata;
            var keyBuilder = entityTypeBuilder.HasKey(new[] { idProperty.Name }, ConfigurationSource.Convention);

            Assert.NotNull(keyBuilder.HasName("Splew"));
            Assert.Equal("Splew", keyBuilder.Metadata.GetName());

            Assert.NotNull(keyBuilder.HasName("Splow", fromDataAnnotation: true));
            Assert.Equal("Splow", keyBuilder.Metadata.GetName());

            Assert.Null(keyBuilder.HasName("Splod"));
            Assert.Equal("Splow", keyBuilder.Metadata.GetName());
        }

        [Fact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { "Id" }, ConfigurationSource.Convention);

            Assert.NotNull(indexBuilder.HasName("Splew"));
            Assert.Equal("Splew", indexBuilder.Metadata.GetName());

            Assert.NotNull(indexBuilder.HasName("Splow", fromDataAnnotation: true));
            Assert.Equal("Splow", indexBuilder.Metadata.GetName());

            Assert.Null(indexBuilder.HasName("Splod"));
            Assert.Equal("Splow", indexBuilder.Metadata.GetName());
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
            var relationshipBuilder = entityTypeBuilder.HasRelationship("Splot", new[] { "Id" }, ConfigurationSource.Convention);

            Assert.NotNull(relationshipBuilder.HasConstraintName("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.GetConstraintName());

            Assert.NotNull(relationshipBuilder.HasConstraintName("Splow", fromDataAnnotation: true));
            Assert.Equal("Splow", relationshipBuilder.Metadata.GetConstraintName());

            Assert.Null(relationshipBuilder.HasConstraintName("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.GetConstraintName());
        }

        [Fact]
        public void Can_access_discriminator()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.NotNull(typeBuilder.HasDiscriminator());
            Assert.Equal("Discriminator", typeBuilder.Metadata.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), typeBuilder.Metadata.GetDiscriminatorProperty().ClrType);

            Assert.NotNull(typeBuilder.HasNoDeclaredDiscriminator());
            Assert.Null(typeBuilder.Metadata.GetDiscriminatorProperty());
            Assert.Equal(0, typeBuilder.Metadata.GetProperties().Count());

            Assert.NotNull(typeBuilder.HasDiscriminator("Splod", typeof(int?)));
            Assert.Equal("Splod", typeBuilder.Metadata.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(int?), typeBuilder.Metadata.GetDiscriminatorProperty().ClrType);
            Assert.Equal("Splod", typeBuilder.Metadata.GetProperties().Single().Name);

            Assert.NotNull(typeBuilder.HasDiscriminator(Splot.SplowedProperty, fromDataAnnotation: true));
            Assert.Equal(Splot.SplowedProperty.Name, typeBuilder.Metadata.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(int?), typeBuilder.Metadata.GetDiscriminatorProperty().ClrType);
            Assert.Equal(Splot.SplowedProperty.Name, typeBuilder.Metadata.GetProperties().Single().Name);

            Assert.Null(typeBuilder.HasDiscriminator("Splew", typeof(int?)));
            Assert.Equal(Splot.SplowedProperty.Name, typeBuilder.Metadata.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(int?), typeBuilder.Metadata.GetDiscriminatorProperty().ClrType);

            Assert.NotNull(typeBuilder.HasDiscriminator(typeof(int), fromDataAnnotation: true));
            Assert.Null(typeBuilder.HasDiscriminator(typeof(long)));
            Assert.Equal("Discriminator", typeBuilder.Metadata.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(int), typeBuilder.Metadata.GetDiscriminatorProperty().ClrType);

            Assert.Null(typeBuilder.HasNoDeclaredDiscriminator());
        }

        [Fact]
        public void Discriminator_is_not_set_if_ignored()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
            typeBuilder.Ignore("Splod", ConfigurationSource.Explicit);

            Assert.NotNull(typeBuilder.HasDiscriminator("Splew", typeof(string)));
            Assert.Equal("Splew", typeBuilder.Metadata.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), typeBuilder.Metadata.GetDiscriminatorProperty().ClrType);

            Assert.Null(typeBuilder.HasDiscriminator("Splod", typeof(int?)));
            Assert.Equal("Splew", typeBuilder.Metadata.GetDiscriminatorProperty().Name);
            Assert.Equal(typeof(string), typeBuilder.Metadata.GetDiscriminatorProperty().ClrType);
        }

        [Fact]
        public void Discriminator_is_not_set_if_default_ignored()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
            typeBuilder.Ignore("Discriminator", ConfigurationSource.Explicit);

            Assert.Null(typeBuilder.HasDiscriminator());
            Assert.Equal(0, typeBuilder.Metadata.GetProperties().Count());
        }

        [Fact]
        public void Can_access_discriminator_value()
        {
            var typeBuilder = CreateBuilder().Entity("Splot", ConfigurationSource.Convention);
            var derivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention);
            derivedTypeBuilder.HasBaseType(typeBuilder.Metadata, ConfigurationSource.DataAnnotation);
            var otherDerivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention);

            Assert.NotNull(typeBuilder.HasDiscriminator());
            Assert.Equal(1, typeBuilder.Metadata.GetDeclaredProperties().Count());
            Assert.Equal(0, derivedTypeBuilder.Metadata.GetDeclaredProperties().Count());

            var discriminatorBuilder = typeBuilder
                .HasDiscriminator(Splot.SplowedProperty.Name, Splot.SplowedProperty.PropertyType);
            Assert.NotNull(discriminatorBuilder.HasValue(typeBuilder.Metadata, 1));
            Assert.NotNull(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, 2));
            Assert.NotNull(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, 3));

            Assert.Same(typeBuilder.Metadata, otherDerivedTypeBuilder.Metadata.BaseType);
            Assert.Equal(1, typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(
                2, typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
            Assert.Equal(
                3, typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
            Assert.Same(typeBuilder.Metadata, typeBuilder.ModelBuilder.Metadata.FindEntityType("Splow").BaseType);

            discriminatorBuilder = typeBuilder.HasDiscriminator(fromDataAnnotation: true);
            Assert.NotNull(discriminatorBuilder.HasValue(typeBuilder.Metadata, 4, fromDataAnnotation: true));
            Assert.NotNull(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, 5, fromDataAnnotation: true));
            Assert.NotNull(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, 6, fromDataAnnotation: true));
            Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(
                5, typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
            Assert.Equal(
                6, typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());

            discriminatorBuilder = typeBuilder.HasDiscriminator();
            Assert.Null(discriminatorBuilder.HasValue(typeBuilder.Metadata, 1));
            Assert.Null(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, 2));
            Assert.Null(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, 3));
            Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(
                5, typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
            Assert.Equal(
                6, typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());

            Assert.NotNull(typeBuilder.HasNoDeclaredDiscriminator(fromDataAnnotation: true));
            Assert.Null(typeBuilder.Metadata.GetDiscriminatorProperty());
            Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Empty(typeBuilder.Metadata.GetProperties());
        }

        [Fact]
        public void Changing_discriminator_type_removes_values()
        {
            var typeBuilder = CreateBuilder().Entity("Splot", ConfigurationSource.Convention);
            var derivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention);
            derivedTypeBuilder.HasBaseType(typeBuilder.Metadata, ConfigurationSource.DataAnnotation);
            var otherDerivedTypeBuilder = typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention);

            Assert.NotNull(typeBuilder.HasDiscriminator());
            Assert.Equal(1, typeBuilder.Metadata.GetDeclaredProperties().Count());
            Assert.Equal(0, derivedTypeBuilder.Metadata.GetDeclaredProperties().Count());

            var discriminatorBuilder = typeBuilder.HasDiscriminator("Splowed", typeof(int));
            Assert.NotNull(discriminatorBuilder.HasValue(typeBuilder.Metadata, 1));
            Assert.NotNull(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, 2));
            Assert.NotNull(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, 3));

            discriminatorBuilder = typeBuilder.HasDiscriminator("Splowed", typeof(string));
            Assert.Null(typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Null(
                typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
            Assert.Null(
                typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
            Assert.NotNull(discriminatorBuilder.HasValue(typeBuilder.Metadata, "4"));
            Assert.NotNull(discriminatorBuilder.HasValue(otherDerivedTypeBuilder.Metadata, "5"));
            Assert.NotNull(discriminatorBuilder.HasValue(derivedTypeBuilder.Metadata, "6"));

            discriminatorBuilder = typeBuilder.HasDiscriminator("Splotted", typeof(string));

            Assert.NotNull(discriminatorBuilder);
            Assert.Equal("4", typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(
                "5", typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
            Assert.Equal(
                "6", typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());

            discriminatorBuilder = typeBuilder.HasDiscriminator(typeof(int));

            Assert.NotNull(discriminatorBuilder);
            Assert.Null(typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Null(
                typeBuilder.ModelBuilder.Entity("Splow", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
            Assert.Null(
                typeBuilder.ModelBuilder.Entity("Splod", ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());
        }

        [Fact]
        public void Can_access_discriminator_value_generic()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            var discriminatorBuilder = new DiscriminatorBuilder<int?>(
                (DiscriminatorBuilder)typeBuilder.HasDiscriminator(Splot.SplowedProperty));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splot), 1));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splow), 2));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splod), 3));

            var splow = typeBuilder.ModelBuilder.Entity(typeof(Splow), ConfigurationSource.Convention).Metadata;
            var splod = typeBuilder.ModelBuilder.Entity(typeof(Splod), ConfigurationSource.Convention).Metadata;
            Assert.Equal(1, typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(2, splow.GetDiscriminatorValue());
            Assert.Equal(
                3, typeBuilder.ModelBuilder.Entity(typeof(Splod), ConfigurationSource.Convention)
                    .Metadata.GetDiscriminatorValue());

            discriminatorBuilder = new DiscriminatorBuilder<int?>(
                (DiscriminatorBuilder)typeBuilder.HasDiscriminator(fromDataAnnotation: true));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splot), 4));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splow), 5));
            Assert.NotNull(discriminatorBuilder.HasValue(typeof(Splod), 6));
            Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(5, splow.GetDiscriminatorValue());
            Assert.Equal(6, splod.GetDiscriminatorValue());

            var conventionDiscriminatorBuilder = typeBuilder.HasDiscriminator();
            Assert.Null(conventionDiscriminatorBuilder.HasValue(typeBuilder.Metadata, 1));
            Assert.Null(conventionDiscriminatorBuilder.HasValue(splow, 2));
            Assert.Null(conventionDiscriminatorBuilder.HasValue(splod, 3));
            Assert.Equal(4, typeBuilder.Metadata.GetDiscriminatorValue());
            Assert.Equal(5, splow.GetDiscriminatorValue());
            Assert.Equal(6, splod.GetDiscriminatorValue());
        }

        [Fact]
        public void DiscriminatorValue_throws_if_base_cannot_be_set()
        {
            var modelBuilder = CreateBuilder();
            var typeBuilder = modelBuilder.Entity("Splot", ConfigurationSource.Convention);
            var nonDerivedTypeBuilder = modelBuilder.Entity("Splow", ConfigurationSource.Convention);
            nonDerivedTypeBuilder.HasBaseType(
                modelBuilder.Entity("Splod", ConfigurationSource.Convention).Metadata, ConfigurationSource.Explicit);

            var discriminatorBuilder = typeBuilder.HasDiscriminator();
            Assert.Equal(
                RelationalStrings.DiscriminatorEntityTypeNotDerived("Splow", "Splot"),
                Assert.Throws<InvalidOperationException>(()
                    => discriminatorBuilder.HasValue(nonDerivedTypeBuilder.Metadata, "1")).Message);
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
