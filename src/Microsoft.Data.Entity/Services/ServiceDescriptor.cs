// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Services
{
    public sealed class Service
    {
        public static IServiceDescriptor Singleton<TService, TImplementation>()
        {
            return FromType(LifecycleKind.Singleton, typeof(TService), typeof(TImplementation));
        }

        public static IServiceDescriptor Singleton<TService>([NotNull] object implementationInstance)
        {
            Check.NotNull(implementationInstance, "implementationInstance");

            return FromInstance(LifecycleKind.Singleton, typeof(TService), implementationInstance);
        }

        public static IServiceDescriptor Transient<TService, TImplementation>()
        {
            return FromType(LifecycleKind.Transient, typeof(TService), typeof(TImplementation));
        }

        public static IServiceDescriptor Transient<TService>([NotNull] object implementationInstance)
        {
            Check.NotNull(implementationInstance, "implementationInstance");

            return FromInstance(LifecycleKind.Transient, typeof(TService), implementationInstance);
        }

        public static IServiceDescriptor Scoped<TService, TImplementation>()
        {
            return FromType(LifecycleKind.Scoped, typeof(TService), typeof(TImplementation));
        }

        public static IServiceDescriptor Scoped<TService>([NotNull] object implementationInstance)
        {
            Check.NotNull(implementationInstance, "implementationInstance");

            return FromInstance(LifecycleKind.Scoped, typeof(TService), implementationInstance);
        }

        private static IServiceDescriptor FromType(
            LifecycleKind lifecycleKind, [NotNull] Type serviceType, [NotNull] Type implementationType)
        {
            return new ServiceDescriptor
                {
                    Lifecycle = lifecycleKind,
                    ServiceType = serviceType,
                    ImplementationType = implementationType
                };
        }

        private static IServiceDescriptor FromInstance(
            LifecycleKind lifecycleKind, [NotNull] Type serviceType, [NotNull] object implementationInstance)
        {
            return new ServiceDescriptor
                {
                    Lifecycle = lifecycleKind,
                    ServiceType = serviceType,
                    ImplementationInstance = implementationInstance
                };
        }
    }
}
