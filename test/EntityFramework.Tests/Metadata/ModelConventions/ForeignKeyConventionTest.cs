// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class ForeignKeyConventionTest
    {
        private readonly Model _model = BuildModel();

        [Fact]
        public void Foreign_key_matching_navigation_plus_Id_is_found()
        {
            var fkProperty = DependentType.AddProperty("SomeNavID", typeof(int));
            DependentType.AddProperty("SomeNavPeEKaY", typeof(int));
            DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = DependentType.AddForeignKey(PrincipalType.GetKey(), fkProperty);

            Assert.Same(fk, new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav"));
        }

        [Fact]
        public void Foreign_key_matching_navigation_plus_PK_name_is_found()
        {
            var fkProperty = DependentType.AddProperty("SomeNavPeEKaY", typeof(int));
            DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = DependentType.AddForeignKey(PrincipalType.GetKey(), fkProperty);

            Assert.Same(fk, new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav"));
        }

        [Fact]
        public void Foreign_key_matching_principal_type_name_plus_Id_is_found()
        {
            var fkProperty = DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = DependentType.AddForeignKey(PrincipalType.GetKey(), fkProperty);

            Assert.Same(fk, new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav"));
        }

        [Fact]
        public void Foreign_key_matching_principal_type_name_plus_PK_name_is_found()
        {
            var fkProperty = DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = DependentType.AddForeignKey(PrincipalType.GetKey(), fkProperty);

            Assert.Same(fk, new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav"));
        }

        [Fact]
        public void Creates_foreign_key_matching_navigation_plus_Id()
        {
            var fkProperty = DependentType.AddProperty("SomeNavID", typeof(int));
            DependentType.AddProperty("SomeNavPeEKaY", typeof(int));
            DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav");

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_matching_navigation_plus_PK_name()
        {
            var fkProperty = DependentType.AddProperty("SomeNavPeEKaY", typeof(int));
            DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav");

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_matching_principal_type_name_plus_Id()
        {
            var fkProperty = DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav");

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_matching_principal_type_name_plus_PK_name()
        {
            var fkProperty = DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav");

            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Creates_foreign_key_based_on_nav_name_if_no_appropriate_property_is_found()
        {
            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, "SomeNav");

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
            var fk = new ForeignKeyConvention().FindOrCreateForeignKey(PrincipalType, DependentType, null);

            Assert.Equal("PrincipalEntityId", fk.Properties.Single().Name);
            Assert.Equal(typeof(int), fk.Properties.Single().PropertyType);
            Assert.True(fk.Properties.Single().IsShadowProperty);
            Assert.False(fk.Properties.Single().IsConcurrencyToken);
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var principalType = new EntityType(typeof(PrincipalEntity));
            principalType.SetKey(principalType.AddProperty("PeeKay", typeof(int)));

            model.AddEntityType(principalType);
            model.AddEntityType(new EntityType(typeof(DependentEntity)));

            return model;
        }

        private Property PrimaryKey
        {
            get { return PrincipalType.GetKey().Properties.Single(); }
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
        }

        private class DependentEntity
        {
            public PrincipalEntity Navigator { get; set; }
        }
    }
}
