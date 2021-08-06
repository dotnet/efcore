// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerOptionsExtensionTest
    {
        [ConditionalFact]
        public void ApplyServices_adds_SQL_server_services()
        {
            var services = new ServiceCollection();

            new SqlServerOptionsExtension().ApplyServices(services);

            Assert.Contains(services, sd => sd.ServiceType == typeof(ISqlServerConnection));
        }

        private class ChangedRowNumberContext : DbContext
        {
            private static readonly IServiceProvider _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .BuildServiceProvider(validateScopes: true);

            private readonly bool _setInternalServiceProvider;

            public ChangedRowNumberContext(bool setInternalServiceProvider)
            {
                _setInternalServiceProvider = setInternalServiceProvider;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (_setInternalServiceProvider)
                {
                    optionsBuilder.UseInternalServiceProvider(_serviceProvider);
                }

                optionsBuilder.UseSqlServer("Database=Maltesers");
            }
        }
    }
}
