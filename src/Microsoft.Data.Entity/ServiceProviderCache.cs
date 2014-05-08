// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
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

        public virtual IServiceProvider GetOrAdd(ImmutableDbContextOptions contextOptions)
        {
            var services = new ServiceCollection();
            var builder = services.AddEntityFramework();
            foreach (var extension in contextOptions.Extensions)
            {
                extension.ApplyServices(builder);
            }

            // TODO: Consider more robust hashing algorithm
            // Note that no cryptographic algorithm is available on all of phone/store/k/desktop
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
                   ^ (descriptor.ImplementationInstance ?? descriptor.ImplementationType).GetHashCode();
        }
    }
}
