// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class IndexAttributeConventionTest
    {
        #region IndexAttribute

        [ConditionalFact]
        public void IndexAttribute_overrides_configuration_from_convention()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var entityBuilder = modelBuilder.Entity(typeof(EntityWithIndex), ConfigurationSource.Convention);
            entityBuilder.Property("Id", ConfigurationSource.Convention);
            var propA = entityBuilder.Property("A", ConfigurationSource.Convention);
            var propB = entityBuilder.Property("B", ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            var indexProperties = new List<string> { propA.Metadata.Name, propB.Metadata.Name };
            var indexBuilder = entityBuilder.HasIndex(indexProperties, ConfigurationSource.Convention);
            indexBuilder.HasName("ConventionalIndexName", ConfigurationSource.Convention);
            indexBuilder.IsUnique(false, ConfigurationSource.Convention);

            RunConvention(modelBuilder);

            var index = entityBuilder.Metadata.GetIndexes().Single();
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetConfigurationSource());
            Assert.Equal("IndexOnAAndB", index.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetNameConfigurationSource());
            Assert.True(index.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetIsUniqueConfigurationSource());
            Assert.Collection(index.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));
        }

        [ConditionalFact]
        public void IndexAttribute_can_be_overriden_using_explicit_configuration()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityBuilder = modelBuilder.Entity<EntityWithIndex>();

            entityBuilder.HasIndex("A", "B")
                .HasName("OverridenIndexName")
                .IsUnique(false);

            modelBuilder.Model.FinalizeModel();

            var index = (Metadata.Internal.Index)entityBuilder.Metadata.GetIndexes().Single();
            Assert.Equal(ConfigurationSource.Explicit, index.GetConfigurationSource());
            Assert.Equal("OverridenIndexName", index.Name);
            Assert.Equal(ConfigurationSource.Explicit, index.GetNameConfigurationSource());
            Assert.False(index.IsUnique);
            Assert.Equal(ConfigurationSource.Explicit, index.GetIsUniqueConfigurationSource());
            Assert.Collection(index.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));
        }

        [ConditionalFact]
        public void IndexAttribute_with_no_property_names_throws()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<EntityWithInvalidEmptyIndex>();

            Assert.Equal(
                AbstractionsStrings.CollectionArgumentIsEmpty("propertyNames"),
                Assert.Throws<ArgumentException>(
                    () => modelBuilder.Model.FinalizeModel()).Message);
        }

        [InlineData(typeof(EntityWithInvalidNullIndexProperty))]
        [InlineData(typeof(EntityWithInvalidEmptyIndexProperty))]
        [InlineData(typeof(EntityWithInvalidWhiteSpaceIndexProperty))]
        [ConditionalTheory]
        public void IndexAttribute_properties_cannot_include_whitespace(Type entityTypeWithInvalidIndex)
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity(entityTypeWithInvalidIndex);

            Assert.Equal(
                AbstractionsStrings.CollectionArgumentHasEmptyElements("propertyNames"),
                Assert.Throws<ArgumentException>(
                    () => modelBuilder.Model.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public void IndexAttribute_can_be_applied_more_than_once_per_entity_type()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityBuilder = modelBuilder.Entity<EntityWithTwoIndexes>();
            modelBuilder.Model.FinalizeModel();

            var indexes = entityBuilder.Metadata.GetIndexes();
            Assert.Equal(2, indexes.Count());

            var index0 = (Metadata.Internal.Index)indexes.First();
            Assert.Equal(ConfigurationSource.DataAnnotation, index0.GetConfigurationSource());
            Assert.Equal("IndexOnAAndB", index0.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, index0.GetNameConfigurationSource());
            Assert.True(index0.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index0.GetIsUniqueConfigurationSource());
            Assert.Collection(index0.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));

            var index1 = (Metadata.Internal.Index)indexes.Skip(1).First();
            Assert.Equal(ConfigurationSource.DataAnnotation, index1.GetConfigurationSource());
            Assert.Equal("IndexOnBAndC", index1.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, index1.GetNameConfigurationSource());
            Assert.False(index1.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index1.GetIsUniqueConfigurationSource());
            Assert.Collection(index1.Properties,
                prop0 => Assert.Equal("B", prop0.Name),
                prop1 => Assert.Equal("C", prop1.Name));
        }

        [ConditionalFact]
        public void IndexAttribute_can_be_inherited_from_base_entity_type()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityBuilder = modelBuilder.Entity<EntityWithIndexFromBaseType>();
            modelBuilder.Model.FinalizeModel();

            // assert that the base type is not part of the model
            Assert.Empty(modelBuilder.Model.GetEntityTypes()
                .Where(e => e.ClrType == typeof(BaseUnmappedEntityWithIndex)));

            // assert that we see the index anyway
            var index = (Metadata.Internal.Index)entityBuilder.Metadata.GetIndexes().Single();
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetConfigurationSource());
            Assert.Equal("IndexOnAAndB", index.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetNameConfigurationSource());
            Assert.True(index.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetIsUniqueConfigurationSource());
            Assert.Collection(index.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));
        }

        [ConditionalFact]
        public virtual void IndexAttribute_with_an_ignored_property_causes_error()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entity = modelBuilder.Entity<EntityWithIgnoredProperty>();

            Assert.Equal(
                CoreStrings.IndexDefinedOnIgnoredProperty(
                    "",
                    nameof(EntityWithIgnoredProperty),
                    "{'A', 'B'}",
                    "B"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Model.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void IndexAttribute_with_a_non_existent_property_causes_error()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entity = modelBuilder.Entity<EntityWithNonExistentProperty>();

            Assert.Equal(
                CoreStrings.IndexDefinedOnNonExistentProperty(
                        "IndexOnAAndNonExistentProperty",
                        nameof(EntityWithNonExistentProperty),
                        "{'A', 'DoesNotExist'}",
                        "DoesNotExist"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Model.FinalizeModel()).Message);
        }

        #endregion

        private void RunConvention(InternalModelBuilder modelBuilder)
        {
            var context = new ConventionContext<IConventionModelBuilder>(modelBuilder.Metadata.ConventionDispatcher);

            new IndexAttributeConvention(CreateDependencies())
                .ProcessModelFinalizing(modelBuilder, context);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        [Index(nameof(A), nameof(B), Name = "IndexOnAAndB", IsUnique = true)]
        private class EntityWithIndex
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), nameof(B), Name = "IndexOnAAndB", IsUnique = true)]
        [Index(nameof(B), nameof(C), Name = "IndexOnBAndC", IsUnique = false)]
        private class EntityWithTwoIndexes
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }

        [Index(nameof(A), nameof(B), Name = "IndexOnAAndB", IsUnique = true)]
        [NotMapped]
        private class BaseUnmappedEntityWithIndex
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        private class EntityWithIndexFromBaseType : BaseUnmappedEntityWithIndex
        {
            public int C { get; set; }
            public int D { get; set; }
        }

        [Index]
        private class EntityWithInvalidEmptyIndex
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), null, Name = "IndexOnAAndNull")]
        private class EntityWithInvalidNullIndexProperty
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), "", Name = "IndexOnAAndEmpty")]
        private class EntityWithInvalidEmptyIndexProperty
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), " \r\n\t", Name = "IndexOnAAndWhiteSpace")]
        private class EntityWithInvalidWhiteSpaceIndexProperty
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), nameof(B))]
        private class EntityWithIgnoredProperty
        {
            public int Id { get; set; }
            public int A { get; set; }
            [NotMapped]
            public int B { get; set; }
        }

        [Index(nameof(A), "DoesNotExist", Name = "IndexOnAAndNonExistentProperty")]
        private class EntityWithNonExistentProperty
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }
    }
}
