// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Framework.DependencyInjection
{
    public static class EntityServiceCollectionExtensions
    {
        public static EntityServicesBuilder AddEntityFramework([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, "serviceCollection");

            serviceCollection
                .AddSingleton<IModelSource, DefaultModelSource>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<GuidValueGenerator>>()
                .AddSingleton<DbSetFinder>()
                .AddSingleton<DbSetInitializer>()
                .AddSingleton<EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource>()
                .AddSingleton<NavigationAccessorSource>()
                .AddSingleton<EntityMaterializerSource>()
                .AddSingleton<CompositeEntityKeyFactory>()
                .AddSingleton<MemberMapper>()
                .AddSingleton<StateEntrySubscriber>()
                .AddSingleton<FieldMatcher>()
                .AddSingleton<OriginalValuesFactory>()
                .AddSingleton<StoreGeneratedValuesFactory>()
                .AddSingleton<DataStoreSelector>()
                .AddScoped<StateEntryFactory>()
                .AddScoped<IEntityStateListener, NavigationFixer>()
                .AddScoped<StateEntryNotifier>()
                .AddScoped<DbContextConfiguration>()
                .AddScoped<ContextSets>()
                .AddScoped<StateManager>();

            return new EntityServicesBuilder(serviceCollection);
        }
    }
}
