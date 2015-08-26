// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection;
using Microsoft.Data.Entity.Infrastructure;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers;

namespace EntityFramework.Microbenchmarks.Models.AdventureWorks
{
    public class AdventureWorksFixture : AdventureWorksFixtureBase
    {
        // This method is called from timed code, be careful when changing it
        public AdventureWorksContext CreateContext(bool cold)
        {
            if (cold)
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .GetService()
                    .BuildServiceProvider();

                return new AdventureWorksContext(ConnectionString, serviceProvider);
            }
            else
            {
                return new AdventureWorksContext(ConnectionString);
            }
        }
    }
}
