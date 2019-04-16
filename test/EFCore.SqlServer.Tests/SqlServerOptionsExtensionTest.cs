// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerOptionsExtensionTest
    {
        [Fact]
        public void ApplyServices_adds_SQL_server_services()
        {
            var services = new ServiceCollection();

            new SqlServerOptionsExtension().ApplyServices(services);

            Assert.True(services.Any(sd => sd.ServiceType == typeof(ISqlServerConnection)));
        }

        [Fact]
        public void Changing_RowNumberPagingEnabled_causes_new_service_provider_to_be_built()
        {
            ISqlServerOptions singletonOptions;

            using (var context = new ChangedRowNumberContext(rowNumberPagingEnabled: false, setInternalServiceProvider: false))
            {
                _ = context.Model;
                singletonOptions = context.GetService<ISqlServerOptions>();
                Assert.False(singletonOptions.RowNumberPagingEnabled);
            }

            using (var context = new ChangedRowNumberContext(rowNumberPagingEnabled: true, setInternalServiceProvider: false))
            {
                _ = context.Model;
                var newOptions = context.GetService<ISqlServerOptions>();
                Assert.True(newOptions.RowNumberPagingEnabled);
                Assert.NotSame(newOptions, singletonOptions);
            }
        }

        [Fact]
        public void Changing_RowNumberPagingEnabled_when_UseInternalServiceProvider_throws()
        {
            using (var context = new ChangedRowNumberContext(rowNumberPagingEnabled: false, setInternalServiceProvider: true))
            {
                _ = context.Model;
            }

            using (var context = new ChangedRowNumberContext(rowNumberPagingEnabled: true, setInternalServiceProvider: true))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(SqlServerDbContextOptionsBuilder.UseRowNumberForPaging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class ChangedRowNumberContext : DbContext
        {
            private static readonly IServiceProvider _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .BuildServiceProvider();

            private readonly bool _rowNumberPagingEnabled;
            private readonly bool _setInternalServiceProvider;

            public ChangedRowNumberContext(bool rowNumberPagingEnabled, bool setInternalServiceProvider)
            {
                _rowNumberPagingEnabled = rowNumberPagingEnabled;
                _setInternalServiceProvider = setInternalServiceProvider;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (_setInternalServiceProvider)
                {
                    optionsBuilder.UseInternalServiceProvider(_serviceProvider);
                }

                optionsBuilder
                    .UseSqlServer(
                        "Database=Maltesers",
                        b =>
                        {
                            if (_rowNumberPagingEnabled)
                            {
                                b.UseRowNumberForPaging();
                            }
                        });
            }
        }
    }
}
