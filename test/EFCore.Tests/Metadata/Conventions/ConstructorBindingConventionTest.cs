// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class ConstructorBindingConventionTest
    {
        [ConditionalFact]
        public void Can_bind_parameterless_constructor()
        {
            var constructorBinding = GetBinding<BlogParameterless>();

            Assert.NotNull(constructorBinding);
            Assert.Empty(constructorBinding.Constructor.GetParameters());
            Assert.Empty(constructorBinding.ParameterBindings);
        }

        private class BlogParameterless : Blog
        {
        }

        [ConditionalFact]
        public void Binds_to_parameterless_constructor_if_no_services()
        {
            var constructorBinding = GetBinding<BlogSeveralNoServices>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Empty(parameters);
            Assert.Equal(0, bindings.Count);
        }

        private class BlogSeveralNoServices : Blog
        {
            public BlogSeveralNoServices()
            {
            }

            public BlogSeveralNoServices(string title, int id)
            {
            }

            public BlogSeveralNoServices(string title, Guid? shadow, int id)
            {
            }

            public BlogSeveralNoServices(string title, Guid? shadow, bool dummy, int id)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_least_parameters_if_no_services()
        {
            var constructorBinding = GetBinding<BlogSeveral>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(2, parameters.Length);
            Assert.Equal(2, bindings.Count);

            Assert.Equal("title", parameters[0].Name);
            Assert.Equal("id", parameters[1].Name);

            Assert.Equal("Title", bindings[0].ConsumedProperties.First().Name);
            Assert.Equal("Id", bindings[1].ConsumedProperties.First().Name);
        }

        private class BlogSeveral : Blog
        {
            public BlogSeveral(string title, int id)
            {
            }

            public BlogSeveral(string title, Guid? shadow, int id)
            {
            }

            public BlogSeveral(string title, Guid? shadow, bool dummy, int id)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_zero_scalars_one_service()
        {
            var constructorBinding = GetBinding<BlogOneService>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Single(parameters);
            Assert.Equal(1, bindings.Count);

            Assert.Equal("loader", parameters[0].Name);
        }

        private class BlogOneService : Blog
        {
            public BlogOneService()
            {
            }

            public BlogOneService(string title, int id)
            {
            }

            public BlogOneService(string title, Guid? shadow, int id)
            {
            }

            public BlogOneService(string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogOneService(ILazyLoader loader)
            {
            }

            public BlogOneService(ILazyLoader loader, string title, int id)
            {
            }

            public BlogOneService(ILazyLoader loader, string title, Guid? shadow, int id)
            {
            }

            public BlogOneService(ILazyLoader loader, string title, Guid? shadow, bool dummy, int id)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_least_scalars_one_service()
        {
            var constructorBinding = GetBinding<BlogSeveralOneService>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(3, parameters.Length);
            Assert.Equal(3, bindings.Count);

            Assert.Equal("loader", parameters[0].Name);
            Assert.Equal("title", parameters[1].Name);
            Assert.Equal("id", parameters[2].Name);

            Assert.Equal("Title", bindings[1].ConsumedProperties.First().Name);
            Assert.Equal("Id", bindings[2].ConsumedProperties.First().Name);
        }

        private class BlogSeveralOneService : Blog
        {
            public BlogSeveralOneService()
            {
            }

            public BlogSeveralOneService(string title, int id)
            {
            }

            public BlogSeveralOneService(string title, Guid? shadow, int id)
            {
            }

            public BlogSeveralOneService(string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogSeveralOneService(ILazyLoader loader, string title, int id)
            {
            }

            public BlogSeveralOneService(ILazyLoader loader, string title, Guid? shadow, int id)
            {
            }

            public BlogSeveralOneService(ILazyLoader loader, string title, Guid? shadow, bool dummy, int id)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_zero_scalars_two_services()
        {
            var constructorBinding = GetBinding<BlogTwoServices>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(2, parameters.Length);
            Assert.Equal(2, bindings.Count);

            Assert.Equal("context", parameters[0].Name);
            Assert.Equal("loader", parameters[1].Name);
        }

        private class BlogTwoServices : Blog
        {
            public BlogTwoServices()
            {
            }

            public BlogTwoServices(string title, int id)
            {
            }

            public BlogTwoServices(string title, Guid? shadow, int id)
            {
            }

            public BlogTwoServices(string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogTwoServices(ILazyLoader loader)
            {
            }

            public BlogTwoServices(ILazyLoader loader, string title, int id)
            {
            }

            public BlogTwoServices(ILazyLoader loader, string title, Guid? shadow, int id)
            {
            }

            public BlogTwoServices(ILazyLoader loader, string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogTwoServices(DbContext context)
            {
            }

            public BlogTwoServices(DbContext context, string title, int id)
            {
            }

            public BlogTwoServices(DbContext context, string title, Guid? shadow, int id)
            {
            }

            public BlogTwoServices(DbContext context, string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogTwoServices(DbContext context, ILazyLoader loader)
            {
            }

            public BlogTwoServices(DbContext context, ILazyLoader loader, string title, int id)
            {
            }

            public BlogTwoServices(DbContext context, ILazyLoader loader, string title, Guid? shadow, int id)
            {
            }

            public BlogTwoServices(DbContext context, ILazyLoader loader, string title, Guid? shadow, bool dummy, int id)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_least_scalars_two_services()
        {
            var constructorBinding = GetBinding<BlogSeveralTwoServices>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(4, parameters.Length);
            Assert.Equal(4, bindings.Count);

            Assert.Equal("context", parameters[0].Name);
            Assert.Equal("loader", parameters[1].Name);
            Assert.Equal("title", parameters[2].Name);
            Assert.Equal("id", parameters[3].Name);

            Assert.Equal("Title", bindings[2].ConsumedProperties.First().Name);
            Assert.Equal("Id", bindings[3].ConsumedProperties.First().Name);
        }

        private class BlogSeveralTwoServices : Blog
        {
            public BlogSeveralTwoServices()
            {
            }

            public BlogSeveralTwoServices(string title, int id)
            {
            }

            public BlogSeveralTwoServices(string title, Guid? shadow, int id)
            {
            }

            public BlogSeveralTwoServices(string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogSeveralTwoServices(ILazyLoader loader, string title, int id)
            {
            }

            public BlogSeveralTwoServices(ILazyLoader loader, string title, Guid? shadow, int id)
            {
            }

            public BlogSeveralTwoServices(ILazyLoader loader, string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogSeveralTwoServices(DbContext context, string title, int id)
            {
            }

            public BlogSeveralTwoServices(DbContext context, string title, Guid? shadow, int id)
            {
            }

            public BlogSeveralTwoServices(DbContext context, string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogSeveralTwoServices(DbContext context, ILazyLoader loader, string title, int id)
            {
            }

            public BlogSeveralTwoServices(DbContext context, ILazyLoader loader, string title, Guid? shadow, int id)
            {
            }

            public BlogSeveralTwoServices(DbContext context, ILazyLoader loader, string title, Guid? shadow, bool dummy, int id)
            {
            }
        }

        [ConditionalFact]
        public void Throws_if_two_constructors_with_same_number_of_parameters_could_be_used()
        {
            Assert.Equal(
                CoreStrings.ConstructorConflict(
                    "BlogConflict(string, int)",
                    "BlogConflict(string, Nullable<Guid>)"),
                Assert.Throws<InvalidOperationException>(
                    () => GetBinding<BlogConflict>()).Message);
        }

        [ConditionalFact]
        public void Does_not_throw_if_explicit_binding_has_been_set()
        {
            var constructorBinding = GetBinding<BlogConflict>(
                e => e[CoreAnnotationNames.ConstructorBinding] = new ConstructorBinding(
                    typeof(BlogConflict).GetConstructor(
                        new[] { typeof(string), typeof(int) }),
                    new[]
                    {
                        new PropertyParameterBinding(e.FindProperty(nameof(Blog.Title))),
                        new PropertyParameterBinding(e.FindProperty(nameof(Blog.Id)))
                    }));

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(2, parameters.Length);
            Assert.Equal(2, bindings.Count);

            Assert.Equal("title", parameters[0].Name);
            Assert.Equal("id", parameters[1].Name);

            Assert.Equal("Title", bindings[0].ConsumedProperties.First().Name);
            Assert.Equal("Id", bindings[1].ConsumedProperties.First().Name);
        }

        private class BlogConflict : Blog
        {
            public BlogConflict(string title, int id)
            {
            }

            public BlogConflict(string title, Guid? shadow)
            {
            }

            public BlogConflict(string title, Guid? shadow, bool dummy, int id)
            {
            }
        }

        [ConditionalFact]
        public void Resolves_properties_with_different_kinds_of_name()
        {
            var constructorBinding = GetBinding<BlogSpanner>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(9, parameters.Length);
            Assert.Equal(9, bindings.Count);

            Assert.Equal("fooBaar1", parameters[0].Name);
            Assert.Equal("fooBaar2", parameters[1].Name);
            Assert.Equal("fooBaar3", parameters[2].Name);
            Assert.Equal("fooBaar4", parameters[3].Name);
            Assert.Equal("fooBaar5", parameters[4].Name);
            Assert.Equal("fooBaar6", parameters[5].Name);
            Assert.Equal("FooBaar1", parameters[6].Name);
            Assert.Equal("FooBaar5", parameters[7].Name);
            Assert.Equal("FooBaar6", parameters[8].Name);

            Assert.Equal("FooBaar1", bindings[0].ConsumedProperties.First().Name);
            Assert.Equal("fooBaar2", bindings[1].ConsumedProperties.First().Name);
            Assert.Equal("_fooBaar3", bindings[2].ConsumedProperties.First().Name);
            Assert.Equal("m_fooBaar4", bindings[3].ConsumedProperties.First().Name);
            Assert.Equal("_FooBaar5", bindings[4].ConsumedProperties.First().Name);
            Assert.Equal("m_FooBaar6", bindings[5].ConsumedProperties.First().Name);
            Assert.Equal("FooBaar1", bindings[6].ConsumedProperties.First().Name);
            Assert.Equal("_FooBaar5", bindings[7].ConsumedProperties.First().Name);
            Assert.Equal("m_FooBaar6", bindings[8].ConsumedProperties.First().Name);
        }

        private class BlogSpanner : Blog
        {
            public BlogSpanner(
                string fooBaar1,
                string fooBaar2,
                string fooBaar3,
                string fooBaar4,
                string fooBaar5,
                string fooBaar6,
                // ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
                string FooBaar1,
                // ReSharper disable once InconsistentNaming
                string FooBaar5,
                // ReSharper disable once InconsistentNaming
                string FooBaar6)
#pragma warning restore IDE1006 // Naming Styles
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_partial_set_of_parameters_that_resolve()
        {
            var constructorBinding = GetBinding<BlogWeirdScience>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(2, parameters.Length);
            Assert.Equal(2, bindings.Count);

            Assert.Equal("content", parameters[0].Name);
            Assert.Equal("follows", parameters[1].Name);

            Assert.Equal("_content", bindings[0].ConsumedProperties.First().Name);
            Assert.Equal("m_follows", bindings[1].ConsumedProperties.First().Name);
        }

        private class BlogWeirdScience : Blog
        {
            public BlogWeirdScience(string content, int follows)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_context()
        {
            var constructorBinding = GetBinding<BlogWithContext>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(2, parameters.Length);
            Assert.Equal(2, bindings.Count);

            Assert.Equal("id", parameters[0].Name);
            Assert.Equal("context", parameters[1].Name);

            Assert.IsType<PropertyParameterBinding>(bindings[0]);
            Assert.Equal("Id", bindings[0].ConsumedProperties.First().Name);

            Assert.IsType<ContextParameterBinding>(bindings[1]);
            Assert.Empty(bindings[1].ConsumedProperties);
            Assert.Same(typeof(DbContext), ((ContextParameterBinding)bindings[1]).ServiceType);
        }

        private class BlogWithContext : Blog
        {
            public BlogWithContext(int id, DbContext context)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_context_typed()
        {
            var constructorBinding = GetBinding<BlogWithTypedContext>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Single(parameters);
            Assert.Equal(1, bindings.Count);

            Assert.Equal("context", parameters[0].Name);

            Assert.IsType<ContextParameterBinding>(bindings[0]);
            Assert.Empty(bindings[0].ConsumedProperties);
            Assert.Same(typeof(TypedContext), ((ContextParameterBinding)bindings[0]).ServiceType);
        }

        private class BlogWithTypedContext : Blog
        {
            public BlogWithTypedContext(TypedContext context)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_ILazyLoader()
        {
            var constructorBinding = GetBinding<BlogWithLazyLoader>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Single(parameters);
            Assert.Equal(1, bindings.Count);

            Assert.Equal("loader", parameters[0].Name);

            Assert.IsType<DependencyInjectionParameterBinding>(bindings[0]);
            Assert.Empty(bindings[0].ConsumedProperties);
            Assert.Same(typeof(ILazyLoader), ((DependencyInjectionParameterBinding)bindings[0]).ServiceType);
        }

        private class BlogWithLazyLoader : Blog
        {
            public BlogWithLazyLoader(ILazyLoader loader)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_delegate_parameter_called_lazyLoader()
        {
            var constructorBinding = GetBinding<BlogWithLazyLoaderMethod>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Single(parameters);
            Assert.Equal(1, bindings.Count);

            Assert.Equal("lazyLoader", parameters[0].Name);

            Assert.IsType<DependencyInjectionMethodParameterBinding>(bindings[0]);
            Assert.Empty(bindings[0].ConsumedProperties);
            Assert.Same(typeof(ILazyLoader), ((DependencyInjectionMethodParameterBinding)bindings[0]).ServiceType);
        }

        private class BlogWithLazyLoaderMethod : Blog
        {
            public BlogWithLazyLoaderMethod(Action<object, string> lazyLoader)
            {
            }
        }

        [ConditionalFact]
        public void Binds_to_IEntityType()
        {
            var constructorBinding = GetBinding<BlogWithEntityType>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Single(parameters);
            Assert.Equal(1, bindings.Count);

            Assert.Equal("entityType", parameters[0].Name);

            Assert.IsType<EntityTypeParameterBinding>(bindings[0]);
            Assert.Empty(bindings[0].ConsumedProperties);
        }

        private class BlogWithEntityType : Blog
        {
            public BlogWithEntityType(IEntityType entityType)
            {
            }
        }

        [ConditionalFact]
        public void Does_not_bind_to_delegate_parameter_not_called_lazyLoader()
        {
            var constructorBinding = GetBinding<BlogWithOtherMethod>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Empty(parameters);
            Assert.Equal(0, bindings.Count);
        }

        private class BlogWithOtherMethod : Blog
        {
            public BlogWithOtherMethod()
            {
            }

            public BlogWithOtherMethod(Action<object, string> loader)
            {
            }
        }

        private class TypedContext : DbContext
        {
        }

        [ConditionalFact]
        public void Throws_if_no_usable_constructor()
        {
            var constructors = new[]
            {
                CoreStrings.ConstructorBindingFailed("did", "BlogNone(string title, int did)"),
                CoreStrings.ConstructorBindingFailed("notTitle", "BlogNone(string notTitle, Nullable<Guid> shadow, int id)"),
                CoreStrings.ConstructorBindingFailed("dummy", "BlogNone(string title, Nullable<Guid> shadow, bool dummy, int id)"),
                CoreStrings.ConstructorBindingFailed(
                    "dummy', 'description",
                    "BlogNone(string title, Nullable<Guid> shadow, bool dummy, int id, string description)")
            };

            Assert.Equal(
                CoreStrings.ConstructorNotFound(nameof(BlogNone), string.Join("; ", constructors)),
                Assert.Throws<InvalidOperationException>(() => GetBinding<BlogNone>()).Message);
        }

        private class BlogNone : Blog
        {
            public BlogNone(string title, int did)
            {
            }

            public BlogNone(string notTitle, Guid? shadow, int id)
            {
            }

            public BlogNone(string title, Guid? shadow, bool dummy, int id)
            {
            }

            public BlogNone(string title, Guid? shadow, bool dummy, int id, string description)
            {
            }
        }

        [ConditionalFact]
        public void Throws_if_no_usable_constructor_due_to_bad_type()
        {
            Assert.Equal(
                CoreStrings.ConstructorNotFound(
                    nameof(BlogBadType),
                    CoreStrings.ConstructorBindingFailed("shadow", "BlogBadType(Guid shadow, int id)")),
                Assert.Throws<InvalidOperationException>(() => GetBinding<BlogBadType>()).Message);
        }

        private class BlogBadType : Blog
        {
            public BlogBadType(Guid shadow, int id)
            {
            }
        }

        [ConditionalFact]
        public void Throws_in_validation_if_field_not_found()
        {
            using var context = new NoFieldContext();
            Assert.Equal(
                CoreStrings.NoBackingFieldLazyLoading("NoFieldRelated", "NoField"),
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        private class NoFieldContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());

            public DbSet<NoField> NoFields { get; }
            public DbSet<NoFieldRelated> NoFieldRelateds { get; }
        }

        private class NoField
        {
            private readonly Action<object, string> _loader;
            private ICollection<NoFieldRelated> _hidden_noFieldRelated;
            public int Id { get; set; }

            public NoField(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            public ICollection<NoFieldRelated> NoFieldRelated
            {
                get => _loader.Load(this, ref _hidden_noFieldRelated);
                set => _hidden_noFieldRelated = value;
            }
        }

        private class NoFieldRelated
        {
            public int Id { get; set; }
            public NoField NoField { get; set; }
        }

        private ConstructorBinding GetBinding<TEntity>(Action<IMutableEntityType> setBinding = null)
        {
            var entityType = ((IMutableModel)new Model()).AddEntityType(typeof(TEntity));
            entityType.AddProperty(nameof(Blog.Id), typeof(int));
            entityType.AddProperty(nameof(Blog.Title), typeof(string));
            entityType.AddProperty(nameof(Blog._content), typeof(string));
            entityType.AddProperty(nameof(Blog.m_follows), typeof(int));
            entityType.AddProperty("Shadow", typeof(Guid?));
            entityType.AddProperty("FooBaar1", typeof(string));
            entityType.AddProperty("fooBaar2", typeof(string));
            entityType.AddProperty("_fooBaar3", typeof(string));
            entityType.AddProperty("m_fooBaar4", typeof(string));
            entityType.AddProperty("_FooBaar5", typeof(string));
            entityType.AddProperty("m_FooBaar6", typeof(string));

            setBinding?.Invoke(entityType);

            var model = (Model)entityType.Model;
            var context = new ConventionContext<IConventionModelBuilder>(model.ConventionDispatcher);

            var convention = new ConstructorBindingConvention(CreateDependencies());
            convention.ProcessModelFinalizing(model.Builder, context);

            return (ConstructorBinding)entityType[CoreAnnotationNames.ConstructorBinding];
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private abstract class Blog
        {
#pragma warning disable 649, IDE1006 // Naming Styles
            public string _content;

            public int m_follows;
#pragma warning restore 649, IDE1006 // Naming Styles

            public int Id { get; set; }
            public string Title { get; set; }
        }
    }
}
