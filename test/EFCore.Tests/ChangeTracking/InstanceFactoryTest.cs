// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    public class InstanceFactoryTest
    {
        [ConditionalFact]
        public void Create_instance_with_parameterless_constructor()
        {
            using var context = new FactoryContext();

            var entityType = context.Model.FindEntityType(typeof(Parameterless));
            var factory = entityType.GetInstanceFactory();
            var instance1 = factory(new MaterializationContext(ValueBuffer.Empty, context));
            var instance2 = factory(new MaterializationContext(ValueBuffer.Empty, context));

            Assert.IsType<Parameterless>(instance1);
            Assert.IsType<Parameterless>(instance2);
            Assert.NotSame(instance1, instance2);
        }

        [ConditionalFact]
        public void Create_instance_with_lazy_loader()
        {
            using var context = new FactoryContext();

            var entityType = context.Model.FindEntityType(typeof(WithLazyLoader));
            var factory = entityType.GetInstanceFactory();
            var instance1 = factory(new MaterializationContext(ValueBuffer.Empty, context));
            var instance2 = factory(new MaterializationContext(ValueBuffer.Empty, context));

            Assert.IsType<WithLazyLoader>(instance1);
            Assert.NotNull(((WithLazyLoader)instance1).LazyLoader);
            Assert.IsType<WithLazyLoader>(instance2);
            Assert.NotSame(instance1, instance2);
            Assert.NotSame(((WithLazyLoader)instance1).LazyLoader, ((WithLazyLoader)instance2).LazyLoader);
        }

        [ConditionalFact]
        public void Create_instance_with_lazy_loading_delegate()
        {
            using var context = new FactoryContext();

            var entityType = context.Model.FindEntityType(typeof(WithLazyLoaderDelegate));
            var factory = entityType.GetInstanceFactory();
            var instance1 = factory(new MaterializationContext(ValueBuffer.Empty, context));
            var instance2 = factory(new MaterializationContext(ValueBuffer.Empty, context));

            Assert.IsType<WithLazyLoaderDelegate>(instance1);
            Assert.NotNull(((WithLazyLoaderDelegate)instance1).LazyLoader);
            Assert.IsType<WithLazyLoaderDelegate>(instance2);
            Assert.NotSame(instance1, instance2);
            Assert.NotSame(((WithLazyLoaderDelegate)instance1).LazyLoader, ((WithLazyLoaderDelegate)instance2).LazyLoader);
        }

        [ConditionalFact]
        public void Create_instance_with_entity_type()
        {
            using var context = new FactoryContext();

            var entityType = context.Model.FindEntityType(typeof(WithEntityType));
            var factory = entityType.GetInstanceFactory();
            var instance1 = factory(new MaterializationContext(ValueBuffer.Empty, context));
            var instance2 = factory(new MaterializationContext(ValueBuffer.Empty, context));

            Assert.IsType<WithEntityType>(instance1);
            Assert.NotNull(((WithEntityType)instance1).EntityType);
            Assert.IsType<WithEntityType>(instance2);
            Assert.NotSame(instance1, instance2);
            Assert.Same(((WithEntityType)instance1).EntityType, ((WithEntityType)instance2).EntityType);
        }

        [ConditionalFact]
        public void Create_instance_with_context()
        {
            using var context = new FactoryContext();

            var entityType = context.Model.FindEntityType(typeof(WithContext));
            var factory = entityType.GetInstanceFactory();
            var instance1 = factory(new MaterializationContext(ValueBuffer.Empty, context));
            var instance2 = factory(new MaterializationContext(ValueBuffer.Empty, context));

            Assert.IsType<WithContext>(instance1);
            Assert.Same(context, ((WithContext)instance1).Context);
            Assert.IsType<WithContext>(instance2);
            Assert.NotSame(instance1, instance2);
            Assert.Same(context, ((WithContext)instance2).Context);
        }

        [ConditionalFact]
        public void Create_instance_with_service_and_with_properties()
        {
            using var context = new FactoryContext();

            var entityType = context.Model.FindEntityType(typeof(WithServiceAndWithProperties));
            var factory = entityType.GetInstanceFactory();
            var instance1 = factory(new MaterializationContext(ValueBuffer.Empty, context));
            var instance2 = factory(new MaterializationContext(ValueBuffer.Empty, context));

            Assert.IsType<WithServiceAndWithProperties>(instance1);
            Assert.NotNull(((WithServiceAndWithProperties)instance1).LazyLoader);
            Assert.IsType<WithServiceAndWithProperties>(instance2);
            Assert.NotSame(instance1, instance2);
            Assert.NotSame(((WithServiceAndWithProperties)instance1).LazyLoader, ((WithServiceAndWithProperties)instance2).LazyLoader);
        }

        [ConditionalFact]
        public void Create_instance_with_parameterless_and_with_properties()
        {
            using var context = new FactoryContext();

            var entityType = context.Model.FindEntityType(typeof(ParameterlessAndWithProperties));
            var factory = entityType.GetInstanceFactory();
            var instance1 = factory(new MaterializationContext(ValueBuffer.Empty, context));
            var instance2 = factory(new MaterializationContext(ValueBuffer.Empty, context));

            Assert.IsType<ParameterlessAndWithProperties>(instance1);
            Assert.IsType<ParameterlessAndWithProperties>(instance2);
            Assert.NotSame(instance1, instance2);
        }

        [ConditionalFact]
        public void Throws_for_constructor_with_properties()
        {
            using var context = new FactoryContext();

            var entityType = context.Model.FindEntityType(typeof(WithProperties));

            Assert.Equal(
                CoreStrings.NoParameterlessConstructor(nameof(WithProperties)),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.GetInstanceFactory()).Message);
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
                => Id = id;

            public int Id { get; set; }
        }

        private class ParameterlessAndWithProperties
        {
            public ParameterlessAndWithProperties()
            {
            }

            public ParameterlessAndWithProperties(int id)
                => Id = id;

            public int Id { get; set; }
        }

        private class WithLazyLoader
        {
            public WithLazyLoader(ILazyLoader lazyLoader)
                => LazyLoader = lazyLoader;

            public int Id { get; set; }
            public ILazyLoader LazyLoader { get; }
        }

        private class WithLazyLoaderDelegate
        {
            public WithLazyLoaderDelegate(Action<object, string> lazyLoader)
                => LazyLoader = lazyLoader;

            public int Id { get; set; }
            public Action<object, string> LazyLoader { get; }
        }

        private class WithEntityType
        {
            public WithEntityType(IEntityType entityType)
                => EntityType = entityType;

            public int Id { get; set; }
            public IEntityType EntityType { get; }
        }

        private class WithContext
        {
            public WithContext(DbContext context)
                => Context = context;

            public int Id { get; set; }
            public DbContext Context { get; }
        }

        private class WithServiceAndWithProperties
        {
            public WithServiceAndWithProperties(ILazyLoader lazyLoader)
                => LazyLoader = lazyLoader;

            public WithServiceAndWithProperties(ILazyLoader lazyLoader, int id)
                : this(lazyLoader)
                => Id = id;

            public ILazyLoader LazyLoader { get; }
            public int Id { get; set; }
        }
    }
}
