// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;

namespace Microsoft.Data.Entity
{
    public static class EntityServices
    {
        public static ServiceCollection GetDefaultServices()
        {
            return new ServiceCollection()
                .AddSingleton<ILoggerFactory, NullLoggerFactory>()
                .AddSingleton<IModelSource, DefaultModelSource>()
                .AddSingleton<IdentityGeneratorFactory, DefaultIdentityGeneratorFactory>()
                .AddSingleton<ActiveIdentityGenerators, ActiveIdentityGenerators>()
                .AddSingleton<EntitySetFinder, EntitySetFinder>()
                .AddSingleton<EntitySetInitializer, EntitySetInitializer>()
                .AddSingleton<EntityKeyFactorySource, EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource, ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource, ClrPropertySetterSource>()
                .AddSingleton<EntitySetSource, EntitySetSource>()
                .AddSingleton<ClrCollectionAccessorSource, ClrCollectionAccessorSource>()
                .AddSingleton<EntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<MemberMapper, MemberMapper>()
                .AddSingleton<StateEntrySubscriber, StateEntrySubscriber>()
                .AddSingleton<FieldMatcher, FieldMatcher>()
                .AddScoped<StateEntryFactory, StateEntryFactory>()
                .AddScoped<IEntityStateListener, NavigationFixer>()
                .AddScoped<StateEntryNotifier, StateEntryNotifier>()
                .AddScoped<ContextConfiguration, ContextConfiguration>()
                .AddScoped<ContextEntitySets, ContextEntitySets>()
                .AddScoped<StateManager, StateManager>();
        }
    }
}
