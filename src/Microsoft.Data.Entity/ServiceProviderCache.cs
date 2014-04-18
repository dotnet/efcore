// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    internal class ServiceProviderCache
    {
        private static readonly ServiceProviderCache _instance = new ServiceProviderCache();

        private readonly ThreadSafeDictionaryCache<int, IServiceProvider> _configurations
            = new ThreadSafeDictionaryCache<int, IServiceProvider>();

        public static ServiceProviderCache Instance
        {
            get { return _instance; }
        }

        public virtual IServiceProvider GetOrAdd(ServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                // TODO: Proper exception message
                throw new InvalidOperationException("Must specify services in some way");
            }

            // TODO: Consider more robust hashing algorithm
            var key = serviceCollection.Aggregate(
                0, (t, d) => (t * 397) ^ (d.ImplementationInstance ?? d.ImplementationType).GetHashCode());

            return _configurations.GetOrAdd(key, k => serviceCollection.BuildServiceProvider());
        }
    }
}
