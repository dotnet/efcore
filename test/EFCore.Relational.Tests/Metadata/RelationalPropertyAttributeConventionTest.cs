// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalPropertyAttributeConventionTest
    {
        [ConditionalFact]
        public void ColumnAttribute_sets_column_name_and_type_with_conventional_builder()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var entityBuilder = modelBuilder.Entity<A>();

            Assert.Equal("Post Name", entityBuilder.Property(e => e.Name).Metadata.GetColumnBaseName());
            Assert.Equal("DECIMAL", entityBuilder.Property(e => e.Name).Metadata.GetColumnType());
        }

        [ConditionalFact]
        public void CommentAttribute_on_property_sets_column_comment_with_conventional_builder()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var entityBuilder = modelBuilder.Entity<A>();

            Assert.Equal("Test column comment", entityBuilder.Property(e => e.Name).Metadata.GetComment());
        }

        [ConditionalFact]
        public void ColumnAttribute_on_field_sets_column_name_and_type_with_conventional_builder()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var entityBuilder = modelBuilder.Entity<F>();

            Assert.Equal("Post Name", entityBuilder.Property<string>(nameof(F.Name)).Metadata.GetColumnBaseName());
            Assert.Equal("DECIMAL", entityBuilder.Property<string>(nameof(F.Name)).Metadata.GetColumnType());
        }

        [ConditionalFact]
        public void CommentAttribute_on_field_sets_column_comment_with_conventional_builder()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var entityBuilder = modelBuilder.Entity<F>();

            Assert.Equal("Test column comment", entityBuilder.Property<string>(nameof(F.Name)).Metadata.GetComment());
        }

        [ConditionalFact]
        public void ColumnAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnName, "ConventionalName", ConfigurationSource.Convention);
            propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnType, "BYTE", ConfigurationSource.Convention);
            propertyBuilder.HasAnnotation(RelationalAnnotationNames.Comment, "ConventionalName", ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.Equal("Post Name", propertyBuilder.Metadata.GetColumnBaseName());
            Assert.Equal("DECIMAL", propertyBuilder.Metadata.GetColumnType());
            Assert.Equal("Test column comment", propertyBuilder.Metadata.GetComment());
        }

        [ConditionalFact]
        public void CommentAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.HasAnnotation(RelationalAnnotationNames.Comment, "ConventionalName", ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.Equal("Test column comment", propertyBuilder.Metadata.GetComment());
        }

        [ConditionalFact]
        public void ColumnAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnName, "ExplicitName", ConfigurationSource.Explicit);
            propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnType, "BYTE", ConfigurationSource.Explicit);
            propertyBuilder.HasAnnotation(RelationalAnnotationNames.Comment, "ExplicitComment", ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.Equal("ExplicitName", propertyBuilder.Metadata.GetColumnBaseName());
            Assert.Equal("BYTE", propertyBuilder.Metadata.GetColumnType());
            Assert.Equal("ExplicitComment", propertyBuilder.Metadata.GetComment());
        }

        [ConditionalFact]
        public void CommentAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.HasAnnotation(RelationalAnnotationNames.Comment, "ExplicitComment", ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.Equal("ExplicitComment", propertyBuilder.Metadata.GetComment());
        }

        private void RunConvention(InternalPropertyBuilder propertyBuilder)
        {
            var context = new ConventionContext<IConventionPropertyBuilder>(
                propertyBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

            new RelationalColumnAttributeConvention(CreateDependencies(), CreateRelationalDependencies())
                .ProcessPropertyAdded(propertyBuilder, context);

            new RelationalColumnCommentAttributeConvention(CreateDependencies(), CreateRelationalDependencies())
                .ProcessPropertyAdded(propertyBuilder, context);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(
                new PropertyDiscoveryConvention(CreateDependencies()));

            var modelBuilder = new Model(conventionSet).Builder;

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private RelationalConventionSetBuilderDependencies CreateRelationalDependencies()
            => RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<RelationalConventionSetBuilderDependencies>();

        protected virtual ModelBuilder CreateConventionalModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder();

        private class A
        {
            public int Id { get; set; }

            [Column("Post Name", Order = 1, TypeName = "DECIMAL")]
            [Comment("Test column comment")]
            public string Name { get; set; }
        }

        public class F
        {
            public int Id { get; set; }

            [Column("Post Name", Order = 1, TypeName = "DECIMAL")]
            [Comment("Test column comment")]
            public string Name;
        }
    }
}
