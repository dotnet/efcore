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
                .AddSingleton<ILoggerFactory, ConsoleLoggerFactory>()
                .AddSingleton<IModelSource, DefaultModelSource>()
                .AddSingleton<IdentityGeneratorFactory, DefaultIdentityGeneratorFactory>()
                .AddSingleton<ActiveIdentityGenerators, ActiveIdentityGenerators>()
                .AddSingleton<StateManagerFactory, StateManagerFactory>()
                .AddSingleton<EntitySetFinder, EntitySetFinder>()
                .AddSingleton<EntitySetInitializer, EntitySetInitializer>()
                .AddSingleton<IEntityStateListener, NavigationFixer>()
                .AddSingleton<EntityKeyFactorySource, EntityKeyFactorySource>()
                .AddSingleton<StateEntryFactory, StateEntryFactory>()
                .AddSingleton<ClrPropertyGetterSource, ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource, ClrPropertySetterSource>()
                .AddSingleton<EntitySetSource, EntitySetSource>()
                .AddSingleton<EntityMaterializerSource, EntityMaterializerSource>();
        }
    }
}
