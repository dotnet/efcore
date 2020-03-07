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
    public class RelationalEntityTypeAttributeConventionTest
    {
        [ConditionalFact]
        public void TableAttribute_sets_column_name_order_and_type_with_conventional_builder()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var entityBuilder = modelBuilder.Entity<A>();

            Assert.Equal("MyTable", entityBuilder.Metadata.GetTableName());
            Assert.Equal("MySchema", entityBuilder.Metadata.GetSchema());
        }

        [ConditionalFact]
        public void TableAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            entityBuilder.HasAnnotation(RelationalAnnotationNames.TableName, "ConventionalName", ConfigurationSource.Convention);
            entityBuilder.HasAnnotation(RelationalAnnotationNames.Schema, "ConventionalSchema", ConfigurationSource.Convention);

            RunConvention(entityBuilder);

            Assert.Equal("MyTable", entityBuilder.Metadata.GetTableName());
            Assert.Equal("MySchema", entityBuilder.Metadata.GetSchema());
        }

        [ConditionalFact]
        public void TableAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            entityBuilder.HasAnnotation(RelationalAnnotationNames.TableName, "ExplicitName", ConfigurationSource.Explicit);
            entityBuilder.HasAnnotation(RelationalAnnotationNames.Schema, "ExplicitName", ConfigurationSource.Explicit);

            RunConvention(entityBuilder);

            Assert.Equal("ExplicitName", entityBuilder.Metadata.GetTableName());
            Assert.Equal("ExplicitName", entityBuilder.Metadata.GetSchema());
        }

        private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var context = new ConventionContext<IConventionEntityTypeBuilder>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            new RelationalTableAttributeConvention(CreateDependencies(), CreateRelationalDependencies())
                .ProcessEntityTypeAdded(entityTypeBuilder, context);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(
                new PropertyDiscoveryConvention(CreateDependencies()));

            var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private RelationalConventionSetBuilderDependencies CreateRelationalDependencies()
            => RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<RelationalConventionSetBuilderDependencies>();

        protected virtual ModelBuilder CreateConventionalModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder();

        [Table("MyTable", Schema = "MySchema")]
        private class A
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
