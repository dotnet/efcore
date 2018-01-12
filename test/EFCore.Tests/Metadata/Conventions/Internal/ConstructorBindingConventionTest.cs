// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class ConstructorBindingConventionTest
    {
        [Fact]
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

        [Fact]
        public void Binds_to_most_parameters_that_resolve()
        {
            var constructorBinding = GetBinding<BlogSeveral>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(3, parameters.Length);
            Assert.Equal(3, bindings.Count);

            Assert.Equal("title", parameters[0].Name);
            Assert.Equal("shadow", parameters[1].Name);
            Assert.Equal("id", parameters[2].Name);

            Assert.Equal("Title", bindings[0].ConsumedProperties.First().Name);
            Assert.Equal("Shadow", bindings[1].ConsumedProperties.First().Name);
            Assert.Equal("Id", bindings[2].ConsumedProperties.First().Name);
        }

        private class BlogSeveral : Blog
        {
            public BlogSeveral()
            {
            }

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

        [Fact]
        public void Throws_if_two_constructors_with_same_number_of_parameters_could_be_used()
        {
            Assert.Equal(
                CoreStrings.ConstructorConflict(
                    "BlogConflict(string, int)",
                    "BlogConflict(string, Nullable<Guid>)"),
                Assert.Throws<InvalidOperationException>(
                    () => GetBinding<BlogConflict>()).Message);
        }

        private class BlogConflict : Blog
        {
            public BlogConflict()
            {
            }

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

        [Fact]
        public void Resolvess_properties_with_different_kinds_of_name()
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
                string FooBaar1,
                // ReSharper disable once InconsistentNaming
                string FooBaar5,
                // ReSharper disable once InconsistentNaming
                string FooBaar6)
            {
            }
        }

        [Fact]
        public void Binds_to_partial_set_of_parameters_that_resolve()
        {
            var constructorBinding = GetBinding<BlogWierdScience>();

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

        private class BlogWierdScience : Blog
        {
            public BlogWierdScience(string content, int follows)
            {
            }
        }

        [Fact]
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
            Assert.Same(typeof(DbContext), ((ContextParameterBinding)bindings[1]).ContextType);
        }

        private class BlogWithContext : Blog
        {
            public BlogWithContext(int id, DbContext context)
            {
            }
        }

        [Fact]
        public void Binds_to_context_typed()
        {
            var constructorBinding = GetBinding<BlogWithTypedContext>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(1, parameters.Length);
            Assert.Equal(1, bindings.Count);

            Assert.Equal("context", parameters[0].Name);

            Assert.IsType<ContextParameterBinding>(bindings[0]);
            Assert.Empty(bindings[0].ConsumedProperties);
            Assert.Same(typeof(TypedContext), ((ContextParameterBinding)bindings[0]).ContextType);
        }

        private class BlogWithTypedContext : Blog
        {
            public BlogWithTypedContext(TypedContext context)
            {
            }
        }

        [Fact]
        public void Binds_to_ILazyLoader()
        {
            var constructorBinding = GetBinding<BlogWithLazyLoader>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(1, parameters.Length);
            Assert.Equal(1, bindings.Count);

            Assert.Equal("loader", parameters[0].Name);

            Assert.IsType<ServiceParameterBinding>(bindings[0]);
            Assert.Empty(bindings[0].ConsumedProperties);
            Assert.Same(typeof(ILazyLoader), ((ServiceParameterBinding)bindings[0]).ServiceType);
        }

        private class BlogWithLazyLoader : Blog
        {
            public BlogWithLazyLoader(ILazyLoader loader)
            {
            }
        }

        [Fact]
        public void Binds_to_delegate_parameter_called_lazyLoader()
        {
            var constructorBinding = GetBinding<BlogWithLazyLoaderMethod>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(1, parameters.Length);
            Assert.Equal(1, bindings.Count);

            Assert.Equal("lazyLoader", parameters[0].Name);

            Assert.IsType<ServiceMethodParameterBinding>(bindings[0]);
            Assert.Empty(bindings[0].ConsumedProperties);
            Assert.Same(typeof(ILazyLoader), ((ServiceMethodParameterBinding)bindings[0]).ServiceType);
        }

        private class BlogWithLazyLoaderMethod : Blog
        {
            public BlogWithLazyLoaderMethod(Action<object, string> lazyLoader)
            {
            }
        }

        [Fact]
        public void Binds_to_IEntityType()
        {
            var constructorBinding = GetBinding<BlogWithEntityType>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(1, parameters.Length);
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

        [Fact]
        public void Does_not_bind_to_delegate_parameter_not_called_lazyLoader()
        {
            var constructorBinding = GetBinding<BlogWithOtherMethod>();

            Assert.NotNull(constructorBinding);

            var parameters = constructorBinding.Constructor.GetParameters();
            var bindings = constructorBinding.ParameterBindings;

            Assert.Equal(0, parameters.Length);
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

        [Fact]
        public void Throws_if_no_usable_constructor()
        {
            Assert.Equal(
                CoreStrings.ConstructorNotFound(nameof(BlogNone), "dummy', 'notTitle', 'did"),
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
        }

        [Fact]
        public void Throws_if_no_usable_constructor_due_to_bad_type()
        {
            Assert.Equal(
                CoreStrings.ConstructorNotFound(nameof(BlogBadType), "shadow"),
                Assert.Throws<InvalidOperationException>(() => GetBinding<BlogBadType>()).Message);
        }

        private class BlogBadType : Blog
        {
            public BlogBadType(Guid shadow, int id)
            {
            }
        }

        [Fact]
        public void Throws_in_validation_if_field_not_found()
        {
            using (var context = new NoFieldContext())
            {
                Assert.Equal(
                    CoreStrings.NoBackingFieldLazyLoading("NoFieldRelated", "NoField"),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class NoFieldContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

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

        private static DirectConstructorBinding GetBinding<TEntity>()
        {
            var convention = TestServiceFactory.Instance.Create<ConstructorBindingConvention>();

            var entityType = new Model().AddEntityType(typeof(TEntity));
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

            convention.Apply(entityType.Model.Builder);

            return (DirectConstructorBinding)entityType[CoreAnnotationNames.ConstructorBinding];
        }

        private abstract class Blog
        {
#pragma warning disable 649
            public string _content;

            // ReSharper disable once InconsistentNaming
            public int m_follows;
#pragma warning restore 649

            public int Id { get; set; }
            public string Title { get; set; }
        }
    }
}
