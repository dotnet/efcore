// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Migrations;
using Microsoft.Data.Relational;
using Xunit;

namespace Microsoft.Data.SqlServer.Tests
{
    public class SqlServerEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = new ServiceCollection().AddEntityFramework(s => s.AddSqlServer());

            Assert.True(services.Any(sd => sd.ServiceType == typeof(IdentityGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerSqlGenerator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStoreCreator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ModelDiffer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(MigrationOperationSqlGenerator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlStatementExecutor)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var serviceProvider = new ServiceCollection().AddEntityFramework(s => s.AddSqlServer()).BuildServiceProvider();

            using (var context = new EntityContext(
                new EntityConfigurationBuilder(serviceProvider)
                    .SqlServerConnectionString("goo").BuildConfiguration()))
            {
                var scopedProvider = context.Configuration.Services.ServiceProvider;

                Assert.NotNull(scopedProvider.GetService<IdentityGeneratorFactory>());
                Assert.NotNull(scopedProvider.GetService<SqlServerDataStore>());
                Assert.NotNull(scopedProvider.GetService<DataStoreSource>());
                Assert.NotNull(scopedProvider.GetService<SqlServerDataStoreCreator>());
                Assert.NotNull(scopedProvider.GetService<SqlServerSqlGenerator>());
                Assert.NotNull(scopedProvider.GetService<ModelDiffer>());
                Assert.NotNull(scopedProvider.GetService<MigrationOperationSqlGenerator>());
                Assert.NotNull(scopedProvider.GetService<SqlStatementExecutor>());
            }
        }
    }
}
