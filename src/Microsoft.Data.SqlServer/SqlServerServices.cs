// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Identity;

namespace Microsoft.Data.SqlServer
{
    public static class SqlServerServices
    {
        public static ServiceCollection GetDefaultServices()
        {
            return new ServiceCollection()
                .AddSingleton<IdentityGeneratorFactory, SqlServerIdentityGeneratorFactory>();
        }
    }
}
