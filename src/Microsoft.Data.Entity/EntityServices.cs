// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.AspNet.DependencyInjection;
    using Microsoft.AspNet.Logging;
    using Microsoft.Data.Entity.Services;
    using Microsoft.Data.Entity.Utilities;

    public static class EntityServices
    {
        public static ServiceProvider CreateDefaultProvider()
        {
            var serviceProvider = new ServiceProvider();

            AddDefaultServices(
                (serviceType, implementationType)
                    => serviceProvider.Add(serviceType, implementationType));

            return serviceProvider;
        }

        public static void AddDefaultServices([NotNull] Action<Type, Type> serviceRegistrar)
        {
            Check.NotNull(serviceRegistrar, "serviceRegistrar");

            serviceRegistrar(typeof(ILoggerFactory), typeof(ConsoleLoggerFactory));
        }
    }
}
