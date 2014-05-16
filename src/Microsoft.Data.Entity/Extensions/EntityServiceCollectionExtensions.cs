// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Framework.DependencyInjection
{
    public static class EntityServiceCollectionExtensions
    {
        public static EntityServicesBuilder AddEntityFramework([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, "serviceCollection");

            serviceCollection
                .AddSingleton<IModelSource, DefaultModelSource>()
                .AddSingleton<IdentityGeneratorFactory, DefaultIdentityGeneratorFactory>()
                .AddSingleton<ActiveIdentityGenerators>()
                .AddSingleton<DbSetFinder>()
                .AddSingleton<DbSetInitializer>()
                .AddSingleton<EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource>()
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
                .AddScoped<StateManager>()
                .AddScoped<Database>();

            return new EntityServicesBuilder(serviceCollection);
        }
    }
}
