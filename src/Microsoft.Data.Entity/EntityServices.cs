// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Services;

namespace Microsoft.Data.Entity
{
    public static class EntityServices
    {
        public static IEnumerable<IServiceDescriptor> GetDefaultServices()
        {
            yield return Service.Singleton<ILoggerFactory, ConsoleLoggerFactory>();
            yield return Service.Singleton<IModelSource, DefaultModelSource>();
            yield return Service.Singleton<IdentityGeneratorFactory, DefaultIdentityGeneratorFactory>();
            yield return Service.Singleton<ActiveIdentityGenerators, ActiveIdentityGenerators>();
            yield return Service.Singleton<StateManagerFactory, StateManagerFactory>();
            yield return Service.Singleton<EntitySetFinder, EntitySetFinder>();
            yield return Service.Singleton<EntitySetInitializer, EntitySetInitializer>();
            yield return Service.Singleton<IEntityStateListener, NavigationFixer>();
        }
    }
}
