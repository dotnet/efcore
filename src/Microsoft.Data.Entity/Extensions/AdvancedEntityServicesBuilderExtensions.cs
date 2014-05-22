// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Framework.DependencyInjection.Advanced
{
    public static class AdvancedEntityServicesBuilderExtensions
    {
        // Dingletons

        public static EntityServicesBuilder UseModelSource([NotNull] this EntityServicesBuilder builder, [NotNull] IModelSource modelSource)
        {
            Check.NotNull(modelSource, "modelSource");

            builder.ServiceCollection.AddInstance(modelSource);

            return builder;
        }

        public static EntityServicesBuilder UseDbSetInitializer([NotNull] this EntityServicesBuilder builder, [NotNull] DbSetInitializer initializer)
        {
            Check.NotNull(initializer, "initializer");

            builder.ServiceCollection.AddInstance(initializer);

            return builder;
        }

        public static EntityServicesBuilder UseIdentityGeneratorFactory([NotNull] this EntityServicesBuilder builder, [NotNull] IdentityGeneratorFactory factory)
        {
            Check.NotNull(factory, "factory");

            builder.ServiceCollection.AddInstance(factory);

            return builder;
        }

        public static EntityServicesBuilder UseActiveIdentityGenerators([NotNull] this EntityServicesBuilder builder, [NotNull] ActiveIdentityGenerators generators)
        {
            Check.NotNull(generators, "generators");

            builder.ServiceCollection.AddInstance(generators);

            return builder;
        }

        public static EntityServicesBuilder UseDbSetFinder([NotNull] this EntityServicesBuilder builder, [NotNull] DbSetFinder finder)
        {
            Check.NotNull(finder, "finder");

            builder.ServiceCollection.AddInstance(finder);

            return builder;
        }

        public static EntityServicesBuilder UseEntityKeyFactorySource([NotNull] this EntityServicesBuilder builder, [NotNull] EntityKeyFactorySource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance(source);

            return builder;
        }

        public static EntityServicesBuilder UseClrCollectionAccessorSource([NotNull] this EntityServicesBuilder builder, [NotNull] ClrCollectionAccessorSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance(source);

            return builder;
        }

        public static EntityServicesBuilder UseClrPropertyGetterSource([NotNull] this EntityServicesBuilder builder, [NotNull] ClrPropertyGetterSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance(source);

            return builder;
        }

        public static EntityServicesBuilder UseClrPropertySetterSource([NotNull] this EntityServicesBuilder builder, [NotNull] ClrPropertySetterSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance(source);

            return builder;
        }

        public static EntityServicesBuilder UseEntityMaterializerSource([NotNull] this EntityServicesBuilder builder, [NotNull] EntityMaterializerSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance(source);

            return builder;
        }

        public static EntityServicesBuilder UseLoggerFactory([NotNull] this EntityServicesBuilder builder, [NotNull] ILoggerFactory factory)
        {
            Check.NotNull(factory, "factory");

            builder.ServiceCollection.AddInstance(factory);

            return builder;
        }

        public static EntityServicesBuilder UseModelSource<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : IModelSource
        {
            builder.ServiceCollection.AddSingleton<IModelSource, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseDbSetInitializer<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : DbSetInitializer
        {
            builder.ServiceCollection.AddSingleton<DbSetInitializer, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseIdentityGeneratorFactory<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : IdentityGeneratorFactory
        {
            builder.ServiceCollection.AddSingleton<IdentityGeneratorFactory, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseActiveIdentityGenerators<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : ActiveIdentityGenerators
        {
            builder.ServiceCollection.AddSingleton<ActiveIdentityGenerators, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseDbSetFinder<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : DbSetFinder
        {
            builder.ServiceCollection.AddSingleton<DbSetFinder, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseEntityKeyFactorySource<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : EntityKeyFactorySource
        {
            builder.ServiceCollection.AddSingleton<EntityKeyFactorySource, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseClrCollectionAccessorSource<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : ClrCollectionAccessorSource
        {
            builder.ServiceCollection.AddSingleton<ClrCollectionAccessorSource, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseClrPropertyGetterSource<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : ClrPropertyGetterSource
        {
            builder.ServiceCollection.AddSingleton<ClrPropertyGetterSource, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseClrPropertySetterSource<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : ClrPropertySetterSource
        {
            builder.ServiceCollection.AddSingleton<ClrPropertySetterSource, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseEntityMaterializerSource<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : EntityMaterializerSource
        {
            builder.ServiceCollection.AddSingleton<EntityMaterializerSource, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseLoggerFactory<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : ILoggerFactory
        {
            builder.ServiceCollection.AddSingleton<ILoggerFactory, TService>();

            return builder;
        }

        // Scoped by context

        public static EntityServicesBuilder UseEntityStateListener<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : IEntityStateListener
        {
            builder.ServiceCollection.AddScoped<IEntityStateListener, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseStateEntryFactory<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : StateEntryFactory
        {
            builder.ServiceCollection.AddScoped<StateEntryFactory, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseStateEntryNotifier<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : StateEntryNotifier
        {
            builder.ServiceCollection.AddScoped<StateEntryNotifier, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseContextSets<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : ContextSets
        {
            builder.ServiceCollection.AddScoped<ContextSets, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseStateManager<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : StateManager
        {
            builder.ServiceCollection.AddScoped<StateManager, TService>();

            return builder;
        }
    }
}
