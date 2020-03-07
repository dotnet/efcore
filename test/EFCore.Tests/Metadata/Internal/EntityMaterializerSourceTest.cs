// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable ConvertToAutoProperty
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class EntityMaterializerSourceTest
    {
        private readonly DbContext _fakeContext = new DbContext(new DbContextOptions<DbContext>());

        [ConditionalFact]
        public void Can_create_materializer_for_entity_with_constructor_properties()
        {
            var entityType = CreateEntityType();

            entityType[CoreAnnotationNames.ConstructorBinding]
                = new ConstructorBinding(
                    typeof(SomeEntity).GetTypeInfo().DeclaredConstructors.Single(c => c.GetParameters().Length == 2),
                    new List<ParameterBinding>
                    {
                        new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Id))),
                        new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Goo)))
                    }
                );
            ((Model)entityType.Model).FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, SomeEnum.EnumValue }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Equal(SomeEnum.EnumValue, entity.MaybeEnum);

            Assert.False(entity.FactoryUsed);
            Assert.True(entity.ParameterizedConstructorUsed);
            Assert.False(entity.IdSetterCalled);
            Assert.False(entity.GooSetterCalled);
        }

        [ConditionalFact]
        public void Can_create_materializer_for_entity_with_factory_method()
        {
            var entityType = CreateEntityType();

            entityType[CoreAnnotationNames.ConstructorBinding]
                = new FactoryMethodBinding(
                    typeof(SomeEntity).GetTypeInfo().GetDeclaredMethod(nameof(SomeEntity.Factory)),
                    new List<ParameterBinding>
                    {
                        new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Id))),
                        new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Goo)))
                    },
                    entityType.ClrType);
            ((Model)entityType.Model).FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, SomeEnum.EnumValue }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Equal(SomeEnum.EnumValue, entity.MaybeEnum);

            Assert.True(entity.FactoryUsed);
            Assert.True(entity.ParameterizedConstructorUsed);
            Assert.False(entity.IdSetterCalled);
            Assert.False(entity.GooSetterCalled);
        }

        [ConditionalFact]
        public void Can_create_materializer_for_entity_with_factory_method_with_object_array()
        {
            var entityType = CreateEntityType();

            entityType[CoreAnnotationNames.ConstructorBinding]
                = new FactoryMethodBinding(
                    typeof(SomeEntity).GetTypeInfo().GetDeclaredMethod(nameof(SomeEntity.GeneralFactory)),
                    new List<ParameterBinding>
                    {
                        new ObjectArrayParameterBinding(
                            new List<ParameterBinding>
                            {
                                new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Id))),
                                new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Goo)))
                            })
                    },
                    entityType.ClrType);
            ((Model)entityType.Model).FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, SomeEnum.EnumValue }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Equal(SomeEnum.EnumValue, entity.MaybeEnum);

            Assert.True(entity.FactoryUsed);
            Assert.True(entity.ParameterizedConstructorUsed);
            Assert.False(entity.IdSetterCalled);
            Assert.False(entity.GooSetterCalled);
        }

        [ConditionalFact]
        public void Can_create_materializer_for_entity_with_instance_factory_method()
        {
            var entityType = CreateEntityType();

            entityType[CoreAnnotationNames.ConstructorBinding]
                = new FactoryMethodBinding(
                    TestProxyFactory.Instance,
                    typeof(TestProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(TestProxyFactory.Create)),
                    new List<ParameterBinding> { new EntityTypeParameterBinding() },
                    entityType.ClrType);
            ((Model)entityType.Model).FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, SomeEnum.EnumValue }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Equal(SomeEnum.EnumValue, entity.MaybeEnum);

            Assert.False(entity.FactoryUsed);
            Assert.False(entity.ParameterizedConstructorUsed);
            Assert.True(entity.IdSetterCalled);
            Assert.True(entity.GooSetterCalled);
        }

        private class TestProxyFactory
        {
            public static readonly TestProxyFactory Instance = new TestProxyFactory();

            public object Create(IEntityType entityType)
                => Activator.CreateInstance(entityType.ClrType);
        }

        private static IMutableEntityType CreateEntityType()
        {
            var entityType = ((IMutableModel)new Model()).AddEntityType(typeof(SomeEntity));
            entityType.AddProperty(SomeEntity.EnumProperty);
            entityType.AddProperty(SomeEntity.FooProperty);
            entityType.AddProperty(SomeEntity.GooProperty);
            entityType.AddProperty(SomeEntity.IdProperty);
            entityType.AddProperty(SomeEntity.MaybeEnumProperty);
            return entityType;
        }

        [ConditionalFact]
        public void Can_create_materializer_for_entity_with_auto_properties()
        {
            var entityType = CreateEntityType();
            ((Model)entityType.Model).FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, SomeEnum.EnumValue }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Equal(SomeEnum.EnumValue, entity.MaybeEnum);
        }

        [ConditionalFact]
        public void Can_create_materializer_for_entity_with_fields()
        {
            var entityType = ((IMutableModel)new Model()).AddEntityType(typeof(SomeEntityWithFields));
            entityType.AddProperty(SomeEntityWithFields.EnumProperty).SetField("_enum");
            entityType.AddProperty(SomeEntityWithFields.FooProperty).SetField("_foo");
            entityType.AddProperty(SomeEntityWithFields.GooProperty).SetField("_goo");
            entityType.AddProperty(SomeEntityWithFields.IdProperty).SetField("_id");
            entityType.AddProperty(SomeEntityWithFields.MaybeEnumProperty).SetField("_maybeEnum");
            ((Model)entityType.Model).FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntityWithFields)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, null }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Null(entity.MaybeEnum);
        }

        [ConditionalFact]
        public void Can_read_nulls()
        {
            var entityType = ((IMutableModel)new Model()).AddEntityType(typeof(SomeEntity));
            entityType.AddProperty(SomeEntity.FooProperty);
            entityType.AddProperty(SomeEntity.GooProperty);
            entityType.AddProperty(SomeEntity.IdProperty);
            ((Model)entityType.Model).FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { null, null, 77 }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Null(entity.Foo);
            Assert.Null(entity.Goo);
        }

        [ConditionalFact]
        public void Can_create_materializer_for_entity_ignoring_shadow_fields()
        {
            var entityType = ((IMutableModel)new Model()).AddEntityType(typeof(SomeEntity));
            entityType.AddProperty(SomeEntity.IdProperty);
            entityType.AddProperty("IdShadow", typeof(int));
            entityType.AddProperty(SomeEntity.FooProperty);
            entityType.AddProperty("FooShadow", typeof(string));
            entityType.AddProperty(SomeEntity.GooProperty);
            entityType.AddProperty("GooShadow", typeof(Guid));
            ((Model)entityType.Model).FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { "Fu", "FuS", gu, Guid.NewGuid(), 77, 777 }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        [ConditionalFact]
        public void Throws_if_parameterless_constructor_is_not_defined_on_entity_type()
        {
            var entityType = ((IMutableModel)new Model()).AddEntityType(typeof(EntityWithoutParameterlessConstructor));
            entityType.AddProperty(EntityWithoutParameterlessConstructor.IdProperty);

            Assert.Equal(
                CoreStrings.NoParameterlessConstructor(typeof(EntityWithoutParameterlessConstructor).Name),
                Assert.Throws<InvalidOperationException>(
                    () => GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType)).Message);
        }

        private static readonly ParameterExpression _contextParameter
            = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

        public virtual Func<MaterializationContext, object> GetMaterializer(IEntityMaterializerSource source, IEntityType entityType)
            => Expression.Lambda<Func<MaterializationContext, object>>(
                    source.CreateMaterializeExpression(entityType, "instance", _contextParameter),
                    _contextParameter)
                .Compile();

        private class SomeEntity
        {
            private int _hiddenId;
            private Guid? _hiddenGoo;

            public SomeEntity()
            {
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public SomeEntity(int id, Guid? goo)
            {
                _hiddenId = id;
                _hiddenGoo = goo;

                ParameterizedConstructorUsed = true;
            }

            public static SomeEntity Factory(int id, Guid? goo)
                => new SomeEntity(id, goo) { FactoryUsed = true };

            public static SomeEntity GeneralFactory(object[] constructorArguments)
            {
                Assert.Equal(2, constructorArguments.Length);

                return Factory((int)constructorArguments[0], (Guid?)constructorArguments[1]);
            }

            public bool FactoryUsed { get; set; }
            public bool ParameterizedConstructorUsed { get; }
            public bool IdSetterCalled { get; set; }
            public bool GooSetterCalled { get; set; }

            public static readonly PropertyInfo IdProperty = typeof(SomeEntity).GetProperty("Id");
            public static readonly PropertyInfo FooProperty = typeof(SomeEntity).GetProperty("Foo");
            public static readonly PropertyInfo GooProperty = typeof(SomeEntity).GetProperty("Goo");
            public static readonly PropertyInfo EnumProperty = typeof(SomeEntity).GetProperty("Enum");
            public static readonly PropertyInfo MaybeEnumProperty = typeof(SomeEntity).GetProperty("MaybeEnum");

            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int Id
            {
                get => _hiddenId;
                set
                {
                    IdSetterCalled = true;
                    _hiddenId = value;
                }
            }

            public string Foo { get; set; }

            public Guid? Goo
            {
                get => _hiddenGoo;
                set
                {
                    GooSetterCalled = true;
                    _hiddenGoo = value;
                }
            }

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

#pragma warning disable 649, IDE0044 // Add readonly modifier
            private int _id;
            private string _foo;
            private Guid? _goo;
            private SomeEnum _enum;
            private SomeEnum? _maybeEnum;
#pragma warning restore 649, IDE0044 // Add readonly modifier

            public int Id => _id;
            public string Foo => _foo;
            public Guid? Goo => _goo;
            public SomeEnum Enum => _enum;
            public SomeEnum? MaybeEnum => _maybeEnum;
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
