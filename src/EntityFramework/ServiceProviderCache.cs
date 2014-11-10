// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity
{
    internal class ServiceProviderCache
    {
        private static readonly ServiceProviderCache _instance = new ServiceProviderCache();

        private readonly ThreadSafeDictionaryCache<long, IServiceProvider> _configurations
            = new ThreadSafeDictionaryCache<long, IServiceProvider>();

        public static ServiceProviderCache Instance
        {
            get { return _instance; }
        }

        public virtual IServiceProvider GetOrAdd(IDbContextOptions options)
        {
            var services = new ServiceCollection();
            var builder = services.AddEntityFramework();

            foreach (var extension in options.Extensions)
            {
                extension.ApplyServices(builder);
            }

            // TODO: Consider more robust hashing algorithm
            // Note that no cryptographic algorithm is available on all of phone/store/k/desktop
            // Issue #762
            unchecked
            {
                var key = services.Aggregate(0, (t, d) => (t * 397) ^ CalculateHash(d).GetHashCode());

                return _configurations.GetOrAdd(key, k => services.BuildServiceProvider());
            }
        }

        private static long CalculateHash(IServiceDescriptor descriptor)
        {
            return ((((long)descriptor.Lifecycle * 397)
                     ^ descriptor.ServiceType.GetHashCode()) * 397)
                   ^ (descriptor.ImplementationInstance
                      ?? descriptor.ImplementationType
                      ?? (object)descriptor.ImplementationFactory).GetHashCode();
        }
    }
}
