// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Internal
{
    public class ServiceProviderCache
    {
        private readonly ConcurrentDictionary<long, IServiceProvider> _configurations
            = new ConcurrentDictionary<long, IServiceProvider>();

        public static ServiceProviderCache Instance { get; } = new ServiceProviderCache();

        public virtual IServiceProvider GetOrAdd([NotNull] IDbContextOptions options)
        {
            // Decided that this hashing algorithm is robust enough. See issue #762.
            unchecked
            {
                var key = options.Extensions.Aggregate(0, (t, e) => (t * 397) ^ e.GetType().GetHashCode());

                return _configurations.GetOrAdd(
                    key,
                    k =>
                        {
                            var services = new ServiceCollection();
                            var builder = services.AddEntityFramework();

                            foreach (var extension in options.Extensions)
                            {
                                extension.ApplyServices(builder);
                            }

                            return services.BuildServiceProvider();
                        });
            }
        }
    }
}
