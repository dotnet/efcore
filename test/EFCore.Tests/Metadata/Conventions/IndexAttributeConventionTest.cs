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
using Index = Microsoft.EntityFrameworkCore.Metadata.Internal.Index;

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
            var propABuilder = entityBuilder.Property("A", ConfigurationSource.Convention);
            var propBBuilder = entityBuilder.Property("B", ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            var indexProperties = new List<string> { propABuilder.Metadata.Name, propBBuilder.Metadata.Name };
            var indexBuilder = entityBuilder.HasIndex(indexProperties, "IndexOnAAndB", ConfigurationSource.Convention);
            indexBuilder.IsUnique(false, ConfigurationSource.Convention);

            RunConvention(entityBuilder);
            RunConvention(modelBuilder);

            var index = entityBuilder.Metadata.GetIndexes().Single();
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetConfigurationSource());
            Assert.Equal("IndexOnAAndB", index.Name);
            Assert.True(index.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetIsUniqueConfigurationSource());
            Assert.Collection(
                index.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));
        }

        [ConditionalFact]
        public void IndexAttribute_can_be_overriden_using_explicit_configuration()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityBuilder = modelBuilder.Entity<EntityWithIndex>();

            entityBuilder.HasIndex(new[] { "A", "B" }, "IndexOnAAndB")
                .IsUnique(false);

            modelBuilder.Model.FinalizeModel();

            var index = (Index)entityBuilder.Metadata.GetIndexes().Single();
            Assert.Equal(ConfigurationSource.Explicit, index.GetConfigurationSource());
            Assert.Equal("IndexOnAAndB", index.Name);
            Assert.False(index.IsUnique);
            Assert.Equal(ConfigurationSource.Explicit, index.GetIsUniqueConfigurationSource());
            Assert.Collection(
                index.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));
        }

        [ConditionalFact]
        public void IndexAttribute_with_no_property_names_throws()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            Assert.Equal(
                AbstractionsStrings.CollectionArgumentIsEmpty("propertyNames"),
                Assert.Throws<ArgumentException>(
                    () => modelBuilder.Entity<EntityWithInvalidEmptyIndex>()).Message);
        }

        [InlineData(typeof(EntityWithInvalidNullIndexProperty))]
        [InlineData(typeof(EntityWithInvalidEmptyIndexProperty))]
        [InlineData(typeof(EntityWithInvalidWhiteSpaceIndexProperty))]
        [ConditionalTheory]
        public void IndexAttribute_properties_cannot_include_whitespace(Type entityTypeWithInvalidIndex)
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            Assert.Equal(
                AbstractionsStrings.CollectionArgumentHasEmptyElements("propertyNames"),
                Assert.Throws<ArgumentException>(
                    () => modelBuilder.Entity(entityTypeWithInvalidIndex)).Message);
        }

        [ConditionalFact]
        public void IndexAttribute_can_be_applied_more_than_once_per_entity_type()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityBuilder = modelBuilder.Entity<EntityWithTwoIndexes>();
            modelBuilder.Model.FinalizeModel();

            var indexes = entityBuilder.Metadata.GetIndexes();
            Assert.Equal(2, indexes.Count());

            var index0 = (Index)indexes.First();
            Assert.Equal(ConfigurationSource.DataAnnotation, index0.GetConfigurationSource());
            Assert.Equal("IndexOnAAndB", index0.Name);
            Assert.True(index0.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index0.GetIsUniqueConfigurationSource());
            Assert.Collection(
                index0.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));

            var index1 = (Index)indexes.Skip(1).First();
            Assert.Equal(ConfigurationSource.DataAnnotation, index1.GetConfigurationSource());
            Assert.Equal("IndexOnBAndC", index1.Name);
            Assert.False(index1.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index1.GetIsUniqueConfigurationSource());
            Assert.Collection(
                index1.Properties,
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
            Assert.Empty(
                modelBuilder.Model.GetEntityTypes()
                    .Where(e => e.ClrType == typeof(BaseUnmappedEntityWithIndex)));

            // assert that we see the index anyway
            var index = (Index)entityBuilder.Metadata.GetIndexes().Single();
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetConfigurationSource());
            Assert.Equal("IndexOnAAndB", index.Name);
            Assert.True(index.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetIsUniqueConfigurationSource());
            Assert.Collection(
                index.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));
        }

        [ConditionalFact]
        public virtual void IndexAttribute_without_name_and_an_ignored_property_causes_error()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<EntityUnnamedIndexWithIgnoredProperty>();

            Assert.Equal(
                CoreStrings.UnnamedIndexDefinedOnIgnoredProperty(
                    nameof(EntityUnnamedIndexWithIgnoredProperty),
                    "{'A', 'B'}",
                    "B"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Model.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void IndexAttribute_with_name_and_an_ignored_property_causes_error()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityBuilder = modelBuilder.Entity<EntityIndexWithIgnoredProperty>();

            Assert.Equal(
                CoreStrings.NamedIndexDefinedOnIgnoredProperty(
                    "IndexOnAAndIgnoredProperty",
                    nameof(EntityIndexWithIgnoredProperty),
                    "{'A', 'B'}",
                    "B"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Model.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void IndexAttribute_without_name_and_non_existent_property_causes_error()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<EntityUnnamedIndexWithNonExistentProperty>();

            Assert.Equal(
                CoreStrings.UnnamedIndexDefinedOnNonExistentProperty(
                    nameof(EntityUnnamedIndexWithNonExistentProperty),
                    "{'A', 'DoesNotExist'}",
                    "DoesNotExist"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Model.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void IndexAttribute_with_name_and_non_existent_property_causes_error()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entity = modelBuilder.Entity<EntityIndexWithNonExistentProperty>();

            Assert.Equal(
                CoreStrings.NamedIndexDefinedOnNonExistentProperty(
                    "IndexOnAAndNonExistentProperty",
                    nameof(EntityIndexWithNonExistentProperty),
                    "{'A', 'DoesNotExist'}",
                    "DoesNotExist"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Model.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public void IndexAttribute_index_replicated_to_derived_type_when_base_type_changes()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var grandparentEntityBuilder = modelBuilder.Entity(typeof(GrandparentEntityWithIndex));
            var parentEntityBuilder = modelBuilder.Entity<ParentEntity>();
            var childEntityBuilder = modelBuilder.Entity<ChildEntity>();

            // Index is created on Grandparent type but not on Parent type.
            // Child has its own index.
            Assert.NotNull(parentEntityBuilder.Metadata.BaseType);
            Assert.NotNull(childEntityBuilder.Metadata.BaseType);
            Assert.Single(grandparentEntityBuilder.Metadata.GetDeclaredIndexes());
            Assert.Empty(parentEntityBuilder.Metadata.GetDeclaredIndexes());
            Assert.Single(childEntityBuilder.Metadata.GetDeclaredIndexes());

            parentEntityBuilder.HasBaseType((string)null);

            Assert.Null(parentEntityBuilder.Metadata.BaseType);
            Assert.NotNull(childEntityBuilder.Metadata.BaseType);

            // The Index is replicated on the Parent type, but not on the Child.
            var index = (Index)
                Assert.Single(parentEntityBuilder.Metadata.GetDeclaredIndexes());
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetConfigurationSource());
            Assert.Equal("IndexOnGrandparentGetsReplicatedToParent", index.Name);
            Assert.False(index.IsUnique);
            Assert.Null(index.GetIsUniqueConfigurationSource());
            var indexProperty = Assert.Single(index.Properties);
            Assert.Equal("B", indexProperty.Name);

            // The Child still has its own index even though
            // the property is defined on the Grandparent type.
            var childIndex = (Index)
                Assert.Single(childEntityBuilder.Metadata.GetDeclaredIndexes());
            Assert.Equal(ConfigurationSource.DataAnnotation, childIndex.GetConfigurationSource());
            Assert.Equal("IndexOnChildUnaffectedWhenParentBaseTypeRemoved", childIndex.Name);
            Assert.True(childIndex.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, childIndex.GetIsUniqueConfigurationSource());
            var childIndexProperty = Assert.Single(childIndex.Properties);
            Assert.Equal("A", childIndexProperty.Name);

            // Check there are no errors.
            modelBuilder.Model.FinalizeModel();
        }

        [ConditionalFact]
        public void IndexAttribute_index_is_created_when_missing_property_added()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(EntityWithIndexOnShadowProperty));

            Assert.Empty(entityBuilder.Metadata.GetDeclaredIndexes());

            entityBuilder.Property<int>("Y");
            modelBuilder.Model.FinalizeModel();

            var index = (Index)
                Assert.Single(entityBuilder.Metadata.GetDeclaredIndexes());

            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetConfigurationSource());
            Assert.Equal("IndexOnShadowProperty", index.Name);
            Assert.Collection(
                index.Properties,
                prop0 => Assert.Equal("X", prop0.Name),
                prop1 => Assert.Equal("Y", prop1.Name));
        }

        [ConditionalFact]
        public void IndexAttribute_index_is_created_when_index_on_private_property()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(EntityWithIndexOnPrivateProperty));
            modelBuilder.Model.FinalizeModel();

            var index = (Index)
                Assert.Single(entityBuilder.Metadata.GetDeclaredIndexes());

            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetConfigurationSource());
            Assert.Equal("IndexOnPrivateProperty", index.Name);
            Assert.Collection(
                index.Properties,
                prop0 => Assert.Equal("X", prop0.Name),
                prop1 => Assert.Equal("Y", prop1.Name));
        }

        #endregion

        private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var context = new ConventionContext<IConventionEntityTypeBuilder>(
                entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            CreateIndexAttributeConvention().ProcessEntityTypeAdded(entityTypeBuilder, context);
        }

        private void RunConvention(InternalModelBuilder modelBuilder)
        {
            var context = new ConventionContext<IConventionModelBuilder>(modelBuilder.Metadata.ConventionDispatcher);

            CreateIndexAttributeConvention().ProcessModelFinalizing(modelBuilder, context);
        }

        private IndexAttributeConvention CreateIndexAttributeConvention()
            => new IndexAttributeConvention(CreateDependencies());

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
        private class EntityUnnamedIndexWithIgnoredProperty
        {
            public int Id { get; set; }
            public int A { get; set; }

            [NotMapped]
            public int B { get; set; }
        }

        [Index(nameof(A), nameof(B), Name = "IndexOnAAndIgnoredProperty")]
        private class EntityIndexWithIgnoredProperty
        {
            public int Id { get; set; }
            public int A { get; set; }

            [NotMapped]
            public int B { get; set; }
        }

        [Index(nameof(A), "DoesNotExist")]
        private class EntityUnnamedIndexWithNonExistentProperty
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), "DoesNotExist", Name = "IndexOnAAndNonExistentProperty")]
        private class EntityIndexWithNonExistentProperty
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(B), Name = "IndexOnGrandparentGetsReplicatedToParent")]
        private class GrandparentEntityWithIndex
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        private class ParentEntity : GrandparentEntityWithIndex
        {
            public int C { get; set; }
            public int D { get; set; }
        }

        [Index(nameof(A), Name = "IndexOnChildUnaffectedWhenParentBaseTypeRemoved", IsUnique = true)]
        private class ChildEntity : ParentEntity
        {
            public int E { get; set; }
            public int F { get; set; }
        }

        [Index(nameof(X), "Y", Name = "IndexOnShadowProperty")]
        private class EntityWithIndexOnShadowProperty
        {
            public int Id { get; set; }
            public int X { get; set; }
        }

        [Index(nameof(X), nameof(Y), Name = "IndexOnPrivateProperty")]
        private class EntityWithIndexOnPrivateProperty
        {
            public int Id { get; set; }
            public int X { get; set; }
            private int Y { get; set; }
        }
    }
}
