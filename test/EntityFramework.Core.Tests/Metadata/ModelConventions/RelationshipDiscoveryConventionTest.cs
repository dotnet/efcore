// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.Conventions
{
    public class RelationshipDiscoveryConventionTest
    {
        [Fact]
        public void Entity_type_is_discovered_through_private_unidirectional_nonCollection_navigation_when_no_PK_on_principal()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>(OneToManyPrincipal.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            VerifyOneToManyDependent(entityBuilder, unidirectional: true, dependentHasPK: false);
        }

        [Fact]
        public void Entity_type_is_discovered_through_unidirectional_collection_navigation_when_no_PK_on_principal()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>(OneToManyDependent.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            VerifyOneToManyPrincipal(entityBuilder, unidirectional: true, dependentHasPK: false);
        }

        [Fact]
        public void Entity_type_is_not_discovered_if_ignored()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>();
            entityBuilder.ModelBuilder.Ignore(typeof(OneToManyPrincipal).FullName, ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.Properties);
            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.Navigations);
            Assert.Equal(entityBuilder.Metadata.ClrType, entityBuilder.Metadata.Model.EntityTypes.Single().ClrType);
        }

        [Fact]
        public void Entity_type_is_not_discovered_if_navigation_is_ignored()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>();
            entityBuilder.Ignore(OneToManyDependent.NavigationProperty.Name, ConfigurationSource.DataAnnotation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.Properties);
            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.Navigations);
            Assert.Equal(entityBuilder.Metadata.ClrType, entityBuilder.Metadata.Model.EntityTypes.Single().ClrType);
        }

        [Fact]
        public void One_to_one_bidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>(ConfigureKeys);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            VerifyOneToOne(entityBuilder.Metadata.Model);

            Assert.NotSame(entityBuilder, new RelationshipDiscoveryConvention().Apply(
                entityBuilder.ModelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention)));

            VerifyOneToOne(entityBuilder.Metadata.Model);
        }

        [Fact]
        public void One_to_many_unidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>(
                ConfigureKeys, OneToManyPrincipal.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            VerifyOneToManyDependent(entityBuilder, unidirectional: true);
        }

        [Fact]
        public void One_to_many_unidirectional_remains_unchanged_if_already_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>(
                ConfigureKeys, OneToManyPrincipal.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));
            var entityType = entityBuilder.Metadata;
            var fk = entityType.GetForeignKeys().Single();
            Assert.Null(fk.IsRequired);
            Assert.False(((IForeignKey)fk).IsUnique);

            fk.IsRequired = true;
            fk.IsUnique = true;

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            var newFk = (IForeignKey)entityType.GetForeignKeys().Single();
            Assert.True(newFk.IsRequired);
            Assert.True(newFk.IsUnique);
        }

        [Fact]
        public void One_to_many_bidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyDependent>(ConfigureKeys);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            VerifyOneToManyDependent(entityBuilder, unidirectional: false);
        }

        [Fact]
        public void Many_to_one_unidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>(
                ConfigureKeys, OneToManyDependent.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            VerifyOneToManyPrincipal(entityBuilder, unidirectional: true);
        }

        [Fact]
        public void Many_to_one_bidirectional_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<OneToManyPrincipal>(ConfigureKeys);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            VerifyOneToManyPrincipal(entityBuilder, unidirectional: false);
        }

        [Fact]
        public void Many_to_many_bidirectional_is_not_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<ManyToManyFirst>(ConfigureKeys);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));
            new ModelCleanupConvention().Apply(entityBuilder.ModelBuilder);

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.Navigations);
            Assert.Equal(1, entityBuilder.Metadata.Model.EntityTypes.Count);
        }

        [Fact]
        public void Ambiguous_navigations_are_not_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<MultipleNavigationsFirst>(
                ConfigureKeys, MultipleNavigationsSecond.IgnoreCollectionNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));
            new ModelCleanupConvention().Apply(entityBuilder.ModelBuilder);

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.Navigations);
            Assert.Equal(1, entityBuilder.Metadata.Model.EntityTypes.Count);
        }

        [Fact]
        public void Ambiguous_reverse_navigations_are_not_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<MultipleNavigationsSecond>(
                ConfigureKeys, MultipleNavigationsSecond.IgnoreCollectionNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));
            new ModelCleanupConvention().Apply(entityBuilder.ModelBuilder);

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.Navigations);
            Assert.Equal(1, entityBuilder.Metadata.Model.EntityTypes.Count);
        }

        [Fact]
        public void Multiple_navigations_to_same_entity_type_are_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<MultipleNavigationsFirst>(
                ConfigureKeys, MultipleNavigationsSecond.IgnoreCollectionNavigation, MultipleNavigationsSecond.IgnoreNonCollectionNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            var model = (IModel)entityBuilder.Metadata.Model;
            Assert.Equal(2, model.EntityTypes.Count);
            var firstEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(MultipleNavigationsFirst));
            var secondEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(MultipleNavigationsSecond));

            Assert.Equal(2, firstEntityType.GetProperties().Count());
            Assert.Equal(1, firstEntityType.GetKeys().Count());
            var firstFK = firstEntityType.GetForeignKeys().Single();
            Assert.False(firstFK.IsRequired);
            Assert.False(firstFK.IsUnique);
            Assert.Equal(
                new[] { MultipleNavigationsFirst.NonCollectionNavigationProperty.Name, MultipleNavigationsFirst.CollectionNavigationProperty.Name },
                firstEntityType.GetNavigations().Select(n => n.Name));

            Assert.Equal(2, secondEntityType.GetProperties().Count());
            Assert.Equal(1, secondEntityType.GetKeys().Count());
            var secondFK = firstEntityType.GetForeignKeys().Single();
            Assert.False(secondFK.IsRequired);
            Assert.False(secondFK.IsUnique);
            Assert.Empty(secondEntityType.GetNavigations());
        }

        [Fact]
        public void Bidirectional_ambiguous_cardinality_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<AmbiguousCardinalityOne>(ConfigureKeys);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            var model = (IModel)entityBuilder.Metadata.Model;
            Assert.Equal(2, model.EntityTypes.Count);
            var firstEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(AmbiguousCardinalityOne));
            var secondEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(AmbiguousCardinalityTwo));

            var fk = firstEntityType.GetNavigations().Single().ForeignKey;
            Assert.Same(fk, secondEntityType.GetNavigations().Single().ForeignKey);
            Assert.True(fk.IsUnique);
        }

        [Fact]
        public void Unidirectional_ambiguous_cardinality_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<AmbiguousCardinalityOne>(ConfigureKeys,
                AmbiguousCardinalityTwo.IgnoreNavigation);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            var model = (IModel)entityBuilder.Metadata.Model;
            Assert.Equal(2, model.EntityTypes.Count);
            var firstEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(AmbiguousCardinalityOne));
            var secondEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(AmbiguousCardinalityTwo));

            var fk = firstEntityType.GetNavigations().Single().ForeignKey;
            Assert.Empty(secondEntityType.GetNavigations());
            Assert.False(fk.IsUnique);
        }

        [Fact]
        public void Does_not_discover_nonNavigation_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoValidNavigations>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            Assert.Empty(entityBuilder.Metadata.GetForeignKeys());
            Assert.Empty(entityBuilder.Metadata.Navigations);
            Assert.Empty(entityBuilder.Metadata.Properties);
        }

        [Fact]
        public void One_to_one_bidirectional_self_ref_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<SelfRef>(ConfigureKeys);

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            IModel model = entityBuilder.Metadata.Model;
            var entityType = model.EntityTypes.Single();

            Assert.Equal(2, entityType.GetProperties().Count());
            Assert.Equal(1, entityType.GetKeys().Count());

            var fk = entityType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);
            Assert.True(fk.IsUnique);
            Assert.NotSame(fk.Properties.Single(), entityType.GetPrimaryKey().Properties.Single());
            Assert.Equal(2, entityType.GetNavigations().Count());
            Assert.Equal(SelfRef.SelfRef1NavigationProperty.Name, fk.PrincipalToDependent?.Name);
            Assert.Equal(SelfRef.SelfRef2NavigationProperty.Name, fk.DependentToPrincipal?.Name);
        }

        [Fact]
        public void Navigation_to_abstract_is_discovered()
        {
            var entityBuilder = CreateInternalEntityBuilder<AbstractClass>();

            Assert.Same(entityBuilder, new RelationshipDiscoveryConvention().Apply(entityBuilder));

            IModel model = entityBuilder.Metadata.Model;
            var entityType = model.EntityTypes.Single();

            Assert.Equal(2, entityType.GetProperties().Count());
            Assert.Equal(1, entityType.GetKeys().Count());

            var fk = entityType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);
            Assert.False(fk.IsUnique);
            Assert.True(fk.PrincipalEntityType.ClrType.IsAbstract);
            Assert.Equal(1, entityType.GetNavigations().Count());
        }

        private class EntityWithNoValidNavigations
        {
            public int Id { get; set; }

            public static OneToManyDependent Static { get; set; }

            public OneToManyDependent WriteOnly
            {
                set { }
            }

            public OneToManyDependent ReadOnly
            {
                get { return null; }
            }

            public OneToManyDependent this[int index]
            {
                get { return null; }
                set { }
            }

            public MyStruct Struct { get; set; }
            public IInterface Interface { get; set; }
        }

        private struct MyStruct
        {
            public int Id { get; set; }
        }

        private interface IInterface
        {
            int Id { get; set; }
        }

        private abstract class AbstractClass
        {
            public int Id { get; set; }
            public AbstractClass Abstract { get; set; }
        }

        private static void VerifyOneToOne(IModel model)
        {
            Assert.Equal(2, model.EntityTypes.Count);
            var principalEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(OneToOnePrincipal));
            var dependentEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(OneToOneDependent));

            Assert.Equal(1, principalEntityType.GetProperties().Count());
            Assert.Equal(1, principalEntityType.GetKeys().Count());
            Assert.Empty(principalEntityType.GetForeignKeys());
            Assert.Equal(OneToOnePrincipal.NavigationProperty.Name, principalEntityType.GetNavigations().Single().Name);

            Assert.Equal(2, dependentEntityType.GetProperties().Count());
            Assert.Equal(1, dependentEntityType.GetKeys().Count());
            var fk = dependentEntityType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);
            Assert.True(fk.IsUnique);
            Assert.NotSame(fk.Properties.Single(), dependentEntityType.GetPrimaryKey().Properties.Single());
            Assert.Equal(OneToOneDependent.NavigationProperty.Name, dependentEntityType.GetNavigations().Single().Name);
        }

        private static void VerifyOneToManyPrincipal(InternalEntityTypeBuilder entityTypeBuilder, bool unidirectional, bool dependentHasPK = true)
        {
            VerifyOneToMany(entityTypeBuilder.Metadata.Model, dependentHasPK, hasNavigationToDependent: true, hasNavigationToPrincipal: !unidirectional);
        }

        private static void VerifyOneToManyDependent(InternalEntityTypeBuilder entityTypeBuilder, bool unidirectional, bool dependentHasPK = true)
        {
            VerifyOneToMany(entityTypeBuilder.Metadata.Model, dependentHasPK, hasNavigationToDependent: !unidirectional, hasNavigationToPrincipal: true);
        }

        private static void VerifyOneToMany(
            IModel model,
            bool dependentHasPK,
            bool hasNavigationToDependent,
            bool hasNavigationToPrincipal)
        {
            Assert.Equal(2, model.EntityTypes.Count);
            var principalEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(OneToManyPrincipal));
            var dependentEntityType = model.EntityTypes.Single(e => e.ClrType == typeof(OneToManyDependent));

            Assert.Equal(dependentHasPK ? 2 : 1, dependentEntityType.GetProperties().Count());
            Assert.Equal(dependentHasPK ? 1 : 0, dependentEntityType.GetKeys().Count());
            var fk = dependentEntityType.GetForeignKeys().Single();
            Assert.False(fk.IsRequired);
            Assert.False(fk.IsUnique);
            if (hasNavigationToPrincipal)
            {
                Assert.Equal(OneToManyDependent.NavigationProperty.Name, dependentEntityType.GetNavigations().Single().Name);
            }
            else
            {
                Assert.Empty(dependentEntityType.GetNavigations());
            }

            Assert.Equal(1, principalEntityType.GetProperties().Count());
            Assert.Equal(1, principalEntityType.GetKeys().Count());
            Assert.Empty(principalEntityType.GetForeignKeys());
            if (hasNavigationToDependent)
            {
                Assert.Equal(OneToManyPrincipal.NavigationProperty.Name, principalEntityType.GetNavigations().Single().Name);
            }
            else
            {
                Assert.Empty(principalEntityType.GetNavigations());
            }
        }

        private static InternalEntityTypeBuilder CreateInternalEntityBuilder<T>(params Action<InternalEntityTypeBuilder>[] onEntityAdded)
        {
            var conventions = new ConventionSet();
            if (onEntityAdded != null)
            {
                conventions.EntityTypeAddedConventions.Add(new TestModelChangeListener(onEntityAdded));
            }
            var modelBuilder = new InternalModelBuilder(new Model(), conventions);
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.DataAnnotation);

            return entityBuilder;
        }

        private static void ConfigureKeys(InternalEntityTypeBuilder entityTypeBuilder)
        {
            entityTypeBuilder.PrimaryKey(new[] { "Id" }, ConfigurationSource.Convention);
        }

        private class TestModelChangeListener : IEntityTypeConvention
        {
            private readonly Action<InternalEntityTypeBuilder>[] _onEntityAdded;

            public TestModelChangeListener(Action<InternalEntityTypeBuilder>[] onEntityAdded)
            {
                _onEntityAdded = onEntityAdded;
            }

            public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
            {
                foreach (var action in _onEntityAdded)
                {
                    action(entityTypeBuilder);
                }

                return entityTypeBuilder;
            }
        }

        public class OneToOnePrincipal
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToOnePrincipal).GetProperty("OneToOneDependent", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }
            public OneToOneDependent OneToOneDependent { get; set; }
        }

        public class OneToOneDependent
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToOneDependent).GetProperty("OneToOnePrincipal", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }
            public OneToOnePrincipal OneToOnePrincipal { get; set; }
        }

        public class OneToManyPrincipal
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToManyPrincipal).GetProperty("OneToManyDependents", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }

            public IEnumerable<OneToManyDependent> OneToManyDependents { get; set; }

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(OneToManyPrincipal))
                {
                    entityTypeBuilder.Ignore(NavigationProperty.Name, ConfigurationSource.DataAnnotation);
                }
            }
        }

        public class OneToManyDependent
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToManyDependent).GetProperty("OneToManyPrincipal", BindingFlags.NonPublic | BindingFlags.Instance);

            public int Id { get; set; }

            private OneToManyPrincipal OneToManyPrincipal { get; set; }

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(OneToManyDependent))
                {
                    entityTypeBuilder.Ignore(NavigationProperty.Name, ConfigurationSource.DataAnnotation);
                }
            }
        }

        public class ManyToManyFirst
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToManyPrincipal).GetProperty("ManyToManySeconds", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }
            public IEnumerable<ManyToManySecond> ManyToManySeconds { get; set; }
        }

        public class ManyToManySecond
        {
            public static readonly PropertyInfo NavigationProperty =
                typeof(OneToManyPrincipal).GetProperty("ManyToManyFirsts", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }
            public IEnumerable<ManyToManyFirst> ManyToManyFirsts { get; set; }
        }

        public class MultipleNavigationsFirst
        {
            public static readonly PropertyInfo CollectionNavigationProperty =
                typeof(MultipleNavigationsFirst).GetProperty("MultipleNavigationsSeconds", BindingFlags.Public | BindingFlags.Instance);

            public static readonly PropertyInfo NonCollectionNavigationProperty =
                typeof(MultipleNavigationsFirst).GetProperty("MultipleNavigationsSecond", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }

            public IEnumerable<MultipleNavigationsSecond> MultipleNavigationsSeconds { get; set; }
            public MultipleNavigationsSecond MultipleNavigationsSecond { get; set; }
        }

        public class MultipleNavigationsSecond
        {
            public static readonly PropertyInfo CollectionNavigationProperty =
                typeof(MultipleNavigationsSecond).GetProperty("MultipleNavigationsFirsts", BindingFlags.Public | BindingFlags.Instance);

            public static readonly PropertyInfo NonCollectionNavigationProperty =
                typeof(MultipleNavigationsSecond).GetProperty("MultipleNavigationsFirst", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }

            public IEnumerable<MultipleNavigationsFirst> MultipleNavigationsFirsts { get; set; }
            public MultipleNavigationsFirst MultipleNavigationsFirst { get; set; }

            public static void IgnoreCollectionNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(MultipleNavigationsSecond))
                {
                    entityTypeBuilder.Ignore(CollectionNavigationProperty.Name, ConfigurationSource.DataAnnotation);
                }
            }

            public static void IgnoreNonCollectionNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(MultipleNavigationsSecond))
                {
                    entityTypeBuilder.Ignore(NonCollectionNavigationProperty.Name, ConfigurationSource.DataAnnotation);
                }
            }
        }

        private class SelfRef
        {
            public static readonly PropertyInfo SelfRef1NavigationProperty =
                typeof(SelfRef).GetProperty("SelfRef1", BindingFlags.Public | BindingFlags.Instance);

            public static readonly PropertyInfo SelfRef2NavigationProperty =
                typeof(SelfRef).GetProperty("SelfRef2", BindingFlags.Public | BindingFlags.Instance);

            public int Id { get; set; }
            public SelfRef SelfRef1 { get; set; }
            public SelfRef SelfRef2 { get; set; }
            public int SelfRefId { get; set; }
        }

        public class AmbiguousCardinalityOne : IEnumerable<AmbiguousCardinalityOne>
        {
            public int Id { get; set; }
            public AmbiguousCardinalityTwo AmbiguousCardinalityTwo { get; set; }

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(AmbiguousCardinalityOne))
                {
                    entityTypeBuilder.Ignore(nameof(AmbiguousCardinalityTwo), ConfigurationSource.DataAnnotation);
                }
            }

            public IEnumerator<AmbiguousCardinalityOne> GetEnumerator()
            {
                yield return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class AmbiguousCardinalityTwo : IEnumerable<AmbiguousCardinalityTwo>
        {
            public int Id { get; set; }
            public AmbiguousCardinalityOne AmbiguousCardinalityOne { get; set; }

            public static void IgnoreNavigation(InternalEntityTypeBuilder entityTypeBuilder)
            {
                if (entityTypeBuilder.Metadata.ClrType == typeof(AmbiguousCardinalityTwo))
                {
                    entityTypeBuilder.Ignore(nameof(AmbiguousCardinalityOne), ConfigurationSource.DataAnnotation);
                }
            }

            public IEnumerator<AmbiguousCardinalityTwo> GetEnumerator()
            {
                yield return this;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
