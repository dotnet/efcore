// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class AddPooledDbContextFactoryParameterlessTest
{
    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    [Fact]
    public async Task Parameterless_factory_should_use_ConfigureDbContext_options()
    {
        var services = new ServiceCollection();

        services.ConfigureDbContext<TestDbContext>((sp, opts) =>
            opts.UseInMemoryDatabase("test_db"));

        services.AddPooledDbContextFactory<TestDbContext>();

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<TestDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", db.Database.ProviderName);
    }

    [Fact]
    public async Task Parameterless_factory_with_custom_pool_size_should_still_resolve()
    {
        var services = new ServiceCollection();

        services.ConfigureDbContext<TestDbContext>((sp, opts) =>
            opts.UseInMemoryDatabase("test_db_custom_pool"));

        services.AddPooledDbContextFactory<TestDbContext>(poolSize: 256);

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<TestDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", db.Database.ProviderName);
    }

    [Fact]
    public void Scoped_resolution_of_TContext_uses_pooled_factory()
    {
        var services = new ServiceCollection();

        services.ConfigureDbContext<TestDbContext>((sp, opts) =>
            opts.UseInMemoryDatabase("scoped_db"));

        services.AddPooledDbContextFactory<TestDbContext>();

        using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", ctx.Database.ProviderName);
    }

    [Fact]
    public void Pooled_services_are_registered_and_singleton()
    {
        var services = new ServiceCollection();

        services.ConfigureDbContext<TestDbContext>((sp, opts) =>
            opts.UseInMemoryDatabase("pool_reg_db"));

        services.AddPooledDbContextFactory<TestDbContext>();

        using var provider = services.BuildServiceProvider();

        var pool1 = provider.GetRequiredService<IDbContextPool<TestDbContext>>();
        var pool2 = provider.GetRequiredService<IDbContextPool<TestDbContext>>();

        // Should be the same singleton instance
        Assert.Same(pool1, pool2);

        // And the factory should resolve
        var factory = provider.GetRequiredService<IDbContextFactory<TestDbContext>>();
        Assert.NotNull(factory);
    }

    [Fact]
    public async Task Parameterless_factory_without_configuration_throws_meaningful_error()
    {
        var services = new ServiceCollection();

        // No ConfigureDbContext here on purpose.
        services.AddPooledDbContextFactory<TestDbContext>();

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<TestDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        // Trigger provider requirement (any DB operation works; EnsureCreated is simple & provider-agnostic)
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await db.Database.EnsureCreatedAsync();
        });
    }
}
