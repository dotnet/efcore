// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable ConvertToAutoProperty
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class EntityMaterializerSourceTest
    {
        [Fact]
        public void Delegate_from_entity_type_is_returned_if_it_implements_IEntityMaterializer()
        {
            Assert.Equal("Bazinga!", GetMaterializer(new EntityMaterializerSource(), new FakeType())(ValueBuffer.Empty));
        }

        private class FakeType : IEntityType, IEntityMaterializer
        {
            public object CreateEntity(ValueBuffer valueBuffer) => "Bazinga!";

            public object this[string name] => throw new NotImplementedException();
            public IAnnotation FindAnnotation(string name) => throw new NotImplementedException();
            public IEnumerable<IAnnotation> GetAnnotations() => throw new NotImplementedException();
            public IModel Model { get; }
            public string Name { get; }
            public Type ClrType { get; }
            public IEntityType BaseType { get; }
            public string DefiningNavigationName { get; }
            public IEntityType DefiningEntityType { get; }
            public LambdaExpression QueryFilter { get; }
            public IKey FindPrimaryKey() => throw new NotImplementedException();
            public IKey FindKey(IReadOnlyList<IProperty> properties) => throw new NotImplementedException();
            public IEnumerable<IKey> GetKeys() => throw new NotImplementedException();
            public IForeignKey FindForeignKey(IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType) => throw new NotImplementedException();
            public IEnumerable<IForeignKey> GetForeignKeys() => throw new NotImplementedException();
            public IIndex FindIndex(IReadOnlyList<IProperty> properties) => throw new NotImplementedException();
            public IEnumerable<IIndex> GetIndexes() => throw new NotImplementedException();
            public IProperty FindProperty(string name) => throw new NotImplementedException();
            public IEnumerable<IProperty> GetProperties() => throw new NotImplementedException();
        }

        [Fact]
        public void Can_create_materializer_for_entity_with_auto_properties()
        {
            var entityType = new Model().AddEntityType(typeof(SomeEntity));
            entityType.AddProperty(SomeEntity.EnumProperty);
            entityType.AddProperty(SomeEntity.FooProperty);
            entityType.AddProperty(SomeEntity.GooProperty);
            entityType.AddProperty(SomeEntity.IdProperty);
            entityType.AddProperty(SomeEntity.MaybeEnumProperty);

            var factory = GetMaterializer(new EntityMaterializerSource(), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, SomeEnum.EnumValue }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Equal(SomeEnum.EnumValue, entity.MaybeEnum);
        }

        [Fact]
        public void Can_create_materializer_for_entity_with_fields()
        {
            var entityType = new Model().AddEntityType(typeof(SomeEntityWithFields));
            entityType.AddProperty(SomeEntityWithFields.EnumProperty).SetField("_enum");
            entityType.AddProperty(SomeEntityWithFields.FooProperty).SetField("_foo");
            entityType.AddProperty(SomeEntityWithFields.GooProperty).SetField("_goo");
            entityType.AddProperty(SomeEntityWithFields.IdProperty).SetField("_id");
            entityType.AddProperty(SomeEntityWithFields.MaybeEnumProperty).SetField("_maybeEnum");

            var factory = GetMaterializer(new EntityMaterializerSource(), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntityWithFields)factory(new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, null }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Null(entity.MaybeEnum);
        }

        [Fact]
        public void Can_read_nulls()
        {
            var entityType = new Model().AddEntityType(typeof(SomeEntity));
            entityType.AddProperty(SomeEntity.FooProperty);
            entityType.AddProperty(SomeEntity.GooProperty);
            entityType.AddProperty(SomeEntity.IdProperty);

            var factory = GetMaterializer(new EntityMaterializerSource(), entityType);

            var entity = (SomeEntity)factory(new ValueBuffer(new object[] { null, null, 77 }));

            Assert.Equal(77, entity.Id);
            Assert.Null(entity.Foo);
            Assert.Null(entity.Goo);
        }

        [Fact]
        public void Can_create_materializer_for_entity_ignoring_shadow_fields()
        {
            var entityType = new Model().AddEntityType(typeof(SomeEntity));
            entityType.AddProperty(SomeEntity.IdProperty);
            entityType.AddProperty("IdShadow", typeof(int));
            entityType.AddProperty(SomeEntity.FooProperty);
            entityType.AddProperty("FooShadow", typeof(string));
            entityType.AddProperty(SomeEntity.GooProperty);
            entityType.AddProperty("GooShadow", typeof(Guid));

            var factory = GetMaterializer(new EntityMaterializerSource(), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new ValueBuffer(new object[] { "Fu", "FuS", gu, Guid.NewGuid(), 77, 777 }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        [Fact]
        public void Throws_if_parameterless_constructor_is_not_defined_on_entity_type()
        {
            var entityType = new Model().AddEntityType(typeof(EntityWithoutParameterlessConstructor));
            entityType.AddProperty(EntityWithoutParameterlessConstructor.IdProperty);

            Assert.Equal(
                CoreStrings.NoParameterlessConstructor(typeof(EntityWithoutParameterlessConstructor).Name),
                Assert.Throws<InvalidOperationException>(() => GetMaterializer(new EntityMaterializerSource(), entityType)).Message);
        }

        private static readonly ParameterExpression _readerParameter
            = Expression.Parameter(typeof(ValueBuffer), "valueBuffer");

        public virtual Func<ValueBuffer, object> GetMaterializer(IEntityMaterializerSource source, IEntityType entityType) => Expression.Lambda<Func<ValueBuffer, object>>(
                source.CreateMaterializeExpression(entityType, _readerParameter),
                _readerParameter)
            .Compile();

        private class SomeEntity
        {
            public static readonly PropertyInfo IdProperty = typeof(SomeEntity).GetProperty("Id");
            public static readonly PropertyInfo FooProperty = typeof(SomeEntity).GetProperty("Foo");
            public static readonly PropertyInfo GooProperty = typeof(SomeEntity).GetProperty("Goo");
            public static readonly PropertyInfo EnumProperty = typeof(SomeEntity).GetProperty("Enum");
            public static readonly PropertyInfo MaybeEnumProperty = typeof(SomeEntity).GetProperty("MaybeEnum");

            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int Id { get; set; }
            public string Foo { get; set; }
            public Guid? Goo { get; set; }
            public SomeEnum Enum { get; set; }
            public SomeEnum? MaybeEnum { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        private class SomeEntityWithFields
        {
            public static readonly PropertyInfo IdProperty = typeof(SomeEntityWithFields).GetProperty("Id");
            public static readonly PropertyInfo FooProperty = typeof(SomeEntityWithFields).GetProperty("Foo");
            public static readonly PropertyInfo GooProperty = typeof(SomeEntityWithFields).GetProperty("Goo");
            public static readonly PropertyInfo EnumProperty = typeof(SomeEntityWithFields).GetProperty("Enum");
            public static readonly PropertyInfo MaybeEnumProperty = typeof(SomeEntityWithFields).GetProperty("MaybeEnum");

#pragma warning disable 649
            private int _id;
            private string _foo;
            private Guid? _goo;
            private SomeEnum _enum;
            private SomeEnum? _maybeEnum;
#pragma warning restore 649

            public int Id
            {
                get { return _id; }
            }

            public string Foo
            {
                get { return _foo; }
            }

            public Guid? Goo
            {
                get { return _goo; }
            }

            public SomeEnum Enum
            {
                get { return _enum; }
            }

            public SomeEnum? MaybeEnum
            {
                get { return _maybeEnum; }
            }
        }

        private enum SomeEnum
        {
            EnumValue
        }

        private class EntityWithoutParameterlessConstructor
        {
            public static readonly PropertyInfo IdProperty = typeof(EntityWithoutParameterlessConstructor).GetProperty("Id");

            public int Id { get; set; }

            private readonly int _value;

            public EntityWithoutParameterlessConstructor(int value)
            {
                _value = value;
            }
        }
    }
}
