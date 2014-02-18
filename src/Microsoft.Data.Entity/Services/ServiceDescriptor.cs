// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Services
{
    public sealed class ServiceDescriptor : IServiceDescriptor
    {
        public static IServiceDescriptor Singleton<TService, TImplementation>()
        {
            return new ServiceDescriptor(LifecycleKind.Singleton, typeof(TService), typeof(TImplementation));
        }

        public static IServiceDescriptor Singleton<TService>([NotNull] object implementationInstance)
        {
            Check.NotNull(implementationInstance, "implementationInstance");

            return new ServiceDescriptor(LifecycleKind.Singleton, typeof(TService), implementationInstance);
        }

        public static IServiceDescriptor Transient<TService, TImplementation>()
        {
            return new ServiceDescriptor(LifecycleKind.Transient, typeof(TService), typeof(TImplementation));
        }

        public static IServiceDescriptor Transient<TService>([NotNull] object implementationInstance)
        {
            Check.NotNull(implementationInstance, "implementationInstance");

            return new ServiceDescriptor(LifecycleKind.Transient, typeof(TService), implementationInstance);
        }

        public static IServiceDescriptor Scoped<TService, TImplementation>()
        {
            return new ServiceDescriptor(LifecycleKind.Scoped, typeof(TService), typeof(TImplementation));
        }

        public static IServiceDescriptor Scoped<TService>([NotNull] object implementationInstance)
        {
            Check.NotNull(implementationInstance, "implementationInstance");

            return new ServiceDescriptor(LifecycleKind.Scoped, typeof(TService), implementationInstance);
        }

        private readonly LifecycleKind _lifecycleKind;
        private readonly Type _serviceType;
        private readonly Type _implementationType;
        private readonly object _implementationInstance;

        private ServiceDescriptor(
            LifecycleKind lifecycleKind, [NotNull] Type serviceType, [NotNull] Type implementationType)
        {
            Check.IsDefined(lifecycleKind, "lifecycleKind");
            Check.NotNull(serviceType, "serviceType");
            Check.NotNull(implementationType, "implementationType");

            _lifecycleKind = lifecycleKind;
            _serviceType = serviceType;
            _implementationType = implementationType;
        }

        private ServiceDescriptor(
            LifecycleKind lifecycleKind, [NotNull] Type serviceType, [NotNull] object implementationInstance)
        {
            Check.IsDefined(lifecycleKind, "lifecycleKind");
            Check.NotNull(serviceType, "serviceType");
            Check.NotNull(implementationInstance, "implementationInstance");

            _lifecycleKind = lifecycleKind;
            _serviceType = serviceType;
            _implementationInstance = implementationInstance;
        }

        public LifecycleKind Lifecycle
        {
            get { return _lifecycleKind; }
        }

        public Type ServiceType
        {
            get { return _serviceType; }
        }

        public Type ImplementationType
        {
            get { return _implementationType; }
        }

        public object ImplementationInstance
        {
            get { return _implementationInstance; }
        }
    }
}
