// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
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

        [ConditionalFact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            ((IMutableModel)builder.Metadata).AddSequence("Mine").IncrementBy = 77;

            Assert.Equal(77, ((IMutableModel)builder.Metadata).FindSequence("Mine").IncrementBy);
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property(typeof(int), "Id", ConfigurationSource.Convention);

            Assert.NotNull(propertyBuilder.IsFixedLength(true));
            Assert.True(propertyBuilder.Metadata.IsFixedLength());
            Assert.NotNull(propertyBuilder.HasColumnName("Splew"));
            Assert.Equal("Splew", propertyBuilder.Metadata.GetColumnBaseName());
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
            Assert.Equal("Splow", propertyBuilder.Metadata.GetColumnBaseName());
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

        [ConditionalFact]
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

        [ConditionalFact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { "Id" }, ConfigurationSource.Convention);

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.NotNull(indexBuilder.HasName("Splew"));
            Assert.Equal("Splew", indexBuilder.Metadata.GetName());

            Assert.NotNull(indexBuilder.HasName("Splow", fromDataAnnotation: true));
            Assert.Equal("Splow", indexBuilder.Metadata.GetName());

            Assert.Null(indexBuilder.HasName("Splod"));
            Assert.Equal("Splow", indexBuilder.Metadata.GetName());

            Assert.NotNull(indexBuilder.HasName(null, fromDataAnnotation: true));
            Assert.Equal("IX_Splot_Id", indexBuilder.Metadata.GetName());

            Assert.NotNull(indexBuilder.HasName("Splod"));
            Assert.Equal("Splod", indexBuilder.Metadata.GetName());
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.NotNull(indexBuilder.HasFilter("Splew"));
            Assert.Equal("Splew", indexBuilder.Metadata.GetFilter());

            Assert.NotNull(indexBuilder.HasFilter("Splow", fromDataAnnotation: true));
            Assert.Equal("Splow", indexBuilder.Metadata.GetFilter());

            Assert.Null(indexBuilder.HasFilter("Splod"));
            Assert.Equal("Splow", indexBuilder.Metadata.GetFilter());

            Assert.NotNull(indexBuilder.HasFilter(null, fromDataAnnotation: true));
            Assert.Null(indexBuilder.Metadata.GetFilter());

            Assert.Null(indexBuilder.HasFilter("Splod"));
            Assert.Null(indexBuilder.Metadata.GetFilter());
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public void Can_access_check_constraint()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
            IEntityType entityType = typeBuilder.Metadata;

            Assert.NotNull(typeBuilder.HasCheckConstraint("Splew", "s > p"));
            Assert.Equal("Splew", entityType.GetCheckConstraints().Single().Name);
            Assert.Equal("s > p", entityType.GetCheckConstraints().Single().Sql);

            Assert.NotNull(typeBuilder.HasCheckConstraint("Splew", "s < p", fromDataAnnotation: true));
            Assert.Equal("Splew", entityType.GetCheckConstraints().Single().Name);
            Assert.Equal("s < p", entityType.GetCheckConstraints().Single().Sql);

            Assert.Null(typeBuilder.HasCheckConstraint("Splew", "s > p"));
            Assert.Equal("Splew", entityType.GetCheckConstraints().Single().Name);
            Assert.Equal("s < p", entityType.GetCheckConstraints().Single().Sql);
        }

        [ConditionalFact]
        public void Can_access_comment()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
            var entityType = typeBuilder.Metadata;

            Assert.NotNull(typeBuilder.HasComment("My Comment"));
            Assert.Equal("My Comment", entityType.GetComment());

            Assert.NotNull(typeBuilder.HasComment("My Comment 2", fromDataAnnotation: true));
            Assert.Equal("My Comment 2", entityType.GetComment());

            Assert.Null(typeBuilder.HasComment("My Comment"));
            Assert.Equal("My Comment 2", entityType.GetComment());
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
