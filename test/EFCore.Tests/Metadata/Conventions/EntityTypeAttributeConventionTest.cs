// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class EntityTypeAttributeConventionTest
    {
        #region NotMappedAttribute

        [ConditionalFact]
        public void NotMappedAttribute_overrides_configuration_from_convention_source()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var entityBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);

            RunConvention(entityBuilder);

            Assert.Empty(modelBuilder.Metadata.GetEntityTypes());
        }

        [ConditionalFact]
        public void NotMappedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var entityBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Explicit);

            RunConvention(entityBuilder);

            Assert.Single(modelBuilder.Metadata.GetEntityTypes());
        }

        [ConditionalFact]
        public void NotMappedAttribute_ignores_entityTypes_with_conventional_builder()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<B>();

            Assert.Single(modelBuilder.Model.GetEntityTypes());
        }

        #endregion

        #region OwnedAttribute

        [ConditionalFact]
        public void OwnedAttribute_configures_entity_as_owned()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<Customer>();

            Assert.Equal(2, modelBuilder.Model.GetEntityTypes().Count());
            Assert.True(modelBuilder.Model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Address)).ForeignKey.IsOwnership);
        }

        [ConditionalFact]
        public void Entity_marked_with_OwnedAttribute_cannot_be_configured_as_regular_entity()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            Assert.Equal(
                CoreStrings.ClashingOwnedEntityType(nameof(Address)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Customer>().HasOne(e => e.Address).WithOne(e => e.Customer)).Message);
        }

        #endregion

        #region KeylessAttribute

        [ConditionalFact]
        public void KeylessAttribute_overrides_configuration_from_convention()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var entityBuilder = modelBuilder.Entity(typeof(KeylessEntity), ConfigurationSource.Convention);
            entityBuilder.Property("Id", ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            Assert.NotNull(entityBuilder.Metadata.FindPrimaryKey());

            RunConvention(entityBuilder);

            Assert.Null(entityBuilder.Metadata.FindPrimaryKey());
            Assert.True(entityBuilder.Metadata.IsKeyless);
        }

        [ConditionalFact]
        public void KeylessAttribute_can_be_overriden_using_explicit_configuration()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            var entityBuilder = modelBuilder.Entity<KeylessEntity>();

            Assert.True(entityBuilder.Metadata.IsKeyless);

            entityBuilder.HasKey(e => e.Id);

            Assert.False(entityBuilder.Metadata.IsKeyless);
            Assert.NotNull(entityBuilder.Metadata.FindPrimaryKey());
        }

        [ConditionalFact]
        public void KeyAttribute_does_not_override_keyless_attribute()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            var entityBuilder = modelBuilder.Entity<KeyClash>();

            Assert.True(entityBuilder.Metadata.IsKeyless);
            Assert.Null(entityBuilder.Metadata.FindPrimaryKey());
        }

        #endregion

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

            RunConvention(entityBuilder);

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

            var index = (Metadata.Internal.Index)entityBuilder.Metadata.GetIndexes().Single();
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetConfigurationSource());
            Assert.Equal("IndexOnAAndB", index.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetNameConfigurationSource());
            Assert.True(index.IsUnique);
            Assert.Equal(ConfigurationSource.DataAnnotation, index.GetIsUniqueConfigurationSource());
            Assert.Collection(index.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));

            entityBuilder.HasIndex(e => new { e.A, e.B })
                .HasName("OverridenIndexName")
                .IsUnique(false);

            index = (Metadata.Internal.Index)entityBuilder.Metadata.GetIndexes().Single();
            Assert.Equal(ConfigurationSource.Explicit, index.GetConfigurationSource());
            Assert.Equal("OverridenIndexName", index.Name);
            Assert.Equal(ConfigurationSource.Explicit, index.GetNameConfigurationSource());
            Assert.False(index.IsUnique);
            Assert.Equal(ConfigurationSource.Explicit, index.GetIsUniqueConfigurationSource());
            Assert.Collection(index.Properties,
                prop0 => Assert.Equal("A", prop0.Name),
                prop1 => Assert.Equal("B", prop1.Name));
        }

        [InlineData(typeof(EntityWithInvalidNullIndexMember), "{'A', ''}")]
        [InlineData(typeof(EntityWithInvalidEmptyIndexMember), "{'A', ''}")]
        [InlineData(typeof(EntityWithInvalidWhiteSpaceIndexMember), "{'A', ' \r\n\t'}")]
        [ConditionalTheory]
        public void IndexAttribute_members_cannot_include_whitespace(Type entityTypeWithInvalidIndex, string indexMembersString)
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            Assert.Equal(
                CoreStrings.IndexMemberNameEmpty(
                            entityTypeWithInvalidIndex.Name,
                            indexMembersString),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity(entityTypeWithInvalidIndex)).Message);
        }

        [ConditionalFact]
        public void IndexAttribute_with_an_ignored_member_throws()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            Assert.Equal(
                CoreStrings.IndexMemberIsIgnored(
                            nameof(EntityWithIgnoredMember),
                            "{'A', 'B'}",
                            "B"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<EntityWithIgnoredMember>()).Message);
        }

        [ConditionalFact]
        public void IndexAttribute_with_a_non_existent_member_throws()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            Assert.Equal(
                CoreStrings.IndexMemberHasNoMatchingMember(
                            nameof(EntityWithNonExistentMember),
                            "{'A', 'DoesNotExist'}",
                            "DoesNotExist"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<EntityWithNonExistentMember>()).Message);
        }

        [ConditionalFact]
        public void IndexAttribute_can_be_applied_more_than_once_per_entity_type()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            var entityBuilder = modelBuilder.Entity<EntityWithTwoIndexes>();

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

        #endregion

        private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var context = new ConventionContext<IConventionEntityTypeBuilder>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            new NotMappedEntityTypeAttributeConvention(CreateDependencies())
                .ProcessEntityTypeAdded(entityTypeBuilder, context);

            new OwnedEntityTypeAttributeConvention(CreateDependencies())
                .ProcessEntityTypeAdded(entityTypeBuilder, context);

            new KeylessEntityTypeAttributeConvention(CreateDependencies())
                .ProcessEntityTypeAdded(entityTypeBuilder, context);

            new IndexEntityTypeAttributeConvention(CreateDependencies())
                .ProcessEntityTypeAdded(entityTypeBuilder, context);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        [NotMapped]
        private class A
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private class B
        {
            public int Id { get; set; }

            public virtual A NavToA { get; set; }
        }

        private class Customer
        {
            public int Id { get; set; }
            public Address Address { get; set; }
        }

        [Owned]
        private class Address
        {
            public int Id { get; set; }
            public Customer Customer { get; set; }
        }

        [Keyless]
        private class KeylessEntity
        {
            public int Id { get; set; }
        }

        [Keyless]
        private class KeyClash
        {
            [Key]
            public int MyId { get; set; }
        }

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

        [Index(nameof(A), null, Name = "IndexOnAAndNull")]
        private class EntityWithInvalidNullIndexMember
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), "", Name = "IndexOnAAndEmpty")]
        private class EntityWithInvalidEmptyIndexMember
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), " \r\n\t", Name = "IndexOnAAndWhiteSpace")]
        private class EntityWithInvalidWhiteSpaceIndexMember
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }

        [Index(nameof(A), nameof(B), Name = "IndexOnAAndUnmappedMember")]
        private class EntityWithIgnoredMember
        {
            public int Id { get; set; }
            public int A { get; set; }
            [NotMapped]
            public int B { get; set; }
        }

        [Index(nameof(A), "DoesNotExist", Name = "IndexOnAAndNonExistentMember")]
        private class EntityWithNonExistentMember
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
        }
    }
}
