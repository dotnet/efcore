// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Tests
{
    public class ForeignKeyTest
    {
        [Fact]
        public void Can_create_foreign_key()
        {
            var entityType = new Model().AddEntityType("E");
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);
            var principalProp = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(principalProp);

            var foreignKey
                = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey(), entityType)
                {
                    IsUnique = true
                };

            Assert.Same(entityType, foreignKey.PrincipalEntityType);
            Assert.Same(principalProp, foreignKey.PrincipalKey.Properties.Single());
            Assert.Same(dependentProp, foreignKey.Properties.Single());
            Assert.True(foreignKey.IsUnique.Value);
            Assert.Same(entityType.GetPrimaryKey(), foreignKey.PrincipalKey);
        }

        [Fact]
        public void Constructor_throws_when_referenced_key_not_on_referenced_entity()
        {
            var model = new Model();

            var principalEntityType = model.AddEntityType("R");
            var dependentEntityType = model.AddEntityType("D");
            var fk = dependentEntityType.AddProperty("Fk", typeof(int), shadowProperty: true);

            var principalKey = dependentEntityType.SetPrimaryKey(fk);

            Assert.Throws<ArgumentException>(() =>
                new ForeignKey(new[] { fk }, principalKey, principalEntityType));
        }

        [Fact]
        public void Constructor_throws_when_principal_and_depedent_property_count_do_not_match()
        {
            var dependentType = new Model().AddEntityType("D");
            var principalType = new Model().AddEntityType("P");

            var dependentProperty1 = dependentType.GetOrAddProperty("P1", typeof(int), shadowProperty: true);
            var dependentProperty2 = dependentType.GetOrAddProperty("P2", typeof(int), shadowProperty: true);

            principalType.GetOrSetPrimaryKey(principalType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            Assert.Equal(
                Strings.ForeignKeyCountMismatch("{'P1', 'P2'}", "D", "{'Id'}", "P"),
                Assert.Throws<InvalidOperationException>(
                    () => new ForeignKey(new[] { dependentProperty1, dependentProperty2 }, principalType.GetPrimaryKey(), principalType)).Message);
        }

        [Fact]
        public void Constructor_throws_when_principal_and_depedent_property_types_do_not_match()
        {
            var dependentType = new Model().AddEntityType("D");
            var principalType = new Model().AddEntityType("P");

            var dependentProperty1 = dependentType.GetOrAddProperty("P1", typeof(int), shadowProperty: true);
            var dependentProperty2 = dependentType.GetOrAddProperty("P2", typeof(string), shadowProperty: true);
            var dependentProperty3 = dependentType.GetOrAddProperty("P3", typeof(int?), shadowProperty: true);

            principalType.GetOrSetPrimaryKey(new[]
            {
                principalType.GetOrAddProperty("Id1", typeof(int), shadowProperty: true),
                principalType.GetOrAddProperty("Id2", typeof(int), shadowProperty: true)
            });

            new ForeignKey(new[] { dependentProperty1, dependentProperty3 }, principalType.GetPrimaryKey(), principalType);

            Assert.Equal(
                Strings.ForeignKeyTypeMismatch("{'P1', 'P2'}", "D", "P"),
                Assert.Throws<InvalidOperationException>(
                    () => new ForeignKey(new[] { dependentProperty1, dependentProperty2 }, principalType.GetPrimaryKey(), principalType)).Message);
        }

        [Fact]
        public void Can_create_foreign_key_with_non_pk_principal()
        {
            var entityType = new Model().AddEntityType("E");
            var keyProp = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);
            var principalProp = entityType.GetOrAddProperty("U", typeof(int), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(keyProp);
            var principalKey = entityType.AddKey(principalProp);

            var foreignKey
                = new ForeignKey(new[] { dependentProp }, principalKey, entityType)
                {
                    IsUnique = false
                };

            Assert.Same(entityType, foreignKey.PrincipalEntityType);
            Assert.Same(principalProp, foreignKey.PrincipalKey.Properties.Single());
            Assert.Same(dependentProp, foreignKey.Properties.Single());
            Assert.False(foreignKey.IsUnique.Value);
            Assert.Same(principalKey, foreignKey.PrincipalKey);
        }

        [Fact]
        public void IsRequired_true_when_dependent_property_not_nullable()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);
            dependentProp.IsNullable = false;

            var foreignKey = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey(), entityType);

            Assert.True(foreignKey.IsRequired);
            Assert.True(((IForeignKey)foreignKey).IsRequired);
        }

        [Fact]
        public void IsRequired_false_when_dependent_property_nullable()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int?), shadowProperty: true);
            dependentProp.IsNullable = true;

            var foreignKey = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey(), entityType);

            Assert.False(foreignKey.IsRequired);
            Assert.False(((IForeignKey)foreignKey).IsRequired);
        }

        [Fact]
        public void IsRequired_and_IsUnique_null_when_dependent_property_not_nullable_by_default()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);

            var foreignKey = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey(), entityType);

            Assert.Null(foreignKey.IsRequired);
            Assert.True(((IForeignKey)foreignKey).IsRequired);
            Assert.Null(foreignKey.IsUnique);
            Assert.False(((IForeignKey)foreignKey).IsUnique);
        }

        [Fact]
        public void IsRequired_and_IsUnique_null_when_dependent_property_nullable_by_default()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int?), shadowProperty: true);

            var foreignKey = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey(), entityType);

            Assert.Null(foreignKey.IsRequired);
            Assert.False(((IForeignKey)foreignKey).IsRequired);
            Assert.Null(foreignKey.IsUnique);
            Assert.False(((IForeignKey)foreignKey).IsUnique);
        }

        [Fact]
        public void IsRequired_false_for_composite_FK_by_default()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(new[]
            {
                entityType.GetOrAddProperty("Id1", typeof(int), shadowProperty: true),
                entityType.GetOrAddProperty("Id2", typeof(string), shadowProperty: true)
            });

            var dependentProp1 = entityType.GetOrAddProperty("P1", typeof(int), shadowProperty: true);
            var dependentProp2 = entityType.GetOrAddProperty("P2", typeof(string), shadowProperty: true);

            var foreignKey = new ForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.GetPrimaryKey(), entityType);

            Assert.Null(foreignKey.IsRequired);
            Assert.False(((IForeignKey)foreignKey).IsRequired);
        }

        [Fact]
        public void IsRequired_false_when_any_part_of_composite_FK_is_nullable()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(new[]
            {
                entityType.GetOrAddProperty("Id1", typeof(int), shadowProperty: true),
                entityType.GetOrAddProperty("Id2", typeof(string), shadowProperty: true)
            });

            var dependentProp1 = entityType.GetOrAddProperty("P1", typeof(int), shadowProperty: true);
            var dependentProp2 = entityType.GetOrAddProperty("P2", typeof(string), shadowProperty: true);
            dependentProp2.IsNullable = true;

            var foreignKey = new ForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.GetPrimaryKey(), entityType);

            Assert.False(foreignKey.IsRequired);
            Assert.False(((IForeignKey)foreignKey).IsRequired);

            dependentProp2.IsNullable = false;

            Assert.True(foreignKey.IsRequired);
            Assert.True(((IForeignKey)foreignKey).IsRequired);
        }

        [Fact]
        public void Setting_IsRequired_to_true_will_set_all_FK_properties_as_non_nullable()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(
                new[]
                {
                    entityType.GetOrAddProperty("Id1", typeof(int), shadowProperty: true),
                    entityType.GetOrAddProperty("Id2", typeof(string), shadowProperty: true)
                });

            var dependentProp1 = entityType.GetOrAddProperty("P1", typeof(int), shadowProperty: true);
            var dependentProp2 = entityType.GetOrAddProperty("P2", typeof(string), shadowProperty: true);

            var foreignKey = new ForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.GetPrimaryKey(), entityType)
            { IsRequired = true };

            Assert.True(foreignKey.IsRequired.Value);
            Assert.False(dependentProp1.IsNullable.Value);
            Assert.False(dependentProp2.IsNullable.Value);
        }

        [Fact]
        public void Setting_IsRequired_to_false_will_set_all_FK_properties_as_nullable()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(new[]
            {
                entityType.GetOrAddProperty("Id1", typeof(int), shadowProperty: true),
                entityType.GetOrAddProperty("Id2", typeof(string), shadowProperty: true)
            });

            var dependentProp1 = entityType.GetOrAddProperty("P1", typeof(int?), shadowProperty: true);
            var dependentProp2 = entityType.GetOrAddProperty("P2", typeof(string), shadowProperty: true);

            var foreignKey = new ForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.GetPrimaryKey(), entityType) { IsRequired = false };

            Assert.False(foreignKey.IsRequired.Value);
            Assert.True(dependentProp1.IsNullable.Value);
            Assert.True(dependentProp2.IsNullable.Value);
        }

        [Fact]
        public void IsCompatible_returns_true_for_one_to_many_if_all_critaria_match()
        {
            var fk = CreateOneToManyFK();

            Assert.True(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "OneToManyPrincipal",
                "OneToManyDependents",
                fk.Properties,
                fk.PrincipalKey.Properties,
                false));
        }

        [Fact]
        public void IsCompatible_returns_true_for_one_to_many_if_using_nulls()
        {
            var fk = CreateOneToManyFK();

            Assert.True(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "OneToManyPrincipal",
                "OneToManyDependents",
                null,
                null,
                null));

            Assert.True(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "OneToManyPrincipal",
                "OneToManyDependents",
                new Property[0],
                new Property[0],
                null));
        }

        [Fact]
        public void IsCompatible_returns_true_for_one_to_many_if_no_navigations_exist()
        {
            var fk = CreateOneToManyFK();
            fk.PrincipalEntityType.RemoveNavigation(fk.PrincipalToDependent);
            fk.DeclaringEntityType.RemoveNavigation(fk.DependentToPrincipal);

            Assert.True(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "Nav",
                "Nav",
                fk.Properties,
                fk.PrincipalKey.Properties,
                false));
        }

        [Fact]
        public void IsCompatible_returns_false_for_one_to_many_if_any_critaria_does_not_match()
        {
            var fk = CreateOneToManyFK();

            Assert.False(fk.IsCompatible(
                fk.DeclaringEntityType,
                fk.DeclaringEntityType,
                "OneToManyPrincipal",
                "OneToManyDependents",
                fk.Properties,
                fk.PrincipalKey.Properties,
                false));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.PrincipalEntityType,
                "OneToManyPrincipal",
                "OneToManyDependents",
                fk.Properties,
                fk.PrincipalKey.Properties,
                false));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                null,
                "OneToManyDependents",
                fk.Properties,
                fk.PrincipalKey.Properties,
                false));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "OneToManyPrincipal",
                null,
                fk.Properties,
                fk.PrincipalKey.Properties,
                false));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "OneToManyPrincipal",
                "OneToManyDependents",
                fk.PrincipalKey.Properties,
                fk.PrincipalKey.Properties,
                false));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "OneToManyPrincipal",
                "OneToManyDependents",
                fk.Properties,
                fk.Properties,
                false));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "OneToManyPrincipal",
                "OneToManyDependents",
                fk.Properties,
                fk.PrincipalKey.Properties,
                true));
        }

        private ForeignKey CreateOneToManyFK()
        {
            var model = new Model();
            var principalEntityType = model.AddEntityType(typeof(OneToManyPrincipal));
            var pk = principalEntityType.GetOrSetPrimaryKey(principalEntityType.GetOrAddProperty("Id", typeof(int)));

            var dependentEntityType = model.AddEntityType(typeof(OneToManyDependent));
            var fkProp = dependentEntityType.GetOrAddProperty("Id", typeof(int));
            var fk = dependentEntityType.AddForeignKey(new[] { fkProp }, pk, principalEntityType);

            principalEntityType.AddNavigation("OneToManyDependents", fk, pointsToPrincipal: false);
            dependentEntityType.AddNavigation("OneToManyPrincipal", fk, pointsToPrincipal: true);
            return fk;
        }

        private ForeignKey CreateOneToManySameBaseFK()
        {
            var model = new Model();

            var baseEntityType = model.AddEntityType(typeof(NavigationBase));
            var pk = baseEntityType.GetOrSetPrimaryKey(baseEntityType.GetOrAddProperty("Id", typeof(int)));

            var principalEntityType = model.AddEntityType(typeof(OneToManyPrincipal));
            principalEntityType.BaseType = baseEntityType;

            var dependentEntityType = model.AddEntityType(typeof(OneToManyDependent));
            dependentEntityType.BaseType = baseEntityType;
            var fkProp = dependentEntityType.GetOrAddProperty("Fk", typeof(int), shadowProperty: true);
            var fk = dependentEntityType.AddForeignKey(new[] { fkProp }, pk, principalEntityType);

            principalEntityType.AddNavigation("OneToManyDependents", fk, pointsToPrincipal: false);
            dependentEntityType.AddNavigation("OneToManyPrincipal", fk, pointsToPrincipal: true);
            return fk;
        }

        private ForeignKey CreateOneToManySameHierarchyFK()
        {
            var model = new Model();

            var baseEntityType = model.AddEntityType(typeof(NavigationBase));
            var pk = baseEntityType.GetOrSetPrimaryKey(baseEntityType.GetOrAddProperty("Id", typeof(int)));

            var dependentEntityType = model.AddEntityType(typeof(OneToManyDependent));
            dependentEntityType.BaseType = baseEntityType;
            var fkProp = dependentEntityType.GetOrAddProperty("Fk", typeof(int), shadowProperty: true);
            var fk = dependentEntityType.AddForeignKey(new[] { fkProp }, pk, baseEntityType);

            baseEntityType.AddNavigation("OneToManyDependents", fk, pointsToPrincipal: false);
            return fk;
        }

        public abstract class NavigationBase
        {
            public int Id { get; set; }
            public IEnumerable<OneToManyDependent> OneToManyDependents { get; set; }
            public OneToManyPrincipal OneToManyPrincipal { get; set; }
        }

        public class OneToManyPrincipal : NavigationBase
        {
            public IEnumerable<OneToManyDependent> Deception { get; set; }
        }

        public class OneToManyDependent : NavigationBase
        {
            public OneToManyPrincipal Deception { get; set; }
        }

        [Fact]
        public void IsCompatible_returns_true_for_self_ref_one_to_one_if_all_critaria_match()
        {
            var fk = CreateSelfRefFK();

            Assert.True(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "SelfRefPrincipal",
                "SelfRefDependent",
                fk.Properties,
                fk.PrincipalKey.Properties,
                true));
        }

        [Fact]
        public void IsCompatible_returns_false_for_self_ref_one_to_one_if_any_critaria_does_not_match()
        {
            var fk = CreateSelfRefFK();

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "SelfRefDependent",
                "SelfRefPrincipal",
                fk.Properties,
                fk.PrincipalKey.Properties,
                true));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                null,
                null,
                fk.Properties,
                fk.PrincipalKey.Properties,
                true));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "SelfRefPrincipal",
                "SelfRefDependent",
                fk.PrincipalKey.Properties,
                fk.Properties,
                true));

            Assert.False(fk.IsCompatible(
                fk.PrincipalEntityType,
                fk.DeclaringEntityType,
                "SelfRefPrincipal",
                "SelfRefDependent",
                fk.Properties,
                fk.PrincipalKey.Properties,
                false));
        }

        [Fact]
        public void Throws_when_setting_navigation_to_principal_on_wrong_FK()
        {
            var foreignKey1 = CreateOneToManyFK();
            foreignKey1.DeclaringEntityType.RemoveNavigation(foreignKey1.DependentToPrincipal);
            var navigation = foreignKey1.DeclaringEntityType.AddNavigation("Deception", foreignKey1, pointsToPrincipal: true);

            var foreignKey2 = CreateSelfRefFK();

            Assert.Equal(
                Strings.NavigationForWrongForeignKey("Deception", "OneToManyDependent", Property.Format(foreignKey2.Properties), Property.Format(foreignKey1.Properties)),
                Assert.Throws<InvalidOperationException>(() => foreignKey2.DependentToPrincipal = navigation).Message);
        }

        [Fact]
        public void Throws_when_setting_navigation_to_dependent_on_wrong_FK()
        {
            var foreignKey1 = CreateOneToManyFK();
            foreignKey1.PrincipalEntityType.RemoveNavigation(foreignKey1.PrincipalToDependent);
            var navigation = foreignKey1.PrincipalEntityType.AddNavigation("Deception", foreignKey1, pointsToPrincipal: false);

            var foreignKey2 = CreateSelfRefFK();

            Assert.Equal(
                Strings.NavigationForWrongForeignKey("Deception", "OneToManyPrincipal", Property.Format(foreignKey2.Properties), Property.Format(foreignKey1.Properties)),
                Assert.Throws<InvalidOperationException>(() => foreignKey2.PrincipalToDependent = navigation).Message);
        }

        [Fact]
        public void Throws_when_setting_navigation_to_principal_directly()
        {
            var foreignKey = CreateOneToManyFK();

            var newNav = new Navigation("NewNav", foreignKey);
            Assert.Equal(
                Strings.NavigationNotFound("NewNav", foreignKey.DeclaringEntityType.Name),
                Assert.Throws<InvalidOperationException>(() => foreignKey.DependentToPrincipal = newNav).Message);
        }

        [Fact]
        public void Throws_when_setting_navigation_to_dependent_directly()
        {
            var foreignKey = CreateOneToManyFK();

            var newNav = new Navigation("NewNav", foreignKey);
            Assert.Equal(
                Strings.NavigationNotFound("NewNav", foreignKey.PrincipalEntityType.Name),
                Assert.Throws<InvalidOperationException>(() => foreignKey.PrincipalToDependent = newNav).Message);
        }

        [Fact]
        public void Throws_when_setting_navigation_to_principal_to_null_directly()
        {
            var foreignKey = CreateOneToManyFK();

            Assert.Equal(
                Strings.NavigationStillOnEntityType(foreignKey.DependentToPrincipal.Name, foreignKey.DeclaringEntityType),
                Assert.Throws<InvalidOperationException>(() => foreignKey.DependentToPrincipal = null).Message);
        }

        [Fact]
        public void Throws_when_setting_navigation_to_dependent_to_null_directly()
        {
            var foreignKey = CreateOneToManyFK();

            Assert.Equal(
                Strings.NavigationStillOnEntityType(foreignKey.PrincipalToDependent.Name, foreignKey.PrincipalEntityType),
                Assert.Throws<InvalidOperationException>(() => foreignKey.PrincipalToDependent = null).Message);
        }

        private ForeignKey CreateSelfRefFK(bool useAltKey = false)
        {
            var entityType = new Model().AddEntityType(typeof(SelfRef));
            var pk = entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int)));
            var fkProp = entityType.GetOrAddProperty("SelfRefId", typeof(int?));

            var principalKey = useAltKey
                ? entityType.GetOrAddKey(entityType.GetOrAddProperty("AltId", typeof(int), shadowProperty: true))
                : pk;

            var fk = entityType.AddForeignKey(new[] { fkProp }, principalKey, entityType);
            fk.IsUnique = true;
            entityType.AddNavigation("SelfRefPrincipal", fk, pointsToPrincipal: true);
            entityType.AddNavigation("SelfRefDependent", fk, pointsToPrincipal: false);
            return fk;
        }

        private class SelfRef
        {
            public int Id { get; set; }
            public SelfRef SelfRefPrincipal { get; set; }
            public SelfRef SelfRefDependent { get; set; }
            public int? SelfRefId { get; set; }
        }

        [Fact]
        public void IsSelfReferencing_returns_true_for_self_ref_foreign_keys()
        {
            var fk = CreateSelfRefFK();

            Assert.True(fk.IsSelfReferencing());
        }

        [Fact]
        public void IsSelfReferencing_returns_true_for_non_pk_self_ref_foreign_keys()
        {
            var fk = CreateSelfRefFK(useAltKey: true);

            Assert.True(fk.IsSelfReferencing());
        }

        [Fact]
        public void IsSelfReferencing_returns_false_for_same_hierarchy_foreign_keys()
        {
            var fk = CreateOneToManySameHierarchyFK();

            Assert.False(fk.IsSelfReferencing());
        }

        [Fact]
        public void IsSelfReferencing_returns_false_for_same_base_foreign_keys()
        {
            var fk = CreateOneToManySameBaseFK();

            Assert.False(fk.IsSelfReferencing());
        }

        [Fact]
        public void IsSelfReferencing_returns_false_for_non_hierarchical_foreign_keys()
        {
            var fk = CreateOneToManyFK();

            Assert.False(fk.IsSelfReferencing());
        }

        [Fact]
        public void IsIntraHierarchical_returns_true_for_self_ref_foreign_keys()
        {
            var fk = CreateSelfRefFK();

            Assert.True(fk.IsIntraHierarchical());
        }

        [Fact]
        public void IsIntraHierarchical_returns_true_for_non_pk_self_ref_foreign_keys()
        {
            var fk = CreateSelfRefFK(useAltKey: true);

            Assert.True(fk.IsIntraHierarchical());
        }

        [Fact]
        public void IsIntraHierarchical_returns_true_for_same_hierarchy_foreign_keys()
        {
            var fk = CreateOneToManySameHierarchyFK();

            Assert.True(fk.IsIntraHierarchical());
        }

        [Fact]
        public void IsIntraHierarchical_returns_false_for_same_base_foreign_keys()
        {
            var fk = CreateOneToManySameBaseFK();

            Assert.False(fk.IsIntraHierarchical());
        }

        [Fact]
        public void IsIntraHierarchical_returns_false_for_non_hierarchical_foreign_keys()
        {
            var fk = CreateOneToManyFK();

            Assert.False(fk.IsIntraHierarchical());
        }

        [Fact]
        public void IsSelfPrimaryKeyReferencing_returns_true_for_self_ref_foreign_keys()
        {
            var fk = CreateSelfRefFK();

            Assert.True(fk.IsSelfPrimaryKeyReferencing());
        }

        [Fact]
        public void IsSelfPrimaryKeyReferencing_returns_false_for_non_pk_self_ref_foreign_keys()
        {
            var fk = CreateSelfRefFK(useAltKey: true);

            Assert.False(fk.IsSelfPrimaryKeyReferencing());
        }

        [Fact]
        public void IsSelfPrimaryKeyReferencing_returns_true_for_same_hierarchy_foreign_keys()
        {
            var fk = CreateOneToManySameHierarchyFK();

            Assert.True(fk.IsSelfPrimaryKeyReferencing());
        }

        [Fact]
        public void IsSelfPrimaryKeyReferencing_returns_true_for_same_base_foreign_keys()
        {
            var fk = CreateOneToManySameBaseFK();

            Assert.True(fk.IsSelfPrimaryKeyReferencing());
        }

        [Fact]
        public void IsSelfPrimaryKeyReferencing_returns_false_for_non_hierarchical_foreign_keys()
        {
            var fk = CreateOneToManyFK();

            Assert.False(fk.IsSelfPrimaryKeyReferencing());
        }

        [Fact]
        public void Can_find_targets_for_non_hierarchical_foreign_keys()
        {
            var fk = CreateOneToManyFK();

            Assert.Same(fk.PrincipalEntityType, fk.GetOtherEntityType(fk.DeclaringEntityType));
            Assert.Same(fk.DeclaringEntityType, fk.GetOtherEntityType(fk.PrincipalEntityType));
            Assert.Same(fk.PrincipalToDependent, fk.FindNavigationFrom(fk.PrincipalEntityType));
            Assert.Same(fk.DependentToPrincipal, fk.FindNavigationFrom(fk.DeclaringEntityType));
            Assert.Same(fk.DependentToPrincipal, fk.FindNavigationTo(fk.PrincipalEntityType));
            Assert.Same(fk.PrincipalToDependent, fk.FindNavigationTo(fk.DeclaringEntityType));
        }

        [Fact]
        public void Can_find_targets_for_same_base_foreign_keys()
        {
            var fk = CreateOneToManySameBaseFK();

            var model = fk.DeclaringEntityType.Model;
            var derivedPrincipal = model.AddEntityType("DerivedPrincipal");
            derivedPrincipal.BaseType = fk.PrincipalEntityType;

            var derivedDependent = model.AddEntityType("DerivedDependent");
            derivedDependent.BaseType = fk.DeclaringEntityType;

            Assert.Same(fk.PrincipalEntityType, fk.GetOtherEntityType(fk.DeclaringEntityType));
            Assert.Same(fk.DeclaringEntityType, fk.GetOtherEntityType(fk.PrincipalEntityType));
            Assert.Same(fk.PrincipalToDependent, fk.FindNavigationFrom(fk.PrincipalEntityType));
            Assert.Same(fk.DependentToPrincipal, fk.FindNavigationFrom(fk.DeclaringEntityType));
            Assert.Same(fk.DependentToPrincipal, fk.FindNavigationTo(fk.PrincipalEntityType));
            Assert.Same(fk.PrincipalToDependent, fk.FindNavigationTo(fk.DeclaringEntityType));

            Assert.Same(fk.PrincipalEntityType, fk.GetOtherEntityType(derivedDependent));
            Assert.Same(fk.DeclaringEntityType, fk.GetOtherEntityType(derivedPrincipal));
            Assert.Same(fk.PrincipalToDependent, fk.FindNavigationFrom(derivedPrincipal));
            Assert.Same(fk.DependentToPrincipal, fk.FindNavigationFrom(derivedDependent));
            Assert.Same(fk.DependentToPrincipal, fk.FindNavigationTo(derivedPrincipal));
            Assert.Same(fk.PrincipalToDependent, fk.FindNavigationTo(derivedDependent));
        }

        [Fact]
        public void Finding_targets_throws_for_self_ref_foreign_keys()
        {
            var fk = CreateSelfRefFK();

            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousTargetEntityType(fk.DeclaringEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.GetOtherEntityType(fk.DeclaringEntityType)).Message);
            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousTargetEntityType(fk.PrincipalEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.GetOtherEntityType(fk.PrincipalEntityType)).Message);

            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousNavigation(fk.PrincipalEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.FindNavigationFrom(fk.PrincipalEntityType)).Message);
            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousNavigation(fk.DeclaringEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.FindNavigationFrom(fk.DeclaringEntityType)).Message);

            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousNavigation(fk.PrincipalEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.FindNavigationTo(fk.PrincipalEntityType)).Message);
            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousNavigation(fk.DeclaringEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.FindNavigationTo(fk.DeclaringEntityType)).Message);
        }

        [Fact]
        public void Finding_targets_throws_for_same_hierarchy_foreign_keys()
        {
            var fk = CreateOneToManySameHierarchyFK();

            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousTargetEntityType(fk.DeclaringEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.GetOtherEntityType(fk.DeclaringEntityType)).Message);
            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousTargetEntityType(fk.PrincipalEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.GetOtherEntityType(fk.PrincipalEntityType)).Message);

            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousNavigation(fk.PrincipalEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.FindNavigationFrom(fk.PrincipalEntityType)).Message);
            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousNavigation(fk.DeclaringEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.FindNavigationFrom(fk.DeclaringEntityType)).Message);

            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousNavigation(fk.PrincipalEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.FindNavigationTo(fk.PrincipalEntityType)).Message);
            Assert.Equal(
                Strings.IntraHierarchicalAmbiguousNavigation(fk.DeclaringEntityType.Name, Property.Format(fk.Properties)),
                Assert.Throws<InvalidOperationException>(() => fk.FindNavigationTo(fk.DeclaringEntityType)).Message);
        }

        [Fact]
        public void Finding_targets_throws_for_entity_types_not_in_the_relationship()
        {
            var fk = CreateOneToManyFK();
            var unrelatedType = fk.DeclaringEntityType.Model.AddEntityType(typeof(NavigationBase));

            Assert.Equal(
                Strings.EntityTypeNotInRelationship(unrelatedType.Name, fk.DeclaringEntityType.Name, fk.PrincipalEntityType.Name),
                Assert.Throws<ArgumentException>(() => fk.GetOtherEntityType(unrelatedType)).Message);
            Assert.Equal(
                Strings.EntityTypeNotInRelationship(unrelatedType.Name, fk.DeclaringEntityType.Name, fk.PrincipalEntityType.Name),
                Assert.Throws<ArgumentException>(() => fk.GetOtherEntityType(unrelatedType)).Message);

            Assert.Equal(
                Strings.EntityTypeNotInRelationship(unrelatedType.Name, fk.DeclaringEntityType.Name, fk.PrincipalEntityType.Name),
                Assert.Throws<ArgumentException>(() => fk.FindNavigationFrom(unrelatedType)).Message);
            Assert.Equal(
                Strings.EntityTypeNotInRelationship(unrelatedType.Name, fk.DeclaringEntityType.Name, fk.PrincipalEntityType.Name),
                Assert.Throws<ArgumentException>(() => fk.FindNavigationFrom(unrelatedType)).Message);

            Assert.Equal(
                Strings.EntityTypeNotInRelationship(unrelatedType.Name, fk.DeclaringEntityType.Name, fk.PrincipalEntityType.Name),
                Assert.Throws<ArgumentException>(() => fk.FindNavigationTo(unrelatedType)).Message);
            Assert.Equal(
                Strings.EntityTypeNotInRelationship(unrelatedType.Name, fk.DeclaringEntityType.Name, fk.PrincipalEntityType.Name),
                Assert.Throws<ArgumentException>(() => fk.FindNavigationTo(unrelatedType)).Message);
        }
    }
}
