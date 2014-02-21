// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Services;

namespace Microsoft.Data.SqlServer
{
    public static class SqlServerServices
    {
        public static IEnumerable<IServiceDescriptor> GetDefaultServices()
        {
            yield return ServiceDescriptor.Singleton<IdentityGeneratorFactory, SqlServerIdentityGeneratorFactory>();
        }
    }
}
