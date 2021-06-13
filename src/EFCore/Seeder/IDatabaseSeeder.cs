// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Seeder
{
    /// <summary>
    /// Seeds database.
    /// </summary>
    public interface IDatabaseSeeder
    {
        /// <summary>
        /// Sets environment for seeder.
        /// </summary>
        /// <param name="environment">The environment that seeder is run in it</param>
        /// <returns>IDatabaseSeeder that can seed the database.</returns>
        public IDatabaseSeeder SetEnvironment(string environment);

        /// <summary>
        /// Runs seeders.
        /// NOTE: Seeders SHOULD NOT run on a dotnet-ef process.
        /// </summary>
        /// <param name="isEfProcess">Determines the process is dotnet-ef process or not.</param>
        public void EnsureSeeded(bool isEfProcess);
    }
}
