// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.AspNet.DependencyInjection
{
    public static class EntityServiceCollectionExtensions
    {
        public static ServiceCollection AddEntityFramework([NotNull] this ServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, "serviceCollection");

            return AddEntityFramework(serviceCollection, null);
        }

        public static ServiceCollection AddEntityFramework(
            [NotNull] this ServiceCollection serviceCollection, [CanBeNull] Action<EntityServicesBuilder> nestedBuilder)
        {
            Check.NotNull(serviceCollection, "serviceCollection");

            serviceCollection
                .AddSingleton<IModelSource, DefaultModelSource>()
                .AddSingleton<IdentityGeneratorFactory, DefaultIdentityGeneratorFactory>()
                .AddSingleton<ActiveIdentityGenerators, ActiveIdentityGenerators>()
                .AddSingleton<EntitySetFinder, EntitySetFinder>()
                .AddSingleton<EntitySetInitializer, EntitySetInitializer>()
                .AddSingleton<EntityKeyFactorySource, EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource, ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource, ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource, ClrCollectionAccessorSource>()
                .AddSingleton<EntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<MemberMapper, MemberMapper>()
                .AddSingleton<StateEntrySubscriber, StateEntrySubscriber>()
                .AddSingleton<FieldMatcher, FieldMatcher>()
                .AddSingleton<OriginalValuesFactory, OriginalValuesFactory>()
                .AddSingleton<StoreGeneratedValuesFactory, StoreGeneratedValuesFactory>()
                .AddSingleton<DataStoreSelector, DataStoreSelector>()
                .AddScoped<StateEntryFactory, StateEntryFactory>()
                .AddScoped<IEntityStateListener, NavigationFixer>()
                .AddScoped<StateEntryNotifier, StateEntryNotifier>()
                .AddScoped<ContextConfiguration, ContextConfiguration>()
                .AddScoped<ContextEntitySets, ContextEntitySets>()
                .AddScoped<StateManager, StateManager>();

            if (nestedBuilder != null)
            {
                nestedBuilder(new EntityServicesBuilder(serviceCollection));
            }

            return serviceCollection;
        }
    }
}
