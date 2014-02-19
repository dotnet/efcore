// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Services;

namespace Microsoft.Data.Entity
{
    public static class EntityServices
    {
        public static IEnumerable<IServiceDescriptor> GetDefaultServices()
        {
            yield return ServiceDescriptor.Singleton<ILoggerFactory, ConsoleLoggerFactory>();
            yield return ServiceDescriptor.Singleton<IIdentityGenerator<Guid>, GuidIdentityGenerator>();
        }
    }
}
