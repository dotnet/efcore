// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerOptionsExtensionTest
    {
        private static readonly MethodInfo _applyServices
            = typeof(SqlServerOptionsExtension).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "ApplyServices");

        [Fact]
        public void Adds_SQL_server_services()
        {
            var services = new ServiceCollection();
            var builder = new EntityServicesBuilder(services);

            _applyServices.Invoke(new SqlServerOptionsExtension(), new object[] { builder });

            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStore)));
        }

        [Fact]
        public void Can_access_properties()
        {
            var configuration = new SqlServerOptionsExtension();

            Assert.Null(configuration.Connection);
            Assert.Null(configuration.ConnectionString);

            var connection = Mock.Of<DbConnection>();
            configuration.Connection = connection;
            configuration.ConnectionString = "Fraggle=Rock";

            Assert.Same(connection, configuration.Connection);
            Assert.Equal("Fraggle=Rock", configuration.ConnectionString);
        }
    }
}
