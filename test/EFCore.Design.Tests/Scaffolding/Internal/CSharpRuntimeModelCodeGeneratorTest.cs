// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.TestModel.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.Migrations.Design.CSharpMigrationsGeneratorTest;
using static Microsoft.EntityFrameworkCore.Scaffolding.Internal.CSharpRuntimeModelCodeGeneratorTest;
using IdentityUser = Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity.IdentityUser;
using Index = Microsoft.EntityFrameworkCore.Scaffolding.Internal.Index;

public class GlobalNamespaceContext : ContextBase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity(
            "1", e =>
            {
                e.Property<int>("Id");
                e.HasKey("Id");
            });
    }
}

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpRuntimeModelCodeGeneratorTest
    {
        [ConditionalFact]
        public void Self_referential_property()
            => Test(
                new SelfReferentialDbContext(),
                new CompiledModelCodeGenerationOptions(),
                assertModel: model =>
                {
                    Assert.Single(model.GetEntityTypes());
                    Assert.Same(model, model.FindRuntimeAnnotationValue("ReadOnlyModel"));
                }
            );

        [ConditionalFact]
        public void Empty_model()
            => Test(
                new EmptyContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    Assert.Empty(model.GetEntityTypes());
                    Assert.Same(model, model.FindRuntimeAnnotationValue("ReadOnlyModel"));
                });

        public class EmptyContext : ContextBase
        {
        }

        [ConditionalFact]
        public void Global_namespace_works()
            => Test(
                new GlobalNamespaceContext(),
                new CompiledModelCodeGenerationOptions { ModelNamespace = string.Empty },
                model =>
                {
                    Assert.NotNull(model.FindEntityType("1"));
                });

        [ConditionalFact]
        public void Throws_for_constructor_binding()
            => Test(
                new ConstructorBindingContext(),
                new CompiledModelCodeGenerationOptions(),
                expectedExceptionMessage: DesignStrings.CompiledModelConstructorBinding("Lazy", "Customize()", "LazyEntityType"));

        public class ConstructorBindingContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity(
                    "Lazy", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                        ((EntityType)e.Metadata).ConstructorBinding = new ConstructorBinding(
                            typeof(object).GetConstructor(Type.EmptyTypes)!,
                            Array.Empty<ParameterBinding>());
                    });
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => base.OnConfiguring(options.UseLazyLoadingProxies());
        }

        [ConditionalFact]
        public void Manual_lazy_loading()
            => Test(
                new LazyLoadingContext(),
                new CompiledModelCodeGenerationOptions(),
                assertModel: model =>
                {
                    var lazyConstructorEntity = model.FindEntityType(typeof(LazyConstructorEntity));
                    var lazyParameterBinding = lazyConstructorEntity!.ConstructorBinding!.ParameterBindings.Single();
                    Assert.Equal(typeof(ILazyLoader), lazyParameterBinding.ParameterType);

                    var lazyPropertyEntity = model.FindEntityType(typeof(LazyPropertyEntity));
                    var lazyServiceProperty = lazyPropertyEntity!.GetServiceProperties().Single();
                    Assert.Equal(typeof(ILazyLoader), lazyServiceProperty.ClrType);

                    var lazyPropertyDelegateEntity = model.FindEntityType(typeof(LazyPropertyDelegateEntity));
                    Assert.Equal(2, lazyPropertyDelegateEntity!.GetServiceProperties().Count());
                    Assert.Contains(lazyPropertyDelegateEntity!.GetServiceProperties(), p => p.ClrType == typeof(ILazyLoader));
                    Assert.Contains(lazyPropertyDelegateEntity!.GetServiceProperties(), p => p.ClrType == typeof(Action<object, string>));
                });

        public class LazyLoadingContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<LazyConstructorEntity>();

                modelBuilder.Entity<LazyPropertyDelegateEntity>(
                    b =>
                    {
                        var serviceProperty = (ServiceProperty)b.Metadata.AddServiceProperty(
                            typeof(LazyPropertyDelegateEntity).GetAnyProperty("LoaderState")!,
                            typeof(ILazyLoader));

                        serviceProperty.SetParameterBinding(
                            new DependencyInjectionParameterBinding(typeof(object), typeof(ILazyLoader), serviceProperty),
                            ConfigurationSource.Explicit);
                    });
            }
        }

        public class LazyConstructorEntity
        {
            private readonly ILazyLoader _loader;

            public LazyConstructorEntity(ILazyLoader loader)
            {
                _loader = loader;
            }

            public int Id { get; set; }

            public LazyPropertyEntity LazyPropertyEntity { get; set; }
            public LazyPropertyDelegateEntity LazyPropertyDelegateEntity { get; set; }
        }

        public class LazyPropertyEntity
        {
            public ILazyLoader Loader { get; set; }

            public int Id { get; set; }
            public int LazyConstructorEntityId { get; set; }

            public LazyConstructorEntity LazyConstructorEntity { get; set; }
        }

        public class LazyPropertyDelegateEntity
        {
            public object LoaderState { get; set; }
            private Action<object, string> LazyLoader { get; set; }

            public int Id { get; set; }
            public int LazyConstructorEntityId { get; set; }

            public LazyConstructorEntity LazyConstructorEntity { get; set; }
        }

        [ConditionalFact]
        public void Lazy_loading_proxies()
            => Test(
                new LazyLoadingProxiesContext(),
                new CompiledModelCodeGenerationOptions(),
                assertModel: model =>
                {
                    Assert.Equal(
                        typeof(ILazyLoader), model.FindEntityType(typeof(LazyProxiesEntity1))!.GetServiceProperties().Single().ClrType);
                    Assert.Equal(
                        typeof(ILazyLoader), model.FindEntityType(typeof(LazyProxiesEntity1))!.GetServiceProperties().Single().ClrType);
                });

        public class LazyLoadingProxiesContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<LazyProxiesEntity1>();
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => base.OnConfiguring(options.UseLazyLoadingProxies());
        }

        public class LazyProxiesEntity1
        {
            public int Id { get; set; }

            public virtual LazyProxiesEntity2 ReferenceNavigation { get; set; }
        }

        public class LazyProxiesEntity2
        {
            public ILazyLoader Loader { get; set; }

            public int Id { get; set; }
            public virtual ICollection<LazyProxiesEntity1> CollectionNavigation { get; set; }
        }

        [ConditionalFact]
        public void Throws_for_query_filter()
            => Test(
                new QueryFilterContext(),
                new CompiledModelCodeGenerationOptions(),
                expectedExceptionMessage: DesignStrings.CompiledModelQueryFilter("QueryFilter"));

        public class QueryFilterContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity(
                    "QueryFilter", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                        e.HasQueryFilter((Expression<Func<Dictionary<string, object>, bool>>)(e => e != null));
                    });
            }
        }

        [ConditionalFact]
        public void Throws_for_defining_query()
            => Test(
                new DefiningQueryContext(),
                new CompiledModelCodeGenerationOptions(),
                expectedExceptionMessage: DesignStrings.CompiledModelDefiningQuery("object"));

        public class DefiningQueryContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<object>(
                    e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                        e.Metadata.SetInMemoryQuery(() => (IQueryable<object>)Set<object>());
                    });
            }
        }

        [ConditionalFact]
        public void Throws_for_value_generator()
            => Test(
                new ValueGeneratorContext(),
                new CompiledModelCodeGenerationOptions(),
                expectedExceptionMessage: DesignStrings.CompiledModelValueGenerator(
                    "MyEntity", "Id", nameof(PropertyBuilder.HasValueGeneratorFactory)));

        public class ValueGeneratorContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").HasValueGenerator((p, e) => null);
                        e.HasKey("Id");
                    });
            }
        }

        [ConditionalFact]
        public void Custom_value_converter()
            => Test(
                new ValueConverterContext(),
                new CompiledModelCodeGenerationOptions(),
                 model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    var converter = entityType.FindProperty("Id").GetTypeMapping().Converter;
                    Assert.Equal(1, converter.ConvertToProvider(1));
                });

        public class ValueConverterContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").HasConversion(i => i, i => i);
                        e.HasKey("Id");
                    });
            }
        }

        [ConditionalFact]
        public void Custom_value_comparer()
            => Test(
                new ValueComparerContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    Assert.True(
                        entityType.FindProperty("Id").GetValueComparer().SnapshotExpression
                            is Expression<Func<int, int>> lambda
                        && lambda.Body is ConstantExpression constant
                        && ((int)constant.Value) == 1);
                });

        public class ValueComparerContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").HasConversion(typeof(int), new FakeValueComparer());
                        e.HasKey("Id");
                    });
            }
        }

        private class FakeValueComparer : ValueComparer<int>
        {
            public FakeValueComparer()
                : base((l, r) => false, v => 0, v => 1)
            {
            }

            public override Type Type { get; } = typeof(int);

            public override bool Equals(object left, object right)
                => throw new NotImplementedException();

            public override int GetHashCode(object instance)
                => throw new NotImplementedException();

            public override object Snapshot(object instance)
                => throw new NotImplementedException();
        }

        [ConditionalFact]
        public void Custom_provider_value_comparer()
            => Test(
                new ProviderValueComparerContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    Assert.True(
                        entityType.FindProperty("Id").GetProviderValueComparer().SnapshotExpression
                            is Expression<Func<int, int>> lambda
                        && lambda.Body is ConstantExpression constant
                        && ((int)constant.Value) == 1);
                });

        public class ProviderValueComparerContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").HasConversion(typeof(int), null, new FakeValueComparer());
                        e.HasKey("Id");
                    });
            }
        }

        [ConditionalFact]
        public void Custom_type_mapping()
            => Test(
                new TypeMappingContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    var typeMapping = entityType.FindProperty("Id").FindTypeMapping()!;
                    Assert.IsType<InMemoryTypeMapping>(typeMapping);
                    Assert.IsType<JsonInt32ReaderWriter>(typeMapping.JsonValueReaderWriter);
                });

        public class TypeMappingContext : ContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").Metadata.SetTypeMapping(
                            new InMemoryTypeMapping(typeof(int), jsonValueReaderWriter: JsonInt32ReaderWriter.Instance));
                        e.HasKey("Id");
                    });
            }
        }

        [ConditionalFact]
        public void Custom_function_type_mapping()
            => Test(
                new FunctionTypeMappingContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    var function = model.GetDbFunctions().Single();

                    var typeMapping = function.TypeMapping;
                    Assert.IsType<StringTypeMapping>(typeMapping);
                    Assert.Equal("varchar", typeMapping.StoreType);
                });

        public class FunctionTypeMappingContext : SqlServerContextBase
        {
            public static string GetSqlFragmentStatic(string param)
                => throw new NotImplementedException();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDbFunction(typeof(FunctionTypeMappingContext).GetMethod(nameof(GetSqlFragmentStatic)))
                    .Metadata.TypeMapping = new StringTypeMapping("varchar", DbType.AnsiString);
            }
        }

        [ConditionalFact]
        public void Custom_function_parameter_type_mapping()
            => Test(
                new FunctionParameterTypeMappingContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    var function = model.GetDbFunctions().Single();
                    var parameter = function.Parameters.Single();

                    var typeMapping = parameter.TypeMapping;
                    Assert.IsType<StringTypeMapping>(typeMapping);
                    Assert.Equal("varchar", typeMapping.StoreType);
                });

        public class FunctionParameterTypeMappingContext : SqlServerContextBase
        {
            public static string GetSqlFragmentStatic(string param)
                => throw new NotImplementedException();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDbFunction(typeof(FunctionParameterTypeMappingContext).GetMethod(nameof(GetSqlFragmentStatic)))
                    .HasParameter("param").Metadata.TypeMapping = new StringTypeMapping("varchar", DbType.AnsiString);
            }
        }

        [ConditionalFact]
        public void Throws_for_custom_function_translation()
            => Test(
                new FunctionTranslationContext(),
                new CompiledModelCodeGenerationOptions(),
                expectedExceptionMessage: RelationalStrings.CompiledModelFunctionTranslation("GetSqlFragmentStatic"));

        public class FunctionTranslationContext : SqlServerContextBase
        {
            public static string GetSqlFragmentStatic()
                => throw new NotImplementedException();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDbFunction(typeof(FunctionTranslationContext).GetMethod(nameof(GetSqlFragmentStatic)))
                    .HasTranslation(args => new SqlFragmentExpression("NULL"));
            }
        }

        [ConditionalFact]
        public void Fully_qualified_model()
            => Test(
                new TestModel.Internal.DbContext(),
                new CompiledModelCodeGenerationOptions { ModelNamespace = "Internal" },
                model =>
                {
                    Assert.Equal(4, model.GetEntityTypes().Count());
                    Assert.Same(model, model.FindRuntimeAnnotationValue("ReadOnlyModel"));
                },
                typeof(FullyQualifiedDesignTimeServices));

        private class FullyQualifiedDesignTimeServices : IDesignTimeServices
        {
            public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
                => serviceCollection.AddSingleton<ICSharpHelper, FullyQualifiedCSharpHelper>();
        }

        private class FullyQualifiedCSharpHelper : CSharpHelper
        {
            public FullyQualifiedCSharpHelper(ITypeMappingSource typeMappingSource)
                : base(typeMappingSource)
            {
            }

            protected override bool ShouldUseFullName(Type type)
                => base.ShouldUseFullName(type);

            protected override bool ShouldUseFullName(string shortTypeName)
                => base.ShouldUseFullName(shortTypeName) || shortTypeName is nameof(Index) or nameof(Internal);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSqlClr)]
        public void SpatialTypesTest()
            => Test(
                new SpatialTypesContext(),
                new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true },
                model =>
                {
                    var entityType = model.FindEntityType(typeof(SpatialTypes));
                    var pointProperty = entityType.FindProperty("Point");
                    Assert.Equal(typeof(Point), pointProperty.ClrType);
                    Assert.True(pointProperty.IsNullable);
                    Assert.Equal(ValueGenerated.OnAdd, pointProperty.ValueGenerated);
                    Assert.Equal("Point", pointProperty.GetColumnName());
                    Assert.Equal("geometry", pointProperty.GetColumnType());
                    Assert.Equal(0, ((Point)pointProperty.GetDefaultValue()).SRID);
                    Assert.IsType<CastingConverter<Point, Point>>(pointProperty.GetValueConverter());
                    Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetValueComparer());
                    Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetKeyValueComparer());
                    Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetProviderValueComparer());
                    Assert.Equal(SqlServerValueGenerationStrategy.None, pointProperty.GetValueGenerationStrategy());
                    Assert.Null(pointProperty[CoreAnnotationNames.PropertyAccessMode]);
                },
                typeof(SqlServerNetTopologySuiteDesignTimeServices));


        [ConditionalFact]
        [SqlServerConfiguredCondition]
        public void BigModel()
            => Test(
                new BigContext(),
                new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true },
                model =>
                {
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetCollation()).Message);
                    Assert.Equal(
                        new[] { RelationalAnnotationNames.MaxIdentifierLength, SqlServerAnnotationNames.ValueGenerationStrategy },
                        model.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.GetValueGenerationStrategy());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetPropertyAccessMode()).Message);
                    Assert.Null(model[SqlServerAnnotationNames.IdentitySeed]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetIdentitySeed()).Message);
                    Assert.Null(model[SqlServerAnnotationNames.IdentityIncrement]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetIdentityIncrement()).Message);

                    var manyTypesType = model.FindEntityType(typeof(ManyTypes));

                    Assert.Equal(typeof(ManyTypes).FullName, manyTypesType.Name);
                    Assert.False(manyTypesType.HasSharedClrType);
                    Assert.False(manyTypesType.IsPropertyBag);
                    Assert.False(manyTypesType.IsOwned());
                    Assert.IsType<ConstructorBinding>(manyTypesType.ConstructorBinding);
                    Assert.Null(manyTypesType.FindIndexerPropertyInfo());
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, manyTypesType.GetChangeTrackingStrategy());
                    Assert.Equal("ManyTypes", manyTypesType.GetTableName());
                    Assert.Null(manyTypesType.GetSchema());

                    Assert.Null(model.FindEntityType(typeof(AbstractBase)));
                    var principalBase = model.FindEntityType(typeof(PrincipalBase));
                    Assert.Equal(typeof(PrincipalBase).FullName, principalBase.Name);
                    Assert.False(principalBase.HasSharedClrType);
                    Assert.False(principalBase.IsPropertyBag);
                    Assert.False(principalBase.IsOwned());
                    Assert.Null(principalBase.BaseType);
                    Assert.IsType<ConstructorBinding>(principalBase.ConstructorBinding);
                    Assert.Null(principalBase.FindIndexerPropertyInfo());
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, principalBase.GetChangeTrackingStrategy());
                    Assert.Null(principalBase.GetQueryFilter());
                    Assert.Equal("PrincipalBase", principalBase.GetTableName());
                    Assert.Equal("mySchema", principalBase.GetSchema());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalBase.GetSeedData()).Message);

                    var principalId = principalBase.FindProperty(nameof(PrincipalBase.Id));
                    Assert.Equal(
                        new[] { RelationalAnnotationNames.RelationalOverrides, SqlServerAnnotationNames.ValueGenerationStrategy },
                        principalId.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(typeof(long?), principalId.ClrType);
                    Assert.Equal(typeof(long?), principalId.PropertyInfo.PropertyType);
                    Assert.Equal(typeof(long?), principalId.FieldInfo.FieldType);
                    Assert.False(principalId.IsNullable);
                    Assert.Equal(ValueGenerated.OnAdd, principalId.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Throw, principalId.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Save, principalId.GetBeforeSaveBehavior());
                    Assert.Null(principalId[CoreAnnotationNames.BeforeSaveBehavior]);
                    Assert.Null(principalId[CoreAnnotationNames.AfterSaveBehavior]);
                    Assert.Equal("Id", principalId.GetColumnName());
                    Assert.Equal("Id", principalId.GetColumnName(StoreObjectIdentifier.Table("PrincipalBase", "mySchema")));
                    Assert.Equal("DerivedId", principalId.GetColumnName(StoreObjectIdentifier.Table("PrincipalDerived")));
                    Assert.Equal("bigint", principalId.GetColumnType());
                    Assert.Null(principalId.GetValueConverter());
                    Assert.NotNull(principalId.GetValueComparer());
                    Assert.NotNull(principalId.GetKeyValueComparer());
                    Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, principalId.GetValueGenerationStrategy());
                    Assert.Null(principalId[SqlServerAnnotationNames.IdentitySeed]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalId.GetIdentitySeed()).Message);
                    Assert.Null(principalId[SqlServerAnnotationNames.IdentityIncrement]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalId.GetIdentityIncrement()).Message);

                    Assert.Null(principalBase.FindDiscriminatorProperty());

                    var principalAlternateId = principalBase.FindProperty(nameof(PrincipalBase.AlternateId));
                    var compositeIndex = principalBase.GetIndexes().Single();
                    Assert.Equal(PropertyAccessMode.FieldDuringConstruction, principalAlternateId.GetPropertyAccessMode());
                    Assert.Empty(compositeIndex.GetAnnotations());
                    Assert.Equal(new[] { principalAlternateId, principalId }, compositeIndex.Properties);
                    Assert.False(compositeIndex.IsUnique);
                    Assert.Null(compositeIndex.Name);
                    Assert.Equal("IX_PrincipalBase_AlternateId_Id", compositeIndex.GetDatabaseName());

                    Assert.Equal(new[] { compositeIndex }, principalAlternateId.GetContainingIndexes());

                    Assert.Equal(2, principalBase.GetKeys().Count());

                    var principalAlternateKey = principalBase.GetKeys().First();
                    Assert.Same(principalId, principalAlternateKey.Properties.Single());
                    Assert.False(principalAlternateKey.IsPrimaryKey());
                    Assert.Equal("AK_PrincipalBase_Id", principalAlternateKey.GetName());

                    var principalKey = principalBase.GetKeys().Last();
                    Assert.Equal(
                        new[] { RelationalAnnotationNames.Name },
                        principalKey.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(new[] { principalId, principalAlternateId }, principalKey.Properties);
                    Assert.True(principalKey.IsPrimaryKey());
                    Assert.Equal("PK", principalKey.GetName());
                    Assert.Null(principalKey[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalKey.IsClustered()).Message);

                    Assert.Equal(new[] { principalAlternateKey, principalKey }, principalId.GetContainingKeys());

                    var referenceOwnedNavigation = principalBase.GetNavigations().Single();
                    Assert.Equal(
                        new[] { CoreAnnotationNames.EagerLoaded },
                        referenceOwnedNavigation.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(nameof(PrincipalBase.Owned), referenceOwnedNavigation.Name);
                    Assert.False(referenceOwnedNavigation.IsCollection);
                    Assert.True(referenceOwnedNavigation.IsEagerLoaded);
                    Assert.False(referenceOwnedNavigation.IsOnDependent);
                    Assert.Equal(typeof(OwnedType), referenceOwnedNavigation.ClrType);
                    Assert.Equal("_ownedField", referenceOwnedNavigation.FieldInfo.Name);
                    Assert.Equal(nameof(PrincipalBase.Owned), referenceOwnedNavigation.PropertyInfo.Name);
                    Assert.Null(referenceOwnedNavigation.Inverse);
                    Assert.Equal(principalBase, referenceOwnedNavigation.DeclaringEntityType);
                    Assert.Equal(PropertyAccessMode.Field, referenceOwnedNavigation.GetPropertyAccessMode());
                    Assert.Null(referenceOwnedNavigation[CoreAnnotationNames.PropertyAccessMode]);

                    var referenceOwnedType = referenceOwnedNavigation.TargetEntityType;
                    Assert.Equal(typeof(PrincipalBase).FullName + ".Owned#OwnedType", referenceOwnedType.Name);
                    Assert.Equal(typeof(OwnedType), referenceOwnedType.ClrType);
                    Assert.True(referenceOwnedType.HasSharedClrType);
                    Assert.False(referenceOwnedType.IsPropertyBag);
                    Assert.True(referenceOwnedType.IsOwned());
                    Assert.Null(referenceOwnedType.BaseType);
                    Assert.False(referenceOwnedType.IsMemoryOptimized());
                    Assert.IsType<ConstructorBinding>(referenceOwnedType.ConstructorBinding);
                    Assert.Null(referenceOwnedType.FindIndexerPropertyInfo());
                    Assert.Equal(
                        ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues,
                        referenceOwnedType.GetChangeTrackingStrategy());
                    Assert.Null(referenceOwnedType.GetQueryFilter());
                    Assert.Null(referenceOwnedType[CoreAnnotationNames.PropertyAccessMode]);
                    Assert.Null(referenceOwnedType[CoreAnnotationNames.NavigationAccessMode]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => referenceOwnedType.GetPropertyAccessMode()).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => referenceOwnedType.GetNavigationAccessMode()).Message);

                    var principalTable = StoreObjectIdentifier.Create(referenceOwnedType, StoreObjectType.Table).Value;

                    var ownedId = referenceOwnedType.FindProperty("PrincipalBaseId");
                    Assert.True(ownedId.IsPrimaryKey());
                    Assert.Equal(
                        SqlServerValueGenerationStrategy.IdentityColumn,
                        principalId.GetValueGenerationStrategy(principalTable));
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalId.GetIdentityIncrement(principalTable)).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalId.GetIdentitySeed(principalTable)).Message);

                    var detailsProperty = referenceOwnedType.FindProperty(nameof(OwnedType.Details));
                    Assert.Null(detailsProperty[SqlServerAnnotationNames.Sparse]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.IsSparse()).Message);
                    Assert.Null(detailsProperty[RelationalAnnotationNames.Collation]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.GetCollation()).Message);

                    var ownedFragment = referenceOwnedType.GetMappingFragments().Single();
                    Assert.Equal(nameof(OwnedType.Details), detailsProperty.GetColumnName(ownedFragment.StoreObject));
                    Assert.Null(detailsProperty.GetColumnName(principalTable));

                    var referenceOwnership = referenceOwnedNavigation.ForeignKey;
                    Assert.Empty(referenceOwnership.GetAnnotations());
                    Assert.Same(referenceOwnership, referenceOwnedType.FindOwnership());
                    Assert.True(referenceOwnership.IsOwnership);
                    Assert.True(referenceOwnership.IsRequired);
                    Assert.True(referenceOwnership.IsRequiredDependent);
                    Assert.True(referenceOwnership.IsUnique);
                    Assert.Null(referenceOwnership.DependentToPrincipal);
                    Assert.Same(referenceOwnedNavigation, referenceOwnership.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.Cascade, referenceOwnership.DeleteBehavior);
                    Assert.Equal(2, referenceOwnership.Properties.Count());
                    Assert.Same(principalKey, referenceOwnership.PrincipalKey);

                    var ownedServiceProperty = referenceOwnedType.GetServiceProperties().Single();
                    Assert.Empty(ownedServiceProperty.GetAnnotations());
                    Assert.Equal(typeof(DbContext), ownedServiceProperty.ClrType);
                    Assert.Equal(typeof(DbContext), ownedServiceProperty.PropertyInfo.PropertyType);
                    Assert.Null(ownedServiceProperty.FieldInfo);
                    Assert.Same(referenceOwnedType, ownedServiceProperty.DeclaringEntityType);
                    var ownedServicePropertyBinding = ownedServiceProperty.ParameterBinding;
                    Assert.IsType<ContextParameterBinding>(ownedServicePropertyBinding);
                    Assert.Equal(typeof(DbContext), ownedServicePropertyBinding.ServiceType);
                    Assert.Equal(ownedServiceProperty, ownedServicePropertyBinding.ConsumedProperties.Single());
                    Assert.Equal(PropertyAccessMode.PreferField, ownedServiceProperty.GetPropertyAccessMode());
                    Assert.Null(ownedServiceProperty[CoreAnnotationNames.PropertyAccessMode]);

                    var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>));
                    Assert.Equal(principalBase, principalDerived.BaseType);
                    Assert.Equal(
                        "Microsoft.EntityFrameworkCore.Scaffolding.Internal.CSharpRuntimeModelCodeGeneratorTest+"
                        + "PrincipalDerived<Microsoft.EntityFrameworkCore.Scaffolding.Internal.CSharpRuntimeModelCodeGeneratorTest+DependentBase<byte?>>",
                        principalDerived.Name);
                    Assert.False(principalDerived.IsOwned());
                    Assert.IsType<ConstructorBinding>(principalDerived.ConstructorBinding);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, principalDerived.GetChangeTrackingStrategy());
                    Assert.Equal("PrincipalDerived<DependentBase<byte?>>", principalDerived.GetDiscriminatorValue());

                    var tptForeignKey = principalDerived.GetForeignKeys().Single();
                    Assert.False(tptForeignKey.IsOwnership);
                    Assert.True(tptForeignKey.IsRequired);
                    Assert.False(tptForeignKey.IsRequiredDependent);
                    Assert.True(tptForeignKey.IsUnique);
                    Assert.Null(tptForeignKey.DependentToPrincipal);
                    Assert.Null(tptForeignKey.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.Cascade, tptForeignKey.DeleteBehavior);
                    Assert.Equal(principalKey.Properties, tptForeignKey.Properties);
                    Assert.Same(principalKey, tptForeignKey.PrincipalKey);

                    Assert.Equal(2, principalDerived.GetDeclaredNavigations().Count());
                    var dependentNavigation = principalDerived.GetDeclaredNavigations().First();
                    Assert.Equal("Dependent", dependentNavigation.Name);
                    Assert.Equal("Dependent", dependentNavigation.PropertyInfo.Name);
                    Assert.Equal("<Dependent>k__BackingField", dependentNavigation.FieldInfo.Name);
                    Assert.False(dependentNavigation.IsCollection);
                    Assert.True(dependentNavigation.IsEagerLoaded);
                    Assert.False(dependentNavigation.LazyLoadingEnabled);
                    Assert.False(dependentNavigation.IsOnDependent);
                    Assert.Equal(principalDerived, dependentNavigation.DeclaringEntityType);
                    Assert.Equal("Principal", dependentNavigation.Inverse.Name);

                    var ownedCollectionNavigation = principalDerived.GetDeclaredNavigations().Last();
                    Assert.Equal("ManyOwned", ownedCollectionNavigation.Name);
                    Assert.Null(ownedCollectionNavigation.PropertyInfo);
                    Assert.Equal("ManyOwned", ownedCollectionNavigation.FieldInfo.Name);
                    Assert.Equal(typeof(ICollection<OwnedType>), ownedCollectionNavigation.ClrType);
                    Assert.True(ownedCollectionNavigation.IsCollection);
                    Assert.True(ownedCollectionNavigation.IsEagerLoaded);
                    Assert.False(ownedCollectionNavigation.IsOnDependent);
                    Assert.Null(ownedCollectionNavigation.Inverse);
                    Assert.Equal(principalDerived, ownedCollectionNavigation.DeclaringEntityType);

                    var collectionOwnedType = ownedCollectionNavigation.TargetEntityType;
                    Assert.Equal(principalDerived.Name + ".ManyOwned#OwnedType", collectionOwnedType.Name);
                    Assert.Equal(typeof(OwnedType), collectionOwnedType.ClrType);
                    Assert.True(collectionOwnedType.HasSharedClrType);
                    Assert.False(collectionOwnedType.IsPropertyBag);
                    Assert.True(collectionOwnedType.IsOwned());
                    Assert.True(collectionOwnedType.IsMemoryOptimized());
                    Assert.Null(collectionOwnedType[RelationalAnnotationNames.IsTableExcludedFromMigrations]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => collectionOwnedType.IsTableExcludedFromMigrations()).Message);
                    Assert.Null(collectionOwnedType.BaseType);
                    Assert.IsType<ConstructorBinding>(collectionOwnedType.ConstructorBinding);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, collectionOwnedType.GetChangeTrackingStrategy());

                    var collectionOwnership = ownedCollectionNavigation.ForeignKey;
                    Assert.Same(collectionOwnership, collectionOwnedType.FindOwnership());
                    Assert.True(collectionOwnership.IsOwnership);
                    Assert.True(collectionOwnership.IsRequired);
                    Assert.False(collectionOwnership.IsRequiredDependent);
                    Assert.False(collectionOwnership.IsUnique);
                    Assert.Null(collectionOwnership.DependentToPrincipal);
                    Assert.Same(ownedCollectionNavigation, collectionOwnership.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.Cascade, collectionOwnership.DeleteBehavior);
                    Assert.Equal(2, collectionOwnership.Properties.Count());

                    var derivedSkipNavigation = principalDerived.GetDeclaredSkipNavigations().Single();
                    Assert.Equal("Principals", derivedSkipNavigation.Name);
                    Assert.Equal("Principals", derivedSkipNavigation.PropertyInfo.Name);
                    Assert.Equal("<Principals>k__BackingField", derivedSkipNavigation.FieldInfo.Name);
                    Assert.Equal(typeof(ICollection<PrincipalBase>), derivedSkipNavigation.ClrType);
                    Assert.True(derivedSkipNavigation.IsCollection);
                    Assert.True(derivedSkipNavigation.IsEagerLoaded);
                    Assert.False(derivedSkipNavigation.LazyLoadingEnabled);
                    Assert.False(derivedSkipNavigation.IsOnDependent);
                    Assert.Equal(principalDerived, derivedSkipNavigation.DeclaringEntityType);
                    Assert.Equal("Deriveds", derivedSkipNavigation.Inverse.Name);
                    Assert.Same(principalBase.GetSkipNavigations().Single(), derivedSkipNavigation.Inverse);

                    Assert.Same(derivedSkipNavigation, derivedSkipNavigation.ForeignKey.GetReferencingSkipNavigations().Single());
                    Assert.Same(
                        derivedSkipNavigation.Inverse, derivedSkipNavigation.Inverse.ForeignKey.GetReferencingSkipNavigations().Single());

                    Assert.Equal(new[] { derivedSkipNavigation.Inverse, derivedSkipNavigation }, principalDerived.GetSkipNavigations());

                    var joinType = derivedSkipNavigation.JoinEntityType;

                    Assert.Equal("PrincipalBasePrincipalDerived<DependentBase<byte?>>", joinType.Name);
                    Assert.Equal(typeof(Dictionary<string, object>), joinType.ClrType);
                    Assert.True(joinType.HasSharedClrType);
                    Assert.True(joinType.IsPropertyBag);
                    Assert.False(joinType.IsOwned());
                    Assert.Null(joinType.BaseType);
                    Assert.IsType<ConstructorBinding>(joinType.ConstructorBinding);
                    Assert.Equal("Item", joinType.FindIndexerPropertyInfo().Name);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, joinType.GetChangeTrackingStrategy());
                    Assert.Null(joinType[RelationalAnnotationNames.Comment]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => joinType.GetComment()).Message);
                    Assert.Null(joinType.GetQueryFilter());

                    var rowid = joinType.GetProperties().Single(p => !p.IsForeignKey());
                    Assert.Equal(typeof(byte[]), rowid.ClrType);
                    Assert.True(rowid.IsIndexerProperty());
                    Assert.Same(joinType.FindIndexerPropertyInfo(), rowid.PropertyInfo);
                    Assert.Null(rowid.FieldInfo);
                    Assert.True(rowid.IsNullable);
                    Assert.False(rowid.IsShadowProperty());
                    Assert.True(rowid.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.OnAddOrUpdate, rowid.ValueGenerated);
                    Assert.Equal("rowid", rowid.GetColumnName());
                    Assert.Equal("rowversion", rowid.GetColumnType());
                    Assert.Null(rowid[RelationalAnnotationNames.Comment]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => rowid.GetComment()).Message);
                    Assert.Null(rowid[RelationalAnnotationNames.ColumnOrder]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => rowid.GetColumnOrder()).Message);
                    Assert.Null(rowid.GetValueConverter());
                    Assert.NotNull(rowid.GetValueComparer());
                    Assert.NotNull(rowid.GetKeyValueComparer());
                    Assert.Equal(SqlServerValueGenerationStrategy.None, rowid.GetValueGenerationStrategy());

                    var dependentForeignKey = dependentNavigation.ForeignKey;
                    Assert.False(dependentForeignKey.IsOwnership);
                    Assert.True(dependentForeignKey.IsRequired);
                    Assert.False(dependentForeignKey.IsRequiredDependent);
                    Assert.True(dependentForeignKey.IsUnique);
                    Assert.Same(dependentNavigation.Inverse, dependentForeignKey.DependentToPrincipal);
                    Assert.Same(dependentNavigation, dependentForeignKey.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.ClientNoAction, dependentForeignKey.DeleteBehavior);
                    Assert.Equal(new[] { "PrincipalId", "PrincipalAlternateId" }, dependentForeignKey.Properties.Select(p => p.Name));
                    Assert.Same(principalKey, dependentForeignKey.PrincipalKey);

                    var dependentBase = dependentNavigation.TargetEntityType;

                    Assert.False(dependentBase.GetIsDiscriminatorMappingComplete());
                    var principalDiscriminator = dependentBase.FindDiscriminatorProperty();
                    Assert.IsType<DiscriminatorValueGenerator>(
                        principalDiscriminator.GetValueGeneratorFactory()(principalDiscriminator, dependentBase));
                    Assert.Equal(Enum1.One, dependentBase.GetDiscriminatorValue());

                    var dependentBaseForeignKey = dependentBase.GetForeignKeys().Single(fk => fk != dependentForeignKey);
                    var dependentForeignKeyProperty = dependentBaseForeignKey.Properties.Single();

                    Assert.Equal(
                        new[] { dependentBaseForeignKey, dependentForeignKey }, dependentForeignKeyProperty.GetContainingForeignKeys());

                    var dependentDerived = dependentBase.GetDerivedTypes().Single();
                    Assert.Equal(Enum1.Two, dependentDerived.GetDiscriminatorValue());

                    Assert.Equal(2, dependentDerived.GetDeclaredProperties().Count());

                    var dependentData = dependentDerived.GetDeclaredProperties().First();
                    Assert.Equal(typeof(string), dependentData.ClrType);
                    Assert.Equal("Data", dependentData.Name);
                    Assert.Equal("Data", dependentData.PropertyInfo.Name);
                    Assert.Equal("<Data>k__BackingField", dependentData.FieldInfo.Name);
                    Assert.True(dependentData.IsNullable);
                    Assert.False(dependentData.IsShadowProperty());
                    Assert.False(dependentData.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, dependentData.ValueGenerated);
                    Assert.Equal("Data", dependentData.GetColumnName());
                    Assert.Equal("char(20)", dependentData.GetColumnType());
                    Assert.Equal(20, dependentData.GetMaxLength());
                    Assert.False(dependentData.IsUnicode());
                    Assert.True(dependentData.IsFixedLength());
                    Assert.Null(dependentData.GetPrecision());
                    Assert.Null(dependentData.GetScale());

                    var dependentMoney = dependentDerived.GetDeclaredProperties().Last();
                    Assert.Equal(typeof(decimal), dependentMoney.ClrType);
                    Assert.Equal("Money", dependentMoney.Name);
                    Assert.Null(dependentMoney.PropertyInfo);
                    Assert.Null(dependentMoney.FieldInfo);
                    Assert.False(dependentMoney.IsNullable);
                    Assert.True(dependentMoney.IsShadowProperty());
                    Assert.False(dependentMoney.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, dependentMoney.ValueGenerated);
                    Assert.Equal("Money", dependentMoney.GetColumnName());
                    Assert.Equal("decimal(9,3)", dependentMoney.GetColumnType());
                    Assert.Null(dependentMoney.GetMaxLength());
                    Assert.Null(dependentMoney.IsUnicode());
                    Assert.Null(dependentMoney.IsFixedLength());
                    Assert.Equal(9, dependentMoney.GetPrecision());
                    Assert.Equal(3, dependentMoney.GetScale());

                    Assert.Equal(
                        new[]
                        {
                            derivedSkipNavigation.ForeignKey,
                            tptForeignKey,
                            referenceOwnership,
                            collectionOwnership,
                            dependentForeignKey,
                            derivedSkipNavigation.Inverse.ForeignKey
                        },
                        principalKey.GetReferencingForeignKeys());

                    Assert.Equal(
                        new[] { dependentBaseForeignKey, tptForeignKey, referenceOwnership, derivedSkipNavigation.Inverse.ForeignKey },
                        principalBase.GetReferencingForeignKeys());

                    Assert.Equal(
                        new[] { derivedSkipNavigation.ForeignKey, collectionOwnership, dependentForeignKey },
                        principalDerived.GetDeclaredReferencingForeignKeys());

                    Assert.Equal(
                        new[]
                        {
                            dependentBase,
                            dependentDerived,
                            manyTypesType,
                            principalBase,
                            referenceOwnedType,
                            principalDerived,
                            collectionOwnedType,
                            joinType
                        },
                        model.GetEntityTypes());
                },
                typeof(SqlServerNetTopologySuiteDesignTimeServices),
                c =>
                {
                    c.Set<PrincipalDerived<DependentBase<byte?>>>().Add(
                        new PrincipalDerived<DependentBase<byte?>>
                        {
                            AlternateId = new Guid(),
                            Dependent = new DependentBase<byte?>(1),
                            Owned = new OwnedType(c)
                        });

                    c.SaveChanges();
                });

        [ConditionalFact]
        [SqlServerConfiguredCondition]
        public void BigModel_with_JSON_columns()
            => Test(
                new BigContextWithJson(),
                new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true },
                model =>
                {
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetCollation()).Message);
                    Assert.Equal(
                        new[] { RelationalAnnotationNames.MaxIdentifierLength, SqlServerAnnotationNames.ValueGenerationStrategy },
                        model.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.GetValueGenerationStrategy());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetPropertyAccessMode()).Message);
                    Assert.Null(model[SqlServerAnnotationNames.IdentitySeed]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetIdentitySeed()).Message);
                    Assert.Null(model[SqlServerAnnotationNames.IdentityIncrement]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetIdentityIncrement()).Message);

                    var manyTypesType = model.FindEntityType(typeof(ManyTypes));

                    Assert.Equal(typeof(ManyTypes).FullName, manyTypesType.Name);
                    Assert.False(manyTypesType.HasSharedClrType);
                    Assert.False(manyTypesType.IsPropertyBag);
                    Assert.False(manyTypesType.IsOwned());
                    Assert.IsType<ConstructorBinding>(manyTypesType.ConstructorBinding);
                    Assert.Null(manyTypesType.FindIndexerPropertyInfo());
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, manyTypesType.GetChangeTrackingStrategy());
                    Assert.Equal("ManyTypes", manyTypesType.GetTableName());
                    Assert.Null(manyTypesType.GetSchema());

                    Assert.Null(model.FindEntityType(typeof(AbstractBase)));
                    var principalBase = model.FindEntityType(typeof(PrincipalBase));
                    Assert.Equal(typeof(PrincipalBase).FullName, principalBase.Name);
                    Assert.False(principalBase.HasSharedClrType);
                    Assert.False(principalBase.IsPropertyBag);
                    Assert.False(principalBase.IsOwned());
                    Assert.Null(principalBase.BaseType);
                    Assert.IsType<ConstructorBinding>(principalBase.ConstructorBinding);
                    Assert.Null(principalBase.FindIndexerPropertyInfo());
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, principalBase.GetChangeTrackingStrategy());
                    Assert.Null(principalBase.GetQueryFilter());
                    Assert.Equal("PrincipalBase", principalBase.GetTableName());
                    Assert.Null(principalBase.GetSchema());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalBase.GetSeedData()).Message);

                    var principalId = principalBase.FindProperty(nameof(PrincipalBase.Id));
                    Assert.Equal(
                        new[] { SqlServerAnnotationNames.ValueGenerationStrategy },
                        principalId.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(typeof(long?), principalId.ClrType);
                    Assert.Equal(typeof(long?), principalId.PropertyInfo.PropertyType);
                    Assert.Equal(typeof(long?), principalId.FieldInfo.FieldType);
                    Assert.False(principalId.IsNullable);
                    Assert.Equal(ValueGenerated.Never, principalId.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Throw, principalId.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Save, principalId.GetBeforeSaveBehavior());
                    Assert.Null(principalId[CoreAnnotationNames.BeforeSaveBehavior]);
                    Assert.Null(principalId[CoreAnnotationNames.AfterSaveBehavior]);
                    Assert.Equal("Id", principalId.GetColumnName());
                    Assert.Equal("bigint", principalId.GetColumnType());
                    Assert.Null(principalId.GetValueConverter());
                    Assert.NotNull(principalId.GetValueComparer());
                    Assert.NotNull(principalId.GetKeyValueComparer());
                    Assert.Equal(SqlServerValueGenerationStrategy.None, principalId.GetValueGenerationStrategy());
                    Assert.Null(principalId[SqlServerAnnotationNames.IdentitySeed]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalId.GetIdentitySeed()).Message);
                    Assert.Null(principalId[SqlServerAnnotationNames.IdentityIncrement]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalId.GetIdentityIncrement()).Message);

                    var discriminatorProperty = principalBase.FindDiscriminatorProperty();
                    Assert.Equal("Discriminator", discriminatorProperty.Name);
                    Assert.Equal(typeof(string), discriminatorProperty.ClrType);

                    var principalAlternateId = principalBase.FindProperty(nameof(PrincipalBase.AlternateId));
                    var compositeIndex = principalBase.GetIndexes().Single();
                    Assert.Equal(PropertyAccessMode.FieldDuringConstruction, principalAlternateId.GetPropertyAccessMode());
                    Assert.Empty(compositeIndex.GetAnnotations());
                    Assert.Equal(new[] { principalAlternateId, principalId }, compositeIndex.Properties);
                    Assert.False(compositeIndex.IsUnique);
                    Assert.Null(compositeIndex.Name);
                    Assert.Equal("IX_PrincipalBase_AlternateId_Id", compositeIndex.GetDatabaseName());

                    Assert.Equal(new[] { compositeIndex }, principalAlternateId.GetContainingIndexes());

                    Assert.Equal(2, principalBase.GetKeys().Count());

                    var principalAlternateKey = principalBase.GetKeys().First();
                    Assert.Same(principalId, principalAlternateKey.Properties.Single());
                    Assert.False(principalAlternateKey.IsPrimaryKey());
                    Assert.Equal("AK_PrincipalBase_Id", principalAlternateKey.GetName());

                    var principalKey = principalBase.GetKeys().Last();
                    Assert.Equal(
                        new[] { RelationalAnnotationNames.Name },
                        principalKey.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(new[] { principalId, principalAlternateId }, principalKey.Properties);
                    Assert.True(principalKey.IsPrimaryKey());
                    Assert.Equal("PK", principalKey.GetName());
                    Assert.Null(principalKey[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalKey.IsClustered()).Message);

                    Assert.Equal(new[] { principalAlternateKey, principalKey }, principalId.GetContainingKeys());

                    var referenceOwnedNavigation = principalBase.GetNavigations().Single();
                    Assert.Equal(
                        new[] { CoreAnnotationNames.EagerLoaded },
                        referenceOwnedNavigation.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(nameof(PrincipalBase.Owned), referenceOwnedNavigation.Name);
                    Assert.False(referenceOwnedNavigation.IsCollection);
                    Assert.True(referenceOwnedNavigation.IsEagerLoaded);
                    Assert.False(referenceOwnedNavigation.IsOnDependent);
                    Assert.Equal(typeof(OwnedType), referenceOwnedNavigation.ClrType);
                    Assert.Equal("_ownedField", referenceOwnedNavigation.FieldInfo.Name);
                    Assert.Equal(nameof(PrincipalBase.Owned), referenceOwnedNavigation.PropertyInfo.Name);
                    Assert.Null(referenceOwnedNavigation.Inverse);
                    Assert.Equal(principalBase, referenceOwnedNavigation.DeclaringEntityType);
                    Assert.Equal(PropertyAccessMode.Field, referenceOwnedNavigation.GetPropertyAccessMode());
                    Assert.Null(referenceOwnedNavigation[CoreAnnotationNames.PropertyAccessMode]);

                    var referenceOwnedType = referenceOwnedNavigation.TargetEntityType;
                    Assert.Equal(typeof(PrincipalBase).FullName + ".Owned#OwnedType", referenceOwnedType.Name);
                    Assert.Equal(typeof(OwnedType), referenceOwnedType.ClrType);
                    Assert.True(referenceOwnedType.HasSharedClrType);
                    Assert.False(referenceOwnedType.IsPropertyBag);
                    Assert.True(referenceOwnedType.IsOwned());
                    Assert.Null(referenceOwnedType.BaseType);
                    Assert.False(referenceOwnedType.IsMemoryOptimized());
                    Assert.IsType<ConstructorBinding>(referenceOwnedType.ConstructorBinding);
                    Assert.Null(referenceOwnedType.FindIndexerPropertyInfo());
                    Assert.Equal(
                        ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues,
                        referenceOwnedType.GetChangeTrackingStrategy());
                    Assert.Null(referenceOwnedType.GetQueryFilter());
                    Assert.Null(referenceOwnedType[CoreAnnotationNames.PropertyAccessMode]);
                    Assert.Null(referenceOwnedType[CoreAnnotationNames.NavigationAccessMode]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => referenceOwnedType.GetPropertyAccessMode()).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => referenceOwnedType.GetNavigationAccessMode()).Message);

                    var principalTable = StoreObjectIdentifier.Create(referenceOwnedType, StoreObjectType.Table).Value;

                    var ownedId = referenceOwnedType.FindProperty("PrincipalBaseId");
                    Assert.True(ownedId.IsPrimaryKey());
                    Assert.Equal(
                        SqlServerValueGenerationStrategy.None,
                        principalId.GetValueGenerationStrategy(principalTable));
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalId.GetIdentityIncrement(principalTable)).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalId.GetIdentitySeed(principalTable)).Message);

                    var detailsProperty = referenceOwnedType.FindProperty(nameof(OwnedType.Details));
                    Assert.Null(detailsProperty[SqlServerAnnotationNames.Sparse]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.IsSparse()).Message);
                    Assert.Null(detailsProperty[RelationalAnnotationNames.Collation]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.GetCollation()).Message);

                    Assert.Null(detailsProperty.GetColumnName(principalTable));

                    var referenceOwnership = referenceOwnedNavigation.ForeignKey;
                    Assert.Empty(referenceOwnership.GetAnnotations());
                    Assert.Same(referenceOwnership, referenceOwnedType.FindOwnership());
                    Assert.True(referenceOwnership.IsOwnership);
                    Assert.True(referenceOwnership.IsRequired);
                    Assert.True(referenceOwnership.IsRequiredDependent);
                    Assert.True(referenceOwnership.IsUnique);
                    Assert.Null(referenceOwnership.DependentToPrincipal);
                    Assert.Same(referenceOwnedNavigation, referenceOwnership.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.Cascade, referenceOwnership.DeleteBehavior);
                    Assert.Equal(2, referenceOwnership.Properties.Count());
                    Assert.Same(principalKey, referenceOwnership.PrincipalKey);

                    var ownedServiceProperty = referenceOwnedType.GetServiceProperties().Single();
                    Assert.Empty(ownedServiceProperty.GetAnnotations());
                    Assert.Equal(typeof(DbContext), ownedServiceProperty.ClrType);
                    Assert.Equal(typeof(DbContext), ownedServiceProperty.PropertyInfo.PropertyType);
                    Assert.Null(ownedServiceProperty.FieldInfo);
                    Assert.Same(referenceOwnedType, ownedServiceProperty.DeclaringEntityType);
                    var ownedServicePropertyBinding = ownedServiceProperty.ParameterBinding;
                    Assert.IsType<ContextParameterBinding>(ownedServicePropertyBinding);
                    Assert.Equal(typeof(DbContext), ownedServicePropertyBinding.ServiceType);
                    Assert.Equal(ownedServiceProperty, ownedServicePropertyBinding.ConsumedProperties.Single());
                    Assert.Equal(PropertyAccessMode.PreferField, ownedServiceProperty.GetPropertyAccessMode());
                    Assert.Null(ownedServiceProperty[CoreAnnotationNames.PropertyAccessMode]);

                    var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>));
                    Assert.Equal(principalBase, principalDerived.BaseType);
                    Assert.Equal(
                        "Microsoft.EntityFrameworkCore.Scaffolding.Internal.CSharpRuntimeModelCodeGeneratorTest+"
                        + "PrincipalDerived<Microsoft.EntityFrameworkCore.Scaffolding.Internal.CSharpRuntimeModelCodeGeneratorTest+DependentBase<byte?>>",
                        principalDerived.Name);
                    Assert.False(principalDerived.IsOwned());
                    Assert.IsType<ConstructorBinding>(principalDerived.ConstructorBinding);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, principalDerived.GetChangeTrackingStrategy());
                    Assert.Equal("PrincipalDerived<DependentBase<byte?>>", principalDerived.GetDiscriminatorValue());

                    Assert.Equal(2, principalDerived.GetDeclaredNavigations().Count());
                    var dependentNavigation = principalDerived.GetDeclaredNavigations().First();
                    Assert.Equal("Dependent", dependentNavigation.Name);
                    Assert.Equal("Dependent", dependentNavigation.PropertyInfo.Name);
                    Assert.Equal("<Dependent>k__BackingField", dependentNavigation.FieldInfo.Name);
                    Assert.False(dependentNavigation.IsCollection);
                    Assert.True(dependentNavigation.IsEagerLoaded);
                    Assert.False(dependentNavigation.LazyLoadingEnabled);
                    Assert.False(dependentNavigation.IsOnDependent);
                    Assert.Equal(principalDerived, dependentNavigation.DeclaringEntityType);
                    Assert.Equal("Principal", dependentNavigation.Inverse.Name);

                    var ownedCollectionNavigation = principalDerived.GetDeclaredNavigations().Last();
                    Assert.Equal("ManyOwned", ownedCollectionNavigation.Name);
                    Assert.Null(ownedCollectionNavigation.PropertyInfo);
                    Assert.Equal("ManyOwned", ownedCollectionNavigation.FieldInfo.Name);
                    Assert.Equal(typeof(ICollection<OwnedType>), ownedCollectionNavigation.ClrType);
                    Assert.True(ownedCollectionNavigation.IsCollection);
                    Assert.True(ownedCollectionNavigation.IsEagerLoaded);
                    Assert.False(ownedCollectionNavigation.IsOnDependent);
                    Assert.Null(ownedCollectionNavigation.Inverse);
                    Assert.Equal(principalDerived, ownedCollectionNavigation.DeclaringEntityType);

                    var collectionOwnedType = ownedCollectionNavigation.TargetEntityType;
                    Assert.Equal(principalDerived.Name + ".ManyOwned#OwnedType", collectionOwnedType.Name);
                    Assert.Equal(typeof(OwnedType), collectionOwnedType.ClrType);
                    Assert.True(collectionOwnedType.HasSharedClrType);
                    Assert.False(collectionOwnedType.IsPropertyBag);
                    Assert.True(collectionOwnedType.IsOwned());
                    Assert.False(collectionOwnedType.IsMemoryOptimized());
                    Assert.Null(collectionOwnedType[RelationalAnnotationNames.IsTableExcludedFromMigrations]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => collectionOwnedType.IsTableExcludedFromMigrations()).Message);
                    Assert.Null(collectionOwnedType.BaseType);
                    Assert.IsType<ConstructorBinding>(collectionOwnedType.ConstructorBinding);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, collectionOwnedType.GetChangeTrackingStrategy());

                    var collectionOwnership = ownedCollectionNavigation.ForeignKey;
                    Assert.Same(collectionOwnership, collectionOwnedType.FindOwnership());
                    Assert.True(collectionOwnership.IsOwnership);
                    Assert.True(collectionOwnership.IsRequired);
                    Assert.False(collectionOwnership.IsRequiredDependent);
                    Assert.False(collectionOwnership.IsUnique);
                    Assert.Null(collectionOwnership.DependentToPrincipal);
                    Assert.Same(ownedCollectionNavigation, collectionOwnership.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.Cascade, collectionOwnership.DeleteBehavior);
                    Assert.Equal(2, collectionOwnership.Properties.Count());

                    var derivedSkipNavigation = principalDerived.GetDeclaredSkipNavigations().Single();
                    Assert.Equal("Principals", derivedSkipNavigation.Name);
                    Assert.Equal("Principals", derivedSkipNavigation.PropertyInfo.Name);
                    Assert.Equal("<Principals>k__BackingField", derivedSkipNavigation.FieldInfo.Name);
                    Assert.Equal(typeof(ICollection<PrincipalBase>), derivedSkipNavigation.ClrType);
                    Assert.True(derivedSkipNavigation.IsCollection);
                    Assert.True(derivedSkipNavigation.IsEagerLoaded);
                    Assert.False(derivedSkipNavigation.LazyLoadingEnabled);
                    Assert.False(derivedSkipNavigation.IsOnDependent);
                    Assert.Equal(principalDerived, derivedSkipNavigation.DeclaringEntityType);
                    Assert.Equal("Deriveds", derivedSkipNavigation.Inverse.Name);
                    Assert.Same(principalBase.GetSkipNavigations().Single(), derivedSkipNavigation.Inverse);

                    Assert.Same(derivedSkipNavigation, derivedSkipNavigation.ForeignKey.GetReferencingSkipNavigations().Single());
                    Assert.Same(
                        derivedSkipNavigation.Inverse, derivedSkipNavigation.Inverse.ForeignKey.GetReferencingSkipNavigations().Single());

                    Assert.Equal(new[] { derivedSkipNavigation.Inverse, derivedSkipNavigation }, principalDerived.GetSkipNavigations());

                    var joinType = derivedSkipNavigation.JoinEntityType;

                    Assert.Equal("PrincipalBasePrincipalDerived<DependentBase<byte?>>", joinType.Name);
                    Assert.Equal(typeof(Dictionary<string, object>), joinType.ClrType);
                    Assert.True(joinType.HasSharedClrType);
                    Assert.True(joinType.IsPropertyBag);
                    Assert.False(joinType.IsOwned());
                    Assert.Null(joinType.BaseType);
                    Assert.IsType<ConstructorBinding>(joinType.ConstructorBinding);
                    Assert.Equal("Item", joinType.FindIndexerPropertyInfo().Name);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, joinType.GetChangeTrackingStrategy());
                    Assert.Null(joinType[RelationalAnnotationNames.Comment]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => joinType.GetComment()).Message);
                    Assert.Null(joinType.GetQueryFilter());

                    var rowid = joinType.GetProperties().Single(p => !p.IsForeignKey());
                    Assert.Equal(typeof(byte[]), rowid.ClrType);
                    Assert.True(rowid.IsIndexerProperty());
                    Assert.Same(joinType.FindIndexerPropertyInfo(), rowid.PropertyInfo);
                    Assert.Null(rowid.FieldInfo);
                    Assert.True(rowid.IsNullable);
                    Assert.False(rowid.IsShadowProperty());
                    Assert.True(rowid.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.OnAddOrUpdate, rowid.ValueGenerated);
                    Assert.Equal("rowid", rowid.GetColumnName());
                    Assert.Equal("rowversion", rowid.GetColumnType());
                    Assert.Null(rowid[RelationalAnnotationNames.Comment]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => rowid.GetComment()).Message);
                    Assert.Null(rowid[RelationalAnnotationNames.ColumnOrder]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => rowid.GetColumnOrder()).Message);
                    Assert.Null(rowid.GetValueConverter());
                    Assert.NotNull(rowid.GetValueComparer());
                    Assert.NotNull(rowid.GetKeyValueComparer());
                    Assert.Equal(SqlServerValueGenerationStrategy.None, rowid.GetValueGenerationStrategy());

                    var dependentForeignKey = dependentNavigation.ForeignKey;
                    Assert.False(dependentForeignKey.IsOwnership);
                    Assert.True(dependentForeignKey.IsRequired);
                    Assert.False(dependentForeignKey.IsRequiredDependent);
                    Assert.True(dependentForeignKey.IsUnique);
                    Assert.Same(dependentNavigation.Inverse, dependentForeignKey.DependentToPrincipal);
                    Assert.Same(dependentNavigation, dependentForeignKey.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.ClientNoAction, dependentForeignKey.DeleteBehavior);
                    Assert.Equal(new[] { "PrincipalId", "PrincipalAlternateId" }, dependentForeignKey.Properties.Select(p => p.Name));
                    Assert.Same(principalKey, dependentForeignKey.PrincipalKey);

                    var dependentBase = dependentNavigation.TargetEntityType;

                    Assert.False(dependentBase.GetIsDiscriminatorMappingComplete());
                    var principalDiscriminator = dependentBase.FindDiscriminatorProperty();
                    Assert.IsType<DiscriminatorValueGenerator>(
                        principalDiscriminator.GetValueGeneratorFactory()(principalDiscriminator, dependentBase));
                    Assert.Equal(Enum1.One, dependentBase.GetDiscriminatorValue());

                    var dependentBaseForeignKey = dependentBase.GetForeignKeys().Single(fk => fk != dependentForeignKey);
                    var dependentForeignKeyProperty = dependentBaseForeignKey.Properties.Single();

                    Assert.Equal(
                        new[] { dependentBaseForeignKey, dependentForeignKey }, dependentForeignKeyProperty.GetContainingForeignKeys());

                    var dependentDerived = dependentBase.GetDerivedTypes().Single();
                    Assert.Equal(Enum1.Two, dependentDerived.GetDiscriminatorValue());

                    Assert.Equal(2, dependentDerived.GetDeclaredProperties().Count());

                    var dependentData = dependentDerived.GetDeclaredProperties().First();
                    Assert.Equal(typeof(string), dependentData.ClrType);
                    Assert.Equal("Data", dependentData.Name);
                    Assert.Equal("Data", dependentData.PropertyInfo.Name);
                    Assert.Equal("<Data>k__BackingField", dependentData.FieldInfo.Name);
                    Assert.True(dependentData.IsNullable);
                    Assert.False(dependentData.IsShadowProperty());
                    Assert.False(dependentData.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, dependentData.ValueGenerated);
                    Assert.Equal("Data", dependentData.GetColumnName());
                    Assert.Equal("char(20)", dependentData.GetColumnType());
                    Assert.Equal(20, dependentData.GetMaxLength());
                    Assert.False(dependentData.IsUnicode());
                    Assert.True(dependentData.IsFixedLength());
                    Assert.Null(dependentData.GetPrecision());
                    Assert.Null(dependentData.GetScale());

                    var dependentMoney = dependentDerived.GetDeclaredProperties().Last();
                    Assert.Equal(typeof(decimal), dependentMoney.ClrType);
                    Assert.Equal("Money", dependentMoney.Name);
                    Assert.Null(dependentMoney.PropertyInfo);
                    Assert.Null(dependentMoney.FieldInfo);
                    Assert.False(dependentMoney.IsNullable);
                    Assert.True(dependentMoney.IsShadowProperty());
                    Assert.False(dependentMoney.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, dependentMoney.ValueGenerated);
                    Assert.Equal("Money", dependentMoney.GetColumnName());
                    Assert.Equal("decimal(9,3)", dependentMoney.GetColumnType());
                    Assert.Null(dependentMoney.GetMaxLength());
                    Assert.Null(dependentMoney.IsUnicode());
                    Assert.Null(dependentMoney.IsFixedLength());
                    Assert.Equal(9, dependentMoney.GetPrecision());
                    Assert.Equal(3, dependentMoney.GetScale());

                    Assert.Equal(
                        new[]
                        {
                            derivedSkipNavigation.ForeignKey,
                            referenceOwnership,
                            collectionOwnership,
                            dependentForeignKey,
                            derivedSkipNavigation.Inverse.ForeignKey
                        },
                        principalKey.GetReferencingForeignKeys());

                    Assert.Equal(
                        new[] { dependentBaseForeignKey, referenceOwnership, derivedSkipNavigation.Inverse.ForeignKey },
                        principalBase.GetReferencingForeignKeys());

                    Assert.Equal(
                        new[] { derivedSkipNavigation.ForeignKey, collectionOwnership, dependentForeignKey },
                        principalDerived.GetDeclaredReferencingForeignKeys());

                    Assert.Equal(
                        new[]
                        {
                            dependentBase,
                            dependentDerived,
                            manyTypesType,
                            principalBase,
                            referenceOwnedType,
                            principalDerived,
                            collectionOwnedType,
                            joinType
                        },
                        model.GetEntityTypes());
                },
                typeof(SqlServerNetTopologySuiteDesignTimeServices),
                c =>
                {
                    c.Set<PrincipalDerived<DependentBase<byte?>>>().Add(
                        new PrincipalDerived<DependentBase<byte?>>
                        {
                            Id = 1,
                            AlternateId = new Guid(),
                            Dependent = new DependentBase<byte?>(1),
                            Owned = new OwnedType(c)
                        });

                    c.SaveChanges();
                });

        [ConditionalFact]
        [SqlServerConfiguredCondition]
        public void ComplexTypes()
            => Test(
                new ComplexTypesContext(),
                new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true },
                model =>
                {
                    var principalBase = model.FindEntityType(typeof(PrincipalBase));

                    var complexProperty = principalBase.GetComplexProperties().Single();
                    Assert.Equal(
                        new[] { "goo" },
                        complexProperty.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(nameof(PrincipalBase.Owned), complexProperty.Name);
                    Assert.False(complexProperty.IsCollection);
                    Assert.False(complexProperty.IsNullable);
                    Assert.Equal(typeof(OwnedType), complexProperty.ClrType);
                    Assert.Equal("_ownedField", complexProperty.FieldInfo.Name);
                    Assert.Equal(nameof(PrincipalBase.Owned), complexProperty.PropertyInfo.Name);
                    Assert.Equal(principalBase, complexProperty.DeclaringType);
                    Assert.Equal(PropertyAccessMode.Field, complexProperty.GetPropertyAccessMode());
                    Assert.Equal("ber", complexProperty["goo"]);

                    var complexType = complexProperty.ComplexType;
                    Assert.Equal(
                        new[]
                        {
                            RelationalAnnotationNames.FunctionName,
                            RelationalAnnotationNames.Schema,
                            RelationalAnnotationNames.SqlQuery,
                            RelationalAnnotationNames.TableName,
                            RelationalAnnotationNames.ViewName,
                            RelationalAnnotationNames.ViewSchema,
                            "go"
                        },
                        complexType.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(typeof(PrincipalBase).FullName + ".Owned#OwnedType", complexType.Name);
                    Assert.Equal(typeof(OwnedType), complexType.ClrType);
                    Assert.True(complexType.HasSharedClrType);
                    Assert.False(complexType.IsPropertyBag);
                    Assert.IsType<ConstructorBinding>(complexType.ConstructorBinding);
                    Assert.Null(complexType.FindIndexerPropertyInfo());
                    Assert.Equal(
                        ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues,
                        complexType.GetChangeTrackingStrategy());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => complexType.GetPropertyAccessMode()).Message);
                    Assert.Equal("brr", complexType["go"]);

                    var detailsProperty = complexType.FindProperty(nameof(OwnedType.Details));
                    Assert.Equal(
                        new[]
                        {
                            CoreAnnotationNames.MaxLength,
                            CoreAnnotationNames.Precision,
                            RelationalAnnotationNames.ColumnName,
                            RelationalAnnotationNames.ColumnType,
                            CoreAnnotationNames.Scale,
                            SqlServerAnnotationNames.ValueGenerationStrategy,
                            CoreAnnotationNames.Unicode,
                            "foo"
                        },
                        detailsProperty.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(typeof(string), detailsProperty.ClrType);
                    Assert.Equal(typeof(string), detailsProperty.PropertyInfo.PropertyType);
                    Assert.Equal(typeof(string), detailsProperty.FieldInfo.FieldType);
                    Assert.Equal("_details", detailsProperty.FieldInfo.Name);
                    Assert.True(detailsProperty.IsNullable);
                    Assert.Equal(ValueGenerated.OnAddOrUpdate, detailsProperty.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Ignore, detailsProperty.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Ignore, detailsProperty.GetBeforeSaveBehavior());
                    Assert.Equal("Deets", detailsProperty.GetColumnName());
                    Assert.Equal("varchar(64)", detailsProperty.GetColumnType());
                    Assert.False(detailsProperty.IsUnicode());
                    Assert.True(detailsProperty.IsConcurrencyToken);
                    Assert.Equal(64, detailsProperty.GetMaxLength());
                    Assert.Null(detailsProperty.IsFixedLength());
                    Assert.Equal(3, detailsProperty.GetPrecision());
                    Assert.Equal(2, detailsProperty.GetScale());
                    Assert.Equal("", detailsProperty.Sentinel);
                    Assert.Equal(PropertyAccessMode.FieldDuringConstruction, detailsProperty.GetPropertyAccessMode());
                    Assert.Null(detailsProperty.GetValueConverter());
                    Assert.NotNull(detailsProperty.GetValueComparer());
                    Assert.NotNull(detailsProperty.GetKeyValueComparer());
                    Assert.Equal(SqlServerValueGenerationStrategy.None, detailsProperty.GetValueGenerationStrategy());
                    Assert.Null(detailsProperty.GetDefaultValueSql());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.GetIdentitySeed()).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.GetIdentityIncrement()).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.IsSparse()).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.GetCollation()).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.GetComment()).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => detailsProperty.GetColumnOrder()).Message);

                    var nestedComplexType = complexType.FindComplexProperty(nameof(OwnedType.Principal)).ComplexType;

                    Assert.Equal(14, nestedComplexType.GetProperties().Count());

                    var principalTable = StoreObjectIdentifier.Create(complexType, StoreObjectType.Table).Value;

                    Assert.Equal("Deets", detailsProperty.GetColumnName(principalTable));

                    var dbFunction = model.FindDbFunction("PrincipalBaseTvf");
                    Assert.Equal("dbo", dbFunction.Schema);
                    Assert.False(dbFunction.IsNullable);
                    Assert.False(dbFunction.IsScalar);
                    Assert.False(dbFunction.IsBuiltIn);
                    Assert.False(dbFunction.IsAggregate);
                    Assert.Null(dbFunction.Translation);
                    Assert.Null(dbFunction.TypeMapping);
                    Assert.Equal(typeof(IQueryable<PrincipalBase>), dbFunction.ReturnType);
                    Assert.Null(dbFunction.MethodInfo);
                    Assert.Empty(dbFunction.GetAnnotations());
                    Assert.Empty(dbFunction.GetRuntimeAnnotations());
                    Assert.Equal("PrincipalBaseTvf", dbFunction.StoreFunction.Name);
                    Assert.False(dbFunction.StoreFunction.IsShared);
                    Assert.NotNull(dbFunction.ToString());
                    Assert.Empty(dbFunction.Parameters);

                    var principalBaseFunctionMapping = principalBase.GetFunctionMappings().Single(m => m.IsDefaultFunctionMapping);
                    Assert.True(principalBaseFunctionMapping.IncludesDerivedTypes);
                    Assert.Null(principalBaseFunctionMapping.IsSharedTablePrincipal);
                    Assert.Null(principalBaseFunctionMapping.IsSplitEntityTypePrincipal);
                    Assert.Same(dbFunction, principalBaseFunctionMapping.DbFunction);

                    var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>));
                    Assert.Equal(principalBase, principalDerived.BaseType);

                    Assert.Equal(
                        new[] { principalBase, principalDerived },
                        model.GetEntityTypes());
                },
                null,
                c =>
                {
                    c.Set<PrincipalDerived<DependentBase<byte?>>>().Add(
                        new PrincipalDerived<DependentBase<byte?>>
                        {
                            Id = 1,
                            AlternateId = new Guid(),
                            Dependent = new DependentBase<byte?>(1),
                            Owned = new OwnedType(c) { Principal = new PrincipalBase() }
                        });

                    //c.SaveChanges();
                });

        public class BigContextWithJson : BigContext
        {
            public BigContextWithJson()
                : base(jsonColumns: true)
            {
            }
        }

        public class BigContext : SqlServerContextBase
        {
            private readonly bool _jsonColumns;

            public BigContext(bool jsonColumns = false)
            {
                _jsonColumns = jsonColumns;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder
                    .UseCollation("Latin1_General_CS_AS")
                    .UseIdentityColumns(3, 2);

                modelBuilder.Entity<PrincipalBase>(
                    eb =>
                    {
                        if (!_jsonColumns)
                        {
                            eb.Property(e => e.Id).UseIdentityColumn(2, 3)
                                .Metadata.SetColumnName("DerivedId", StoreObjectIdentifier.Table("PrincipalDerived"));
                        }

                        eb.Property(e => e.FlagsEnum2)
                            .HasSentinel(AFlagsEnum.C | AFlagsEnum.B);

                        eb.Property(e => e.AlternateId)
                            .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);

                        eb.HasIndex(e => new { e.AlternateId, e.Id });

                        eb.HasKey(e => new { e.Id, e.AlternateId })
                            .HasName("PK")
                            .IsClustered();

                        eb.HasAlternateKey(e => e.Id);

                        eb.Property(e => e.AlternateId).Metadata.SetJsonValueReaderWriterType(
                            _jsonColumns
                                ? typeof(MyJsonGuidReaderWriter)
                                : typeof(JsonGuidReaderWriter));

                        eb.OwnsOne(
                            e => e.Owned, ob =>
                            {
                                ob.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
                                ob.UsePropertyAccessMode(PropertyAccessMode.Field);
                                ob.Property(e => e.Details)
                                    .IsSparse()
                                    .UseCollation("Latin1_General_CI_AI");

                                if (_jsonColumns)
                                {
                                    ob.ToJson();
                                }
                                else
                                {
                                    ob.ToTable(
                                        "PrincipalBase", "mySchema",
                                        t => t.Property("PrincipalBaseId").UseIdentityColumn(2, 3));

                                    ob.SplitToTable("Details", s => s.Property(e => e.Details));

                                    ob.HasData(
                                        new
                                        {
                                            Number = 10,
                                            PrincipalBaseId = 1L,
                                            PrincipalBaseAlternateId = new Guid()
                                        });
                                }
                            });

                        eb.Navigation(e => e.Owned).IsRequired().HasField("_ownedField")
                            .UsePropertyAccessMode(PropertyAccessMode.Field);

                        if (!_jsonColumns)
                        {
                            eb.HasData(new PrincipalBase { Id = 1, AlternateId = new Guid() });

                            eb.ToTable("PrincipalBase", "mySchema");
                        }
                    });

                modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
                    eb =>
                    {
                        eb.HasOne(e => e.Dependent).WithOne(e => e.Principal)
                            .HasForeignKey<DependentBase<byte?>>()
                            .OnDelete(DeleteBehavior.ClientNoAction);

                        eb.Navigation(e => e.Dependent).AutoInclude().EnableLazyLoading(false);

                        eb.OwnsMany(
                            typeof(OwnedType).FullName, "ManyOwned", ob =>
                            {
                                if (_jsonColumns)
                                {
                                    ob.ToJson();
                                }
                                else
                                {
                                    ob.ToTable("ManyOwned", t => t.IsMemoryOptimized().ExcludeFromMigrations());
                                }
                            });

                        eb.HasMany(e => e.Principals).WithMany(e => (ICollection<PrincipalDerived<DependentBase<byte?>>>)e.Deriveds)
                            .UsingEntity(
                                jb =>
                                {
                                    jb.ToTable(tb => tb.HasComment("Join table"));
                                    jb.Property<byte[]>("rowid")
                                        .IsRowVersion()
                                        .HasComment("RowVersion")
                                        .HasColumnOrder(1);
                                });

                        eb.Navigation(e => e.Principals).AutoInclude().EnableLazyLoading(false);

                        if (!_jsonColumns)
                        {
                            eb.ToTable("PrincipalDerived");
                        }
                    });

                modelBuilder.Entity<DependentBase<byte?>>(
                    eb =>
                    {
                        eb.Property<byte?>("Id");

                        eb.HasKey("PrincipalId", "PrincipalAlternateId");

                        eb.HasOne<PrincipalBase>().WithOne()
                            .HasForeignKey<DependentBase<byte?>>("PrincipalId")
                            .HasPrincipalKey<PrincipalBase>(e => e.Id);

                        eb.HasDiscriminator<Enum1>("EnumDiscriminator")
                            .HasValue(Enum1.One)
                            .HasValue<DependentDerived<byte?>>(Enum1.Two)
                            .IsComplete(false);
                    });

                modelBuilder.Entity<DependentDerived<byte?>>(
                    eb =>
                    {
                        eb.Property<string>("Data")
                            .HasMaxLength(20)
                            .IsFixedLength()
                            .IsUnicode(false);

                        eb.Property<decimal>("Money")
                            .HasPrecision(9, 3);
                    });

                modelBuilder.Entity<ManyTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion<ManyTypesIdConverter>().ValueGeneratedOnAdd();
                        b.HasKey(e => e.Id);

                        b.Property(e => e.Enum8AsString).HasConversion<string>();
                        b.Property(e => e.Enum16AsString).HasConversion<string>();
                        b.Property(e => e.Enum32AsString).HasConversion<string>();
                        b.Property(e => e.Enum64AsString).HasConversion<string>();
                        b.Property(e => e.EnumU8AsString).HasConversion<string>();
                        b.Property(e => e.EnumU16AsString).HasConversion<string>();
                        b.Property(e => e.EnumU32AsString).HasConversion<string>();
                        b.Property(e => e.EnumU64AsString).HasConversion<string>();

                        b.PrimitiveCollection(e => e.Enum8AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum16AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum32AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum64AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU8AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU16AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU32AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU64AsStringCollection).ElementType(b => b.HasConversion<string>());

                        b.PrimitiveCollection(e => e.Enum8AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum16AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum32AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum64AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU8AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU16AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU32AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU64AsStringArray).ElementType(b => b.HasConversion<string>());

                        b.Property(e => e.BoolToStringConverterProperty).HasConversion(new BoolToStringConverter("A", "B"));
                        b.Property(e => e.BoolToTwoValuesConverterProperty).HasConversion(new BoolToTwoValuesConverter<byte>(0, 1));
                        b.Property(e => e.BoolToZeroOneConverterProperty).HasConversion<BoolToZeroOneConverter<short>>();
                        b.Property(e => e.BytesToStringConverterProperty).HasConversion<BytesToStringConverter>();
                        b.Property(e => e.CastingConverterProperty).HasConversion<CastingConverter<int, decimal>>();
                        b.Property(e => e.CharToStringConverterProperty).HasConversion<CharToStringConverter>();
                        b.Property(e => e.DateOnlyToStringConverterProperty).HasConversion<DateOnlyToStringConverter>();
                        b.Property(e => e.DateTimeOffsetToBinaryConverterProperty).HasConversion<DateTimeOffsetToBinaryConverter>();
                        b.Property(e => e.DateTimeOffsetToBytesConverterProperty).HasConversion<DateTimeOffsetToBytesConverter>();
                        b.Property(e => e.DateTimeOffsetToStringConverterProperty).HasConversion<DateTimeOffsetToStringConverter>();
                        b.Property(e => e.DateTimeToBinaryConverterProperty).HasConversion<DateTimeToBinaryConverter>();
                        b.Property(e => e.DateTimeToStringConverterProperty).HasConversion<DateTimeToStringConverter>();
                        b.Property(e => e.EnumToNumberConverterProperty).HasConversion<EnumToNumberConverter<Enum32, int>>();
                        b.Property(e => e.EnumToStringConverterProperty).HasConversion<EnumToStringConverter<Enum32>>();
                        b.Property(e => e.GuidToBytesConverterProperty).HasConversion<GuidToBytesConverter>();
                        b.Property(e => e.GuidToStringConverterProperty).HasConversion<GuidToStringConverter>();
                        b.Property(e => e.IPAddressToBytesConverterProperty).HasConversion<IPAddressToBytesConverter>();
                        b.Property(e => e.IPAddressToStringConverterProperty).HasConversion<IPAddressToStringConverter>();
                        b.Property(e => e.IntNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<int>>();
                        b.Property(e => e.DecimalNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<decimal>>();
                        b.Property(e => e.DoubleNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<double>>();
                        b.Property(e => e.IntNumberToStringConverterProperty).HasConversion<NumberToStringConverter<int>>();
                        b.Property(e => e.DecimalNumberToStringConverterProperty).HasConversion<NumberToStringConverter<decimal>>();
                        b.Property(e => e.DoubleNumberToStringConverterProperty).HasConversion<NumberToStringConverter<double>>();
                        b.Property(e => e.PhysicalAddressToBytesConverterProperty).HasConversion<PhysicalAddressToBytesConverter>();
                        b.Property(e => e.PhysicalAddressToStringConverterProperty).HasConversion<PhysicalAddressToStringConverter>();
                        b.Property(e => e.StringToBoolConverterProperty).HasConversion<StringToBoolConverter>();
                        b.Property(e => e.StringToBytesConverterProperty).HasConversion(new StringToBytesConverter(Encoding.UTF32));
                        b.Property(e => e.StringToCharConverterProperty).HasConversion<StringToCharConverter>();
                        b.Property(e => e.StringToDateOnlyConverterProperty).HasConversion<StringToDateOnlyConverter>();
                        b.Property(e => e.StringToDateTimeConverterProperty).HasConversion<StringToDateTimeConverter>();
                        b.Property(e => e.StringToDateTimeOffsetConverterProperty).HasConversion<StringToDateTimeOffsetConverter>();
                        b.Property(e => e.StringToEnumConverterProperty).HasConversion<StringToEnumConverter<EnumU32>>();
                        b.Property(e => e.StringToIntNumberConverterProperty).HasConversion<StringToNumberConverter<int>>();
                        b.Property(e => e.StringToDecimalNumberConverterProperty).HasConversion<StringToNumberConverter<decimal>>();
                        b.Property(e => e.StringToDoubleNumberConverterProperty).HasConversion<StringToNumberConverter<double>>();
                        b.Property(e => e.StringToTimeOnlyConverterProperty).HasConversion<StringToTimeOnlyConverter>();
                        b.Property(e => e.StringToTimeSpanConverterProperty).HasConversion<StringToTimeSpanConverter>();
                        b.Property(e => e.StringToUriConverterProperty).HasConversion<StringToUriConverter>();
                        b.Property(e => e.TimeOnlyToStringConverterProperty).HasConversion<TimeOnlyToStringConverter>();
                        b.Property(e => e.TimeOnlyToTicksConverterProperty).HasConversion<TimeOnlyToTicksConverter>();
                        b.Property(e => e.TimeSpanToStringConverterProperty).HasConversion<TimeSpanToStringConverter>();
                        b.Property(e => e.TimeSpanToTicksConverterProperty).HasConversion<TimeSpanToTicksConverter>();
                        b.Property(e => e.UriToStringConverterProperty).HasConversion<UriToStringConverter>();
                        b.Property(e => e.NullIntToNullStringConverterProperty).HasConversion<NullIntToNullStringConverter>();
                    });
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
            {
                SqlServerTestStore.Create("RuntimeModelTest" + GetType().Name).AddProviderOptions(options);
                new SqlServerDbContextOptionsBuilder(options).UseNetTopologySuite();
            }
        }

        public class SpatialTypesContext : SqlServerContextBase
        {
            private readonly bool _jsonColumns;

            public SpatialTypesContext(bool jsonColumns = false)
            {
                _jsonColumns = jsonColumns;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<SpatialTypes>(
                    eb =>
                    {
                        eb.Property<Point>("Point")
                            .HasColumnType("geometry")
                            .HasDefaultValue(
                                NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0).CreatePoint(new CoordinateZM(0, 0, 0, 0)))
                            .HasConversion<CastingConverter<Point, Point>, CustomValueComparer<Point>, CustomValueComparer<Point>>();
                    });
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
            {
                SqlServerTestStore.Create("RuntimeModelTest" + GetType().Name).AddProviderOptions(options);
                new SqlServerDbContextOptionsBuilder(options).UseNetTopologySuite();
            }
        }

        public class ComplexTypesContext : SqlServerContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<PrincipalBase>(
                    eb =>
                    {
                        eb.ComplexProperty(
                            e => e.Owned, eb =>
                            {
                                eb.IsRequired()
                                    .HasField("_ownedField")
                                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                                    .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
                                    .HasPropertyAnnotation("goo", "ber")
                                    .HasTypeAnnotation("go", "brr");
                                eb.Property(c => c.Details)
                                    .HasColumnName("Deets")
                                    .HasColumnOrder(1)
                                    .HasColumnType("varchar")
                                    .IsUnicode(false)
                                    .IsRequired(false)
                                    .HasField("_details")
                                    .HasSentinel("")
                                    .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                                    .IsSparse()
                                    .UseCollation("Latin1_General_CI_AI")
                                    .HasMaxLength(64)
                                    .HasPrecision(3, 2)
                                    .HasComment("Dt")
                                    .IsRowVersion()
                                    .HasAnnotation("foo", "bar");
                                eb.Ignore(e => e.Context);
                                eb.ComplexProperty(o => o.Principal).IsRequired();
                            });

                        eb.ToTable("PrincipalBase");
                        eb.ToView("PrincipalBaseView");
                        eb.ToSqlQuery("select * from PrincipalBase");
                        eb.ToFunction("PrincipalBaseTvf");

                        eb.InsertUsingStoredProcedure(
                            s => s
                                .HasParameter("PrincipalBaseId")
                                .HasParameter("Enum1")
                                .HasParameter("Enum2")
                                .HasParameter("FlagsEnum1")
                                .HasParameter("FlagsEnum2")
                                .HasParameter("ValueTypeList")
                                .HasParameter("ValueTypeIList")
                                .HasParameter("ValueTypeArray")
                                .HasParameter("ValueTypeEnumerable")
                                .HasParameter("RefTypeList")
                                .HasParameter("RefTypeIList")
                                .HasParameter("RefTypeArray")
                                .HasParameter("RefTypeEnumerable")
                                .HasParameter("Discriminator")
                                .HasParameter(p => p.Id, p => p.IsOutput()));
                        eb.UpdateUsingStoredProcedure(
                            s => s
                                .HasParameter("PrincipalBaseId")
                                .HasParameter("Enum1")
                                .HasParameter("Enum2")
                                .HasParameter("FlagsEnum1")
                                .HasParameter("FlagsEnum2")
                                .HasParameter("ValueTypeList")
                                .HasParameter("ValueTypeIList")
                                .HasParameter("ValueTypeArray")
                                .HasParameter("ValueTypeEnumerable")
                                .HasParameter("RefTypeList")
                                .HasParameter("RefTypeIList")
                                .HasParameter("RefTypeArray")
                                .HasParameter("RefTypeEnumerable")
                                .HasOriginalValueParameter(p => p.Id));
                        eb.DeleteUsingStoredProcedure(
                            s => s
                                .HasRowsAffectedReturnValue()
                                .HasOriginalValueParameter(p => p.Id));
                    });

                modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
                    eb =>
                    {
                        //eb.ComplexCollection(typeof(OwnedType).Name, "ManyOwned");
                        eb.Ignore(p => p.Dependent);
                        eb.Ignore(p => p.Principals);
                        eb.ToTable("PrincipalBase");
                        eb.ToFunction((string)null);
                    });
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => SqlServerTestStore.Create("RuntimeModelTest" + GetType().Name).AddProviderOptions(options);
        }

        [ConditionalFact]
        public void TPC_model()
            => Test(
                new TpcContext(),
                new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true },
                model =>
                {
                    Assert.Equal("TPC", model.GetDefaultSchema());
                    Assert.Null(model[SqlServerAnnotationNames.MaxDatabaseSize]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetDatabaseMaxSize()).Message);
                    Assert.Null(model[SqlServerAnnotationNames.PerformanceLevelSql]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetPerformanceLevelSql()).Message);
                    Assert.Null(model[SqlServerAnnotationNames.ServiceTierSql]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetServiceTierSql()).Message);

                    var principalBase = model.FindEntityType(typeof(PrincipalBase));
                    var id = principalBase.FindProperty("Id");

                    Assert.Equal("Id", id.GetColumnName());
                    Assert.Equal("PrincipalBase", principalBase.GetTableName());
                    Assert.Equal("TPC", principalBase.GetSchema());
                    Assert.Equal("Id", id.GetColumnName(StoreObjectIdentifier.Create(principalBase, StoreObjectType.Table).Value));
                    Assert.Null(id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.Table).Value));

                    Assert.Equal("PrincipalBaseView", principalBase.GetViewName());
                    Assert.Equal("TPC", principalBase.GetViewSchema());
                    Assert.Equal("Id", id.GetColumnName(StoreObjectIdentifier.Create(principalBase, StoreObjectType.View).Value));
                    Assert.Equal(
                        "bar2",
                        id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.View).Value)["foo"]);

                    var principalBaseId = principalBase.FindProperty("PrincipalBaseId");

                    var alternateIndex = principalBase.GetIndexes().Last();
                    Assert.Same(principalBaseId, alternateIndex.Properties.Single());
                    Assert.True(alternateIndex.IsUnique);
                    Assert.Equal("PrincipalIndex", alternateIndex.Name);
                    Assert.Equal("PIX", alternateIndex.GetDatabaseName());
                    Assert.Null(alternateIndex[RelationalAnnotationNames.Filter]);
                    Assert.Null(alternateIndex.GetFilter());
                    Assert.Null(alternateIndex[SqlServerAnnotationNames.Clustered]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => alternateIndex.IsClustered()).Message);
                    Assert.Null(alternateIndex[SqlServerAnnotationNames.CreatedOnline]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => alternateIndex.IsCreatedOnline()).Message);
                    Assert.Null(alternateIndex[SqlServerAnnotationNames.FillFactor]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => alternateIndex.GetFillFactor()).Message);
                    Assert.Null(alternateIndex[SqlServerAnnotationNames.Include]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => alternateIndex.GetIncludeProperties()).Message);
                    Assert.Null(alternateIndex[SqlServerAnnotationNames.SortInTempDb]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => alternateIndex.GetSortInTempDb()).Message);
                    Assert.Null(alternateIndex[SqlServerAnnotationNames.DataCompression]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => alternateIndex.GetDataCompression()).Message);

                    Assert.Equal(new[] { alternateIndex }, principalBaseId.GetContainingIndexes());

                    var insertSproc = principalBase.GetInsertStoredProcedure()!;
                    Assert.Equal("PrincipalBase_Insert", insertSproc.Name);
                    Assert.Equal("TPC", insertSproc.Schema);
                    Assert.Equal(
                        new[]
                        {
                            "PrincipalBaseId",
                            "PrincipalDerivedId",
                            "Enum1",
                            "Enum2",
                            "FlagsEnum1",
                            "FlagsEnum2",
                            "ValueTypeList",
                            "ValueTypeIList",
                            "ValueTypeArray",
                            "ValueTypeEnumerable",
                            "RefTypeList",
                            "RefTypeIList",
                            "RefTypeArray",
                            "RefTypeEnumerable",
                            "Id"
                        },
                        insertSproc.Parameters.Select(p => p.PropertyName));
                    Assert.Empty(insertSproc.ResultColumns);
                    Assert.False(insertSproc.IsRowsAffectedReturned);
                    Assert.Equal("bar1", insertSproc["foo"]);
                    Assert.Same(principalBase, insertSproc.EntityType);
                    Assert.Equal("BaseId", insertSproc.Parameters.Last().Name);
                    Assert.Equal("bar", insertSproc.Parameters.Last()["foo"]);
                    Assert.Null(id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.InsertStoredProcedure).Value));

                    var updateSproc = principalBase.GetUpdateStoredProcedure()!;
                    Assert.Equal("PrincipalBase_Update", updateSproc.Name);
                    Assert.Equal("TPC", updateSproc.Schema);
                    Assert.Equal(
                        new[]
                        {
                            "PrincipalBaseId",
                            "PrincipalDerivedId",
                            "Enum1",
                            "Enum2",
                            "FlagsEnum1",
                            "FlagsEnum2",
                            "ValueTypeList",
                            "ValueTypeIList",
                            "ValueTypeArray",
                            "ValueTypeEnumerable",
                            "RefTypeList",
                            "RefTypeIList",
                            "RefTypeArray",
                            "RefTypeEnumerable",
                            "Id"
                        },
                        updateSproc.Parameters.Select(p => p.PropertyName));
                    Assert.Empty(updateSproc.ResultColumns);
                    Assert.False(updateSproc.IsRowsAffectedReturned);
                    Assert.Empty(updateSproc.GetAnnotations());
                    Assert.Same(principalBase, updateSproc.EntityType);
                    Assert.Equal("Id_Original", updateSproc.Parameters.Last().Name);
                    Assert.Null(id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.UpdateStoredProcedure).Value));

                    var deleteSproc = principalBase.GetDeleteStoredProcedure()!;
                    Assert.Equal("PrincipalBase_Delete", deleteSproc.Name);
                    Assert.Equal("TPC", deleteSproc.Schema);
                    Assert.Equal(new[] { "Id_Original" }, deleteSproc.Parameters.Select(p => p.Name));
                    Assert.Empty(deleteSproc.ResultColumns);
                    Assert.True(deleteSproc.IsRowsAffectedReturned);
                    Assert.Same(principalBase, deleteSproc.EntityType);
                    Assert.Equal("Id_Original", deleteSproc.Parameters.Last().Name);
                    Assert.Null(id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.DeleteStoredProcedure).Value));

                    Assert.Equal("PrincipalBase", principalBase.GetDiscriminatorValue());
                    Assert.Null(principalBase.FindDiscriminatorProperty());
                    Assert.Equal("TPC", principalBase.GetMappingStrategy());

                    var selfRefNavigation = principalBase.GetDeclaredNavigations().Last();
                    Assert.Equal("Deriveds", selfRefNavigation.Name);
                    Assert.True(selfRefNavigation.IsCollection);
                    Assert.False(selfRefNavigation.IsOnDependent);
                    Assert.Equal(principalBase, selfRefNavigation.TargetEntityType);
                    Assert.Null(selfRefNavigation.Inverse);

                    var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>));
                    Assert.Equal(principalBase, principalDerived.BaseType);

                    Assert.Equal("PrincipalDerived", principalDerived.GetTableName());
                    Assert.Equal("TPC", principalDerived.GetSchema());
                    Assert.Equal("PrincipalDerivedView", principalDerived.GetViewName());
                    Assert.Equal("TPC", principalBase.GetViewSchema());

                    insertSproc = principalDerived.GetInsertStoredProcedure()!;
                    Assert.Equal("Derived_Insert", insertSproc.Name);
                    Assert.Equal("TPC", insertSproc.Schema);
                    Assert.Equal(
                        new[]
                        {
                            "PrincipalBaseId",
                            "PrincipalDerivedId",
                            "Enum1",
                            "Enum2",
                            "FlagsEnum1",
                            "FlagsEnum2",
                            "ValueTypeList",
                            "ValueTypeIList",
                            "ValueTypeArray",
                            "ValueTypeEnumerable",
                            "RefTypeList",
                            "RefTypeIList",
                            "RefTypeArray",
                            "RefTypeEnumerable"
                        },
                        insertSproc.Parameters.Select(p => p.PropertyName));
                    Assert.Equal(new[] { "Id" }, insertSproc.ResultColumns.Select(p => p.PropertyName));
                    Assert.Null(insertSproc["foo"]);
                    Assert.Same(principalDerived, insertSproc.EntityType);
                    Assert.Equal("DerivedId", insertSproc.ResultColumns.Last().Name);
                    Assert.Equal(
                        "DerivedId",
                        id.GetColumnName(StoreObjectIdentifier.Create(principalDerived, StoreObjectType.InsertStoredProcedure).Value));
                    Assert.Equal("bar3", insertSproc.ResultColumns.Last()["foo"]);
                    Assert.Null(
                        id.FindOverrides(
                            StoreObjectIdentifier.Create(principalDerived, StoreObjectType.InsertStoredProcedure).Value)["foo"]);

                    updateSproc = principalDerived.GetUpdateStoredProcedure()!;
                    Assert.Equal("Derived_Update", updateSproc.Name);
                    Assert.Equal("Derived", updateSproc.Schema);
                    Assert.Equal(
                        new[]
                        {
                            "PrincipalBaseId",
                            "PrincipalDerivedId",
                            "Enum1",
                            "Enum2",
                            "FlagsEnum1",
                            "FlagsEnum2",
                            "ValueTypeList",
                            "ValueTypeIList",
                            "ValueTypeArray",
                            "ValueTypeEnumerable",
                            "RefTypeList",
                            "RefTypeIList",
                            "RefTypeArray",
                            "RefTypeEnumerable",
                            "Id"
                        },
                        updateSproc.Parameters.Select(p => p.PropertyName));
                    Assert.Empty(updateSproc.ResultColumns);
                    Assert.Empty(updateSproc.GetAnnotations());
                    Assert.Same(principalDerived, updateSproc.EntityType);
                    Assert.Equal("Id_Original", updateSproc.Parameters.Last().Name);
                    Assert.Null(
                        id.FindOverrides(StoreObjectIdentifier.Create(principalDerived, StoreObjectType.UpdateStoredProcedure).Value));

                    deleteSproc = principalDerived.GetDeleteStoredProcedure()!;
                    Assert.Equal("Derived_Delete", deleteSproc.Name);
                    Assert.Equal("TPC", deleteSproc.Schema);
                    Assert.Equal(new[] { "Id" }, deleteSproc.Parameters.Select(p => p.PropertyName));
                    Assert.Empty(deleteSproc.ResultColumns);
                    Assert.Same(principalDerived, deleteSproc.EntityType);
                    Assert.Equal("Id_Original", deleteSproc.Parameters.Last().Name);
                    Assert.Null(
                        id.FindOverrides(StoreObjectIdentifier.Create(principalDerived, StoreObjectType.DeleteStoredProcedure).Value));

                    Assert.Equal("PrincipalDerived<DependentBase<byte?>>", principalDerived.GetDiscriminatorValue());
                    Assert.Null(principalDerived.FindDiscriminatorProperty());
                    Assert.Equal("TPC", principalDerived.GetMappingStrategy());

                    Assert.Equal(2, principalDerived.GetDeclaredNavigations().Count());
                    var derivedNavigation = principalDerived.GetDeclaredNavigations().Last();
                    Assert.Equal("Principals", derivedNavigation.Name);
                    Assert.True(derivedNavigation.IsCollection);
                    Assert.False(derivedNavigation.IsOnDependent);
                    Assert.Equal(principalBase, derivedNavigation.TargetEntityType);
                    Assert.Null(derivedNavigation.Inverse);

                    var dependentNavigation = principalDerived.GetDeclaredNavigations().First();
                    Assert.Equal("Dependent", dependentNavigation.Name);
                    Assert.Equal("Dependent", dependentNavigation.PropertyInfo.Name);
                    Assert.Equal("<Dependent>k__BackingField", dependentNavigation.FieldInfo.Name);
                    Assert.False(dependentNavigation.IsCollection);
                    Assert.False(dependentNavigation.IsEagerLoaded);
                    Assert.True(dependentNavigation.LazyLoadingEnabled);
                    Assert.False(dependentNavigation.IsOnDependent);
                    Assert.Equal(principalDerived, dependentNavigation.DeclaringEntityType);
                    Assert.Equal("Principal", dependentNavigation.Inverse.Name);

                    var dependentForeignKey = dependentNavigation.ForeignKey;
                    Assert.False(dependentForeignKey.IsOwnership);
                    Assert.False(dependentForeignKey.IsRequired);
                    Assert.True(dependentForeignKey.IsRequiredDependent);
                    Assert.True(dependentForeignKey.IsUnique);
                    Assert.Same(principalDerived, dependentForeignKey.PrincipalEntityType);
                    Assert.Same(dependentNavigation.Inverse, dependentForeignKey.DependentToPrincipal);
                    Assert.Same(dependentNavigation, dependentForeignKey.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.ClientCascade, dependentForeignKey.DeleteBehavior);
                    Assert.Equal(new[] { "PrincipalId" }, dependentForeignKey.Properties.Select(p => p.Name));

                    var dependentBase = dependentNavigation.TargetEntityType;

                    Assert.True(dependentBase.GetIsDiscriminatorMappingComplete());
                    Assert.Null(dependentBase.FindDiscriminatorProperty());

                    Assert.Same(dependentForeignKey, dependentBase.GetForeignKeys().Single());

                    Assert.Equal(
                        new[] { dependentBase, principalBase, principalDerived },
                        model.GetEntityTypes());

                    var principalBaseSequence = model.FindSequence("PrincipalBaseSequence");
                    Assert.Equal("TPC", principalBaseSequence.Schema);
                },
                typeof(SqlServerNetTopologySuiteDesignTimeServices));

        public class TpcContext : SqlServerContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDefaultSchema("TPC")
                    .HasDatabaseMaxSize("20TB")
                    .HasPerformanceLevel("High")
                    .HasServiceTier("AB");

                modelBuilder.Entity<PrincipalBase>(
                    eb =>
                    {
                        eb.Ignore(e => e.Owned);

                        eb.UseTpcMappingStrategy();

                        eb.ToTable("PrincipalBase");
                        eb.ToView("PrincipalBaseView", tb => tb.Property(e => e.Id).HasAnnotation("foo", "bar2"));

                        eb.InsertUsingStoredProcedure(
                            s => s
                                .HasParameter("PrincipalBaseId")
                                .HasParameter("PrincipalDerivedId")
                                .HasParameter("Enum1")
                                .HasParameter("Enum2")
                                .HasParameter("FlagsEnum1")
                                .HasParameter("FlagsEnum2")
                                .HasParameter("ValueTypeList")
                                .HasParameter("ValueTypeIList")
                                .HasParameter("ValueTypeArray")
                                .HasParameter("ValueTypeEnumerable")
                                .HasParameter("RefTypeList")
                                .HasParameter("RefTypeIList")
                                .HasParameter("RefTypeArray")
                                .HasParameter("RefTypeEnumerable")
                                .HasParameter(p => p.Id, pb => pb.HasName("BaseId").IsOutput().HasAnnotation("foo", "bar"))
                                .HasAnnotation("foo", "bar1"));
                        eb.UpdateUsingStoredProcedure(
                            s => s
                                .HasParameter("PrincipalBaseId")
                                .HasParameter("PrincipalDerivedId")
                                .HasParameter("Enum1")
                                .HasParameter("Enum2")
                                .HasParameter("FlagsEnum1")
                                .HasParameter("FlagsEnum2")
                                .HasParameter("ValueTypeList")
                                .HasParameter("ValueTypeIList")
                                .HasParameter("ValueTypeArray")
                                .HasParameter("ValueTypeEnumerable")
                                .HasParameter("RefTypeList")
                                .HasParameter("RefTypeIList")
                                .HasParameter("RefTypeArray")
                                .HasParameter("RefTypeEnumerable")
                                .HasOriginalValueParameter(p => p.Id));
                        eb.DeleteUsingStoredProcedure(
                            s => s
                                .HasRowsAffectedReturnValue()
                                .HasOriginalValueParameter(p => p.Id));

                        eb.HasIndex(new[] { "PrincipalBaseId" }, "PrincipalIndex")
                            .IsUnique()
                            .HasDatabaseName("PIX")
                            .IsClustered()
                            .HasFilter("AlternateId <> NULL")
                            .IsCreatedOnline()
                            .HasFillFactor(40)
                            .IncludeProperties(e => e.Id)
                            .SortInTempDb()
                            .UseDataCompression(DataCompressionType.Page);
                    });

                modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
                    eb =>
                    {
                        eb.HasOne(e => e.Dependent).WithOne(e => e.Principal)
                            .HasForeignKey<DependentBase<byte?>>()
                            .OnDelete(DeleteBehavior.ClientCascade);

                        eb.Navigation(e => e.Dependent).IsRequired();

                        eb.ToTable("PrincipalDerived");
                        eb.ToView("PrincipalDerivedView");

                        eb.InsertUsingStoredProcedure(
                            "Derived_Insert", s => s
                                .HasParameter("PrincipalBaseId")
                                .HasParameter("PrincipalDerivedId")
                                .HasParameter("Enum1")
                                .HasParameter("Enum2")
                                .HasParameter("FlagsEnum1")
                                .HasParameter("FlagsEnum2")
                                .HasParameter("ValueTypeList")
                                .HasParameter("ValueTypeIList")
                                .HasParameter("ValueTypeArray")
                                .HasParameter("ValueTypeEnumerable")
                                .HasParameter("RefTypeList")
                                .HasParameter("RefTypeIList")
                                .HasParameter("RefTypeArray")
                                .HasParameter("RefTypeEnumerable")
                                .HasResultColumn(p => p.Id, pb => pb.HasName("DerivedId").HasAnnotation("foo", "bar3")));
                        eb.UpdateUsingStoredProcedure(
                            "Derived_Update", "Derived", s => s
                                .HasParameter("PrincipalBaseId")
                                .HasParameter("PrincipalDerivedId")
                                .HasParameter("Enum1")
                                .HasParameter("Enum2")
                                .HasParameter("FlagsEnum1")
                                .HasParameter("FlagsEnum2")
                                .HasParameter("ValueTypeList")
                                .HasParameter("ValueTypeIList")
                                .HasParameter("ValueTypeArray")
                                .HasParameter("ValueTypeEnumerable")
                                .HasParameter("RefTypeList")
                                .HasParameter("RefTypeIList")
                                .HasParameter("RefTypeArray")
                                .HasParameter("RefTypeEnumerable")
                                .HasOriginalValueParameter(p => p.Id));
                        eb.DeleteUsingStoredProcedure(
                            "Derived_Delete", s => s
                                .HasOriginalValueParameter(p => p.Id));
                    });

                modelBuilder.Entity<DependentBase<byte?>>(
                    eb =>
                    {
                        eb.Property<byte?>("Id");
                    });
            }
        }

        public class CustomValueComparer<T> : ValueComparer<T>
        {
            public CustomValueComparer()
                : base(false)
            {
            }
        }

        public abstract class AbstractBase
        {
            public int Id { get; set; }
        }

        public enum AnEnum
        {
            A = 1,
            B,
        }

        public enum AFlagsEnum
        {
            A = 1,
            B = 2,
            C = 4,
        }

        public sealed class MyJsonGuidReaderWriter : JsonValueReaderWriter<Guid>
        {
            public override Guid FromJsonTyped(ref Utf8JsonReaderManager manager, object existingObject = null)
                => manager.CurrentReader.GetGuid();

            public override void ToJsonTyped(Utf8JsonWriter writer, Guid value)
                => writer.WriteStringValue(value);
        }

        public class ManyTypes
        {
            public ManyTypesId Id { get; set; }
            public bool Bool { get; set; }
            public byte UInt8 { get; set; }
            public ushort UInt16 { get; set; }
            public uint UInt32 { get; set; }
            public ulong UInt64 { get; set; }
            public sbyte Int8 { get; set; }
            public short Int16 { get; set; }
            public int Int32 { get; set; }
            public long Int64 { get; set; }
            public char Char { get; set; }
            public decimal Decimal { get; set; }
            public double Double { get; set; }
            public float Float { get; set; }
            public Guid Guid { get; set; }
            public DateTime DateTime { get; set; }
            public DateOnly DateOnly { get; set; }
            public TimeOnly TimeOnly { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public string String { get; set; }
            public byte[] Bytes { get; set; }
            public Uri Uri { get; set; }
            public IPAddress IPAddress { get; set; }
            public PhysicalAddress PhysicalAddress { get; set; }

            public bool? NullableBool { get; set; }
            public byte? NullableUInt8 { get; set; }
            public ushort? NullableUInt16 { get; set; }
            public uint? NullableUInt32 { get; set; }
            public ulong? NullableUInt64 { get; set; }
            public sbyte? NullableInt8 { get; set; }
            public short? NullableInt16 { get; set; }
            public int? NullableInt32 { get; set; }
            public long? NullableInt64 { get; set; }
            public char? NullableChar { get; set; }
            public decimal? NullableDecimal { get; set; }
            public double? NullableDouble { get; set; }
            public float? NullableFloat { get; set; }
            public Guid? NullableGuid { get; set; }
            public DateTime? NullableDateTime { get; set; }
            public DateOnly? NullableDateOnly { get; set; }
            public TimeOnly? NullableTimeOnly { get; set; }
            public TimeSpan? NullableTimeSpan { get; set; }
            public string NullableString { get; set; }
            public byte[] NullableBytes { get; set; }
            public Uri NullableUri { get; set; }
            public IPAddress NullableIPAddress { get; set; }
            public PhysicalAddress NullablePhysicalAddress { get; set; }

            public bool[] BoolArray { get; set; }
            public byte[] UInt8Array { get; set; }
            public ushort[] UInt16Array { get; set; }
            public uint[] UInt32Array { get; set; }
            public ulong[] UInt64Array { get; set; }
            public sbyte[] Int8Array { get; set; }
            public short[] Int16Array { get; set; }
            public int[] Int32Array { get; set; }
            public long[] Int64Array { get; set; }
            public char[] CharArray { get; set; }
            public decimal[] DecimalArray { get; set; }
            public double[] DoubleArray { get; set; }
            public float[] FloatArray { get; set; }
            public Guid[] GuidArray { get; set; }
            public DateTime[] DateTimeArray { get; set; }
            public DateOnly[] DateOnlyArray { get; set; }
            public TimeOnly[] TimeOnlyArray { get; set; }
            public TimeSpan[] TimeSpanArray { get; set; }
            public string[] StringArray { get; set; }
            public byte[][] BytesArray { get; set; }
            public Uri[] UriArray { get; set; }
            public IPAddress[] IPAddressArray { get; set; }
            public PhysicalAddress[] PhysicalAddressArray { get; set; }

            public bool?[] NullableBoolArray { get; set; }
            public byte?[] NullableUInt8Array { get; set; }
            public ushort?[] NullableUInt16Array { get; set; }
            public uint?[] NullableUInt32Array { get; set; }
            public ulong?[] NullableUInt64Array { get; set; }
            public sbyte?[] NullableInt8Array { get; set; }
            public short?[] NullableInt16Array { get; set; }
            public int?[] NullableInt32Array { get; set; }
            public long?[] NullableInt64Array { get; set; }
            public char?[] NullableCharArray { get; set; }
            public decimal?[] NullableDecimalArray { get; set; }
            public double?[] NullableDoubleArray { get; set; }
            public float?[] NullableFloatArray { get; set; }
            public Guid?[] NullableGuidArray { get; set; }
            public DateTime?[] NullableDateTimeArray { get; set; }
            public DateOnly?[] NullableDateOnlyArray { get; set; }
            public TimeOnly?[] NullableTimeOnlyArray { get; set; }
            public TimeSpan?[] NullableTimeSpanArray { get; set; }
            public string[] NullableStringArray { get; set; }
            public byte[][] NullableBytesArray { get; set; }
            public Uri[] NullableUriArray { get; set; }
            public IPAddress[] NullableIPAddressArray { get; set; }
            public PhysicalAddress[] NullablePhysicalAddressArray { get; set; }

            public Enum8 Enum8 { get; set; }
            public Enum16 Enum16 { get; set; }
            public Enum32 Enum32 { get; set; }
            public Enum64 Enum64 { get; set; }
            public EnumU8 EnumU8 { get; set; }
            public EnumU16 EnumU16 { get; set; }
            public EnumU32 EnumU32 { get; set; }
            public EnumU64 EnumU64 { get; set; }

            public Enum8 Enum8AsString { get; set; }
            public Enum16 Enum16AsString { get; set; }
            public Enum32 Enum32AsString { get; set; }
            public Enum64 Enum64AsString { get; set; }
            public EnumU8 EnumU8AsString { get; set; }
            public EnumU16 EnumU16AsString { get; set; }
            public EnumU32 EnumU32AsString { get; set; }
            public EnumU64 EnumU64AsString { get; set; }

            public Enum8? NullableEnum8 { get; set; }
            public Enum16? NullableEnum16 { get; set; }
            public Enum32? NullableEnum32 { get; set; }
            public Enum64? NullableEnum64 { get; set; }
            public EnumU8? NullableEnumU8 { get; set; }
            public EnumU16? NullableEnumU16 { get; set; }
            public EnumU32? NullableEnumU32 { get; set; }
            public EnumU64? NullableEnumU64 { get; set; }

            public Enum8? NullableEnum8AsString { get; set; }
            public Enum16? NullableEnum16AsString { get; set; }
            public Enum32? NullableEnum32AsString { get; set; }
            public Enum64? NullableEnum64AsString { get; set; }
            public EnumU8? NullableEnumU8AsString { get; set; }
            public EnumU16? NullableEnumU16AsString { get; set; }
            public EnumU32? NullableEnumU32AsString { get; set; }
            public EnumU64? NullableEnumU64AsString { get; set; }

            public List<Enum8> Enum8Collection { get; set; }
            public List<Enum16> Enum16Collection { get; set; }
            public List<Enum32> Enum32Collection { get; set; }
            public List<Enum64> Enum64Collection { get; set; }
            public List<EnumU8> EnumU8Collection { get; set; }
            public List<EnumU16> EnumU16Collection { get; set; }
            public List<EnumU32> EnumU32Collection { get; set; }
            public List<EnumU64> EnumU64Collection { get; set; }

            public List<Enum8> Enum8AsStringCollection { get; set; }
            public List<Enum16> Enum16AsStringCollection { get; set; }
            public List<Enum32> Enum32AsStringCollection { get; set; }
            public List<Enum64> Enum64AsStringCollection { get; set; }
            public List<EnumU8> EnumU8AsStringCollection { get; set; }
            public List<EnumU16> EnumU16AsStringCollection { get; set; }
            public List<EnumU32> EnumU32AsStringCollection { get; set; }
            public List<EnumU64> EnumU64AsStringCollection { get; set; }

            public List<Enum8?> NullableEnum8Collection { get; set; }
            public List<Enum16?> NullableEnum16Collection { get; set; }
            public List<Enum32?> NullableEnum32Collection { get; set; }
            public List<Enum64?> NullableEnum64Collection { get; set; }
            public List<EnumU8?> NullableEnumU8Collection { get; set; }
            public List<EnumU16?> NullableEnumU16Collection { get; set; }
            public List<EnumU32?> NullableEnumU32Collection { get; set; }
            public List<EnumU64?> NullableEnumU64Collection { get; set; }

            public List<Enum8?> NullableEnum8AsStringCollection { get; set; }
            public List<Enum16?> NullableEnum16AsStringCollection { get; set; }
            public List<Enum32?> NullableEnum32AsStringCollection { get; set; }
            public List<Enum64?> NullableEnum64AsStringCollection { get; set; }
            public List<EnumU8?> NullableEnumU8AsStringCollection { get; set; }
            public List<EnumU16?> NullableEnumU16AsStringCollection { get; set; }
            public List<EnumU32?> NullableEnumU32AsStringCollection { get; set; }
            public List<EnumU64?> NullableEnumU64AsStringCollection { get; set; }

            public Enum8[] Enum8Array { get; set; }
            public Enum16[] Enum16Array { get; set; }
            public Enum32[] Enum32Array { get; set; }
            public Enum64[] Enum64Array { get; set; }
            public EnumU8[] EnumU8Array { get; set; }
            public EnumU16[] EnumU16Array { get; set; }
            public EnumU32[] EnumU32Array { get; set; }
            public EnumU64[] EnumU64Array { get; set; }

            public Enum8[] Enum8AsStringArray { get; set; }
            public Enum16[] Enum16AsStringArray { get; set; }
            public Enum32[] Enum32AsStringArray { get; set; }
            public Enum64[] Enum64AsStringArray { get; set; }
            public EnumU8[] EnumU8AsStringArray { get; set; }
            public EnumU16[] EnumU16AsStringArray { get; set; }
            public EnumU32[] EnumU32AsStringArray { get; set; }
            public EnumU64[] EnumU64AsStringArray { get; set; }

            public Enum8?[] NullableEnum8Array { get; set; }
            public Enum16?[] NullableEnum16Array { get; set; }
            public Enum32?[] NullableEnum32Array { get; set; }
            public Enum64?[] NullableEnum64Array { get; set; }
            public EnumU8?[] NullableEnumU8Array { get; set; }
            public EnumU16?[] NullableEnumU16Array { get; set; }
            public EnumU32?[] NullableEnumU32Array { get; set; }
            public EnumU64?[] NullableEnumU64Array { get; set; }

            public Enum8?[] NullableEnum8AsStringArray { get; set; }
            public Enum16?[] NullableEnum16AsStringArray { get; set; }
            public Enum32?[] NullableEnum32AsStringArray { get; set; }
            public Enum64?[] NullableEnum64AsStringArray { get; set; }
            public EnumU8?[] NullableEnumU8AsStringArray { get; set; }
            public EnumU16?[] NullableEnumU16AsStringArray { get; set; }
            public EnumU32?[] NullableEnumU32AsStringArray { get; set; }
            public EnumU64?[] NullableEnumU64AsStringArray { get; set; }

            public bool BoolToStringConverterProperty { get; set; }
            public bool BoolToTwoValuesConverterProperty { get; set; }
            public bool BoolToZeroOneConverterProperty { get; set; }
            public byte[] BytesToStringConverterProperty { get; set; }
            public int CastingConverterProperty { get; set; }
            public char CharToStringConverterProperty { get; set; }
            public DateOnly DateOnlyToStringConverterProperty { get; set; }
            public DateTimeOffset DateTimeOffsetToBinaryConverterProperty { get; set; }
            public DateTimeOffset DateTimeOffsetToBytesConverterProperty { get; set; }
            public DateTimeOffset DateTimeOffsetToStringConverterProperty { get; set; }
            public DateTime DateTimeToBinaryConverterProperty { get; set; }
            public DateTime DateTimeToStringConverterProperty { get; set; }
            public DateTime DateTimeToTicksConverterProperty { get; set; }
            public Enum32 EnumToNumberConverterProperty { get; set; }
            public Enum32 EnumToStringConverterProperty { get; set; }
            public Guid GuidToBytesConverterProperty { get; set; }
            public Guid GuidToStringConverterProperty { get; set; }
            public IPAddress IPAddressToBytesConverterProperty { get; set; }
            public IPAddress IPAddressToStringConverterProperty { get; set; }
            public int IntNumberToBytesConverterProperty { get; set; }
            public decimal DecimalNumberToBytesConverterProperty { get; set; }
            public double DoubleNumberToBytesConverterProperty { get; set; }
            public int IntNumberToStringConverterProperty { get; set; }
            public decimal DecimalNumberToStringConverterProperty { get; set; }
            public double DoubleNumberToStringConverterProperty { get; set; }
            public PhysicalAddress PhysicalAddressToBytesConverterProperty { get; set; }
            public PhysicalAddress PhysicalAddressToStringConverterProperty { get; set; }
            public string StringToBoolConverterProperty { get; set; }
            public string StringToBytesConverterProperty { get; set; }
            public string StringToCharConverterProperty { get; set; }
            public string StringToDateOnlyConverterProperty { get; set; }
            public string StringToDateTimeConverterProperty { get; set; }
            public string StringToDateTimeOffsetConverterProperty { get; set; }
            public string StringToEnumConverterProperty { get; set; }
            public string StringToGuidConverterProperty { get; set; }
            public string StringToIntNumberConverterProperty { get; set; }
            public string StringToDecimalNumberConverterProperty { get; set; }
            public string StringToDoubleNumberConverterProperty { get; set; }
            public string StringToTimeOnlyConverterProperty { get; set; }
            public string StringToTimeSpanConverterProperty { get; set; }
            public string StringToUriConverterProperty { get; set; }
            public TimeOnly TimeOnlyToStringConverterProperty { get; set; }
            public TimeOnly TimeOnlyToTicksConverterProperty { get; set; }
            public TimeSpan TimeSpanToStringConverterProperty { get; set; }
            public TimeSpan TimeSpanToTicksConverterProperty { get; set; }
            public Uri UriToStringConverterProperty { get; set; }

            public int? NullIntToNullStringConverterProperty { get; set; }
        }

        public readonly record struct ManyTypesId(int Id);

        public class ManyTypesIdConverter : ValueConverter<ManyTypesId, int>
        {
            public ManyTypesIdConverter()
                : base(v => v.Id, v => new ManyTypesId(v))
            {
            }
        }

        public enum Enum8 : sbyte
        {
            Min = sbyte.MinValue,
            Default = 0,
            One = 1,
            Max = sbyte.MaxValue
        }

        public enum Enum16 : short
        {
            Min = short.MinValue,
            Default = 0,
            One = 1,
            Max = short.MaxValue
        }

        public enum Enum32
        {
            Min = int.MinValue,
            Default = 0,
            One = 1,
            Max = int.MaxValue
        }

        public enum Enum64 : long
        {
            Min = long.MinValue,
            Default = 0,
            One = 1,
            Max = long.MaxValue
        }

        public enum EnumU8 : byte
        {
            Min = byte.MinValue,
            Default = 0,
            One = 1,
            Max = byte.MaxValue
        }

        public enum EnumU16 : ushort
        {
            Min = ushort.MinValue,
            Default = 0,
            One = 1,
            Max = ushort.MaxValue
        }

        public enum EnumU32 : uint
        {
            Min = uint.MinValue,
            Default = 0,
            One = 1,
            Max = uint.MaxValue
        }

        public enum EnumU64 : ulong
        {
            Min = ulong.MinValue,
            Default = 0,
            One = 1,
            Max = ulong.MaxValue
        }

        public class PrincipalBase : AbstractBase
        {
            public new long? Id { get; set; }
            public Guid AlternateId;

            public AnEnum Enum1 { get; set; }
            public AnEnum? Enum2 { get; set; }
            public AFlagsEnum FlagsEnum1 { get; set; }
            public AFlagsEnum FlagsEnum2 { get; set; }

            public List<short> ValueTypeList { get; set; }
            public IList<byte> ValueTypeIList { get; set; }
            public DateTime[] ValueTypeArray { get; set; }
            public IEnumerable<byte> ValueTypeEnumerable { get; set; }

            public List<IPAddress> RefTypeList { get; set; }
            public IList<string> RefTypeIList { get; set; }
            public IPAddress[] RefTypeArray { get; set; }
            public IEnumerable<string> RefTypeEnumerable { get; set; }

            private OwnedType _ownedField;
            public OwnedType Owned { get => _ownedField; set => _ownedField = value; }
            public ICollection<PrincipalBase> Deriveds { get; set; }
        }

        public class PrincipalDerived<TDependent> : PrincipalBase
        {
            public TDependent Dependent { get; set; }
            protected ICollection<OwnedType> ManyOwned;
            public ICollection<PrincipalBase> Principals { get; set; }
        }

        public class DependentBase<TKey> : AbstractBase
        {
            public DependentBase(TKey id)
            {
                Id = id;
            }

            private new TKey Id { get; }

            public PrincipalDerived<DependentBase<TKey>> Principal { get; set; }
        }

        public class DependentDerived<TKey> : DependentBase<TKey>
        {
            public DependentDerived(TKey id)
                : base(id)
            {
            }

            private string Data { get; set; }
        }

        public class SpatialTypes : AbstractBase
        {
        }

        public class OwnedType : INotifyPropertyChanged, INotifyPropertyChanging
        {
            private DbContext _context;

            public OwnedType()
            {
            }

            public OwnedType(DbContext context)
            {
                Context = context;
            }

            public DbContext Context
            {
                get => _context;
                set
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Context"));
                    _context = value;
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs("Context"));
                }
            }

            public int Number { get; set; }

            [NotMapped]
            public PrincipalBase Principal { get; set; }

            private string _details;
            private List<short> _valueTypeList;
            private DateTime[] _valueTypeArray;
            private IEnumerable<byte> _valueTypeEnumerable;
            private List<IPAddress> _refTypeList;
            private IList<string> _refTypeIList;
            private IPAddress[] _refTypeArray;
            private IEnumerable<string> _refTypeEnumerable;

            public string Details
            {
                get => _details;
                set => _details = value;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public event PropertyChangingEventHandler PropertyChanging;

            public List<short> ValueTypeList
            {
                get => _valueTypeList;
                set
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueTypeList)));
                    _valueTypeList = value;
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(ValueTypeList)));
                }
            }

            public IList<byte> ValueTypeIList { get; set; }

            public DateTime[] ValueTypeArray
            {
                get => _valueTypeArray;
                set
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueTypeArray)));
                    _valueTypeArray = value;
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(ValueTypeArray)));
                }
            }

            public IEnumerable<byte> ValueTypeEnumerable
            {
                get => _valueTypeEnumerable;
                set
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueTypeEnumerable)));
                    _valueTypeEnumerable = value;
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(ValueTypeEnumerable)));
                }
            }

            public List<IPAddress> RefTypeList
            {
                get => _refTypeList;
                set
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTypeList)));
                    _refTypeList = value;
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(RefTypeList)));
                }
            }

            public IList<string> RefTypeIList
            {
                get => _refTypeIList;
                set
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTypeIList)));
                    _refTypeIList = value;
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(RefTypeIList)));
                }
            }

            public IPAddress[] RefTypeArray
            {
                get => _refTypeArray;
                set
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTypeArray)));
                    _refTypeArray = value;
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(RefTypeArray)));
                }
            }

            public IEnumerable<string> RefTypeEnumerable
            {
                get => _refTypeEnumerable;
                set
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTypeEnumerable)));
                    _refTypeEnumerable = value;
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(RefTypeEnumerable)));
                }
            }
        }

        [ConditionalFact]
        public void DbFunctions()
            => Test(
                new DbFunctionContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    Assert.Equal(5, model.GetDbFunctions().Count());

                    var getCount = model.FindDbFunction(
                        typeof(DbFunctionContext)
                            .GetMethod("GetCount", BindingFlags.NonPublic | BindingFlags.Instance));
                    Assert.Equal("CustomerOrderCount", getCount.Name);
                    Assert.Same(model, getCount.Model);
                    Assert.Same(model, ((IReadOnlyDbFunction)getCount).Model);
                    Assert.Equal(typeof(DbFunctionContext).FullName + ".GetCount(System.Guid?,string)", getCount.ModelName);
                    Assert.Equal("dbf", getCount.Schema);
                    Assert.False(getCount.IsNullable);
                    Assert.True(getCount.IsScalar);
                    Assert.False(getCount.IsBuiltIn);
                    Assert.False(getCount.IsAggregate);
                    Assert.Null(getCount.Translation);
                    Assert.Equal("int", getCount.TypeMapping?.StoreType);
                    Assert.Equal(typeof(int), getCount.ReturnType);
                    Assert.Equal("GetCount", getCount.MethodInfo.Name);
                    Assert.Empty(getCount.GetAnnotations());
                    Assert.Empty(getCount.GetRuntimeAnnotations());
                    Assert.Equal("CustomerOrderCount", getCount.StoreFunction.Name);
                    Assert.False(getCount.StoreFunction.IsShared);
                    Assert.NotNull(getCount.ToString());
                    Assert.Equal(getCount.Parameters, ((IReadOnlyDbFunction)getCount).Parameters);
                    Assert.Equal(2, getCount.Parameters.Count);

                    var getCountParameter1 = getCount.Parameters[0];
                    Assert.Same(getCount, getCountParameter1.Function);
                    Assert.Same(getCount, ((IReadOnlyDbFunctionParameter)getCountParameter1).Function);
                    Assert.Equal("id", getCountParameter1.Name);
                    Assert.Equal("uniqueidentifier", getCountParameter1.StoreType);
                    Assert.Equal("uniqueidentifier", ((IReadOnlyDbFunctionParameter)getCountParameter1).StoreType);
                    Assert.True(getCountParameter1.PropagatesNullability);
                    Assert.Equal(typeof(Guid?), getCountParameter1.ClrType);
                    Assert.Equal("uniqueidentifier", getCountParameter1.TypeMapping.StoreType);
                    Assert.Single((IEnumerable)getCountParameter1.GetAnnotations());
                    Assert.Equal(new[] { 1L }, getCountParameter1["MyAnnotation"]);
                    Assert.Equal("id", getCountParameter1.StoreFunctionParameter.Name);
                    Assert.Equal("uniqueidentifier", getCountParameter1.StoreFunctionParameter.StoreType);
                    Assert.NotNull(getCountParameter1.ToString());

                    var getCountParameter2 = getCount.Parameters[1];
                    Assert.Same(getCount, getCountParameter2.Function);
                    Assert.Equal("condition", getCountParameter2.Name);
                    Assert.Equal("nchar(256)", getCountParameter2.StoreType);
                    Assert.False(getCountParameter2.PropagatesNullability);
                    Assert.Equal(typeof(string), getCountParameter2.ClrType);
                    Assert.Equal("nchar(256)", getCountParameter2.TypeMapping.StoreType);
                    Assert.Equal("condition", getCountParameter2.StoreFunctionParameter.Name);
                    Assert.Equal("nchar(256)", getCountParameter2.StoreFunctionParameter.StoreType);
                    Assert.NotNull(getCountParameter2.ToString());

                    var isDate = model.FindDbFunction(typeof(DbFunctionContext).GetMethod("IsDateStatic"));
                    Assert.Equal("IsDate", isDate.Name);
                    Assert.Null(isDate.Schema);
                    Assert.Equal(typeof(DbFunctionContext).FullName + ".IsDateStatic(string)", isDate.ModelName);
                    Assert.True(isDate.IsNullable);
                    Assert.True(isDate.IsScalar);
                    Assert.True(isDate.IsBuiltIn);
                    Assert.False(isDate.IsAggregate);
                    Assert.Null(isDate.Translation);
                    Assert.Equal(typeof(bool), isDate.ReturnType);
                    Assert.Equal("IsDateStatic", isDate.MethodInfo.Name);
                    Assert.Single((IEnumerable)isDate.GetAnnotations());
                    Assert.Equal(new Guid(), isDate["MyGuid"]);
                    Assert.Empty(isDate.GetRuntimeAnnotations());
                    Assert.Equal("bit", isDate.StoreFunction.ReturnType);
                    Assert.Empty(isDate.StoreFunction.EntityTypeMappings);
                    Assert.Single((IEnumerable)isDate.Parameters);

                    var isDateParameter = isDate.Parameters[0];
                    Assert.Same(isDate, isDateParameter.Function);
                    Assert.Equal("date", isDateParameter.Name);
                    Assert.Equal("nchar(256)", isDateParameter.StoreType);
                    Assert.False(isDateParameter.PropagatesNullability);
                    Assert.Equal(typeof(string), isDateParameter.ClrType);
                    Assert.Equal("nchar(256)", isDateParameter.TypeMapping.StoreType);
                    Assert.Equal("date", isDateParameter.StoreFunctionParameter.Name);
                    Assert.Equal("nchar(256)", isDateParameter.StoreFunctionParameter.StoreType);

                    var getData = model.FindDbFunction(
                        typeof(DbFunctionContext)
                            .GetMethod("GetData", new[] { typeof(int) }));
                    Assert.Equal("GetData", getData.Name);
                    Assert.Equal("dbo", getData.Schema);
                    Assert.Equal(typeof(DbFunctionContext).FullName + ".GetData(int)", getData.ModelName);
                    Assert.False(getData.IsNullable);
                    Assert.False(getData.IsScalar);
                    Assert.False(getData.IsBuiltIn);
                    Assert.False(getData.IsAggregate);
                    Assert.Null(getData.Translation);
                    Assert.Equal(typeof(IQueryable<Data>), getData.ReturnType);
                    Assert.Equal("GetData", getData.MethodInfo.Name);
                    Assert.Empty(getData.GetAnnotations());
                    Assert.Empty(getData.GetRuntimeAnnotations());
                    Assert.Null(getData.TypeMapping?.StoreType);
                    Assert.Null(getData.StoreFunction.ReturnType);
                    Assert.Equal(typeof(Data), getData.StoreFunction.EntityTypeMappings.Single().TypeBase.ClrType);
                    Assert.Single((IEnumerable)getData.Parameters);

                    var getDataParameter = getData.Parameters[0];
                    Assert.Same(getData, getDataParameter.Function);
                    Assert.Equal("id", getDataParameter.Name);
                    Assert.Equal("int", getDataParameter.StoreType);
                    Assert.False(getDataParameter.PropagatesNullability);
                    Assert.Equal(typeof(int), getDataParameter.ClrType);
                    Assert.Equal("int", getDataParameter.TypeMapping.StoreType);
                    Assert.Equal("id", getDataParameter.StoreFunctionParameter.Name);
                    Assert.Equal("int", getDataParameter.StoreFunctionParameter.StoreType);

                    var getDataParameterless = model.FindDbFunction(
                        typeof(DbFunctionContext)
                            .GetMethod("GetData", new Type[0]));
                    Assert.Equal("GetAllData", getDataParameterless.Name);
                    Assert.Equal("dbo", getDataParameterless.Schema);
                    Assert.Equal(typeof(DbFunctionContext).FullName + ".GetData()", getDataParameterless.ModelName);
                    Assert.False(getDataParameterless.IsNullable);
                    Assert.False(getDataParameterless.IsScalar);
                    Assert.False(getDataParameterless.IsBuiltIn);
                    Assert.False(getDataParameterless.IsAggregate);
                    Assert.Null(getDataParameterless.Translation);
                    Assert.Equal(typeof(IQueryable<Data>), getDataParameterless.ReturnType);
                    Assert.Equal("GetData", getDataParameterless.MethodInfo.Name);
                    Assert.Empty(getDataParameterless.GetAnnotations());
                    Assert.Empty(getDataParameterless.GetRuntimeAnnotations());
                    Assert.False(getDataParameterless.StoreFunction.IsBuiltIn);
                    Assert.Equal(typeof(Data), getDataParameterless.StoreFunction.EntityTypeMappings.Single().TypeBase.ClrType);
                    Assert.Equal(0, getDataParameterless.Parameters.Count);

                    Assert.Equal(2, model.GetEntityTypes().Count());
                    var dataEntity = model.FindEntityType(typeof(Data));
                    Assert.Null(dataEntity.FindPrimaryKey());
                    var dataEntityFunctionMapping = dataEntity.GetFunctionMappings().Single(m => m.IsDefaultFunctionMapping);
                    Assert.True(dataEntityFunctionMapping.IncludesDerivedTypes);
                    Assert.Null(dataEntityFunctionMapping.IsSharedTablePrincipal);
                    Assert.Null(dataEntityFunctionMapping.IsSplitEntityTypePrincipal);
                    Assert.Same(getDataParameterless, dataEntityFunctionMapping.DbFunction);

                    var getDataStoreFunction = dataEntityFunctionMapping.StoreFunction;
                    Assert.Same(getDataParameterless, getDataStoreFunction.DbFunctions.Single());
                    Assert.False(getDataStoreFunction.IsOptional(dataEntity));

                    var dataEntityOtherFunctionMapping = dataEntity.GetFunctionMappings().Single(m => !m.IsDefaultFunctionMapping);
                    Assert.True(dataEntityOtherFunctionMapping.IncludesDerivedTypes);
                    Assert.Null(dataEntityOtherFunctionMapping.IsSharedTablePrincipal);
                    Assert.Null(dataEntityOtherFunctionMapping.IsSplitEntityTypePrincipal);
                    Assert.Same(getData, dataEntityOtherFunctionMapping.DbFunction);

                    var getDataOtherStoreFunction = dataEntityOtherFunctionMapping.StoreFunction;
                    Assert.Same(getData, getDataOtherStoreFunction.DbFunctions.Single());
                    Assert.False(getDataOtherStoreFunction.IsOptional(dataEntity));

                    var getBlobs = model.FindDbFunction("GetBlobs()");
                    Assert.Equal("dbo", getBlobs.Schema);
                    Assert.False(getBlobs.IsNullable);
                    Assert.False(getBlobs.IsScalar);
                    Assert.False(getBlobs.IsBuiltIn);
                    Assert.False(getBlobs.IsAggregate);
                    Assert.Null(getBlobs.Translation);
                    Assert.Null(getBlobs.TypeMapping);
                    Assert.Equal(typeof(IQueryable<object>), getBlobs.ReturnType);
                    Assert.Null(getBlobs.MethodInfo);
                    Assert.Empty(getBlobs.GetAnnotations());
                    Assert.Empty(getBlobs.GetRuntimeAnnotations());
                    Assert.Equal("GetBlobs", getBlobs.StoreFunction.Name);
                    Assert.False(getBlobs.StoreFunction.IsShared);
                    Assert.NotNull(getBlobs.ToString());
                    Assert.Empty(getBlobs.Parameters);

                    var objectEntity = model.FindEntityType(typeof(object));
                    Assert.Null(objectEntity.FindPrimaryKey());
                    var objectEntityFunctionMapping = objectEntity.GetFunctionMappings().Single(m => m.IsDefaultFunctionMapping);
                    Assert.True(objectEntityFunctionMapping.IncludesDerivedTypes);
                    Assert.Null(objectEntityFunctionMapping.IsSharedTablePrincipal);
                    Assert.Null(objectEntityFunctionMapping.IsSplitEntityTypePrincipal);
                    Assert.Same(getBlobs, objectEntityFunctionMapping.DbFunction);
                });

        public class DbFunctionContext : SqlServerContextBase
        {
            public static bool IsDateStatic(string date)
                => throw new NotImplementedException();

            private int GetCount(Guid? id, string condition)
                => throw new NotImplementedException();

            public IQueryable<Data> GetData(int id)
                => FromExpression(() => GetData(id));

            public IQueryable<Data> GetData()
                => FromExpression(() => GetData());

            protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
                => configurationBuilder.DefaultTypeMapping<string>().HasMaxLength(256).IsFixedLength();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDbFunction(
                        typeof(DbFunctionContext).GetMethod(nameof(GetCount), BindingFlags.NonPublic | BindingFlags.Instance))
                    .HasName("CustomerOrderCount").HasSchema("dbf").IsNullable(false)
                    .HasParameter("id").PropagatesNullability().Metadata.SetAnnotation("MyAnnotation", new[] { 1L });

                modelBuilder.HasDbFunction(typeof(DbFunctionContext).GetMethod(nameof(IsDateStatic))).HasName("IsDate").IsBuiltIn()
                    .Metadata.SetAnnotation("MyGuid", new Guid());

                modelBuilder.HasDbFunction(typeof(DbFunctionContext).GetMethod(nameof(GetData), new[] { typeof(int) }));
                modelBuilder.HasDbFunction(typeof(DbFunctionContext).GetMethod(nameof(GetData), new Type[0]));

                modelBuilder.Entity<Data>().ToFunction(typeof(DbFunctionContext).FullName + ".GetData()", f => f.HasName("GetAllData"))
                    .HasNoKey();

                modelBuilder.Entity<object>().ToFunction("GetBlobs()", f => f.HasName("GetBlobs")).HasNoKey();
            }
        }

        [ConditionalFact]
        public void Sequences()
            => Test(
                new SequencesContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    Assert.Equal(2, model.GetSequences().Count());

                    var longSequence = model.FindSequence("Long");
                    Assert.Same(model, longSequence.Model);
                    Assert.Equal(typeof(long), longSequence.Type);
                    Assert.True(longSequence.IsCyclic);
                    Assert.Equal(-4, longSequence.StartValue);
                    Assert.Equal(-2, longSequence.MinValue);
                    Assert.Equal(2, longSequence.MaxValue);
                    Assert.Equal(2, longSequence.IncrementBy);
                    Assert.NotNull(longSequence.ToString());

                    var hiLo = model.FindSequence("HL", "S");
                    Assert.Same(model, ((IReadOnlySequence)hiLo).Model);
                    Assert.Equal("HL", hiLo.Name);
                    Assert.Equal("S", hiLo.Schema);
                    Assert.False(hiLo.IsCyclic);
                    Assert.Equal(1, hiLo.StartValue);
                    Assert.Null(hiLo.MinValue);
                    Assert.Null(hiLo.MaxValue);
                    Assert.Equal(10, hiLo.IncrementBy);
                    Assert.NotNull(hiLo.ToString());

                    Assert.Single((IEnumerable)model.GetEntityTypes());
                    var dataEntity = model.FindEntityType(typeof(Data));
                    Assert.Same(hiLo, dataEntity.FindPrimaryKey().Properties.Single().FindHiLoSequence());
                });

        public class SequencesContext : SqlServerContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasSequence<long>("Long")
                    .HasMin(-2)
                    .HasMax(2)
                    .IsCyclic()
                    .IncrementsBy(2)
                    .StartsAt(-4);

                modelBuilder.Entity<Data>(
                    eb =>
                    {
                        eb.Property<int>("Id").UseHiLo("HL", "S");
                        eb.HasKey("Id");
                    });
            }
        }

        [ConditionalFact]
        public void Key_sequences()
            => Test(
                new KeySequencesContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    Assert.Single(model.GetSequences());

                    var keySequence = model.FindSequence("KeySeq", "KeySeqSchema")!;
                    Assert.Same(model, ((IReadOnlySequence)keySequence).Model);
                    Assert.Equal("KeySeq", keySequence.Name);
                    Assert.Equal("KeySeqSchema", keySequence.Schema);
                    Assert.False(keySequence.IsCyclic);
                    Assert.Equal(1, keySequence.StartValue);
                    Assert.Null(keySequence.MinValue);
                    Assert.Null(keySequence.MaxValue);
                    Assert.Equal(1, keySequence.IncrementBy);
                    Assert.NotNull(keySequence.ToString());

                    Assert.Single((IEnumerable)model.GetEntityTypes());
                    var dataEntity = model.FindEntityType(typeof(Data));
                    Assert.Same(keySequence, dataEntity!.FindPrimaryKey().Properties.Single().FindSequence());
                });

        public class KeySequencesContext : SqlServerContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Data>(
                    eb =>
                    {
                        eb.Property<int>("Id").UseSequence("KeySeq", "KeySeqSchema");
                        eb.HasKey("Id");
                    });
            }
        }

        [ConditionalFact]
        public void CheckConstraints()
            => Test(
                new ConstraintsContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    var dataEntity = model.GetEntityTypes().Single();

                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => dataEntity.GetCheckConstraints()).Message);
                });

        public class ConstraintsContext : SqlServerContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Data>(
                    eb =>
                    {
                        eb.Property<int>("Id");
                        eb.HasKey("Id");

                        eb.ToTable(tb => tb.HasCheckConstraint("idConstraint", "Id <> 0"));
                        eb.ToTable(tb => tb.HasCheckConstraint("anotherConstraint", "Id <> -1"));
                    });
            }
        }

        [ConditionalFact]
        public void Triggers()
            => Test(
                new TriggersContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    var dataEntity = model.GetEntityTypes().Single();

                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => dataEntity.GetCheckConstraints()).Message);
                });

        public class TriggersContext : SqlServerContextBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Data>(
                    eb =>
                    {
                        eb.Property<int>("Id");
                        eb.HasKey("Id");

                        eb.ToTable(
                            tb =>
                            {
                                tb.HasTrigger("Trigger1");
                                tb.HasTrigger("Trigger2");
                            });
                    });
            }
        }

        [ConditionalFact]
        public void Sqlite()
            => Test(
                new SqliteContext(),
                new CompiledModelCodeGenerationOptions { ModelNamespace = "Microsoft.EntityFrameworkCore.Metadata" },
                model =>
                {
                    var dataEntity = model.FindEntityType(typeof(Data));

                    Assert.Equal(typeof(Data).FullName, dataEntity.Name);
                    Assert.False(dataEntity.HasSharedClrType);
                    Assert.False(dataEntity.IsPropertyBag);
                    Assert.False(dataEntity.IsOwned());
                    Assert.IsType<ConstructorBinding>(dataEntity.ConstructorBinding);
                    Assert.Null(dataEntity.FindIndexerPropertyInfo());
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, dataEntity.GetChangeTrackingStrategy());
                    Assert.Equal("Data", dataEntity.GetTableName());
                    Assert.Null(dataEntity.GetSchema());

                    var point = dataEntity.FindProperty("Point");
                    Assert.Equal(typeof(Point), point.ClrType);
                    Assert.True(point.IsNullable);
                    Assert.Equal(ValueGenerated.Never, point.ValueGenerated);
                    Assert.Equal("Point", point.GetColumnName());
                    Assert.Equal("POINT", point.GetColumnType());
                    Assert.Null(point.GetValueConverter());
                    Assert.IsType<GeometryValueComparer<Point>>(point.GetValueComparer());
                    Assert.IsType<GeometryValueComparer<Point>>(point.GetKeyValueComparer());
                    Assert.Null(point.GetSrid());

                    var manyTypesType = model.FindEntityType(typeof(ManyTypes));

                    Assert.Equal(typeof(ManyTypes).FullName, manyTypesType.Name);
                    Assert.False(manyTypesType.HasSharedClrType);
                    Assert.False(manyTypesType.IsPropertyBag);
                    Assert.False(manyTypesType.IsOwned());
                    Assert.IsType<ConstructorBinding>(manyTypesType.ConstructorBinding);
                    Assert.Null(manyTypesType.FindIndexerPropertyInfo());
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, manyTypesType.GetChangeTrackingStrategy());
                    Assert.Equal("ManyTypes", manyTypesType.GetTableName());
                    Assert.Null(manyTypesType.GetSchema());

                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetCollation()).Message);
                    Assert.Empty(model.GetAnnotations());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => model.GetPropertyAccessMode()).Message);

                    Assert.Null(model.FindEntityType(typeof(AbstractBase)));
                    var principalBase = model.FindEntityType(typeof(PrincipalBase));
                    Assert.Equal(typeof(PrincipalBase).FullName, principalBase.Name);
                    Assert.False(principalBase.HasSharedClrType);
                    Assert.False(principalBase.IsPropertyBag);
                    Assert.False(principalBase.IsOwned());
                    Assert.Null(principalBase.BaseType);
                    Assert.IsType<ConstructorBinding>(principalBase.ConstructorBinding);
                    Assert.Null(principalBase.FindIndexerPropertyInfo());
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, principalBase.GetChangeTrackingStrategy());
                    Assert.Null(principalBase.GetQueryFilter());
                    Assert.Equal("PrincipalBase", principalBase.GetTableName());
                    Assert.Equal("mySchema", principalBase.GetSchema());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalBase.GetSeedData()).Message);

                    var principalId = principalBase.FindProperty(nameof(PrincipalBase.Id));
                    Assert.Equal(
                        new[] { RelationalAnnotationNames.RelationalOverrides },
                        principalId.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(typeof(long?), principalId.ClrType);
                    Assert.Equal(typeof(long?), principalId.PropertyInfo.PropertyType);
                    Assert.Equal(typeof(long?), principalId.FieldInfo.FieldType);
                    Assert.False(principalId.IsNullable);
                    Assert.Equal(ValueGenerated.Never, principalId.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Throw, principalId.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Save, principalId.GetBeforeSaveBehavior());
                    Assert.Null(principalId[CoreAnnotationNames.BeforeSaveBehavior]);
                    Assert.Null(principalId[CoreAnnotationNames.AfterSaveBehavior]);
                    Assert.Equal("Id", principalId.GetColumnName());
                    Assert.Equal("Id", principalId.GetColumnName(StoreObjectIdentifier.Table("PrincipalBase", "mySchema")));
                    Assert.Equal("DerivedId", principalId.GetColumnName(StoreObjectIdentifier.Table("PrincipalDerived")));
                    Assert.Equal("INTEGER", principalId.GetColumnType());
                    Assert.Null(principalId.GetValueConverter());
                    Assert.NotNull(principalId.GetValueComparer());
                    Assert.NotNull(principalId.GetKeyValueComparer());

                    var pointProperty = principalBase.FindProperty("Point");
                    Assert.Equal(typeof(Point), pointProperty.ClrType);
                    Assert.True(pointProperty.IsNullable);
                    Assert.Equal(ValueGenerated.OnAdd, pointProperty.ValueGenerated);
                    Assert.Equal("Point", pointProperty.GetColumnName());
                    Assert.Equal("geometry", pointProperty.GetColumnType());
                    Assert.Equal(0, ((Point)pointProperty.GetDefaultValue()).SRID);
                    Assert.IsType<CastingConverter<Point, Point>>(pointProperty.GetValueConverter());
                    Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetValueComparer());
                    Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetKeyValueComparer());
                    Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetProviderValueComparer());
                    Assert.Null(pointProperty[CoreAnnotationNames.PropertyAccessMode]);

                    Assert.Null(principalBase.FindDiscriminatorProperty());

                    var principalAlternateId = principalBase.FindProperty(nameof(PrincipalBase.AlternateId));
                    var compositeIndex = principalBase.GetIndexes().Single();
                    Assert.Equal(PropertyAccessMode.FieldDuringConstruction, principalAlternateId.GetPropertyAccessMode());
                    Assert.Empty(compositeIndex.GetAnnotations());
                    Assert.Equal(new[] { principalAlternateId, principalId }, compositeIndex.Properties);
                    Assert.False(compositeIndex.IsUnique);
                    Assert.Null(compositeIndex.Name);
                    Assert.Equal("IX_PrincipalBase_AlternateId_Id", compositeIndex.GetDatabaseName());

                    Assert.Equal(new[] { compositeIndex }, principalAlternateId.GetContainingIndexes());

                    Assert.Equal(2, principalBase.GetKeys().Count());

                    var principalAlternateKey = principalBase.GetKeys().First();
                    Assert.Same(principalId, principalAlternateKey.Properties.Single());
                    Assert.False(principalAlternateKey.IsPrimaryKey());
                    Assert.Equal("AK_PrincipalBase_Id", principalAlternateKey.GetName());

                    var principalKey = principalBase.GetKeys().Last();
                    Assert.Equal(
                        new[] { RelationalAnnotationNames.Name },
                        principalKey.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(new[] { principalId, principalAlternateId }, principalKey.Properties);
                    Assert.True(principalKey.IsPrimaryKey());
                    Assert.Equal("PK", principalKey.GetName());
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => principalKey.IsClustered()).Message);

                    Assert.Equal(new[] { principalAlternateKey, principalKey }, principalId.GetContainingKeys());

                    var referenceOwnedNavigation = principalBase.GetNavigations().Single();
                    Assert.Equal(
                        new[] { CoreAnnotationNames.EagerLoaded },
                        referenceOwnedNavigation.GetAnnotations().Select(a => a.Name));
                    Assert.Equal(nameof(PrincipalBase.Owned), referenceOwnedNavigation.Name);
                    Assert.False(referenceOwnedNavigation.IsCollection);
                    Assert.True(referenceOwnedNavigation.IsEagerLoaded);
                    Assert.False(referenceOwnedNavigation.IsOnDependent);
                    Assert.Equal(typeof(OwnedType), referenceOwnedNavigation.ClrType);
                    Assert.Equal("_ownedField", referenceOwnedNavigation.FieldInfo.Name);
                    Assert.Equal(nameof(PrincipalBase.Owned), referenceOwnedNavigation.PropertyInfo.Name);
                    Assert.Null(referenceOwnedNavigation.Inverse);
                    Assert.Equal(principalBase, referenceOwnedNavigation.DeclaringEntityType);
                    Assert.Equal(PropertyAccessMode.Field, referenceOwnedNavigation.GetPropertyAccessMode());
                    Assert.Null(referenceOwnedNavigation[CoreAnnotationNames.PropertyAccessMode]);

                    var referenceOwnedType = referenceOwnedNavigation.TargetEntityType;
                    Assert.Equal(typeof(PrincipalBase).FullName + ".Owned#OwnedType", referenceOwnedType.Name);
                    Assert.Equal(typeof(OwnedType), referenceOwnedType.ClrType);
                    Assert.True(referenceOwnedType.HasSharedClrType);
                    Assert.False(referenceOwnedType.IsPropertyBag);
                    Assert.True(referenceOwnedType.IsOwned());
                    Assert.Null(referenceOwnedType.BaseType);
                    Assert.False(referenceOwnedType.IsMemoryOptimized());
                    Assert.IsType<ConstructorBinding>(referenceOwnedType.ConstructorBinding);
                    Assert.Null(referenceOwnedType.FindIndexerPropertyInfo());
                    Assert.Equal(
                        ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues,
                        referenceOwnedType.GetChangeTrackingStrategy());
                    Assert.Null(referenceOwnedType.GetQueryFilter());
                    Assert.Null(referenceOwnedType[CoreAnnotationNames.PropertyAccessMode]);
                    Assert.Null(referenceOwnedType[CoreAnnotationNames.NavigationAccessMode]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => referenceOwnedType.GetPropertyAccessMode()).Message);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => referenceOwnedType.GetNavigationAccessMode()).Message);

                    var principalTable = StoreObjectIdentifier.Create(referenceOwnedType, StoreObjectType.Table).Value;

                    var ownedId = referenceOwnedType.FindProperty("PrincipalBaseId");
                    Assert.True(ownedId.IsPrimaryKey());

                    var detailsProperty = referenceOwnedType.FindProperty(nameof(OwnedType.Details));

                    var ownedFragment = referenceOwnedType.GetMappingFragments().Single();
                    Assert.Equal(nameof(OwnedType.Details), detailsProperty.GetColumnName(ownedFragment.StoreObject));
                    Assert.Null(detailsProperty.GetColumnName(principalTable));

                    var referenceOwnership = referenceOwnedNavigation.ForeignKey;
                    Assert.Empty(referenceOwnership.GetAnnotations());
                    Assert.Same(referenceOwnership, referenceOwnedType.FindOwnership());
                    Assert.True(referenceOwnership.IsOwnership);
                    Assert.True(referenceOwnership.IsRequired);
                    Assert.True(referenceOwnership.IsRequiredDependent);
                    Assert.True(referenceOwnership.IsUnique);
                    Assert.Null(referenceOwnership.DependentToPrincipal);
                    Assert.Same(referenceOwnedNavigation, referenceOwnership.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.Cascade, referenceOwnership.DeleteBehavior);
                    Assert.Equal(2, referenceOwnership.Properties.Count());
                    Assert.Same(principalKey, referenceOwnership.PrincipalKey);

                    var ownedServiceProperty = referenceOwnedType.GetServiceProperties().Single();
                    Assert.Empty(ownedServiceProperty.GetAnnotations());
                    Assert.Equal(typeof(DbContext), ownedServiceProperty.ClrType);
                    Assert.Equal(typeof(DbContext), ownedServiceProperty.PropertyInfo.PropertyType);
                    Assert.Null(ownedServiceProperty.FieldInfo);
                    Assert.Same(referenceOwnedType, ownedServiceProperty.DeclaringEntityType);
                    var ownedServicePropertyBinding = ownedServiceProperty.ParameterBinding;
                    Assert.IsType<ContextParameterBinding>(ownedServicePropertyBinding);
                    Assert.Equal(typeof(DbContext), ownedServicePropertyBinding.ServiceType);
                    Assert.Equal(ownedServiceProperty, ownedServicePropertyBinding.ConsumedProperties.Single());
                    Assert.Equal(PropertyAccessMode.PreferField, ownedServiceProperty.GetPropertyAccessMode());
                    Assert.Null(ownedServiceProperty[CoreAnnotationNames.PropertyAccessMode]);

                    var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>));
                    Assert.Equal(principalBase, principalDerived.BaseType);
                    Assert.Equal(
                        "Microsoft.EntityFrameworkCore.Scaffolding.Internal.CSharpRuntimeModelCodeGeneratorTest+"
                        + "PrincipalDerived<Microsoft.EntityFrameworkCore.Scaffolding.Internal.CSharpRuntimeModelCodeGeneratorTest+DependentBase<byte?>>",
                        principalDerived.Name);
                    Assert.False(principalDerived.IsOwned());
                    Assert.IsType<ConstructorBinding>(principalDerived.ConstructorBinding);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, principalDerived.GetChangeTrackingStrategy());
                    Assert.Equal("PrincipalDerived<DependentBase<byte?>>", principalDerived.GetDiscriminatorValue());

                    var tptForeignKey = principalDerived.GetForeignKeys().Single();
                    Assert.False(tptForeignKey.IsOwnership);
                    Assert.True(tptForeignKey.IsRequired);
                    Assert.False(tptForeignKey.IsRequiredDependent);
                    Assert.True(tptForeignKey.IsUnique);
                    Assert.Null(tptForeignKey.DependentToPrincipal);
                    Assert.Null(tptForeignKey.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.Cascade, tptForeignKey.DeleteBehavior);
                    Assert.Equal(principalKey.Properties, tptForeignKey.Properties);
                    Assert.Same(principalKey, tptForeignKey.PrincipalKey);

                    Assert.Equal(2, principalDerived.GetDeclaredNavigations().Count());
                    var dependentNavigation = principalDerived.GetDeclaredNavigations().First();
                    Assert.Equal("Dependent", dependentNavigation.Name);
                    Assert.Equal("Dependent", dependentNavigation.PropertyInfo.Name);
                    Assert.Equal("<Dependent>k__BackingField", dependentNavigation.FieldInfo.Name);
                    Assert.False(dependentNavigation.IsCollection);
                    Assert.True(dependentNavigation.IsEagerLoaded);
                    Assert.False(dependentNavigation.LazyLoadingEnabled);
                    Assert.False(dependentNavigation.IsOnDependent);
                    Assert.Equal(principalDerived, dependentNavigation.DeclaringEntityType);
                    Assert.Equal("Principal", dependentNavigation.Inverse.Name);

                    var ownedCollectionNavigation = principalDerived.GetDeclaredNavigations().Last();
                    Assert.Equal("ManyOwned", ownedCollectionNavigation.Name);
                    Assert.Null(ownedCollectionNavigation.PropertyInfo);
                    Assert.Equal("ManyOwned", ownedCollectionNavigation.FieldInfo.Name);
                    Assert.Equal(typeof(ICollection<OwnedType>), ownedCollectionNavigation.ClrType);
                    Assert.True(ownedCollectionNavigation.IsCollection);
                    Assert.True(ownedCollectionNavigation.IsEagerLoaded);
                    Assert.False(ownedCollectionNavigation.IsOnDependent);
                    Assert.Null(ownedCollectionNavigation.Inverse);
                    Assert.Equal(principalDerived, ownedCollectionNavigation.DeclaringEntityType);

                    var collectionOwnedType = ownedCollectionNavigation.TargetEntityType;
                    Assert.Equal(principalDerived.Name + ".ManyOwned#OwnedType", collectionOwnedType.Name);
                    Assert.Equal(typeof(OwnedType), collectionOwnedType.ClrType);
                    Assert.True(collectionOwnedType.HasSharedClrType);
                    Assert.False(collectionOwnedType.IsPropertyBag);
                    Assert.True(collectionOwnedType.IsOwned());
                    Assert.True(collectionOwnedType.IsMemoryOptimized());
                    Assert.Null(collectionOwnedType[RelationalAnnotationNames.IsTableExcludedFromMigrations]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => collectionOwnedType.IsTableExcludedFromMigrations()).Message);
                    Assert.Null(collectionOwnedType.BaseType);
                    Assert.IsType<ConstructorBinding>(collectionOwnedType.ConstructorBinding);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, collectionOwnedType.GetChangeTrackingStrategy());

                    var collectionOwnership = ownedCollectionNavigation.ForeignKey;
                    Assert.Same(collectionOwnership, collectionOwnedType.FindOwnership());
                    Assert.True(collectionOwnership.IsOwnership);
                    Assert.True(collectionOwnership.IsRequired);
                    Assert.False(collectionOwnership.IsRequiredDependent);
                    Assert.False(collectionOwnership.IsUnique);
                    Assert.Null(collectionOwnership.DependentToPrincipal);
                    Assert.Same(ownedCollectionNavigation, collectionOwnership.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.Cascade, collectionOwnership.DeleteBehavior);
                    Assert.Equal(2, collectionOwnership.Properties.Count());

                    var derivedSkipNavigation = principalDerived.GetDeclaredSkipNavigations().Single();
                    Assert.Equal("Principals", derivedSkipNavigation.Name);
                    Assert.Equal("Principals", derivedSkipNavigation.PropertyInfo.Name);
                    Assert.Equal("<Principals>k__BackingField", derivedSkipNavigation.FieldInfo.Name);
                    Assert.Equal(typeof(ICollection<PrincipalBase>), derivedSkipNavigation.ClrType);
                    Assert.True(derivedSkipNavigation.IsCollection);
                    Assert.True(derivedSkipNavigation.IsEagerLoaded);
                    Assert.False(derivedSkipNavigation.LazyLoadingEnabled);
                    Assert.False(derivedSkipNavigation.IsOnDependent);
                    Assert.Equal(principalDerived, derivedSkipNavigation.DeclaringEntityType);
                    Assert.Equal("Deriveds", derivedSkipNavigation.Inverse.Name);
                    Assert.Same(principalBase.GetSkipNavigations().Single(), derivedSkipNavigation.Inverse);

                    Assert.Same(derivedSkipNavigation, derivedSkipNavigation.ForeignKey.GetReferencingSkipNavigations().Single());
                    Assert.Same(
                        derivedSkipNavigation.Inverse, derivedSkipNavigation.Inverse.ForeignKey.GetReferencingSkipNavigations().Single());

                    Assert.Equal(new[] { derivedSkipNavigation.Inverse, derivedSkipNavigation }, principalDerived.GetSkipNavigations());

                    var joinType = derivedSkipNavigation.JoinEntityType;

                    Assert.Equal("PrincipalBasePrincipalDerived<DependentBase<byte?>>", joinType.Name);
                    Assert.Equal(typeof(Dictionary<string, object>), joinType.ClrType);
                    Assert.True(joinType.HasSharedClrType);
                    Assert.True(joinType.IsPropertyBag);
                    Assert.False(joinType.IsOwned());
                    Assert.Null(joinType.BaseType);
                    Assert.IsType<ConstructorBinding>(joinType.ConstructorBinding);
                    Assert.Equal("Item", joinType.FindIndexerPropertyInfo().Name);
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, joinType.GetChangeTrackingStrategy());
                    Assert.Null(joinType[RelationalAnnotationNames.Comment]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => joinType.GetComment()).Message);
                    Assert.Null(joinType.GetQueryFilter());

                    var rowid = joinType.GetProperties().Single(p => !p.IsForeignKey());
                    Assert.Equal(typeof(byte[]), rowid.ClrType);
                    Assert.True(rowid.IsIndexerProperty());
                    Assert.Same(joinType.FindIndexerPropertyInfo(), rowid.PropertyInfo);
                    Assert.Null(rowid.FieldInfo);
                    Assert.True(rowid.IsNullable);
                    Assert.False(rowid.IsShadowProperty());
                    Assert.True(rowid.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.OnAddOrUpdate, rowid.ValueGenerated);
                    Assert.Equal("rowid", rowid.GetColumnName());
                    Assert.Equal("BLOB", rowid.GetColumnType());
                    Assert.Null(rowid[RelationalAnnotationNames.Comment]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => rowid.GetComment()).Message);
                    Assert.Null(rowid[RelationalAnnotationNames.ColumnOrder]);
                    Assert.Equal(
                        CoreStrings.RuntimeModelMissingData,
                        Assert.Throws<InvalidOperationException>(() => rowid.GetColumnOrder()).Message);
                    Assert.Null(rowid.GetValueConverter());
                    Assert.NotNull(rowid.GetValueComparer());
                    Assert.NotNull(rowid.GetKeyValueComparer());

                    var dependentForeignKey = dependentNavigation.ForeignKey;
                    Assert.False(dependentForeignKey.IsOwnership);
                    Assert.True(dependentForeignKey.IsRequired);
                    Assert.False(dependentForeignKey.IsRequiredDependent);
                    Assert.True(dependentForeignKey.IsUnique);
                    Assert.Same(dependentNavigation.Inverse, dependentForeignKey.DependentToPrincipal);
                    Assert.Same(dependentNavigation, dependentForeignKey.PrincipalToDependent);
                    Assert.Equal(DeleteBehavior.ClientNoAction, dependentForeignKey.DeleteBehavior);
                    Assert.Equal(new[] { "PrincipalId", "PrincipalAlternateId" }, dependentForeignKey.Properties.Select(p => p.Name));
                    Assert.Same(principalKey, dependentForeignKey.PrincipalKey);

                    var dependentBase = dependentNavigation.TargetEntityType;

                    Assert.False(dependentBase.GetIsDiscriminatorMappingComplete());
                    var principalDiscriminator = dependentBase.FindDiscriminatorProperty();
                    Assert.IsType<DiscriminatorValueGenerator>(
                        principalDiscriminator.GetValueGeneratorFactory()(principalDiscriminator, dependentBase));
                    Assert.Equal(Enum1.One, dependentBase.GetDiscriminatorValue());

                    var dependentBaseForeignKey = dependentBase.GetForeignKeys().Single(fk => fk != dependentForeignKey);
                    var dependentForeignKeyProperty = dependentBaseForeignKey.Properties.Single();

                    Assert.Equal(
                        new[] { dependentBaseForeignKey, dependentForeignKey }, dependentForeignKeyProperty.GetContainingForeignKeys());

                    var dependentDerived = dependentBase.GetDerivedTypes().Single();
                    Assert.Equal(Enum1.Two, dependentDerived.GetDiscriminatorValue());

                    Assert.Equal(2, dependentDerived.GetDeclaredProperties().Count());

                    var dependentData = dependentDerived.GetDeclaredProperties().First();
                    Assert.Equal(typeof(string), dependentData.ClrType);
                    Assert.Equal("Data", dependentData.Name);
                    Assert.Equal("Data", dependentData.PropertyInfo.Name);
                    Assert.Equal("<Data>k__BackingField", dependentData.FieldInfo.Name);
                    Assert.True(dependentData.IsNullable);
                    Assert.False(dependentData.IsShadowProperty());
                    Assert.False(dependentData.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, dependentData.ValueGenerated);
                    Assert.Equal("Data", dependentData.GetColumnName());
                    Assert.Equal("TEXT", dependentData.GetColumnType());
                    Assert.Equal(20, dependentData.GetMaxLength());
                    Assert.False(dependentData.IsUnicode());
                    Assert.True(dependentData.IsFixedLength());
                    Assert.Null(dependentData.GetPrecision());
                    Assert.Null(dependentData.GetScale());

                    var dependentMoney = dependentDerived.GetDeclaredProperties().Last();
                    Assert.Equal(typeof(decimal), dependentMoney.ClrType);
                    Assert.Equal("Money", dependentMoney.Name);
                    Assert.Null(dependentMoney.PropertyInfo);
                    Assert.Null(dependentMoney.FieldInfo);
                    Assert.False(dependentMoney.IsNullable);
                    Assert.True(dependentMoney.IsShadowProperty());
                    Assert.False(dependentMoney.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, dependentMoney.ValueGenerated);
                    Assert.Equal("Money", dependentMoney.GetColumnName());
                    Assert.Equal("TEXT", dependentMoney.GetColumnType());
                    Assert.Null(dependentMoney.GetMaxLength());
                    Assert.Null(dependentMoney.IsUnicode());
                    Assert.Null(dependentMoney.IsFixedLength());
                    Assert.Equal(9, dependentMoney.GetPrecision());
                    Assert.Equal(3, dependentMoney.GetScale());

                    Assert.Equal(
                        new[]
                        {
                            derivedSkipNavigation.ForeignKey,
                            tptForeignKey,
                            referenceOwnership,
                            collectionOwnership,
                            dependentForeignKey,
                            derivedSkipNavigation.Inverse.ForeignKey
                        },
                        principalKey.GetReferencingForeignKeys());

                    Assert.Equal(
                        new[] { dependentBaseForeignKey, tptForeignKey, referenceOwnership, derivedSkipNavigation.Inverse.ForeignKey },
                        principalBase.GetReferencingForeignKeys());

                    Assert.Equal(
                        new[] { derivedSkipNavigation.ForeignKey, collectionOwnership, dependentForeignKey },
                        principalDerived.GetDeclaredReferencingForeignKeys());

                    Assert.Equal(
                        new[]
                        {
                            dataEntity,
                            dependentBase,
                            dependentDerived,
                            manyTypesType,
                            principalBase,
                            referenceOwnedType,
                            principalDerived,
                            collectionOwnedType,
                            joinType
                        },
                        model.GetEntityTypes());
                },
                typeof(SqliteNetTopologySuiteDesignTimeServices));

        public class SqliteContext : DbContext
        {
            private readonly bool _jsonColumns;

            public SqliteContext(bool jsonColumns = false)
            {
                _jsonColumns = jsonColumns;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options
                    .EnableServiceProviderCaching(false)
                    .UseSqlite(o => o.UseNetTopologySuite());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

                modelBuilder.Entity<Data>(
                    eb =>
                    {
                        eb.Property<int>("Id");
                        eb.HasKey("Id");

                        eb.Property<Point>("Point")
                            .HasSrid(1101);
                    });

                modelBuilder.Entity<PrincipalBase>(
                    eb =>
                    {
                        if (!_jsonColumns)
                        {
                            eb.Property(e => e.Id).Metadata.SetColumnName("DerivedId", StoreObjectIdentifier.Table("PrincipalDerived"));
                        }

                        eb.Property(e => e.FlagsEnum2)
                            .HasSentinel(AFlagsEnum.C | AFlagsEnum.B);

                        eb.Property(e => e.AlternateId)
                            .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);

                        eb.Property<Point>("Point")
                            .HasColumnType("geometry")
                            .HasDefaultValue(
                                NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0).CreatePoint(new CoordinateZM(0, 0, 0, 0)))
                            .HasConversion<CastingConverter<Point, Point>, CustomValueComparer<Point>, CustomValueComparer<Point>>();

                        eb.HasIndex(e => new { e.AlternateId, e.Id });

                        eb.HasKey(e => new { e.Id, e.AlternateId })
                            .HasName("PK");

                        eb.HasAlternateKey(e => e.Id);

                        eb.Property(e => e.AlternateId).Metadata.SetJsonValueReaderWriterType(
                            _jsonColumns
                                ? typeof(MyJsonGuidReaderWriter)
                                : typeof(JsonGuidReaderWriter));

                        eb.OwnsOne(
                            e => e.Owned, ob =>
                            {
                                ob.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
                                ob.UsePropertyAccessMode(PropertyAccessMode.Field);
                                ob.Property(e => e.Details);

                                if (_jsonColumns)
                                {
                                    ob.ToJson();
                                }
                                else
                                {
                                    ob.ToTable(
                                        "PrincipalBase", "mySchema",
                                        t => t.Property("PrincipalBaseId").UseIdentityColumn(2, 3));

                                    ob.SplitToTable("Details", s => s.Property(e => e.Details));

                                    ob.HasData(
                                        new
                                        {
                                            Number = 10,
                                            PrincipalBaseId = 1L,
                                            PrincipalBaseAlternateId = new Guid()
                                        });
                                }
                            });

                        eb.Navigation(e => e.Owned).IsRequired().HasField("_ownedField")
                            .UsePropertyAccessMode(PropertyAccessMode.Field);

                        if (!_jsonColumns)
                        {
                            eb.HasData(new PrincipalBase { Id = 1, AlternateId = new Guid() });

                            eb.ToTable("PrincipalBase", "mySchema");
                        }
                    });

                modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
                    eb =>
                    {
                        eb.HasOne(e => e.Dependent).WithOne(e => e.Principal)
                            .HasForeignKey<DependentBase<byte?>>()
                            .OnDelete(DeleteBehavior.ClientNoAction);

                        eb.Navigation(e => e.Dependent).AutoInclude().EnableLazyLoading(false);

                        eb.OwnsMany(
                            typeof(OwnedType).FullName, "ManyOwned", ob =>
                            {
                                if (_jsonColumns)
                                {
                                    ob.ToJson();
                                }
                                else
                                {
                                    ob.ToTable("ManyOwned", t => t.IsMemoryOptimized().ExcludeFromMigrations());
                                }
                            });

                        eb.HasMany(e => e.Principals).WithMany(e => (ICollection<PrincipalDerived<DependentBase<byte?>>>)e.Deriveds)
                            .UsingEntity(
                                jb =>
                                {
                                    jb.ToTable(tb => tb.HasComment("Join table"));
                                    jb.Property<byte[]>("rowid")
                                        .IsRowVersion()
                                        .HasComment("RowVersion")
                                        .HasColumnOrder(1);
                                });

                        eb.Navigation(e => e.Principals).AutoInclude().EnableLazyLoading(false);

                        if (!_jsonColumns)
                        {
                            eb.ToTable("PrincipalDerived");
                        }
                    });

                modelBuilder.Entity<DependentBase<byte?>>(
                    eb =>
                    {
                        eb.Property<byte?>("Id");

                        eb.HasKey("PrincipalId", "PrincipalAlternateId");

                        eb.HasOne<PrincipalBase>().WithOne()
                            .HasForeignKey<DependentBase<byte?>>("PrincipalId")
                            .HasPrincipalKey<PrincipalBase>(e => e.Id);

                        eb.HasDiscriminator<Enum1>("EnumDiscriminator")
                            .HasValue(Enum1.One)
                            .HasValue<DependentDerived<byte?>>(Enum1.Two)
                            .IsComplete(false);
                    });

                modelBuilder.Entity<DependentDerived<byte?>>(
                    eb =>
                    {
                        eb.Property<string>("Data")
                            .HasMaxLength(20)
                            .IsFixedLength()
                            .IsUnicode(false);

                        eb.Property<decimal>("Money")
                            .HasPrecision(9, 3);
                    });

                modelBuilder.Entity<ManyTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion<ManyTypesIdConverter>().ValueGeneratedOnAdd();
                        b.HasKey(e => e.Id);

                        b.Property(e => e.Enum8AsString).HasConversion<string>();
                        b.Property(e => e.Enum16AsString).HasConversion<string>();
                        b.Property(e => e.Enum32AsString).HasConversion<string>();
                        b.Property(e => e.Enum64AsString).HasConversion<string>();
                        b.Property(e => e.EnumU8AsString).HasConversion<string>();
                        b.Property(e => e.EnumU16AsString).HasConversion<string>();
                        b.Property(e => e.EnumU32AsString).HasConversion<string>();
                        b.Property(e => e.EnumU64AsString).HasConversion<string>();

                        b.PrimitiveCollection(e => e.Enum8AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum16AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum32AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum64AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU8AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU16AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU32AsStringCollection).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU64AsStringCollection).ElementType(b => b.HasConversion<string>());

                        b.PrimitiveCollection(e => e.Enum8AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum16AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum32AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.Enum64AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU8AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU16AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU32AsStringArray).ElementType(b => b.HasConversion<string>());
                        b.PrimitiveCollection(e => e.EnumU64AsStringArray).ElementType(b => b.HasConversion<string>());

                        b.Property(e => e.BoolToStringConverterProperty).HasConversion(new BoolToStringConverter("A", "B"));
                        b.Property(e => e.BoolToTwoValuesConverterProperty).HasConversion(new BoolToTwoValuesConverter<byte>(0, 1));
                        b.Property(e => e.BoolToZeroOneConverterProperty).HasConversion<BoolToZeroOneConverter<short>>();
                        b.Property(e => e.BytesToStringConverterProperty).HasConversion<BytesToStringConverter>();
                        b.Property(e => e.CastingConverterProperty).HasConversion<CastingConverter<int, decimal>>();
                        b.Property(e => e.CharToStringConverterProperty).HasConversion<CharToStringConverter>();
                        b.Property(e => e.DateOnlyToStringConverterProperty).HasConversion<DateOnlyToStringConverter>();
                        b.Property(e => e.DateTimeOffsetToBinaryConverterProperty).HasConversion<DateTimeOffsetToBinaryConverter>();
                        b.Property(e => e.DateTimeOffsetToBytesConverterProperty).HasConversion<DateTimeOffsetToBytesConverter>();
                        b.Property(e => e.DateTimeOffsetToStringConverterProperty).HasConversion<DateTimeOffsetToStringConverter>();
                        b.Property(e => e.DateTimeToBinaryConverterProperty).HasConversion<DateTimeToBinaryConverter>();
                        b.Property(e => e.DateTimeToStringConverterProperty).HasConversion<DateTimeToStringConverter>();
                        b.Property(e => e.EnumToNumberConverterProperty).HasConversion<EnumToNumberConverter<Enum32, int>>();
                        b.Property(e => e.EnumToStringConverterProperty).HasConversion<EnumToStringConverter<Enum32>>();
                        b.Property(e => e.GuidToBytesConverterProperty).HasConversion<GuidToBytesConverter>();
                        b.Property(e => e.GuidToStringConverterProperty).HasConversion<GuidToStringConverter>();
                        b.Property(e => e.IPAddressToBytesConverterProperty).HasConversion<IPAddressToBytesConverter>();
                        b.Property(e => e.IPAddressToStringConverterProperty).HasConversion<IPAddressToStringConverter>();
                        b.Property(e => e.IntNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<int>>();
                        b.Property(e => e.DecimalNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<decimal>>();
                        b.Property(e => e.DoubleNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<double>>();
                        b.Property(e => e.IntNumberToStringConverterProperty).HasConversion<NumberToStringConverter<int>>();
                        b.Property(e => e.DecimalNumberToStringConverterProperty).HasConversion<NumberToStringConverter<decimal>>();
                        b.Property(e => e.DoubleNumberToStringConverterProperty).HasConversion<NumberToStringConverter<double>>();
                        b.Property(e => e.PhysicalAddressToBytesConverterProperty).HasConversion<PhysicalAddressToBytesConverter>();
                        b.Property(e => e.PhysicalAddressToStringConverterProperty).HasConversion<PhysicalAddressToStringConverter>();
                        b.Property(e => e.StringToBoolConverterProperty).HasConversion<StringToBoolConverter>();
                        b.Property(e => e.StringToBytesConverterProperty).HasConversion(new StringToBytesConverter(Encoding.UTF32));
                        b.Property(e => e.StringToCharConverterProperty).HasConversion<StringToCharConverter>();
                        b.Property(e => e.StringToDateOnlyConverterProperty).HasConversion<StringToDateOnlyConverter>();
                        b.Property(e => e.StringToDateTimeConverterProperty).HasConversion<StringToDateTimeConverter>();
                        b.Property(e => e.StringToDateTimeOffsetConverterProperty).HasConversion<StringToDateTimeOffsetConverter>();
                        b.Property(e => e.StringToEnumConverterProperty).HasConversion<StringToEnumConverter<EnumU32>>();
                        b.Property(e => e.StringToIntNumberConverterProperty).HasConversion<StringToNumberConverter<int>>();
                        b.Property(e => e.StringToDecimalNumberConverterProperty).HasConversion<StringToNumberConverter<decimal>>();
                        b.Property(e => e.StringToDoubleNumberConverterProperty).HasConversion<StringToNumberConverter<double>>();
                        b.Property(e => e.StringToTimeOnlyConverterProperty).HasConversion<StringToTimeOnlyConverter>();
                        b.Property(e => e.StringToTimeSpanConverterProperty).HasConversion<StringToTimeSpanConverter>();
                        b.Property(e => e.StringToUriConverterProperty).HasConversion<StringToUriConverter>();
                        b.Property(e => e.TimeOnlyToStringConverterProperty).HasConversion<TimeOnlyToStringConverter>();
                        b.Property(e => e.TimeOnlyToTicksConverterProperty).HasConversion<TimeOnlyToTicksConverter>();
                        b.Property(e => e.TimeSpanToStringConverterProperty).HasConversion<TimeSpanToStringConverter>();
                        b.Property(e => e.TimeSpanToTicksConverterProperty).HasConversion<TimeSpanToTicksConverter>();
                        b.Property(e => e.UriToStringConverterProperty).HasConversion<UriToStringConverter>();
                        b.Property(e => e.NullIntToNullStringConverterProperty).HasConversion<NullIntToNullStringConverter>();
                    });
            }
        }

        public class NullIntToNullStringConverter : ValueConverter<int?, string>
        {
            public NullIntToNullStringConverter()
                : base(v => v == null ? null : v.ToString()!, v => v == null || v == "<null>" ? null : int.Parse(v), convertsNulls: true)
            {
            }
        }

        [ConditionalFact]
        public void Cosmos()
            => Test(
                new CosmosContext(),
                new CompiledModelCodeGenerationOptions(),
                model =>
                {
                    Assert.Single((IEnumerable)model.GetEntityTypes());
                    var dataEntity = model.FindEntityType(typeof(Data));
                    Assert.Equal(typeof(Data).FullName, dataEntity.Name);
                    Assert.False(dataEntity.HasSharedClrType);
                    Assert.False(dataEntity.IsPropertyBag);
                    Assert.False(dataEntity.IsOwned());
                    Assert.IsType<ConstructorBinding>(dataEntity.ConstructorBinding);
                    Assert.Null(dataEntity.FindIndexerPropertyInfo());
                    Assert.Equal(ChangeTrackingStrategy.Snapshot, dataEntity.GetChangeTrackingStrategy());
                    Assert.Equal("DataContainer", dataEntity.GetContainer());
                    Assert.Null(dataEntity.FindDiscriminatorProperty());

                    var id = dataEntity.FindProperty("Id");
                    Assert.Equal(typeof(int), id.ClrType);
                    Assert.Null(id.PropertyInfo);
                    Assert.Null(id.FieldInfo);
                    Assert.False(id.IsNullable);
                    Assert.False(id.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, id.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Throw, id.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Save, id.GetBeforeSaveBehavior());
                    Assert.Equal("Id", CosmosPropertyExtensions.GetJsonPropertyName(id));
                    Assert.Null(id.GetValueGeneratorFactory());
                    Assert.Null(id.GetValueConverter());
                    Assert.NotNull(id.GetValueComparer());
                    Assert.NotNull(id.GetKeyValueComparer());

                    var storeId = dataEntity.FindProperty("__id");
                    Assert.Equal(typeof(string), storeId.ClrType);
                    Assert.Null(storeId.PropertyInfo);
                    Assert.Null(storeId.FieldInfo);
                    Assert.False(storeId.IsNullable);
                    Assert.False(storeId.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, storeId.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Throw, storeId.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Save, storeId.GetBeforeSaveBehavior());
                    Assert.Equal("id", CosmosPropertyExtensions.GetJsonPropertyName(storeId));
                    Assert.IsType<IdValueGenerator>(storeId.GetValueGeneratorFactory()(storeId, dataEntity));
                    Assert.Null(storeId.GetValueConverter());
                    Assert.NotNull(storeId.GetValueComparer());
                    Assert.NotNull(storeId.GetKeyValueComparer());

                    var partitionId = dataEntity.FindProperty("PartitionId");
                    Assert.Equal(typeof(long?), partitionId.ClrType);
                    Assert.Null(partitionId.PropertyInfo);
                    Assert.Null(partitionId.FieldInfo);
                    Assert.False(partitionId.IsNullable);
                    Assert.False(partitionId.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, partitionId.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Throw, partitionId.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Save, partitionId.GetBeforeSaveBehavior());
                    Assert.Equal("PartitionId", CosmosPropertyExtensions.GetJsonPropertyName(partitionId));
                    Assert.Null(partitionId.GetValueGeneratorFactory());
                    Assert.Null(partitionId.GetValueConverter());
                    Assert.Equal("1", partitionId.FindTypeMapping().Converter.ConvertToProvider(1));
                    Assert.NotNull(partitionId.GetValueComparer());
                    Assert.NotNull(partitionId.GetKeyValueComparer());

                    var eTag = dataEntity.FindProperty("_etag");
                    Assert.Equal(typeof(string), eTag.ClrType);
                    Assert.Null(eTag.PropertyInfo);
                    Assert.Null(eTag.FieldInfo);
                    Assert.True(eTag.IsNullable);
                    Assert.True(eTag.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.OnAddOrUpdate, eTag.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Ignore, eTag.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Ignore, eTag.GetBeforeSaveBehavior());
                    Assert.Equal("_etag", CosmosPropertyExtensions.GetJsonPropertyName(eTag));
                    Assert.Null(eTag.GetValueGeneratorFactory());
                    Assert.Null(eTag.GetValueConverter());
                    Assert.NotNull(eTag.GetValueComparer());
                    Assert.NotNull(eTag.GetKeyValueComparer());
                    Assert.Equal("_etag", dataEntity.GetETagPropertyName());
                    Assert.Same(eTag, dataEntity.GetETagProperty());

                    var blob = dataEntity.FindProperty(nameof(Data.Blob));
                    Assert.Equal(typeof(byte[]), blob.ClrType);
                    Assert.Equal(nameof(Data.Blob), blob.PropertyInfo.Name);
                    Assert.Equal("<Blob>k__BackingField", blob.FieldInfo.Name);
                    Assert.True(blob.IsNullable);
                    Assert.False(blob.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.Never, blob.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Save, blob.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Save, blob.GetBeforeSaveBehavior());
                    Assert.Equal("JsonBlob", CosmosPropertyExtensions.GetJsonPropertyName(blob));
                    Assert.Null(blob.GetValueGeneratorFactory());
                    Assert.Null(blob.GetValueConverter());
                    Assert.NotNull(blob.GetValueComparer());
                    Assert.NotNull(blob.GetKeyValueComparer());

                    var jObject = dataEntity.FindProperty("__jObject");
                    Assert.Equal(typeof(JObject), jObject.ClrType);
                    Assert.Null(jObject.PropertyInfo);
                    Assert.Null(jObject.FieldInfo);
                    Assert.True(jObject.IsNullable);
                    Assert.False(jObject.IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.OnAddOrUpdate, jObject.ValueGenerated);
                    Assert.Equal(PropertySaveBehavior.Ignore, jObject.GetAfterSaveBehavior());
                    Assert.Equal(PropertySaveBehavior.Ignore, jObject.GetBeforeSaveBehavior());
                    Assert.Equal("", CosmosPropertyExtensions.GetJsonPropertyName(jObject));
                    Assert.Null(jObject.GetValueGeneratorFactory());
                    Assert.Null(jObject.GetValueConverter());
                    Assert.NotNull(jObject.GetValueComparer());
                    Assert.NotNull(jObject.GetKeyValueComparer());

                    Assert.Equal(2, dataEntity.GetKeys().Count());

                    Assert.Equal(new[] { id, partitionId, blob, storeId, jObject, eTag }, dataEntity.GetProperties());
                });

        public class CosmosContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options
                    .EnableServiceProviderCaching(false)
                    .UseCosmos("localhost", "_", "_");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

                modelBuilder.HasDefaultContainer("Default");

                modelBuilder.Entity<Data>(
                    eb =>
                    {
                        eb.Property<int>("Id");
                        eb.Property<long?>("PartitionId").HasConversion<string>();
                        eb.HasPartitionKey("PartitionId");
                        eb.HasKey("Id", "PartitionId");
                        eb.ToContainer("DataContainer");
                        eb.UseETagConcurrency();
                        eb.HasNoDiscriminator();
                        eb.Property(d => d.Blob).ToJsonProperty("JsonBlob");
                    });
            }
        }

        public class Data
        {
            public byte[] Blob { get; set; }
        }

        public abstract class ContextBase : DbContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options
                    .EnableServiceProviderCaching(false)
                    .UseInMemoryDatabase(nameof(CSharpRuntimeModelCodeGeneratorTest));
        }

        public abstract class SqlServerContextBase : DbContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options
                    .EnableServiceProviderCaching(false)
                    .UseSqlServer(o => o.UseNetTopologySuite());
        }

        protected void Test(
            DbContext context,
            CompiledModelCodeGenerationOptions options,
            Action<IModel> assertModel = null,
            Type additionalDesignTimeServices = null,
            Action<DbContext> useContext = null,
            string expectedExceptionMessage = null,
            [CallerMemberName] string testName = "",
            [CallerFilePath] string filePath = "")
        {
            var model = context.GetService<IDesignTimeModel>().Model;
            ((Model)model).ModelId = new Guid();

            options.ModelNamespace ??= "TestNamespace";
            options.ContextType = context.GetType();

            var generator = DesignTestHelpers.Instance.CreateDesignServiceProvider(
                    context.GetService<IDatabaseProvider>().Name,
                    additionalDesignTimeServices: additionalDesignTimeServices)
                .GetRequiredService<ICompiledModelCodeGeneratorSelector>()
                .Select(options);

            if (expectedExceptionMessage != null)
            {
                Assert.Equal(
                    expectedExceptionMessage,
                    Assert.Throws<InvalidOperationException>(
                        () => generator.GenerateModel(
                            model,
                            options)).Message);
                return;
            }

            var scaffoldedFiles = generator.GenerateModel(
                model,
                options);

            var build = new BuildSource
            {
                References =
                {
                    BuildReference.ByName("System.Linq"),
                    BuildReference.ByName("System.Net.Primitives"),
                    BuildReference.ByName("System.Net.NetworkInformation"),
                    BuildReference.ByName("System.Threading.Thread"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Abstractions"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Cosmos"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.InMemory"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Proxies"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Sqlite"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Sqlite.NetTopologySuite"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Specification.Tests"),
                    BuildReference.ByName("NetTopologySuite"),
                    BuildReference.ByName("Newtonsoft.Json"),
                    BuildReference.ByName(typeof(CSharpRuntimeModelCodeGeneratorTest).Assembly.GetName().Name)
                },
                Sources = scaffoldedFiles.ToDictionary(f => f.Path, f => f.Code),
                NullableReferenceTypes = options.UseNullableReferenceTypes
            };

            var assembly = build.BuildInMemory();

            var modelTypeName = options.ContextType.Name + "Model";
            var modelType = assembly.GetType(
                string.IsNullOrEmpty(options.ModelNamespace)
                    ? modelTypeName
                    : options.ModelNamespace + "." + modelTypeName);
            var instancePropertyInfo = modelType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var compiledModel = (IModel)instancePropertyInfo.GetValue(null);

            var modelRuntimeInitializer = context.GetService<IModelRuntimeInitializer>();
            compiledModel = modelRuntimeInitializer.Initialize(compiledModel, designTime: false);
            assertModel(compiledModel);

            var relationalModel = (IRelationalModel)context.Model.FindRuntimeAnnotationValue(RelationalAnnotationNames.RelationalModel);
            if (relationalModel != null)
            {
                RelationalModelTest.AssertEqual(relationalModel, compiledModel.GetRelationalModel());
            }

            if (useContext != null)
            {
                using var testStore = SqlServerTestStore.Create("RuntimeModelTest" + context.GetType().Name);
                testStore.Clean(context);

                var optionsBuilder = testStore.AddProviderOptions(new DbContextOptionsBuilder().UseModel(compiledModel));
                new SqlServerDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();
                var newContext = new DbContext(optionsBuilder.Options);

                newContext.Database.CreateExecutionStrategy().Execute(
                    newContext,
                    c =>
                    {
                        using var transaction = context.Database.BeginTransaction();
                        useContext(c);
                    });
            }

            AssertBaseline(scaffoldedFiles, testName, filePath);
        }

        private const string FileNewLine = @"
";

        private static void AssertBaseline(
            IReadOnlyCollection<ScaffoldedFile> scaffoldedFiles,
            string testName,
            string filePath)
        {
            var testDirectory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(testDirectory))
            {
                return;
            }

            var baselinesDirectory = Path.Combine(testDirectory, "Baselines", testName);
            try
            {
                Directory.CreateDirectory(baselinesDirectory);
            }
            catch
            {
                return;
            }

            var shouldRewrite = Environment.GetEnvironmentVariable("EF_TEST_REWRITE_BASELINES")?.ToUpper() is "1" or "TRUE";
            foreach (var file in scaffoldedFiles)
            {
                var fullFilePath = Path.Combine(baselinesDirectory, file.Path);
                if (!File.Exists(fullFilePath)
                    || shouldRewrite)
                {
                    File.WriteAllText(fullFilePath, file.Code);
                }
                else
                {
                    Assert.Equal(File.ReadAllText(fullFilePath), file.Code, ignoreLineEndingDifferences: true);
                }
            }
        }
    }

    public class Internal
    {
        public long Id { get; set; }
    }

    public class Index
    {
        public Guid Id { get; set; }
    }

    public class IdentityUser : TestModels.AspNetIdentity.IdentityUser
    {
    }

    public class SelfReferentialEntity
    {
        public long Id { get; set; }

        public SelfReferentialProperty Collection { get; set; }
    }

    public class SelfReferentialProperty : List<SelfReferentialProperty>
    {
    }
}

namespace Microsoft.EntityFrameworkCore.Scaffolding.TestModel.Internal
{
    public class DbContext : ContextBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Index>();
            modelBuilder.Entity<IdentityUser>();
            modelBuilder.Entity<Scaffolding.Internal.IdentityUser>(
                eb =>
                {
                    eb.HasDiscriminator().HasValue("DerivedIdentityUser");
                });
            modelBuilder.Entity<Scaffolding.Internal.Internal>();
        }
    }

    public class SelfReferentialDbContext : ContextBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SelfReferentialEntity>(
                eb =>
                {
                    eb.Property(e => e.Collection).HasConversion(typeof(SelfReferentialPropertyValueConverter));
                });
        }
    }

    public class SelfReferentialPropertyValueConverter : ValueConverter<SelfReferentialProperty, string>
    {
        public SelfReferentialPropertyValueConverter()
            : this(null)
        {
        }

        public SelfReferentialPropertyValueConverter(ConverterMappingHints hints)
            : base(v => null, v => null, hints)
        {
        }
    }
}
