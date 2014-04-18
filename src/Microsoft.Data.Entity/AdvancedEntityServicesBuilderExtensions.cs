// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.AspNet.DependencyInjection.Advanced
{
    public static class AdvancedEntityServicesBuilderExtensions
    {
        // Dingletons

        public static EntityServicesBuilder UseModelSource([NotNull] this EntityServicesBuilder builder, [NotNull] IModelSource modelSource)
        {
            Check.NotNull(modelSource, "modelSource");

            builder.ServiceCollection.AddInstance<IModelSource>(modelSource);

            return builder;
        }

        public static EntityServicesBuilder UseEntitySetInitializer([NotNull] this EntityServicesBuilder builder, [NotNull] EntitySetInitializer initializer)
        {
            Check.NotNull(initializer, "initializer");

            builder.ServiceCollection.AddInstance<EntitySetInitializer>(initializer);

            return builder;
        }

        public static EntityServicesBuilder UseEntitySetSource([NotNull] this EntityServicesBuilder builder, [NotNull] EntitySetSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance<EntitySetSource>(source);

            return builder;
        }

        public static EntityServicesBuilder UseIdentityGeneratorFactory([NotNull] this EntityServicesBuilder builder, [NotNull] IdentityGeneratorFactory factory)
        {
            Check.NotNull(factory, "factory");

            builder.ServiceCollection.AddInstance<IdentityGeneratorFactory>(factory);

            return builder;
        }

        public static EntityServicesBuilder UseActiveIdentityGenerators([NotNull] this EntityServicesBuilder builder, [NotNull] ActiveIdentityGenerators generators)
        {
            Check.NotNull(generators, "generators");

            builder.ServiceCollection.AddInstance<ActiveIdentityGenerators>(generators);

            return builder;
        }

        public static EntityServicesBuilder UseEntitySetFinder([NotNull] this EntityServicesBuilder builder, [NotNull] EntitySetFinder finder)
        {
            Check.NotNull(finder, "finder");

            builder.ServiceCollection.AddInstance<EntitySetFinder>(finder);

            return builder;
        }

        public static EntityServicesBuilder UseEntityKeyFactorySource([NotNull] this EntityServicesBuilder builder, [NotNull] EntityKeyFactorySource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance<EntityKeyFactorySource>(source);

            return builder;
        }

        public static EntityServicesBuilder UseClrCollectionAccessorSource([NotNull] this EntityServicesBuilder builder, [NotNull] ClrCollectionAccessorSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance<ClrCollectionAccessorSource>(source);

            return builder;
        }

        public static EntityServicesBuilder UseClrPropertyGetterSource([NotNull] this EntityServicesBuilder builder, [NotNull] ClrPropertyGetterSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance<ClrPropertyGetterSource>(source);

            return builder;
        }

        public static EntityServicesBuilder UseClrPropertySetterSource([NotNull] this EntityServicesBuilder builder, [NotNull] ClrPropertySetterSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance<ClrPropertySetterSource>(source);

            return builder;
        }

        public static EntityServicesBuilder UseEntityMaterializerSource([NotNull] this EntityServicesBuilder builder, [NotNull] EntityMaterializerSource source)
        {
            Check.NotNull(source, "source");

            builder.ServiceCollection.AddInstance<EntityMaterializerSource>(source);

            return builder;
        }

        public static EntityServicesBuilder UseLoggerFactory([NotNull] this EntityServicesBuilder builder, [NotNull] ILoggerFactory factory)
        {
            Check.NotNull(factory, "factory");

            builder.ServiceCollection.AddInstance<ILoggerFactory>(factory);

            return builder;
        }

        public static EntityServicesBuilder UseModelSource<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : IModelSource
        {
            builder.ServiceCollection.AddSingleton<IModelSource, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseEntitySetInitializer<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : EntitySetInitializer
        {
            builder.ServiceCollection.AddSingleton<EntitySetInitializer, TService>();

            return builder;
        }

        public static EntityServicesBuilder UseEntitySetSource<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : EntitySetSource
        {
            builder.ServiceCollection.AddSingleton<EntitySetSource, TService>();

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

        public static EntityServicesBuilder UseEntitySetFinder<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : EntitySetFinder
        {
            builder.ServiceCollection.AddSingleton<EntitySetFinder, TService>();

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

        public static EntityServicesBuilder UseContextEntitySets<TService>([NotNull] this EntityServicesBuilder builder)
            where TService : ContextEntitySets
        {
            builder.ServiceCollection.AddScoped<ContextEntitySets, TService>();

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
