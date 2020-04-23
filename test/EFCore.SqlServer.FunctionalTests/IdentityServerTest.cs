// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class IdentityServerTest
    {
        [ConditionalFact]
        public void Can_initialize_PersistedGrantDbContext()
        {
            using var testDatabase = SqlServerTestStore.CreateInitialized("IdentityServerPersistedGrantDbContext");
            var options = CreateOptions(testDatabase);
            using (var context = new PersistedGrantDbContext(options, new OperationalStoreOptions()))
            {
                context.Database.EnsureCreatedResiliently();
            }
        }

        private DbContextOptions<PersistedGrantDbContext> CreateOptions(TestStore testStore)
            => (DbContextOptions<PersistedGrantDbContext>)testStore
                .AddProviderOptions(new DbContextOptionsBuilder(new DbContextOptions<PersistedGrantDbContext>()))
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(
                    b => b.Default(WarningBehavior.Throw)
                        .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning))
                .Options;
    }
}
