// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization;

[DisplayName("InitializationTests")]
public abstract class InitializationTests
{
    private ServiceProvider _serviceProvider;
    private ServiceProvider _pooledServiceProvider;

    protected abstract AdventureWorksContextBase CreateContext();
    protected abstract ConventionSet CreateConventionSet();
    protected abstract IServiceCollection AddContext(IServiceCollection services);
    protected abstract IServiceCollection AddContextPool(IServiceCollection services);

    [GlobalSetup]
    public virtual void Initialize()
    {
        _serviceProvider = AddContext(new ServiceCollection()).BuildServiceProvider();
        _pooledServiceProvider = AddContextPool(new ServiceCollection()).BuildServiceProvider();
    }

    [Benchmark]
    public virtual void CreateAndDisposeUnusedContext()
    {
        for (var i = 0; i < 10000; i++)
        {
            // ReSharper disable once UnusedVariable
            using var context = CreateContext();
        }
    }

    [Benchmark]
    public virtual void CreateAndDisposeUnusedContextFromDi()
    {
        for (var i = 0; i < 10000; i++)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<AdventureWorksContextBase>();
        }
    }

    [Benchmark]
    public virtual void CreateAndDisposeUnusedContextFromDiFactory()
    {
        var factory = _serviceProvider.GetService<IDbContextFactory<AdventureWorksContextBase>>();
        for (var i = 0; i < 10000; i++)
        {
            using var _ = factory.CreateDbContext();
        }
    }

    [Benchmark]
    public virtual void CreateAndDisposePooledContextFromDi()
    {
        for (var i = 0; i < 10000; i++)
        {
            using var scope = _pooledServiceProvider.CreateScope();
            var context = (AdventureWorksContextBase)scope.ServiceProvider.GetService(typeof(AdventureWorksContextBase));
            var _ = context.Model;
        }
    }

    [Benchmark]
    public virtual void CreateAndDisposePooledContextFromDiFactory()
    {
        var factory = _pooledServiceProvider.GetService<IDbContextFactory<AdventureWorksContextBase>>();
        for (var i = 0; i < 10000; i++)
        {
            using var context = factory.CreateDbContext();
            var _ = context.Model;
        }
    }

    [Benchmark]
    public virtual void InitializeAndQuery_AdventureWorks()
    {
        for (var i = 0; i < 1000; i++)
        {
            using (var context = CreateContext())
            {
                _ = context.Department.First();
            }
        }
    }

    [Benchmark]
    public virtual void InitializeAndSaveChanges_AdventureWorks()
    {
        for (var i = 0; i < 100; i++)
        {
            using (var context = CreateContext())
            {
                context.Currency.Add(
                    new Currency { CurrencyCode = "TMP", Name = "Temporary" });

                using (context.Database.BeginTransaction())
                {
                    context.SaveChanges();

                    // TODO: Don't measure transaction rollback
                }
            }
        }
    }

    [Benchmark]
    public virtual void BuildModel_AdventureWorks()
    {
        var builder = new ModelBuilder(CreateConventionSet());
        AdventureWorksContextBase.ConfigureModel(builder);

        // ReSharper disable once UnusedVariable
        var model = builder.FinalizeModel();
    }
}
