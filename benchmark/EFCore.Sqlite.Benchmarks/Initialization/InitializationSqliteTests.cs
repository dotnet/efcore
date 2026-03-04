// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization;

public class InitializationSqliteTests : InitializationTests
{
    protected override AdventureWorksContextBase CreateContext()
        => AdventureWorksSqliteFixture.CreateContext();

    protected override ConventionSet CreateConventionSet()
        => SqliteConventionSetBuilder.Build();

    protected override IServiceCollection AddContext(IServiceCollection services)
    {
        services.AddDbContext<AdventureWorksContextBase, AdventureWorksSqliteContext>()
                .AddDbContextFactory<AdventureWorksSqliteContext>()
                .TryAddSingleton<IDbContextFactory<AdventureWorksContextBase>,
                    AdventureWorksSqliteContextFactory<AdventureWorksSqliteContext>>();

        return services;
    }

    protected override IServiceCollection AddContextPool(IServiceCollection services)
    {
        services.AddDbContextPool<AdventureWorksContextBase, AdventureWorksPoolableSqliteContext>(
            AdventureWorksSqliteFixture.ConfigureOptions)
                .AddPooledDbContextFactory<AdventureWorksPoolableSqliteContext>(AdventureWorksSqliteFixture.ConfigureOptions)
                .TryAddSingleton<IDbContextFactory<AdventureWorksContextBase>,
                    AdventureWorksSqliteContextFactory<AdventureWorksPoolableSqliteContext>>();

        return services;
    }

    private class AdventureWorksSqliteContextFactory<T>(IDbContextFactory<T> factory) : IDbContextFactory<AdventureWorksContextBase>
        where T : AdventureWorksContextBase
    {
        private readonly IDbContextFactory<T> _factory = factory;

        public AdventureWorksContextBase CreateDbContext()
            => _factory.CreateDbContext();
    }
}
