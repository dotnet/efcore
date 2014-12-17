// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Infrastructure;
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
            Assert.Null(configuration.MaxBatchSize);

            var connection = Mock.Of<DbConnection>();
            configuration.Connection = connection;
            configuration.ConnectionString = "Fraggle=Rock";
            configuration.MaxBatchSize = 1;

            Assert.Same(connection, configuration.Connection);
            Assert.Equal("Fraggle=Rock", configuration.ConnectionString);
            Assert.Equal(1, configuration.MaxBatchSize);
        }

        [Fact]
        public void Configures_max_batch_size_specified_in_dbContext_options()
        {
            IDbContextOptions options = new DbContextOptions();
            options.RawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "SqlServer:MaxBatchSize", "1" } };
            options.AddExtension(new SqlServerOptionsExtension());

            var optionsExtension = options.Extensions.OfType<SqlServerOptionsExtension>().First();

            Assert.Equal(1, optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void MaxBatchSize_in_sqlServerOptionsExtension_is_optional()
        {
            IDbContextOptions options = new DbContextOptions();
            options.RawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            options.AddExtension(new SqlServerOptionsExtension());

            var optionsExtension = options.Extensions.OfType<SqlServerOptionsExtension>().First();

            Assert.Null(optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void Throws_on_invalid_MaxBatchSize_specified_in_dbContextOptions()
        {
            IDbContextOptions options = new DbContextOptions();
            options.RawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "SqlServer:MaxBatchSize", "one" } };

            Assert.Equal(Strings.IntegerConfigurationValueFormatError("SqlServer:MaxBatchSize", "one"),
                Assert.Throws<InvalidOperationException>(() => options.AddExtension(new SqlServerOptionsExtension())).Message);
        }
    }
}
