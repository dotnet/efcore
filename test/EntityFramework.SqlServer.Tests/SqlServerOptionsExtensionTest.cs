// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerOptionsExtensionTest
    {
        [Fact]
        public void ApplyServices_adds_SQL_server_services()
        {
            var services = new ServiceCollection();
            var builder = new EntityFrameworkServicesBuilder(services);

            new SqlServerOptionsExtension().ApplyServices(builder);

            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalDatabase)));
        }
    }
}
