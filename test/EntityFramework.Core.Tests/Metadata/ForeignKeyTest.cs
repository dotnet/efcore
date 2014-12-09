// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
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
                = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey())
                {
                    IsUnique = true,
                };

            Assert.Same(entityType, foreignKey.ReferencedEntityType);
            Assert.Same(principalProp, foreignKey.ReferencedProperties.Single());
            Assert.Same(dependentProp, foreignKey.Properties.Single());
            Assert.True(foreignKey.IsUnique.Value);
            Assert.Same(entityType.GetPrimaryKey(), foreignKey.ReferencedKey);
        }

        [Fact]
        public void Principal_and_depedent_property_count_must_match()
        {
            var dependentType = new Model().AddEntityType("D");
            var principalType = new Model().AddEntityType("P");

            var dependentProperty1 = dependentType.GetOrAddProperty("P1", typeof(int), shadowProperty: true);
            var dependentProperty2 = dependentType.GetOrAddProperty("P2", typeof(int), shadowProperty: true);

            principalType.GetOrSetPrimaryKey(principalType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));

            Assert.Equal(
                Strings.ForeignKeyCountMismatch("'P1', 'P2'", "D", "'Id'", "P"),
                Assert.Throws<ArgumentException>(
                    () => new ForeignKey(new[] { dependentProperty1, dependentProperty2 }, principalType.GetPrimaryKey())).Message);
        }

        [Fact]
        public void Principal_and_depedent_property_types_must_match()
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

            new ForeignKey(new[] { dependentProperty1, dependentProperty3 }, principalType.GetPrimaryKey());

            Assert.Equal(
                Strings.ForeignKeyTypeMismatch("'P1', 'P2'", "D", "P"),
                Assert.Throws<ArgumentException>(
                    () => new ForeignKey(new[] { dependentProperty1, dependentProperty2 }, principalType.GetPrimaryKey())).Message);
        }

        [Fact]
        public void Can_create_foreign_key_with_non_pk_principal()
        {
            var entityType = new Model().AddEntityType("E");
            var keyProp = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);
            var principalProp = entityType.GetOrAddProperty("U", typeof(int), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(keyProp);
            var referencedKey = new Key(new[] { principalProp });

            var foreignKey
                = new ForeignKey(new[] { dependentProp }, referencedKey)
                {
                    IsUnique = false,
                };

            Assert.Same(entityType, foreignKey.ReferencedEntityType);
            Assert.Same(principalProp, foreignKey.ReferencedProperties.Single());
            Assert.Same(dependentProp, foreignKey.Properties.Single());
            Assert.False(foreignKey.IsUnique.Value);
            Assert.Same(referencedKey, foreignKey.ReferencedKey);
        }

        [Fact]
        public void IsRequired_true_when_dependent_property_not_nullable()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);
            dependentProp.IsNullable = false;

            var foreignKey = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey());

            Assert.NotNull(foreignKey.IsRequired);
            Assert.True(foreignKey.IsRequired.Value);
        }

        [Fact]
        public void IsRequired_true_when_dependent_property_nullable()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int?), shadowProperty: true);
            dependentProp.IsNullable = true;

            var foreignKey = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey());

            Assert.NotNull(foreignKey.IsRequired);
            Assert.False(foreignKey.IsRequired.Value);
        }

        [Fact]
        public void IsRequired_and_IsUnique_null_when_dependent_property_not_nullable_by_default()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);

            var foreignKey = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey());

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

            var foreignKey = new ForeignKey(new[] { dependentProp }, entityType.GetPrimaryKey());

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

            var foreignKey = new ForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.GetPrimaryKey());

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

            var foreignKey = new ForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.GetPrimaryKey());

            Assert.False(foreignKey.IsRequired.Value);
            Assert.False(((IForeignKey)foreignKey).IsRequired);

            dependentProp2.IsNullable = false;

            Assert.True(foreignKey.IsRequired.Value);
        }

        [Fact]
        public void Setting_IsRequired_will_set_all_FK_properties_as_non_nullable()
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

            var foreignKey = new ForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.GetPrimaryKey()) { IsRequired = true };

            Assert.True(foreignKey.IsRequired.Value);
            Assert.False(dependentProp1.IsNullable.Value);
            Assert.False(dependentProp2.IsNullable.Value);
        }

        [Fact]
        public void Clearing_IsRequired_will_set_all_FK_properties_as_nullable()
        {
            var entityType = new Model().AddEntityType("E");
            entityType.GetOrSetPrimaryKey(new[]
                {
                    entityType.GetOrAddProperty("Id1", typeof(int), shadowProperty: true),
                    entityType.GetOrAddProperty("Id2", typeof(string), shadowProperty: true)
                });

            var dependentProp1 = entityType.GetOrAddProperty("P1", typeof(int?), shadowProperty: true);
            var dependentProp2 = entityType.GetOrAddProperty("P2", typeof(string), shadowProperty: true);

            var foreignKey = new ForeignKey(new[] { dependentProp1, dependentProp2 }, entityType.GetPrimaryKey()) { IsRequired = false };

            Assert.False(foreignKey.IsRequired.Value);
            Assert.True(dependentProp1.IsNullable.Value);
            Assert.True(dependentProp2.IsNullable.Value);
        }

        [Fact]
        public void IsCompatible_returns_true_for_one_to_many_if_all_critaria_match()
        {
            var fk = CreateOneToManyFK();

            Assert.True(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                null,
                null,
                fk.Properties,
                fk.ReferencedProperties,
                false));
        }

        [Fact]
        public void IsCompatible_returns_true_for_one_to_many_if_no_navigations_exist()
        {
            var fk = CreateOneToManyFK();

            Assert.True(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                "Nav",
                "Nav",
                fk.Properties,
                fk.ReferencedProperties,
                false));
        }

        [Fact]
        public void IsCompatible_returns_false_for_one_to_many_if_any_critaria_does_not_match()
        {
            var fk = CreateOneToManyFK();

            Assert.False(fk.IsCompatible(
                fk.EntityType,
                fk.EntityType,
                null,
                null,
                fk.Properties,
                fk.ReferencedProperties,
                false));

            Assert.False(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.ReferencedEntityType,
                null,
                null,
                fk.Properties,
                fk.ReferencedProperties,
                false));

            Assert.False(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                null,
                null,
                fk.ReferencedProperties,
                fk.ReferencedProperties,
                false));

            Assert.False(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                null,
                null,
                fk.Properties,
                fk.Properties,
                false));

            Assert.False(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                null,
                null,
                fk.Properties,
                fk.ReferencedProperties,
                true));
        }

        private ForeignKey CreateOneToManyFK()
        {
            var principalEntityType = new Model().AddEntityType(typeof(OneToManyPrincipal));
            var pk = principalEntityType.GetOrSetPrimaryKey(principalEntityType.GetOrAddProperty("Id", typeof(int)));

            var dependentEntityType = new Model().AddEntityType(typeof(OneToManyDependent));
            var fkProp = dependentEntityType.GetOrAddProperty("Id", typeof(int));
            return dependentEntityType.AddForeignKey(new[] { fkProp }, pk);
        }

        public class OneToManyPrincipal
        {
            public int Id { get; set; }
        }

        public class OneToManyDependent
        {
            public int Id { get; set; }
        }

        [Fact]
        public void IsCompatible_returns_true_for_self_ref_one_to_one_if_all_critaria_match()
        {
            var fk = CreateSelfRefFK();

            Assert.True(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                "SelfRefPrincipal",
                "SelfRefDependent",
                fk.Properties,
                fk.ReferencedProperties,
                true));
        }

        [Fact]
        public void IsCompatible_returns_false_for_self_ref_one_to_one_if_any_critaria_does_not_match()
        {
            var fk = CreateSelfRefFK();

            Assert.False(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                "SelfRefDependent",
                "SelfRefPrincipal",
                fk.Properties,
                fk.ReferencedProperties,
                true));

            Assert.False(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                null,
                null,
                fk.Properties,
                fk.ReferencedProperties,
                true));

            Assert.False(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                "SelfRefPrincipal",
                "SelfRefDependent",
                fk.ReferencedProperties,
                fk.Properties,
                true));

            Assert.False(fk.IsCompatible(
                fk.ReferencedEntityType,
                fk.EntityType,
                "SelfRefPrincipal",
                "SelfRefDependent",
                fk.Properties,
                fk.ReferencedProperties,
                false));
        }

        private ForeignKey CreateSelfRefFK()
        {
            var entityType = new Model().AddEntityType(typeof(SelfRef));
            var pk = entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int)));
            var fkProp = entityType.GetOrAddProperty("SelfRefId", typeof(int?));

            var fk = entityType.AddForeignKey(new[] { fkProp }, pk);
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
    }
}
