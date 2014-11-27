// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class MigrationsEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void AddMigrations_does_not_replace_services_already_registered()
        {
            var services = new ServiceCollection()
                .AddSingleton<MigrationAssembly, FakeMigrationAssembly>();

            services.AddEntityFramework().AddMigrations();

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<FakeMigrationAssembly>(serviceProvider.GetRequiredService<MigrationAssembly>());
        }

        private class FakeMigrationAssembly : MigrationAssembly
        {
        }
    }
}
