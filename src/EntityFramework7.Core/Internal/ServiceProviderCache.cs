// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Internal
{
    public class ServiceProviderCache
    {
        private readonly ThreadSafeDictionaryCache<long, IServiceProvider> _configurations
            = new ThreadSafeDictionaryCache<long, IServiceProvider>();

        public static ServiceProviderCache Instance { get; } = new ServiceProviderCache();

        public virtual IServiceProvider GetOrAdd([NotNull] IDbContextOptions options)
        {
            var services = new ServiceCollection();
            var builder = services.AddEntityFramework();

            foreach (var extension in options.Extensions)
            {
                extension.ApplyServices(builder);
            }

            // Decided that this hashing algorithm is robust enough. See issue #762.
            unchecked
            {
                var key = services.Aggregate(0, (t, d) => (t * 397) ^ CalculateHash(d).GetHashCode());

                return _configurations.GetOrAdd(key, k => services.BuildServiceProvider());
            }
        }

        private static long CalculateHash(ServiceDescriptor descriptor)
            => ((((long)descriptor.Lifetime * 397)
                 ^ descriptor.ServiceType.GetHashCode()) * 397)
               ^ (descriptor.ImplementationInstance
                  ?? descriptor.ImplementationType
                  ?? (object)descriptor.ImplementationFactory).GetHashCode();
    }
}
