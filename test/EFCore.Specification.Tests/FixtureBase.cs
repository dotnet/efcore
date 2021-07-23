// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class FixtureBase
    {
        protected virtual IServiceCollection AddServices(IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton(TestModelSource.GetFactory(OnModelCreating));

        public virtual DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(
                    b => b.Default(WarningBehavior.Throw)
                        .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));

        protected virtual void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
        }
    }
}
