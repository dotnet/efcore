// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.ModelConventions
{
    public class ForeignKeyConventionTest
    {
        private readonly Model _model = BuildModel();

        [Fact]
        public void Foreign_key_matching_given_properties_is_found()
        {
            DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);
            var fkProperty = DependentType.GetOrAddProperty("HeToldMeYouKilledMyFk", typeof(int), shadowProperty: true);

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey());

            Assert.Same(
                fk,
                new ForeignKeyConvention().FindOrCreateForeignKey(
                    PrincipalType, DependentType, "SomeNav", "SomeInverse", new[] { fkProperty }, new Property[0], isUnqiue: false));
        }

        [Fact]
        public void Foreign_key_matching_given_property_is_found()
        {
            DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);
            var fkProperty1 = DependentType.GetOrAddProperty("No", typeof(int), shadowProperty: true);
            var fkProperty2 = DependentType.GetOrAddProperty("IAmYourFk", typeof(int), shadowProperty: true);

            var fk = DependentType.GetOrAddForeignKey(new[] { fkProperty1, fkProperty2 }, PrincipalType.GetOrAddKey(
                new[]
                    {
                        PrincipalType.GetOrAddProperty("Id1", typeof(int), shadowProperty: true),
                        PrincipalType.GetOrAddProperty("Id2", typeof(int), shadowProperty: true)
                    }));

            Assert.Same(
                fk,
                new ForeignKeyConvention().FindOrCreateForeignKey(
                    PrincipalType, DependentType, "SomeNav", "SomeInverse", new[] { fkProperty1, fkProperty2 }, new Property[0], isUnqiue: false));
        }

        [Fact]
        public void Foreign_key_matching_navigation_plus_Id_is_found()
        {
            var fkProperty = DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey());

            Assert.Same(fk, new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: false));
        }

        [Fact]
        public void Foreign_key_matching_navigation_plus_PK_name_is_found()
        {
            var fkProperty = DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey());

            Assert.Same(fk, new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: false));
        }

        [Fact]
        public void Foreign_key_matching_principal_type_name_plus_Id_is_found()
        {
            var fkProperty = DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey());

            Assert.Same(fk, new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: false));
        }

        [Fact]
        public void Foreign_key_matching_principal_type_name_plus_PK_name_is_found()
        {
            var fkProperty = DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey());

            Assert.Same(fk, new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: false));
        }

        [Fact]
        public void Creates_foreign_key_using_given_property()
        {
            DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);
            var fkProperty = DependentType.GetOrAddProperty("No!No!", typeof(int), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(
                PrincipalType, DependentType, "SomeNav", "SomeInverse", new[] { fkProperty }, new Property[0], isUnqiue: false);

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_using_given_principal_property()
        {
            var fkProperty = DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);
            var principalKeyProperty = PrincipalType.GetOrAddProperty("No!No!", typeof(int), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(
                PrincipalType, DependentType, "SomeNav", "SomeInverse", new Property[0], new[] { principalKeyProperty }, isUnqiue: false);

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalKeyProperty, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_using_given_dependent_and_principal_property()
        {
            DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);
            var fkProperty = DependentType.GetOrAddProperty("No!", typeof(int), shadowProperty: true);
            var principalKeyProperty = PrincipalType.GetOrAddProperty("No!No!", typeof(int), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(
                PrincipalType,
                DependentType,
                "SomeNav",
                "SomeInverse",
                new[] { fkProperty },
                new[] { principalKeyProperty },
                isUnqiue: false);

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalKeyProperty, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_using_given_properties()
        {
            DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);
            var fkProperty1 = DependentType.GetOrAddProperty("ThatsNotTrue!", typeof(int), shadowProperty: true);
            var fkProperty2 = DependentType.GetOrAddProperty("ThatsImpossible!", typeof(int?), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(
                PrincipalType,
                DependentType,
                "SomeNav",
                "SomeInverse",
                new[] { fkProperty1, fkProperty2 },
                new Property[0],
                isUnqiue: false);

            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Equal(2, fk.ReferencedProperties.Count);
            Assert.Equal("ThatsNotTrue!Key", fk.ReferencedProperties[0].Name);
            Assert.Same(typeof(int), fk.ReferencedProperties[0].PropertyType);
            Assert.True(fk.ReferencedProperties[0].IsShadowProperty);
            Assert.Equal("ThatsImpossible!Key", fk.ReferencedProperties[1].Name);
            Assert.Same(typeof(int), fk.ReferencedProperties[1].PropertyType);
            Assert.True(fk.ReferencedProperties[1].IsShadowProperty);
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_using_given_dependent_and_principal_properties()
        {
            DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);
            var fkProperty1 = DependentType.GetOrAddProperty("ThatsNotTrue!", typeof(int), shadowProperty: true);
            var fkProperty2 = DependentType.GetOrAddProperty("ThatsImpossible!", typeof(int), shadowProperty: true);
            var principalKeyProperty1 = PrincipalType.GetOrAddProperty("SearchYourFeelings", typeof(int), shadowProperty: true);
            var principalKeyProperty2 = PrincipalType.GetOrAddProperty("YouKnowItToBeTrue!", typeof(int), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(
                PrincipalType,
                DependentType,
                "SomeNav",
                "SomeInverse",
                new[] { fkProperty1, fkProperty2 },
                new[] { principalKeyProperty1, principalKeyProperty2 },
                isUnqiue: false);

            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalKeyProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalKeyProperty2, fk.ReferencedProperties[1]);
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_matching_navigation_plus_Id()
        {
            var fkProperty = DependentType.GetOrAddProperty("SomeNavID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: true);

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_matching_navigation_plus_PK_name()
        {
            var fkProperty = DependentType.GetOrAddProperty("SomeNavPeEKaY", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: false);

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_matching_principal_type_name_plus_Id()
        {
            var fkProperty = DependentType.GetOrAddProperty("PrincipalEntityID", typeof(int), shadowProperty: true);
            DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: true);

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_matching_principal_type_name_plus_PK_name()
        {
            var fkProperty = DependentType.GetOrAddProperty("PrincipalEntityPeEKaY", typeof(int), shadowProperty: true);

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: false);

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_based_on_nav_name_if_no_appropriate_property_is_found()
        {
            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: false);

            Assert.Equal("SomeNavId", fk.Properties.Single().Name);
            Assert.Equal(typeof(int), fk.Properties.Single().PropertyType);
            Assert.True(fk.Properties.Single().IsShadowProperty);
            Assert.False(fk.Properties.Single().IsConcurrencyToken);
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_based_on_type_name_if_no_appropriate_property_is_found()
        {
            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, null, null, isUnqiue: false);

            Assert.Equal("PrincipalEntityId", fk.Properties.Single().Name);
            Assert.Equal(typeof(int), fk.Properties.Single().PropertyType);
            Assert.True(fk.Properties.Single().IsShadowProperty);
            Assert.False(fk.Properties.Single().IsConcurrencyToken);
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Does_not_match_existing_FK_if_FK_already_has_different_navigation_to_principal()
        {
            var fkProperty = DependentType.GetOrAddProperty("SharedFk", typeof(int), shadowProperty: true);
            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey());
            DependentType.AddNavigation("AnotherNav", fk, pointsToPrincipal: true);

            var newFk = new ForeignKeyConvention().FindOrCreateForeignKey(
                PrincipalType, DependentType, "SomeNav", "SomeInverse", new[] { fkProperty }, new Property[0], isUnqiue: false);

            Assert.NotSame(fk, newFk);
            Assert.NotEqual(fkProperty, newFk.Properties.Single());
            Assert.Same(PrimaryKey, newFk.ReferencedProperties.Single());
            Assert.False(newFk.IsUnique);
            Assert.True(newFk.IsRequired);
        }

        [Fact]
        public void Does_not_match_existing_FK_if_FK_already_has_different_navigation_to_dependent()
        {
            var fkProperty = DependentType.GetOrAddProperty("SharedFk", typeof(int), shadowProperty: true);
            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey());
            PrincipalType.AddNavigation("AnotherNav", fk, pointsToPrincipal: false);

            var newFk = new ForeignKeyConvention().FindOrCreateForeignKey(
                PrincipalType, DependentType, "SomeNav", "SomeInverse", new[] { fkProperty }, new Property[0], isUnqiue: false);

            Assert.NotSame(fk, newFk);
            Assert.NotEqual(fkProperty, newFk.Properties.Single());
            Assert.Same(PrimaryKey, newFk.ReferencedProperties.Single());
            Assert.False(newFk.IsUnique);
            Assert.True(newFk.IsRequired);
        }

        [Fact]
        public void Does_not_match_existing_FK_if_FK_already_has_different_uniqueness()
        {
            var fkProperty = DependentType.GetOrAddProperty("SharedFk", typeof(int), shadowProperty: true);
            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey());
            fk.IsUnique = true;

            var newFk = new ForeignKeyConvention().FindOrCreateForeignKey(
                PrincipalType, DependentType, "SomeNav", "SomeInverse", new[] { fkProperty }, new Property[0], isUnqiue: false);

            Assert.NotSame(fk, newFk);
            Assert.NotEqual(fkProperty, newFk.Properties.Single());
            Assert.Same(PrimaryKey, newFk.ReferencedProperties.Single());
            Assert.False(newFk.IsUnique);
            Assert.True(newFk.IsRequired);
        }

        [Fact]
        public void Creates_unique_foreign_key_using_dependent_PK_if_no_matching_FK_property_found()
        {
            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav", "SomeInverse", isUnqiue: true);

            Assert.Same(DependentType.GetPrimaryKey().Properties.Single(), fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_correct_foreign_key_for_entity_with_composite_key()
        {
            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(
                _model.GetEntityType(typeof(PrincipalEntityWithCompositeKey)),
                _model.GetEntityType(typeof(DependentEntityWithCompositeKey)),
                "NavProp",
                "NavProp",
                isUnqiue: false);

            Assert.Equal(2, fk.ReferencedProperties.Count);
            Assert.Equal(typeof(int), fk.ReferencedProperties[0].PropertyType);
            Assert.Equal(typeof(string), fk.ReferencedProperties[1].PropertyType);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var principalType = new EntityType(typeof(PrincipalEntity));
            principalType.GetOrSetPrimaryKey(principalType.GetOrAddProperty("PeeKay", typeof(int)));
            model.AddEntityType(principalType);

            var dependentType = new EntityType(typeof(DependentEntity));
            dependentType.GetOrSetPrimaryKey(dependentType.GetOrAddProperty("KayPee", typeof(int), shadowProperty: true));
            model.AddEntityType(dependentType);

            var principalTypeWithCompositeKey = new EntityType(typeof(PrincipalEntityWithCompositeKey));
            principalTypeWithCompositeKey.GetOrSetPrimaryKey(new[]
                {
                    principalTypeWithCompositeKey.GetOrAddProperty("Id", typeof(int)),
                    principalTypeWithCompositeKey.GetOrAddProperty("Name", typeof(string))
                });
            model.AddEntityType(principalTypeWithCompositeKey);

            var dependentTypeWithCompositeKey = new EntityType(typeof(DependentEntityWithCompositeKey));
            dependentTypeWithCompositeKey.GetOrSetPrimaryKey(dependentTypeWithCompositeKey.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            model.AddEntityType(dependentTypeWithCompositeKey);

            return model;
        }

        private Property PrimaryKey
        {
            get { return PrincipalType.GetPrimaryKey().Properties.Single(); }
        }

        private EntityType DependentType
        {
            get { return _model.GetEntityType(typeof(DependentEntity)); }
        }

        private EntityType PrincipalType
        {
            get { return _model.GetEntityType(typeof(PrincipalEntity)); }
        }

        private class PrincipalEntity
        {
            public int PeeKay { get; set; }
            public IEnumerable<DependentEntity> AnotherNav { get; set; }
        }

        private class DependentEntity
        {
            public PrincipalEntity Navigator { get; set; }
            public PrincipalEntity AnotherNav { get; set; }
        }

        private class PrincipalEntityWithCompositeKey
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<DependentEntityWithCompositeKey> NavProp { get; set; }
        }

        private class DependentEntityWithCompositeKey
        {
            public PrincipalEntityWithCompositeKey NavProp { get; set; }
        }
    }
}
