// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Identity;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerServicesTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = SqlServerServices.GetDefaultServices().ToList();

            Assert.True(services.Any(sd => sd.ServiceType == typeof(IdentityGeneratorFactory)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var serviceProvider = new ServiceProvider().Add(SqlServerServices.GetDefaultServices());

            Assert.NotNull(serviceProvider.GetService<IdentityGeneratorFactory>());
        }
    }
}
