// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class ModelTest
    {
        [Fact]
        public void Use_of_custom_IModel_throws()
        {
            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_IModel_throws), nameof(IModel), "IModelProxy"),
                Assert.Throws<NotSupportedException>(() => Mock.Of<IModel>().AsModel()).Message);
        }

        [Fact]
        public void Snapshot_change_tracking_is_used_by_default()
        {
            Assert.Equal(ChangeTrackingStrategy.Snapshot, new Model().ChangeTrackingStrategy);
            Assert.Equal(ChangeTrackingStrategy.Snapshot, new Model().GetChangeTrackingStrategy());
        }

        [Fact]
        public void Change_tracking_strategy_can_be_changed()
        {
            var model = new Model { ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications };
            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, model.ChangeTrackingStrategy);

            model.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;
            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, model.GetChangeTrackingStrategy());
        }

        [Fact]
        public void Can_add_and_remove_entity_by_type()
        {
            var model = new Model();
            Assert.Null(model.FindEntityType(typeof(Customer)));
            Assert.Null(model.RemoveEntityType(typeof(Customer)));

            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(typeof(Customer), entityType.ClrType);
            Assert.NotNull(model.FindEntityType(typeof(Customer)));
            Assert.Same(model, entityType.Model);
            Assert.NotNull(entityType.Builder);

            Assert.Same(entityType, model.GetOrAddEntityType(typeof(Customer)));

            Assert.Equal(new[] { entityType }, model.GetEntityTypes().ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType.ClrType));

            Assert.Null(model.RemoveEntityType(entityType.ClrType));
            Assert.Null(model.FindEntityType(typeof(Customer)));
            Assert.Null(entityType.Builder);
        }

        [Fact]
        public void Can_add_and_remove_entity_by_name()
        {
            var model = new Model();
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

            var entityType = model.AddEntityType(typeof(Customer).FullName);

            Assert.Null(entityType.ClrType);
            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(model, entityType.Model);
            Assert.NotNull(entityType.Builder);

            Assert.Same(entityType, model.GetOrAddEntityType(typeof(Customer).FullName));

            Assert.Equal(new[] { entityType }, model.GetEntityTypes().ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType.Name));

            Assert.Null(model.RemoveEntityType(entityType.Name));
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(entityType.Builder);
        }

        [Fact]
        public void Cannot_remove_entity_type_when_referenced_by_foreign_key()
        {
            var model = new Model();
            var customerType = model.GetOrAddEntityType(typeof(Customer));
            var idProperty = customerType.GetOrAddProperty(Customer.IdProperty);
            var customerKey = customerType.GetOrAddKey(idProperty);
            var orderType = model.GetOrAddEntityType(typeof(Order));
            var customerFk = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            orderType.AddForeignKey(customerFk, customerKey, customerType);

            Assert.Equal(
                CoreStrings.EntityTypeInUseByForeignKey(
                    typeof(Customer).Name,
                    "{'" + Order.CustomerIdProperty.Name + "'}",
                    typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType.Name)).Message);
        }

        [Fact]
        public void Cannot_remove_entity_type_when_it_has_derived_types()
        {
            var model = new Model();
            var customerType = model.GetOrAddEntityType(typeof(Customer));
            var specialCustomerType = model.GetOrAddEntityType(typeof(SpecialCustomer));

            specialCustomerType.HasBaseType(customerType);

            Assert.Equal(
                CoreStrings.EntityTypeInUseByDerived(typeof(Customer).Name, typeof(SpecialCustomer).Name),
                Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType.Name)).Message);
        }

        [Fact]
        public void Adding_duplicate_entity_by_type_throws()
        {
            var model = new Model();
            Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

            model.AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.DuplicateEntityType(nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer))).Message);
        }

        [Fact]
        public void Adding_duplicate_entity_by_name_throws()
        {
            var model = new Model();
            Assert.Null(model.RemoveEntityType(typeof(Customer)));

            model.AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.DuplicateEntityType(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer).FullName)).Message);
        }

        [Fact]
        public void Can_get_entity_by_type()
        {
            var model = new Model();
            var entityType = model.GetOrAddEntityType(typeof(Customer));

            Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
            Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
            Assert.Null(model.FindEntityType(typeof(string)));
        }

        [Fact]
        public void Can_get_entity_by_name()
        {
            var model = new Model();
            var entityType = model.GetOrAddEntityType(typeof(Customer).FullName);

            Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(model.FindEntityType(typeof(string)));
        }

        [Fact]
        public void Entities_are_ordered_by_name()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Order));
            var entityType2 = model.AddEntityType(typeof(Customer));

            Assert.True(new[] { entityType2, entityType1 }.SequenceEqual(model.GetEntityTypes()));
        }

        [Fact]
        public void Can_get_referencing_foreign_keys()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var keyProperty = entityType1.AddProperty("Id", typeof(int));
            var fkProperty = entityType2.AddProperty("CustomerId", typeof(int));
            var foreignKey = entityType2.GetOrAddForeignKey(fkProperty, entityType1.AddKey(keyProperty), entityType1);

            var referencingForeignKeys = entityType1.GetReferencingForeignKeys();

            Assert.Same(foreignKey, referencingForeignKeys.Single());
            Assert.Same(foreignKey, entityType1.GetReferencingForeignKeys().Single());
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");

            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class SpecialCustomer : Customer
        {
        }

        private class Order
        {
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");

            public int Id { get; set; }
            public int CustomerId { get; set; }
        }

        [Fact]
        public virtual void Create_model_with_complex_and_entity_types()
        {
            var model = new Model();

            var entity1 = model.AddEntityType(typeof(Entity1));
            var complex1 = model.AddComplexType(typeof(Complex1));

            var shadowEntity1 = model.AddEntityType("ShadowEntity1");
            var shadowComplex1 = model.AddComplexType("ShadowComplex1");

            Assert.Same(complex1, model.FindComplexType(typeof(Complex1)));
            Assert.Same(complex1, model.FindComplexType(typeof(Complex1).FullName));
            Assert.Same(shadowComplex1, model.FindComplexType("ShadowComplex1"));

            Assert.Same(entity1, model.FindEntityType(typeof(Entity1)));
            Assert.Same(entity1, model.FindEntityType(typeof(Entity1).FullName));
            Assert.Same(shadowEntity1, model.FindEntityType("ShadowEntity1"));

            Assert.Same(complex1, model.FindStructuralType(typeof(Complex1)));
            Assert.Same(complex1, model.FindStructuralType(typeof(Complex1).FullName));
            Assert.Same(shadowComplex1, model.FindStructuralType("ShadowComplex1"));
            Assert.Same(entity1, model.FindStructuralType(typeof(Entity1)));
            Assert.Same(entity1, model.FindStructuralType(typeof(Entity1).FullName));
            Assert.Same(shadowEntity1, model.FindStructuralType("ShadowEntity1"));

            Assert.Null(model.FindEntityType(typeof(Complex1)));
            Assert.Null(model.FindEntityType(typeof(Complex1).FullName));
            Assert.Null(model.FindEntityType("ShadowComplex1"));

            Assert.Null(model.FindComplexType(typeof(Entity1)));
            Assert.Null(model.FindComplexType(typeof(Entity1).FullName));
            Assert.Null(model.FindComplexType("ShadowEntity1"));

            Assert.Equal(
                new[] { complex1, shadowComplex1 },
                model.GetComplexTypes().ToArray());

            Assert.Equal(
                new[] { entity1, shadowEntity1 },
                model.GetEntityTypes().ToArray());

            Assert.Equal(
                new StructuralType[] { complex1, entity1, shadowComplex1, shadowEntity1 },
                model.GetStructuralTypes().ToArray());
        }

        [Fact]
        public virtual void Get_or_add_complex_types()
        {
            var model = new Model();

            var complex1 = model.AddComplexType(typeof(Complex1));
            var shadowComplex1 = model.AddComplexType("ShadowComplex1");

            Assert.Same(complex1, model.GetOrAddComplexType(typeof(Complex1)));
            Assert.Same(complex1, model.GetOrAddComplexType(typeof(Complex1).FullName));
            Assert.Same(shadowComplex1, model.FindComplexType("ShadowComplex1"));

            var complex2 = model.GetOrAddComplexType(typeof(Complex2));
            var shadowComplex2 = model.GetOrAddComplexType("ShadowComplex2");

            Assert.Same(complex2, model.FindComplexType(typeof(Complex2)));
            Assert.Same(complex2, model.FindComplexType(typeof(Complex2).FullName));
            Assert.Same(shadowComplex2, model.FindComplexType("ShadowComplex2"));

            Assert.Equal(
                new[] { complex1, complex2, shadowComplex1, shadowComplex2 },
                model.GetComplexTypes().ToArray());
        }

        [Fact]
        public virtual void Throws_adding_complex_type_if_already_added()
        {
            var model = new Model();

            model.AddComplexType(typeof(Complex1));
            model.AddComplexType(typeof(Complex2).FullName);

            Assert.Equal(
                CoreStrings.DuplicateComplexType(nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexType(typeof(Complex1))).Message);

            Assert.Equal(
                CoreStrings.DuplicateComplexType(typeof(Complex1).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexType(typeof(Complex1).FullName)).Message);

            Assert.Equal(
                CoreStrings.DuplicateComplexType(nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexType(typeof(Complex2))).Message);

            Assert.Equal(
                CoreStrings.DuplicateComplexType(typeof(Complex2).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexType(typeof(Complex2).FullName)).Message);
        }

        [Fact]
        public virtual void Throws_adding_complex_type_if_already_added_as_entity_type()
        {
            var model = new Model();

            model.AddEntityType(typeof(Complex1));
            model.AddEntityType(typeof(Complex2).FullName);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexType(typeof(Complex1))).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(typeof(Complex1).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexType(typeof(Complex1).FullName)).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexType(typeof(Complex2))).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(typeof(Complex2).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexType(typeof(Complex2).FullName)).Message);
        }

        [Fact]
        public virtual void Throws_adding_entity_type_if_already_added_as_complex_type()
        {
            var model = new Model();

            model.AddComplexType(typeof(Complex1));
            model.AddComplexType(typeof(Complex2).FullName);

            Assert.Equal(
                CoreStrings.ComplexTypeAlreadyExists(nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Complex1))).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeAlreadyExists(typeof(Complex1).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Complex1).FullName)).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeAlreadyExists(nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Complex2))).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeAlreadyExists(typeof(Complex2).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Complex2).FullName)).Message);
        }

        [Fact]
        public virtual void Throws_with_GetOrAdd_complex_type_if_already_added_as_entity_type()
        {
            var model = new Model();

            model.AddEntityType(typeof(Complex1));
            model.AddEntityType(typeof(Complex2).FullName);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddComplexType(typeof(Complex1))).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(typeof(Complex1).FullName),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddComplexType(typeof(Complex1).FullName)).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddComplexType(typeof(Complex2))).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(typeof(Complex2).FullName),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddComplexType(typeof(Complex2).FullName)).Message);
        }

        [Fact]
        public virtual void Throws_with_GetOrAdd_entity_type_if_already_added_as_complex_type()
        {
            var model = new Model();

            model.AddComplexType(typeof(Complex1));
            model.AddComplexType(typeof(Complex2).FullName);

            Assert.Equal(
                CoreStrings.ComplexTypeAlreadyExists(nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddEntityType(typeof(Complex1))).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeAlreadyExists(typeof(Complex1).FullName),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddEntityType(typeof(Complex1).FullName)).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeAlreadyExists(nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddEntityType(typeof(Complex2))).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeAlreadyExists(typeof(Complex2).FullName),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddEntityType(typeof(Complex2).FullName)).Message);
        }

        [Fact]
        public virtual void Remove_complex_types()
        {
            var model = new Model();

            var complex1 = model.AddComplexType(typeof(Complex1));
            var complex2 = model.AddComplexType(typeof(Complex2));
            var shadowComplex1 = model.AddComplexType("ShadowComplex1");

            Assert.Same(complex1, model.RemoveComplexType(typeof(Complex1)));
            Assert.Same(complex2, model.RemoveComplexType(typeof(Complex2).FullName));
            Assert.Same(shadowComplex1, model.RemoveComplexType("ShadowComplex1"));

            Assert.Empty(model.GetComplexTypes());

            Assert.Null(model.RemoveComplexType(typeof(Complex1)));
            Assert.Null(model.RemoveComplexType(typeof(Complex2).FullName));
            Assert.Null(model.RemoveComplexType("ShadowComplex1"));
        }

        [Fact]
        public virtual void Remove_complex_types_as_structural_types()
        {
            var model = new Model();

            var complex1 = model.AddComplexType(typeof(Complex1));
            var complex2 = model.AddComplexType(typeof(Complex2));
            var shadowComplex1 = model.AddComplexType("ShadowComplex1");

            Assert.Same(complex1, model.RemoveStructuralType(typeof(Complex1)));
            Assert.Same(complex2, model.RemoveStructuralType(typeof(Complex2).FullName));
            Assert.Same(shadowComplex1, model.RemoveStructuralType("ShadowComplex1"));

            Assert.Empty(model.GetComplexTypes());

            Assert.Null(model.RemoveStructuralType(typeof(Complex1)));
            Assert.Null(model.RemoveStructuralType(typeof(Complex2).FullName));
            Assert.Null(model.RemoveStructuralType("ShadowComplex1"));
        }

        [Fact]
        public virtual void Remove_entity_types_as_structural_types()
        {
            var model = new Model();

            var entity1 = model.AddComplexType(typeof(Entity1));
            var entity2 = model.AddComplexType(typeof(Entity2));
            var shadowEntity1 = model.AddComplexType("ShadowEntity1");

            Assert.Same(entity1, model.RemoveStructuralType(typeof(Entity1)));
            Assert.Same(entity2, model.RemoveStructuralType(typeof(Entity2).FullName));
            Assert.Same(shadowEntity1, model.RemoveStructuralType("ShadowEntity1"));

            Assert.Empty(model.GetEntityTypes());

            Assert.Null(model.RemoveStructuralType(typeof(Entity1)));
            Assert.Null(model.RemoveStructuralType(typeof(Entity2).FullName));
            Assert.Null(model.RemoveStructuralType("ShadowEntity1"));
        }

        private class Complex1
        {
            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        private class Complex2
        {
            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        private class Entity1
        {
            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        private class Entity2
        {
            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
        }
    }
}
