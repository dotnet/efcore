// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization;

public class InitializationSqlServerTests : InitializationTests
{
    protected override AdventureWorksContextBase CreateContext()
        => AdventureWorksSqlServerFixture.CreateContext();

    protected override ConventionSet CreateConventionSet()
        => SqlServerConventionSetBuilder.Build();

    protected override IServiceCollection AddContext(IServiceCollection services)
    {
        services.AddDbContext<AdventureWorksContextBase, AdventureWorksSqlServerContext>()
                .AddDbContextFactory<AdventureWorksSqlServerContext>()
                .TryAddSingleton<IDbContextFactory<AdventureWorksContextBase>,
                    AdventureWorksSqlServerContextFactory<AdventureWorksSqlServerContext>>();

        return services;
    }

    protected override IServiceCollection AddContextPool(IServiceCollection services)
    {
        services.AddDbContextPool<AdventureWorksContextBase, AdventureWorksPoolableSqlServerContext>(
            AdventureWorksSqlServerFixture.ConfigureOptions)
                .AddPooledDbContextFactory<AdventureWorksPoolableSqlServerContext>(AdventureWorksSqlServerFixture.ConfigureOptions)
                .TryAddSingleton<IDbContextFactory<AdventureWorksContextBase>,
                    AdventureWorksSqlServerContextFactory<AdventureWorksPoolableSqlServerContext>>();

        return services;
    }

    private class AdventureWorksSqlServerContextFactory<T>(IDbContextFactory<T> factory) : IDbContextFactory<AdventureWorksContextBase>
        where T : AdventureWorksContextBase
    {
        private readonly IDbContextFactory<T> _factory = factory;

        public AdventureWorksContextBase CreateDbContext()
            => _factory.CreateDbContext();
    }
}
