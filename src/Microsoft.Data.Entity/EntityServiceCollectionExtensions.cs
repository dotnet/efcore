// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
                .AddSingleton<DbSetFinder, DbSetFinder>()
                .AddSingleton<DbSetInitializer, DbSetInitializer>()
                .AddSingleton<EntityKeyFactorySource, EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource, ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource, ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource, ClrCollectionAccessorSource>()
                .AddSingleton<EntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<CompositeEntityKeyFactory, CompositeEntityKeyFactory>()
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
                .AddScoped<ContextSets, ContextSets>()
                .AddScoped<StateManager, StateManager>()
                .AddScoped<Database, Database>();

            if (nestedBuilder != null)
            {
                nestedBuilder(new EntityServicesBuilder(serviceCollection));
            }

            return serviceCollection;
        }
    }
}
