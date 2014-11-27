// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Tests
{
    public class SqliteEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void AddSqlite_does_not_replace_services_already_registered()
        {
            var services = new ServiceCollection()
                .AddSingleton<SqliteDataStore, FakeSqliteDataStore>();

            services.AddEntityFramework().AddSqlite();

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<FakeSqliteDataStore>(serviceProvider.GetRequiredService<SqliteDataStore>());
        }

        private class FakeSqliteDataStore : SqliteDataStore
        {
        }
    }
}
