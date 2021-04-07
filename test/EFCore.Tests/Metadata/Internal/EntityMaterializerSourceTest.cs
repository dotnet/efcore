// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
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
        private readonly DbContext _fakeContext = new(new DbContextOptions<DbContext>());

        [ConditionalFact]
        public void Throws_for_abstract_types()
        {
            var entityType = ((IMutableModel)CreateConventionalModelBuilder().Model).AddEntityType(typeof(SomeAbstractEntity));
            var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies());

            Assert.Equal(
                CoreStrings.CannotMaterializeAbstractType(nameof(SomeAbstractEntity)),
                Assert.Throws<InvalidOperationException>(() => source.CreateMaterializeExpression((IEntityType)entityType, "", null!)).Message);
        }

        [ConditionalFact]
        public void Can_create_materializer_for_entity_with_constructor_properties()
        {
            var entityType = CreateEntityType();

            entityType.ConstructorBinding
                = new ConstructorBinding(
                    typeof(SomeEntity).GetTypeInfo().DeclaredConstructors.Single(c => c.GetParameters().Length == 2),
                    new List<ParameterBinding>
                    {
                        new PropertyParameterBinding((IProperty)entityType.FindProperty(nameof(SomeEntity.Id))),
                        new PropertyParameterBinding((IProperty)entityType.FindProperty(nameof(SomeEntity.Goo)))
                    }
                );

            entityType.Model.FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { 77, SomeEnum.EnumValue, "Fu", gu, SomeEnum.EnumValue }),
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

            entityType.ConstructorBinding
                = new FactoryMethodBinding(
                    typeof(SomeEntity).GetTypeInfo().GetDeclaredMethod(nameof(SomeEntity.Factory)),
                    new List<ParameterBinding>
                    {
                        new PropertyParameterBinding((IProperty)entityType.FindProperty(nameof(SomeEntity.Id))),
                        new PropertyParameterBinding((IProperty)entityType.FindProperty(nameof(SomeEntity.Goo)))
                    },
                    entityType.ClrType);

            entityType.Model.FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { 77, SomeEnum.EnumValue, "Fu", gu, SomeEnum.EnumValue }),
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

            entityType.ConstructorBinding
                = new FactoryMethodBinding(
                    typeof(SomeEntity).GetTypeInfo().GetDeclaredMethod(nameof(SomeEntity.GeneralFactory)),
                    new List<ParameterBinding>
                    {
                        new ObjectArrayParameterBinding(
                            new List<ParameterBinding>
                            {
                                new PropertyParameterBinding((IProperty)entityType.FindProperty(nameof(SomeEntity.Id))),
                                new PropertyParameterBinding((IProperty)entityType.FindProperty(nameof(SomeEntity.Goo)))
                            })
                    },
                    entityType.ClrType);

            entityType.Model.FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { 77, SomeEnum.EnumValue, "Fu", gu, SomeEnum.EnumValue }),
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

            entityType.ConstructorBinding
                = new FactoryMethodBinding(
                    TestProxyFactory.Instance,
                    typeof(TestProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(TestProxyFactory.Create)),
                    new List<ParameterBinding> { new EntityTypeParameterBinding() },
                    entityType.ClrType);

            entityType.Model.FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { 77, SomeEnum.EnumValue, "Fu", gu, SomeEnum.EnumValue }),
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
            public static readonly TestProxyFactory Instance = new();

            public object Create(IEntityType entityType)
                => Activator.CreateInstance(entityType.ClrType);
        }

        private EntityType CreateEntityType()
            => (EntityType)CreateConventionalModelBuilder().Entity<SomeEntity>().Metadata;

        [ConditionalFact]
        public void Can_create_materializer_for_entity_with_auto_properties()
        {
            var entityType = CreateEntityType();
            entityType.Model.FinalizeModel();

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { 77, SomeEnum.EnumValue, "Fu", gu, SomeEnum.EnumValue }),
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
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<SomeEntityWithFields>(eb =>
            {
                eb.UsePropertyAccessMode(PropertyAccessMode.Field);

                eb.Property(e => e.Enum).HasField("_enum");
                eb.Property(e => e.Foo).HasField("_foo");
                eb.Property(e => e.Goo).HasField("_goo");
                eb.Property(e => e.Id).HasField("_id");
                eb.Property(e => e.MaybeEnum).HasField("_maybeEnum");
            });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(SomeEntityWithFields));

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntityWithFields)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { 77, SomeEnum.EnumValue, "Fu", gu, null }),
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
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<SomeEntity>(eb =>
            {
                eb.Ignore(e => e.Enum);
                eb.Ignore(e => e.MaybeEnum);
            });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(SomeEntity));

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { 77, null, null}),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Null(entity.Foo);
            Assert.Null(entity.Goo);
        }

        [ConditionalFact]
        public void Can_create_materializer_for_entity_ignoring_shadow_fields()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<SomeEntity>(eb =>
            {
                eb.UsePropertyAccessMode(PropertyAccessMode.Property);

                eb.Ignore(e => e.Enum);
                eb.Ignore(e => e.MaybeEnum);

                eb.Property<int>("IdShadow");
                eb.Property<string>("FooShadow");
                eb.Property<Guid>("GooShadow");
            });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(SomeEntity));

            var factory = GetMaterializer(new EntityMaterializerSource(new EntityMaterializerSourceDependencies()), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(
                new MaterializationContext(
                    new ValueBuffer(new object[] { 77, "Fu", "FuS", gu, Guid.NewGuid(), 777 }),
                    _fakeContext));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        [ConditionalFact]
        public void Throws_if_parameterless_constructor_is_not_defined_on_entity_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<EntityWithoutParameterlessConstructor>();

            Assert.Equal(
                CoreStrings.ConstructorNotFound(
                    typeof(EntityWithoutParameterlessConstructor).Name, "cannot bind 'value' in 'EntityWithoutParameterlessConstructor(int value)'"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.FinalizeModel()).Message);
        }

        protected virtual ModelBuilder CreateConventionalModelBuilder(bool sensitiveDataLoggingEnabled = false)
            => InMemoryTestHelpers.Instance.CreateConventionBuilder();

        private static readonly ParameterExpression _contextParameter
            = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

        public virtual Func<MaterializationContext, object> GetMaterializer(IEntityMaterializerSource source, IReadOnlyEntityType entityType)
            => Expression.Lambda<Func<MaterializationContext, object>>(
                    source.CreateMaterializeExpression((IEntityType)entityType, "instance", _contextParameter),
                    _contextParameter)
                .Compile();

        private abstract class SomeAbstractEntity
        {
        }

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
                => new(id, goo) { FactoryUsed = true };

            public static SomeEntity GeneralFactory(object[] constructorArguments)
            {
                Assert.Equal(2, constructorArguments.Length);

                return Factory((int)constructorArguments[0], (Guid?)constructorArguments[1]);
            }

            [NotMapped]
            public bool FactoryUsed { get; set; }

            [NotMapped]
            public bool ParameterizedConstructorUsed { get; }

            [NotMapped]
            public bool IdSetterCalled { get; set; }

            [NotMapped]
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

            public int Id
                => _id;

            public string Foo
                => _foo;

            public Guid? Goo
                => _goo;

            public SomeEnum Enum
                => _enum;

            public SomeEnum? MaybeEnum
                => _maybeEnum;
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
