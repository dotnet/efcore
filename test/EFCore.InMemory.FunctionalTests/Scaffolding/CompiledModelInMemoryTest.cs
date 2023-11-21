// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

#nullable enable

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

public class GlobalNamespaceContext : DbContext
{
    public GlobalNamespaceContext(DbContextOptions<GlobalNamespaceContext> options)
        : base(options)
    {
    }
}

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class CompiledModelInMemoryTest : CompiledModelTestBase
    {
        [ConditionalFact]
        public virtual void Empty_model()
            => Test(
                modelBuilder => { },
                model =>
                {
                    Assert.Empty(model.GetEntityTypes());
                    Assert.Same(model, model.FindRuntimeAnnotationValue("ReadOnlyModel"));
                });

        [ConditionalFact]
        public virtual void Global_namespace()
            => Test<GlobalNamespaceContext>(
                modelBuilder => modelBuilder.Entity("1", e =>
                {
                    e.Property<int>("Id");
                    e.HasKey("Id");
                }),
                model =>
                {
                    Assert.NotNull(model.FindEntityType("1"));
                },
                options: new CompiledModelCodeGenerationOptions { ModelNamespace = string.Empty });

        [ConditionalFact]
        public virtual void Self_referential_property()
            => Test(
                modelBuilder =>
                    modelBuilder.Entity<SelfReferentialEntity>(
                        eb =>
                        {
                            eb.Property(e => e.Collection).HasConversion(typeof(SelfReferentialPropertyValueConverter));
                        }),
                model =>
                {
                    Assert.Single(model.GetEntityTypes());
                }
            );

        public class SelfReferentialPropertyValueConverter : ValueConverter<SelfReferentialProperty?, string?>
        {
            public SelfReferentialPropertyValueConverter()
                : this(new ConverterMappingHints())
            {
            }

            public SelfReferentialPropertyValueConverter(ConverterMappingHints hints)
                : base(v => null, v => null, hints)
            {
            }
        }

        public class SelfReferentialEntity
        {
            public long Id { get; set; }

            public SelfReferentialProperty? Collection { get; set; }
        }

        public class SelfReferentialProperty : List<SelfReferentialProperty>
        {
        }

        [ConditionalFact]
        public virtual void Throws_for_constructor_binding()
            => Test(
                modelBuilder => modelBuilder.Entity(
                    "Lazy", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                        ((EntityType)e.Metadata).ConstructorBinding = new ConstructorBinding(
                            typeof(object).GetConstructor(Type.EmptyTypes)!,
                            Array.Empty<ParameterBinding>());
                    }),
                expectedExceptionMessage: DesignStrings.CompiledModelConstructorBinding("Lazy", "Customize()", "LazyEntityType"));

        [ConditionalFact]
        public virtual void Manual_lazy_loading()
            => Test(
                modelBuilder => {
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

        public class LazyConstructorEntity
        {
            private readonly ILazyLoader _loader;

            public LazyConstructorEntity(ILazyLoader loader)
            {
                _loader = loader;
            }

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
        public virtual void Lazy_loading_proxies()
            => Test(
                modelBuilder => modelBuilder.Entity<LazyProxiesEntity1>(),
                model =>
                {
                    Assert.Equal(
                        typeof(ILazyLoader), model.FindEntityType(typeof(LazyProxiesEntity1))!.GetServiceProperties().Single().ClrType);
                    Assert.Equal(
                        typeof(ILazyLoader), model.FindEntityType(typeof(LazyProxiesEntity1))!.GetServiceProperties().Single().ClrType);
                },
                onConfiguring: options => options.UseLazyLoadingProxies(),
                addServices: services => services.AddEntityFrameworkProxies());

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

        [ConditionalFact]
        public virtual void Throws_for_query_filter()
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
        public virtual void Throws_for_defining_query()
            => Test<DefiningQueryContext>(
                expectedExceptionMessage: DesignStrings.CompiledModelDefiningQuery("object"));

        public class DefiningQueryContext : DbContext
        {
            public DefiningQueryContext(DbContextOptions<DefiningQueryContext> options)
                : base(options)
            {
            }

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
        public virtual void Throws_for_value_generator()
            => Test(
                modelBuilder => modelBuilder.Entity("MyEntity", e =>
                {
                    e.Property<int>("Id").HasValueGenerator((p, e) => null!);
                    e.HasKey("Id");
                }),
                expectedExceptionMessage: DesignStrings.CompiledModelValueGenerator(
                    "MyEntity", "Id", nameof(PropertyBuilder.HasValueGeneratorFactory)));

        [ConditionalFact]
        public virtual void Custom_value_converter()
            => Test(
                modelBuilder => modelBuilder.Entity("MyEntity", e =>
                {
                    e.Property<int>("Id").HasConversion(i => i, i => i);
                    e.HasKey("Id");
                }),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    var converter = entityType.FindProperty("Id")!.GetTypeMapping().Converter!;
                    Assert.Equal(1, converter.ConvertToProvider(1));
                });

        [ConditionalFact]
        public virtual void Custom_value_comparer()
            => Test(
                modelBuilder => modelBuilder.Entity("MyEntity", e =>
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

        private class FakeValueComparer : ValueComparer<int>
        {
            public FakeValueComparer()
                : base((l, r) => false, v => 0, v => 1)
            {
            }

            public override Type Type { get; } = typeof(int);

            public override bool Equals(object? left, object? right)
                => throw new NotImplementedException();

            public override int GetHashCode(object? instance)
                => throw new NotImplementedException();

            public override object Snapshot(object? instance)
                => throw new NotImplementedException();
        }

        [ConditionalFact]
        public virtual void Custom_provider_value_comparer()
            => Test(
                modelBuilder => modelBuilder.Entity("MyEntity", e =>
                {
                    e.Property<int>("Id").HasConversion(typeof(int), null, new FakeValueComparer());
                    e.HasKey("Id");
                }),
                model =>
                {
                    var entityType = model.GetEntityTypes().Single();

                    Assert.True(
                        entityType.FindProperty("Id")!.GetProviderValueComparer().SnapshotExpression
                            is Expression<Func<int, int>> lambda
                        && lambda.Body is ConstantExpression constant
                        && ((int)constant.Value!) == 1);
                });

        [ConditionalFact]
        public virtual void Custom_type_mapping()
            => Test(
                modelBuilder => modelBuilder.Entity("MyEntity", e =>
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
        public virtual void Fully_qualified_model()
            => Test<DbContext>(
                modelBuilder => {
                    modelBuilder.Entity<Index>();
                    modelBuilder.Entity<TestModels.AspNetIdentity.IdentityUser>();
                    modelBuilder.Entity<IdentityUser >(
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
                options: new CompiledModelCodeGenerationOptions { ModelNamespace = "Scaffolding" },
                addDesignTimeServices: services => services.AddSingleton<ICSharpHelper, FullyQualifiedCSharpHelper>());

        // Primitive collections not supported completely
        public override void BigModel()
        {
        }

        public class Scaffolding
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

        private class FullyQualifiedCSharpHelper : CSharpHelper
        {
            public FullyQualifiedCSharpHelper(ITypeMappingSource typeMappingSource)
                : base(typeMappingSource)
            {
            }

            protected override bool ShouldUseFullName(Type type)
                => base.ShouldUseFullName(type);

            protected override bool ShouldUseFullName(string shortTypeName)
                => base.ShouldUseFullName(shortTypeName) || shortTypeName is nameof(System.Index) or nameof(Internal);
        }

        protected override TestHelpers TestHelpers => InMemoryTestHelpers.Instance;
        protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

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
