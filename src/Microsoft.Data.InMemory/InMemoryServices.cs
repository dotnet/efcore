// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Services;

namespace Microsoft.Data.InMemory
{
    public static class InMemoryServices
    {
        public static IEnumerable<IServiceDescriptor> GetDefaultServices()
        {
            yield return ServiceDescriptor.Singleton<IIdentityGenerator<long>, InMemoryIdentityGenerator>();
        }
    }
}
