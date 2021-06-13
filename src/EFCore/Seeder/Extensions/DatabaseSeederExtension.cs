// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Seeder.Extensions
{
    /// <summary>
    /// Extension to add seeder to ConfigureServices method in Startup.
    /// </summary>
    public static class DatabaseSeederExtension
    {
        /// <summary>
        /// Determines DbContext for seeds.
        /// </summary>
        /// <param name="services">IServiceCollection which DbContext is registered in it</param>
        /// <typeparam name="T">DbContext which models are defined in it</typeparam>
        /// <returns>IDatabaseSeeder that can seed the database.</returns>
        public static IDatabaseSeeder AddSeeder<T>(this IServiceCollection services) where T : DbContext
        {
            var serviceProvider = services.BuildServiceProvider();

            var databaseSeeder = new DatabaseSeeder<T>(serviceProvider, serviceProvider.GetService<T>());

            return databaseSeeder;
        }
    }
}
