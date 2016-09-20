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
            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var shadowEntity1 = model.AddEntityType("ShadowEntity1");
            var shadowComplex1 = model.AddComplexTypeDefinition("ShadowComplex1");

            AssertComplexTypes<Complex1>(model, entity1, complex1, shadowEntity1, shadowComplex1);
            AssertComplexTypes<Complex1>((IMutableModel)model, entity1, complex1, shadowEntity1, shadowComplex1);
            AssertComplexTypes<Complex1>((IModel)model, entity1, complex1, shadowEntity1, shadowComplex1);
        }

        [Fact]
        public virtual void Create_model_with_complex_and_entity_types_using_interfaces()
        {
            IMutableModel model = new Model();

            var entity1 = model.AddEntityType(typeof(Entity1));
            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var shadowEntity1 = model.AddEntityType("ShadowEntity1");
            var shadowComplex1 = model.AddComplexTypeDefinition("ShadowComplex1");

            AssertComplexTypes<Complex1>((Model)model, entity1, complex1, shadowEntity1, shadowComplex1);
            AssertComplexTypes<Complex1>(model, entity1, complex1, shadowEntity1, shadowComplex1);
            AssertComplexTypes<Complex1>((IModel)model, entity1, complex1, shadowEntity1, shadowComplex1);
        }

        [Fact]
        public virtual void Create_model_with_struct_complex_types()
        {
            var model = new Model();

            var entity1 = model.AddEntityType(typeof(Entity1));
            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1Struct));

            var shadowEntity1 = model.AddEntityType("ShadowEntity1");
            var shadowComplex1 = model.AddComplexTypeDefinition("ShadowComplex1");

            AssertComplexTypes<Complex1Struct>(model, entity1, complex1, shadowEntity1, shadowComplex1);
            AssertComplexTypes<Complex1Struct>((IMutableModel)model, entity1, complex1, shadowEntity1, shadowComplex1);
            AssertComplexTypes<Complex1Struct>((IModel)model, entity1, complex1, shadowEntity1, shadowComplex1);
        }

        [Fact]
        public virtual void Create_model_with_struct_complex_types_using_interfaces()
        {
            IMutableModel model = new Model();

            var entity1 = model.AddEntityType(typeof(Entity1));
            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1Struct));

            var shadowEntity1 = model.AddEntityType("ShadowEntity1");
            var shadowComplex1 = model.AddComplexTypeDefinition("ShadowComplex1");

            AssertComplexTypes<Complex1Struct>((Model)model, entity1, complex1, shadowEntity1, shadowComplex1);
            AssertComplexTypes<Complex1Struct>(model, entity1, complex1, shadowEntity1, shadowComplex1);
            AssertComplexTypes<Complex1Struct>((IModel)model, entity1, complex1, shadowEntity1, shadowComplex1);
        }

        private static void AssertComplexTypes<TComplexType>(
            Model model, 
            EntityType entity1, 
            ComplexTypeDefinition complex1, 
            EntityType shadowEntity1, 
            ComplexTypeDefinition shadowComplex1)
        {
            Assert.Same(complex1, model.FindComplexTypeDefinition(typeof(TComplexType)));
            Assert.Same(complex1, model.FindComplexTypeDefinition(typeof(TComplexType).FullName));
            Assert.Same(shadowComplex1, model.FindComplexTypeDefinition("ShadowComplex1"));

            Assert.Same(entity1, model.FindEntityType(typeof(Entity1)));
            Assert.Same(entity1, model.FindEntityType(typeof(Entity1).FullName));
            Assert.Same(shadowEntity1, model.FindEntityType("ShadowEntity1"));

            Assert.Same(complex1, model.FindMappedType(typeof(TComplexType)));
            Assert.Same(complex1, model.FindMappedType(typeof(TComplexType).FullName));
            Assert.Same(shadowComplex1, model.FindMappedType("ShadowComplex1"));
            Assert.Same(entity1, model.FindMappedType(typeof(Entity1)));
            Assert.Same(entity1, model.FindMappedType(typeof(Entity1).FullName));
            Assert.Same(shadowEntity1, model.FindMappedType("ShadowEntity1"));

            Assert.Null(model.FindEntityType(typeof(TComplexType)));
            Assert.Null(model.FindEntityType(typeof(TComplexType).FullName));
            Assert.Null(model.FindEntityType("ShadowComplex1"));

            Assert.Null(model.FindComplexTypeDefinition(typeof(Entity1)));
            Assert.Null(model.FindComplexTypeDefinition(typeof(Entity1).FullName));
            Assert.Null(model.FindComplexTypeDefinition("ShadowEntity1"));

            Assert.Equal(
                new[] { complex1, shadowComplex1 },
                model.GetComplexTypeDefinitions().ToArray());

            Assert.Equal(
                new[] { entity1, shadowEntity1 },
                model.GetEntityTypes().ToArray());

            Assert.Equal(
                new TypeBase[] { complex1, entity1, shadowComplex1, shadowEntity1 },
                model.GetMappedTypes().ToArray());
        }

        private static void AssertComplexTypes<TComplexType>(
            IMutableModel model,
            IMutableEntityType entity1,
            IMutableComplexTypeDefinition complex1,
            IMutableEntityType shadowEntity1,
            IMutableComplexTypeDefinition shadowComplex1)
        {
            Assert.Same(complex1, model.FindComplexTypeDefinition(typeof(TComplexType)));
            Assert.Same(complex1, model.FindComplexTypeDefinition(typeof(TComplexType).FullName));
            Assert.Same(shadowComplex1, model.FindComplexTypeDefinition("ShadowComplex1"));

            Assert.Same(entity1, model.FindEntityType(typeof(Entity1)));
            Assert.Same(entity1, model.FindEntityType(typeof(Entity1).FullName));
            Assert.Same(shadowEntity1, model.FindEntityType("ShadowEntity1"));

            Assert.Same(complex1, model.FindMappedType(typeof(TComplexType)));
            Assert.Same(complex1, model.FindMappedType(typeof(TComplexType).FullName));
            Assert.Same(shadowComplex1, model.FindMappedType("ShadowComplex1"));
            Assert.Same(entity1, model.FindMappedType(typeof(Entity1)));
            Assert.Same(entity1, model.FindMappedType(typeof(Entity1).FullName));
            Assert.Same(shadowEntity1, model.FindMappedType("ShadowEntity1"));

            Assert.Null(model.FindEntityType(typeof(TComplexType)));
            Assert.Null(model.FindEntityType(typeof(TComplexType).FullName));
            Assert.Null(model.FindEntityType("ShadowComplex1"));

            Assert.Null(model.FindComplexTypeDefinition(typeof(Entity1)));
            Assert.Null(model.FindComplexTypeDefinition(typeof(Entity1).FullName));
            Assert.Null(model.FindComplexTypeDefinition("ShadowEntity1"));

            Assert.Equal(
                new[] { complex1, shadowComplex1 },
                model.GetComplexTypeDefinitions().ToArray());

            Assert.Equal(
                new[] { entity1, shadowEntity1 },
                model.GetEntityTypes().ToArray());

            Assert.Equal(
                new IMutableTypeBase[] { complex1, entity1, shadowComplex1, shadowEntity1 },
                model.GetMappedTypes().ToArray());
        }

        private static void AssertComplexTypes<TComplexType>(
            IModel model,
            IEntityType entity1,
            IComplexTypeDefinition complex1,
            IEntityType shadowEntity1,
            IComplexTypeDefinition shadowComplex1)
        {
            Assert.Same(complex1, model.FindComplexTypeDefinition(typeof(TComplexType)));
            Assert.Same(complex1, model.FindComplexTypeDefinition(typeof(TComplexType).FullName));
            Assert.Same(shadowComplex1, model.FindComplexTypeDefinition("ShadowComplex1"));

            Assert.Same(entity1, model.FindEntityType(typeof(Entity1)));
            Assert.Same(entity1, model.FindEntityType(typeof(Entity1).FullName));
            Assert.Same(shadowEntity1, model.FindEntityType("ShadowEntity1"));

            Assert.Same(complex1, model.FindMappedType(typeof(TComplexType)));
            Assert.Same(complex1, model.FindMappedType(typeof(TComplexType).FullName));
            Assert.Same(shadowComplex1, model.FindMappedType("ShadowComplex1"));
            Assert.Same(entity1, model.FindMappedType(typeof(Entity1)));
            Assert.Same(entity1, model.FindMappedType(typeof(Entity1).FullName));
            Assert.Same(shadowEntity1, model.FindMappedType("ShadowEntity1"));

            Assert.Null(model.FindEntityType(typeof(TComplexType)));
            Assert.Null(model.FindEntityType(typeof(TComplexType).FullName));
            Assert.Null(model.FindEntityType("ShadowComplex1"));

            Assert.Null(model.FindComplexTypeDefinition(typeof(Entity1)));
            Assert.Null(model.FindComplexTypeDefinition(typeof(Entity1).FullName));
            Assert.Null(model.FindComplexTypeDefinition("ShadowEntity1"));

            Assert.Equal(
                new[] { complex1, shadowComplex1 },
                model.GetComplexTypeDefinitions().ToArray());

            Assert.Equal(
                new[] { entity1, shadowEntity1 },
                model.GetEntityTypes().ToArray());

            Assert.Equal(
                new ITypeBase[] { complex1, entity1, shadowComplex1, shadowEntity1 },
                model.GetMappedTypes().ToArray());
        }

        [Fact]
        public virtual void Get_or_add_complex_types()
        {
            IMutableModel model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var shadowComplex1 = model.AddComplexTypeDefinition("ShadowComplex1");

            Assert.Same(complex1, model.GetOrAddComplexTypeDefinition(typeof(Complex1)));
            Assert.Same(complex1, model.GetOrAddComplexTypeDefinition(typeof(Complex1).FullName));
            Assert.Same(shadowComplex1, model.FindComplexTypeDefinition("ShadowComplex1"));

            var complex2 = model.GetOrAddComplexTypeDefinition(typeof(Complex2));
            var shadowComplex2 = model.GetOrAddComplexTypeDefinition("ShadowComplex2");

            Assert.Same(complex2, model.FindComplexTypeDefinition(typeof(Complex2)));
            Assert.Same(complex2, model.FindComplexTypeDefinition(typeof(Complex2).FullName));
            Assert.Same(shadowComplex2, model.FindComplexTypeDefinition("ShadowComplex2"));

            Assert.Equal(
                new[] { complex1, complex2, shadowComplex1, shadowComplex2 },
                model.GetComplexTypeDefinitions().ToArray());
        }

        [Fact]
        public virtual void Throws_adding_complex_type_if_already_added()
        {
            IMutableModel model = new Model();

            model.AddComplexTypeDefinition(typeof(Complex1));
            model.AddComplexTypeDefinition(typeof(Complex2).FullName);

            Assert.Equal(
                CoreStrings.DuplicateComplexType(nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexTypeDefinition(typeof(Complex1))).Message);

            Assert.Equal(
                CoreStrings.DuplicateComplexType(typeof(Complex1).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexTypeDefinition(typeof(Complex1).FullName)).Message);

            Assert.Equal(
                CoreStrings.DuplicateComplexType(nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexTypeDefinition(typeof(Complex2))).Message);

            Assert.Equal(
                CoreStrings.DuplicateComplexType(typeof(Complex2).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexTypeDefinition(typeof(Complex2).FullName)).Message);
        }

        [Fact]
        public virtual void Throws_adding_complex_type_if_already_added_as_entity_type()
        {
            IMutableModel model = new Model();

            model.AddEntityType(typeof(Complex1));
            model.AddEntityType(typeof(Complex2).FullName);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexTypeDefinition(typeof(Complex1))).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(typeof(Complex1).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexTypeDefinition(typeof(Complex1).FullName)).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexTypeDefinition(typeof(Complex2))).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(typeof(Complex2).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddComplexTypeDefinition(typeof(Complex2).FullName)).Message);
        }

        [Fact]
        public virtual void Throws_adding_entity_type_if_already_added_as_complex_type()
        {
            IMutableModel model = new Model();

            model.AddComplexTypeDefinition(typeof(Complex1));
            model.AddComplexTypeDefinition(typeof(Complex2).FullName);

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
            IMutableModel model = new Model();

            model.AddEntityType(typeof(Complex1));
            model.AddEntityType(typeof(Complex2).FullName);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddComplexTypeDefinition(typeof(Complex1))).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(typeof(Complex1).FullName),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddComplexTypeDefinition(typeof(Complex1).FullName)).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddComplexTypeDefinition(typeof(Complex2))).Message);

            Assert.Equal(
                CoreStrings.EntityTypeAlreadyExists(typeof(Complex2).FullName),
                Assert.Throws<InvalidOperationException>(() => model.GetOrAddComplexTypeDefinition(typeof(Complex2).FullName)).Message);
        }

        [Fact]
        public virtual void Throws_with_GetOrAdd_entity_type_if_already_added_as_complex_type()
        {
            IMutableModel model = new Model();

            model.AddComplexTypeDefinition(typeof(Complex1));
            model.AddComplexTypeDefinition(typeof(Complex2).FullName);

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
            IMutableModel model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex2 = model.AddComplexTypeDefinition(typeof(Complex2));
            var shadowComplex1 = model.AddComplexTypeDefinition("ShadowComplex1");

            Assert.Same(complex1, model.RemoveComplexTypeDefinition(typeof(Complex1)));
            Assert.Same(complex2, model.RemoveComplexTypeDefinition(typeof(Complex2).FullName));
            Assert.Same(shadowComplex1, model.RemoveComplexTypeDefinition("ShadowComplex1"));

            Assert.Empty(model.GetComplexTypeDefinitions());

            Assert.Null(model.RemoveComplexTypeDefinition(typeof(Complex1)));
            Assert.Null(model.RemoveComplexTypeDefinition(typeof(Complex2).FullName));
            Assert.Null(model.RemoveComplexTypeDefinition("ShadowComplex1"));
        }

        [Fact]
        public virtual void Remove_complex_types_as_structural_types()
        {
            IMutableModel model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex2 = model.AddComplexTypeDefinition(typeof(Complex2));
            var shadowComplex1 = model.AddComplexTypeDefinition("ShadowComplex1");

            Assert.Same(complex1, model.RemoveMappedType(typeof(Complex1)));
            Assert.Same(complex2, model.RemoveMappedType(typeof(Complex2).FullName));
            Assert.Same(shadowComplex1, model.RemoveMappedType("ShadowComplex1"));

            Assert.Empty(model.GetComplexTypeDefinitions());

            Assert.Null(model.RemoveMappedType(typeof(Complex1)));
            Assert.Null(model.RemoveMappedType(typeof(Complex2).FullName));
            Assert.Null(model.RemoveMappedType("ShadowComplex1"));
        }

        [Fact]
        public virtual void Remove_entity_types_as_structural_types()
        {
            IMutableModel model = new Model();

            var entity1 = model.AddComplexTypeDefinition(typeof(Entity1));
            var entity2 = model.AddComplexTypeDefinition(typeof(Entity2));
            var shadowEntity1 = model.AddComplexTypeDefinition("ShadowEntity1");

            Assert.Same(entity1, model.RemoveMappedType(typeof(Entity1)));
            Assert.Same(entity2, model.RemoveMappedType(typeof(Entity2).FullName));
            Assert.Same(shadowEntity1, model.RemoveMappedType("ShadowEntity1"));

            Assert.Empty(model.GetEntityTypes());

            Assert.Null(model.RemoveMappedType(typeof(Entity1)));
            Assert.Null(model.RemoveMappedType(typeof(Entity2).FullName));
            Assert.Null(model.RemoveMappedType("ShadowEntity1"));
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

        private struct Complex1Struct
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
