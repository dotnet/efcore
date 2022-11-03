// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Query.Internal;

// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable ConvertToAutoProperty
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class EntityMaterializerSourceTest
{
    private readonly DbContext _fakeContext = new(new DbContextOptions<DbContext>());

    [ConditionalFact]
    public void Throws_for_abstract_types()
    {
        var entityType = CreateConventionalModelBuilder().Model.AddEntityType(typeof(SomeAbstractEntity));
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));

        Assert.Equal(
            CoreStrings.CannotMaterializeAbstractType(nameof(SomeAbstractEntity)),
            Assert.Throws<InvalidOperationException>(() => source.CreateMaterializeExpression((IEntityType)entityType, "", null!))
                .Message);
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
                    new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Id))),
                    new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Goo)))
                }
            );

        entityType.Model.FinalizeModel();

        var factory = GetMaterializer(
            new EntityMaterializerSource(
                new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>())), entityType);

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
                    new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Id))),
                    new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Goo)))
                },
                entityType.ClrType);

        entityType.Model.FinalizeModel();

        var factory = GetMaterializer(
            new EntityMaterializerSource(
                new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>())), entityType);

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
                            new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Id))),
                            new PropertyParameterBinding(entityType.FindProperty(nameof(SomeEntity.Goo)))
                        })
                },
                entityType.ClrType);

        entityType.Model.FinalizeModel();

        var factory = GetMaterializer(
            new EntityMaterializerSource(
                new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>())), entityType);

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

        var factory = GetMaterializer(
            new EntityMaterializerSource(
                new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>())), entityType);

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

        var factory = GetMaterializer(
            new EntityMaterializerSource(
                new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>())), entityType);

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
        modelBuilder.Entity<SomeEntityWithFields>(
            eb =>
            {
                eb.UsePropertyAccessMode(PropertyAccessMode.Field);

                eb.Property(e => e.Enum).HasField("_enum");
                eb.Property(e => e.Foo).HasField("_foo");
                eb.Property(e => e.Goo).HasField("_goo");
                eb.Property(e => e.Id).HasField("_id");
                eb.Property(e => e.MaybeEnum).HasField("_maybeEnum");
            });

        var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(SomeEntityWithFields));

        var factory = GetMaterializer(
            new EntityMaterializerSource(
                new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>())), entityType);

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
        modelBuilder.Entity<SomeEntity>(
            eb =>
            {
                eb.Ignore(e => e.Enum);
                eb.Ignore(e => e.MaybeEnum);
            });

        var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(SomeEntity));

        var factory = GetMaterializer(
            new EntityMaterializerSource(
                new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>())), entityType);

        var entity = (SomeEntity)factory(
            new MaterializationContext(
                new ValueBuffer(new object[] { 77, null, null }),
                _fakeContext));

        Assert.Equal(77, entity.Id);
        Assert.Null(entity.Foo);
        Assert.Null(entity.Goo);
    }

    [ConditionalFact]
    public void Can_create_materializer_for_entity_ignoring_shadow_fields()
    {
        var modelBuilder = CreateConventionalModelBuilder();

        modelBuilder.Entity<SomeEntity>(
            eb =>
            {
                eb.UsePropertyAccessMode(PropertyAccessMode.Property);

                eb.Ignore(e => e.Enum);
                eb.Ignore(e => e.MaybeEnum);

                eb.Property<int>("IdShadow");
                eb.Property<string>("FooShadow");
                eb.Property<Guid>("GooShadow");
            });

        var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(SomeEntity));

        var factory = GetMaterializer(
            new EntityMaterializerSource(
                new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>())), entityType);

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
                nameof(EntityWithoutParameterlessConstructor),
                Environment.NewLine
                + "    "
                + CoreStrings.ConstructorBindingFailed("value", "EntityWithoutParameterlessConstructor(int value)")
                + Environment.NewLine),
            Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
    }

    [ConditionalFact]
    public void GetEmptyMaterializer_Create_instance_with_parameterless_constructor()
    {
        using var context = new FactoryContext();

        var entityType = context.Model.FindEntityType(typeof(Parameterless))!;
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));
        var instance1 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));
        var instance2 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));

        Assert.IsType<Parameterless>(instance1);
        Assert.IsType<Parameterless>(instance2);
        Assert.NotSame(instance1, instance2);
    }

    [ConditionalFact]
    public void GetEmptyMaterializer_Create_instance_with_lazy_loader()
    {
        using var context = new FactoryContext();

        var entityType = context.Model.FindEntityType(typeof(WithLazyLoader))!;
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));
        var instance1 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));
        var instance2 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));

        Assert.IsType<WithLazyLoader>(instance1);
        Assert.NotNull(((WithLazyLoader)instance1).LazyLoader);
        Assert.IsType<WithLazyLoader>(instance2);
        Assert.NotSame(instance1, instance2);
        Assert.NotSame(((WithLazyLoader)instance1).LazyLoader, ((WithLazyLoader)instance2).LazyLoader);
    }

    [ConditionalFact]
    public void GetEmptyMaterializer_Create_instance_with_lazy_loading_delegate()
    {
        using var context = new FactoryContext();

        var entityType = context.Model.FindEntityType(typeof(WithLazyLoaderDelegate))!;
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));
        var instance1 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));
        var instance2 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));

        Assert.IsType<WithLazyLoaderDelegate>(instance1);
        Assert.NotNull(((WithLazyLoaderDelegate)instance1).LazyLoader);
        Assert.IsType<WithLazyLoaderDelegate>(instance2);
        Assert.NotSame(instance1, instance2);
        Assert.NotSame(((WithLazyLoaderDelegate)instance1).LazyLoader, ((WithLazyLoaderDelegate)instance2).LazyLoader);
    }

    [ConditionalFact]
    public void GetEmptyMaterializer_Create_instance_with_entity_type()
    {
        using var context = new FactoryContext();

        var entityType = context.Model.FindEntityType(typeof(WithEntityType))!;
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));
        var instance1 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));
        var instance2 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));

        Assert.IsType<WithEntityType>(instance1);
        Assert.NotNull(((WithEntityType)instance1).EntityType);
        Assert.IsType<WithEntityType>(instance2);
        Assert.NotSame(instance1, instance2);
        Assert.Same(((WithEntityType)instance1).EntityType, ((WithEntityType)instance2).EntityType);
    }

    [ConditionalFact]
    public void GetEmptyMaterializer_Create_instance_with_context()
    {
        using var context = new FactoryContext();

        var entityType = context.Model.FindEntityType(typeof(WithContext))!;
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));
        var instance1 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));
        var instance2 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));

        Assert.IsType<WithContext>(instance1);
        Assert.Same(context, ((WithContext)instance1).Context);
        Assert.IsType<WithContext>(instance2);
        Assert.NotSame(instance1, instance2);
        Assert.Same(context, ((WithContext)instance2).Context);
    }

    [ConditionalFact]
    public void GetEmptyMaterializer_Create_instance_with_service_and_with_properties()
    {
        using var context = new FactoryContext();

        var entityType = context.Model.FindEntityType(typeof(WithServiceAndWithProperties))!;
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));
        var instance1 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));
        var instance2 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));

        Assert.IsType<WithServiceAndWithProperties>(instance1);
        Assert.NotNull(((WithServiceAndWithProperties)instance1).LazyLoader);
        Assert.IsType<WithServiceAndWithProperties>(instance2);
        Assert.NotSame(instance1, instance2);
        Assert.NotSame(((WithServiceAndWithProperties)instance1).LazyLoader, ((WithServiceAndWithProperties)instance2).LazyLoader);
    }

    [ConditionalFact]
    public void GetEmptyMaterializer_Create_instance_with_parameterless_and_with_properties()
    {
        using var context = new FactoryContext();

        var entityType = context.Model.FindEntityType(typeof(ParameterlessAndWithProperties))!;
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));
        var instance1 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));
        var instance2 = source.GetEmptyMaterializer(entityType)(new MaterializationContext(ValueBuffer.Empty, context));

        Assert.IsType<ParameterlessAndWithProperties>(instance1);
        Assert.IsType<ParameterlessAndWithProperties>(instance2);
        Assert.NotSame(instance1, instance2);
    }

    [ConditionalFact]
    public void GetEmptyMaterializer_Throws_for_constructor_with_properties()
    {
        using var context = new FactoryContext();

        var entityType = context.Model.FindEntityType(typeof(WithProperties))!;
        var source = new EntityMaterializerSource(new EntityMaterializerSourceDependencies(Array.Empty<ISingletonInterceptor>()));

        Assert.Equal(
            CoreStrings.NoParameterlessConstructor(nameof(WithProperties)),
            Assert.Throws<InvalidOperationException>(
                () => source.GetEmptyMaterializer(entityType)).Message);
    }

    private class FactoryContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(FactoryContext))
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parameterless>();
            modelBuilder.Entity<ParameterlessAndWithProperties>();
            modelBuilder.Entity<WithProperties>();
            modelBuilder.Entity<WithLazyLoader>();
            modelBuilder.Entity<WithLazyLoaderDelegate>();
            modelBuilder.Entity<WithEntityType>();
            modelBuilder.Entity<WithContext>();
            modelBuilder.Entity<WithServiceAndWithProperties>();
        }
    }

    private class Parameterless
    {
        private Parameterless()
        {
        }

        public int Id { get; set; }
    }

    private class WithProperties
    {
        public WithProperties(int id)
        {
            Id = id;
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; set; }
    }

    private class ParameterlessAndWithProperties
    {
        public ParameterlessAndWithProperties()
        {
        }

        public ParameterlessAndWithProperties(int id)
        {
            Id = id;
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; set; }
    }

    private class WithLazyLoader
    {
        public WithLazyLoader(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        public int Id { get; set; }
        public ILazyLoader LazyLoader { get; }
    }

    private class WithLazyLoaderDelegate
    {
        public WithLazyLoaderDelegate(Action<object, string> lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        public int Id { get; set; }
        public Action<object, string> LazyLoader { get; }
    }

    private class WithEntityType
    {
        public WithEntityType(IEntityType entityType)
        {
            EntityType = entityType;
        }

        public int Id { get; set; }
        public IEntityType EntityType { get; }
    }

    private class WithContext
    {
        public WithContext(DbContext context)
        {
            Context = context;
        }

        public int Id { get; set; }
        public DbContext Context { get; }
    }

    private class WithServiceAndWithProperties
    {
        public WithServiceAndWithProperties(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        public WithServiceAndWithProperties(ILazyLoader lazyLoader, int id)
            : this(lazyLoader)
        {
            Id = id;
        }

        public ILazyLoader LazyLoader { get; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; set; }
    }

    protected virtual ModelBuilder CreateConventionalModelBuilder(bool sensitiveDataLoggingEnabled = false)
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();

    private static readonly ParameterExpression _contextParameter
        = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

    public virtual Func<MaterializationContext, object> GetMaterializer(
        IEntityMaterializerSource source,
        IReadOnlyEntityType entityType)
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
