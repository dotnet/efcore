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

using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerConfigurationExtensionTest
    {
        private static readonly MethodInfo _applyServices
            = typeof(SqlServerConfigurationExtension).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "ApplyServices");

        [Fact]
        public void Adds_in_memory_services()
        {
            var services = new ServiceCollection();
            var builder = new EntityServicesBuilder(services);

            _applyServices.Invoke(new SqlServerConfigurationExtension(), new object[] { builder });

            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStore)));
        }

        [Fact]
        public void Can_access_properties()
        {
            var configuration = new SqlServerConfigurationExtension();

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
