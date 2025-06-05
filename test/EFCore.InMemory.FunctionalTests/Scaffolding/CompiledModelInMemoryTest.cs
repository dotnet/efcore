// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

#nullable enable

using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

public class GlobalNamespaceContext(DbContextOptions<GlobalNamespaceContext> options) : DbContext(options);

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class CompiledModelInMemoryTest(NonSharedFixture fixture) : CompiledModelTestBase(fixture)
    {
        [ConditionalFact]
        public virtual Task Empty_model()
            => Test(
                modelBuilder => { },
                model =>
                {
                    Assert.Empty(model.GetEntityTypes());
                    Assert.Same(model, model.FindRuntimeAnnotationValue("ReadOnlyModel"));
                });

        [ConditionalFact]
        public virtual Task Global_namespace()
            => Test<GlobalNamespaceContext>(
                modelBuilder => modelBuilder.Entity(
                    "1", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                    }),
                model =>
                {
                    Assert.NotNull(model.FindEntityType("1"));
                },
                options: new CompiledModelCodeGenerationOptions { ModelNamespace = string.Empty, ForNativeAot = true });

        [ConditionalFact]
        public virtual Task Self_referential_property()
            => Test(
                modelBuilder =>
                    modelBuilder.Entity<SelfReferentialEntity<long>>(eb =>
                    {
                        eb.Property(e => e.Collection)
                            .HasConversion<SelfReferentialEntity<long>.NonGeneric.SelfReferentialPropertyValueConverter<string>>();
                    }),
                model =>
                {
                    Assert.Single(model.GetEntityTypes());
                }
            );

        public class SelfReferentialEntity<T>
            where T : struct
        {
            public T Id { get; set; }

            public SelfReferentialProperty? Collection { get; set; }

            public static class NonGeneric
            {
                public class SelfReferentialPropertyValueConverter<TTarget>(ConverterMappingHints hints)
                    : ValueConverter<SelfReferentialProperty?, TTarget?>(v => ToProvider(v), v => FromProvider(v), hints)
                {
                    public SelfReferentialPropertyValueConverter()
                        : this(new ConverterMappingHints())
                    {
                    }

                    public static TTarget? ToProvider(SelfReferentialProperty? v)
                        => default;

                    public static SelfReferentialProperty? FromProvider(TTarget? v)
                        => null;
                }
            }
        }

        public class SelfReferentialProperty : List<SelfReferentialProperty>;

        [ConditionalFact]
        public virtual Task Throws_for_constructor_binding()
            => Test(
                modelBuilder => modelBuilder.Entity(
                    "Lazy", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                        ((EntityType)e.Metadata).ConstructorBinding = new ConstructorBinding(
                            typeof(object).GetConstructor(Type.EmptyTypes)!,
                            []);
                    }),
                expectedExceptionMessage: DesignStrings.CompiledModelConstructorBinding("Lazy", "Customize()", "LazyEntityType"));

        [ConditionalFact]
        public virtual Task Manual_lazy_loading()
            => Test(
                modelBuilder =>
                {
                    modelBuilder.Entity<LazyConstructorEntity>();

                    modelBuilder.Entity<LazyPropertyDelegateEntity>(
                        b =>
                        {
                            var serviceProperty = (ServiceProperty)b.Metadata.AddServiceProperty(
                                typeof(LazyPropertyDelegateEntity).GetRuntimeProperties().Single(p => p.Name == "LoaderState"),
                                typeof(ILazyLoader));

                            serviceProperty.SetParameterBinding(
                                new DependencyInjectionParameterBinding(typeof(object), typeof(ILazyLoader), serviceProperty),
                                ConfigurationSource.Explicit);
                        });
                },
                model =>
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

        public class LazyConstructorEntity(ILazyLoader loader)
        {
            private readonly ILazyLoader _loader = loader;

            public int Id { get; set; }

            public LazyPropertyEntity? LazyPropertyEntity { get; set; }
            public LazyPropertyDelegateEntity? LazyPropertyDelegateEntity { get; set; }
        }

        public class LazyPropertyEntity
        {
            public ILazyLoader Loader { get; set; } = null!;

            public int Id { get; set; }
            public int LazyConstructorEntityId { get; set; }

            public LazyConstructorEntity? LazyConstructorEntity { get; set; }
        }

        public class LazyPropertyDelegateEntity
        {
            public object? LoaderState { get; set; }
            private Action<object, string> LazyLoader { get; set; } = null!;

            public int Id { get; set; }
            public int LazyConstructorEntityId { get; set; }

            public LazyConstructorEntity? LazyConstructorEntity { get; set; }
        }

        [ConditionalFact]
        public virtual Task Lazy_loading_proxies()
            => Test(
                modelBuilder => modelBuilder.Entity<LazyProxiesEntity1>(),
                model =>
                {
                    Assert.Equal(
                        typeof(ILazyLoader), model.FindEntityType(typeof(LazyProxiesEntity1))!.GetServiceProperties().Single().ClrType);
                    Assert.Equal(
                        typeof(ILazyLoader), model.FindEntityType(typeof(LazyProxiesEntity1))!.GetServiceProperties().Single().ClrType);
                },
                async c =>
                {
                    var principal = new LazyProxiesEntity2
                    {
                        Id = 1, CollectionNavigation = new List<LazyProxiesEntity1> { new() { Id = 1 } }
                    };
                    c.Set<LazyProxiesEntity2>().Add(principal);

                    await c.SaveChangesAsync();

                    c.ChangeTracker.Clear();

                    principal = c.Set<LazyProxiesEntity2>().Single();

                    Assert.Same(principal, principal.CollectionNavigation!.Single().ReferenceNavigation);
                },
                options => options.UseLazyLoadingProxies(),
                new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true, ForNativeAot = true },
                services => services.AddEntityFrameworkProxies());

        [ConditionalFact]
        public virtual Task Lazy_loading_manual()
            => Test(
                b =>
                {
                    b.Entity<LazyProxiesEntity3>().Property(b => b.Id).ValueGeneratedNever();
                    b.Entity<LazyProxiesEntity4>().Property(b => b.Id).ValueGeneratedNever();
                },
                m =>
                {
                    var blog = m.FindEntityType(typeof(LazyProxiesEntity3))!;
                    Assert.Equal(
                        blog.FindServiceProperty("LazyLoader")!,
                        blog.ConstructorBinding!.ParameterBindings.Single().ConsumedProperties.Single());
                },
                async c =>
                {
                    var principal = new LazyProxiesEntity3
                    {
                        Id = 1, CollectionNavigation = new List<LazyProxiesEntity4> { new() { Id = 1 } }
                    };
                    c.Set<LazyProxiesEntity3>().Add(principal);

                    await c.SaveChangesAsync();

                    c.ChangeTracker.Clear();

                    principal = c.Set<LazyProxiesEntity3>().Single();

                    Assert.Same(principal, principal.CollectionNavigation.Single().ReferenceNavigation);
                });

        public class LazyProxiesEntity1
        {
            public int Id { get; set; }

            public virtual LazyProxiesEntity2? ReferenceNavigation { get; set; }
        }

        public class LazyProxiesEntity2
        {
            public ILazyLoader Loader { get; set; } = null!;

            public int Id { get; set; }
            public virtual ICollection<LazyProxiesEntity1>? CollectionNavigation { get; set; }
        }

        public class LazyProxiesEntity3
        {
            private ICollection<LazyProxiesEntity4> _collectionNavigation = null!;

            public LazyProxiesEntity3()
            {
            }

            protected LazyProxiesEntity3(ILazyLoader lazyLoader)
                => LazyLoader = lazyLoader;

            private ILazyLoader? LazyLoader { get; set; }

            public int Id { get; set; }
            public string? Name { get; set; }

            public ICollection<LazyProxiesEntity4> CollectionNavigation
            {
                get => LazyLoader.Load(this, ref _collectionNavigation!)!;
                set => _collectionNavigation = value;
            }
        }

        public class LazyProxiesEntity4
        {
            private LazyProxiesEntity3 _referenceNavigation = null!;

            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Content { get; set; }

            public LazyProxiesEntity4()
            {
            }

            protected LazyProxiesEntity4(Action<object, string> lazyLoader)
                => LazyLoader = lazyLoader;

            private Action<object, string>? LazyLoader { get; set; }

            public virtual LazyProxiesEntity3 ReferenceNavigation
            {
                get => LazyLoader?.Load(this, ref _referenceNavigation)!;
                set => _referenceNavigation = value;
            }
        }

        [ConditionalFact]
        public virtual Task Throws_for_query_filter()
            => Test(
                modelBuilder => modelBuilder.Entity(
                    "QueryFilter", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                        e.HasQueryFilter((Expression<Func<Dictionary<string, object>, bool>>)(e => e != null));
                    }),
                expectedExceptionMessage: DesignStrings.CompiledModelQueryFilter("QueryFilter"));

        [ConditionalFact]
        public virtual Task Throws_for_defining_query()
            => Test<DefiningQueryContext>(
                expectedExceptionMessage: InMemoryStrings.CompiledModelDefiningQuery("object"));

        public class DefiningQueryContext(DbContextOptions<DefiningQueryContext> options) : DbContext(options)
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
        public virtual Task Throws_for_value_generator()
            => Test(
                modelBuilder => modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").HasValueGenerator((p, e) => null!);
                        e.HasKey("Id");
                    }),
                expectedExceptionMessage: DesignStrings.CompiledModelValueGenerator(
                    "MyEntity", "Id", nameof(PropertyBuilder.HasValueGeneratorFactory)));

        [ConditionalFact]
        public virtual Task Custom_value_converter()
            => Test(
                modelBuilder => modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").HasConversion(
                            i => JsonSerializer.Serialize(i, (JsonSerializerOptions?)default),
                            i => JsonSerializer.Deserialize<int>(i, (JsonSerializerOptions?)null));
                        e.HasKey("Id");
                    }),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    var converter = entityType.FindProperty("Id")!.GetTypeMapping().Converter!;
                    Assert.Equal("1", converter.ConvertToProvider(1));
                },
                options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true, ForNativeAot = true });

        [ConditionalFact]
        public virtual Task Custom_value_comparer()
            => Test(
                modelBuilder => modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").HasConversion(typeof(int), new FakeValueComparer());
                        e.HasKey("Id");
                    }),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    Assert.True(
                        entityType.FindProperty("Id")!.GetValueComparer().SnapshotExpression
                            is Expression<Func<int, int>> lambda
                        && lambda.Body is ConstantExpression constant
                        && ((int)constant.Value!) == 1);
                });

        private class FakeValueComparer() : ValueComparer<int>((l, r) => false, v => 0, v => 1)
        {
            public override Type Type { get; } = typeof(int);

            public override bool Equals(object? left, object? right)
                => throw new NotImplementedException();

            public override int GetHashCode(object? instance)
                => throw new NotImplementedException();

            public override object Snapshot(object? instance)
                => throw new NotImplementedException();
        }

        [ConditionalFact]
        public virtual Task Custom_provider_value_comparer()
            => Test(
                modelBuilder => modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").HasConversion(typeof(int), null, new FakeValueComparer());
                        e.HasKey("Id");
                    }),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    var providerValueComparer = entityType.FindProperty("Id")!.GetProviderValueComparer();
                    Assert.True(
                        providerValueComparer.SnapshotExpression
                            is Expression<Func<int, int>> lambda
                        && lambda.Body is ConstantExpression constant
                        && ((int)constant.Value!) == 1);
                });

        [ConditionalFact]
        public virtual Task Custom_type_mapping()
            => Test(
                modelBuilder => modelBuilder.Entity(
                    "MyEntity", e =>
                    {
                        e.Property<int>("Id").Metadata.SetTypeMapping(
                            new InMemoryTypeMapping(typeof(int), jsonValueReaderWriter: JsonInt32ReaderWriter.Instance));
                        e.HasKey("Id");
                    }),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    var typeMapping = entityType.FindProperty("Id")!.FindTypeMapping()!;
                    Assert.IsType<InMemoryTypeMapping>(typeMapping);
                    Assert.IsType<JsonInt32ReaderWriter>(typeMapping.JsonValueReaderWriter);
                });

        [ConditionalFact]
        public virtual Task Fully_qualified_model()
            => Test<DbContext>(
                modelBuilder =>
                {
                    modelBuilder.Entity<Index>();
                    modelBuilder.Entity<TestModels.AspNetIdentity.IdentityUser>();
                    modelBuilder.Entity<IdentityUser>(
                        eb =>
                        {
                            eb.HasDiscriminator().HasValue("DerivedIdentityUser");
                        });
                    modelBuilder.Entity<Scaffolding>();
                },
                assertModel: model =>
                {
                    Assert.Equal(4, model.GetEntityTypes().Count());
                    Assert.Same(model, model.FindRuntimeAnnotationValue("ReadOnlyModel"));
                },
                options: new CompiledModelCodeGenerationOptions { ModelNamespace = "Scaffolding", ForNativeAot = true },
                addDesignTimeServices: services => services.AddSingleton<ICSharpHelper, FullyQualifiedCSharpHelper>());

        [ConditionalFact]
        public virtual Task RelationshipCycles()
            => Test(
                BuildCyclesModel,
                AssertCyclesModel,
                options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true, ForNativeAot = true });

        protected virtual void BuildCyclesModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<OwnedType>();

            modelBuilder.Entity<DependentBase<long?>>(
                eb =>
                {
                    eb.Property<long?>("Id");

                    eb.HasOne<PrincipalBase>().WithOne()
                        .HasPrincipalKey<DependentBase<long?>>("PrincipalId")
                        .HasForeignKey<PrincipalBase>(e => e.Id);
                });

            modelBuilder.Entity<PrincipalDerived<DependentBase<long?>>>(
                eb =>
                {
                    eb.Ignore(d => d.Principals);
                    eb.HasOne<PrincipalBase>()
                        .WithMany(e => (ICollection<PrincipalDerived<DependentBase<long?>>>)e.Deriveds)
                        .HasPrincipalKey(e => e.Id)
                        .HasForeignKey("PrincipalId");

                    eb.HasOne(e => e.Dependent).WithOne(e => e.Principal)
                        .HasForeignKey<DependentBase<long?>>("PrincipalId")
                        .HasPrincipalKey<PrincipalDerived<DependentBase<long?>>>("PrincipalId");
                });
        }

        protected virtual void AssertCyclesModel(IModel model)
        {
            var dependentBase = model.FindEntityType(typeof(DependentBase<long?>))!;
            var dependentFk = dependentBase.GetForeignKeys().Single();

            var principalDerived = dependentFk.PrincipalEntityType;
            var principalFk = principalDerived.GetDeclaredForeignKeys().Single();
            TestHelpers.ModelAsserter.AssertEqual(dependentFk.PrincipalKey.Properties, principalFk.Properties);

            var principalBase = principalFk.PrincipalEntityType;
            var principalBaseFk = principalBase.GetForeignKeys().Single();
            TestHelpers.ModelAsserter.AssertEqual(principalFk.PrincipalKey.Properties, principalBaseFk.Properties);

            Assert.Same(dependentBase, principalBaseFk.PrincipalEntityType);
            TestHelpers.ModelAsserter.AssertEqual(principalBaseFk.PrincipalKey.Properties, dependentFk.Properties);
        }

        //[ConditionalFact(Skip = "Primitive collections not supported completely")]
        public override Task BigModel()
            => base.BigModel();

        public class Scaffolding
        {
            public long Id { get; set; }
        }

        public class Index
        {
            public Guid Id { get; set; }
        }

        public class IdentityUser : TestModels.AspNetIdentity.IdentityUser;

        private class FullyQualifiedCSharpHelper(ITypeMappingSource typeMappingSource) : CSharpHelper(typeMappingSource)
        {
            protected override bool ShouldUseFullName(string shortTypeName)
                => base.ShouldUseFullName(shortTypeName) || shortTypeName is nameof(System.Index) or nameof(Internal);
        }

        protected override TestHelpers TestHelpers
            => InMemoryTestHelpers.Instance;

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .ConfigureWarnings(w => w.Ignore(CoreEventId.CollectionWithoutComparer));

        protected override BuildSource AddReferences(BuildSource build, [CallerFilePath] string filePath = "")
        {
            base.AddReferences(build);
            build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.InMemory"));
            return build;
        }
    }
}
