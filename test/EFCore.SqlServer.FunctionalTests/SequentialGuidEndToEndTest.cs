// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SequentialGuidEndToEndTest : IAsyncLifetime
{
    [ConditionalFact]
    public async Task Can_use_sequential_GUID_end_to_end_async()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            context.Database.EnsureCreatedResiliently();

            for (var i = 0; i < 50; i++)
            {
                await context.AddAsync(
                    new Pegasus { Name = "Rainbow Dash " + i });
            }

            await context.SaveChangesAsync();
        }

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            var pegasuses = await context.Pegasuses.OrderBy(e => e.Id).ToListAsync();

            for (var i = 0; i < 50; i++)
            {
                Assert.Equal("Rainbow Dash " + i, pegasuses[i].Name);
            }
        }
    }

    [ConditionalFact]
    public async Task Can_use_explicit_values()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        var guids = new List<Guid>();

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            context.Database.EnsureCreatedResiliently();

            for (var i = 0; i < 50; i++)
            {
                guids.Add(
                    (await context.AddAsync(
                        new Pegasus
                        {
                            Name = "Rainbow Dash " + i,
                            Index = i,
                            Id = Guid.NewGuid()
                        })).Entity.Id);
            }

            await context.SaveChangesAsync();
        }

        using (var context = new BronieContext(serviceProvider, TestStore.Name))
        {
            var pegasuses = await context.Pegasuses.OrderBy(e => e.Index).ToListAsync();

            for (var i = 0; i < 50; i++)
            {
                Assert.Equal("Rainbow Dash " + i, pegasuses[i].Name);
                Assert.Equal(guids[i], pegasuses[i].Id);
            }
        }
    }

    private class BronieContext(IServiceProvider serviceProvider, string databaseName) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly string _databaseName = databaseName;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Pegasus> Pegasuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider);
    }

    private class Pegasus
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
    }

    protected SqlServerTestStore TestStore { get; private set; }

    public async Task InitializeAsync()
        => TestStore = await SqlServerTestStore.CreateInitializedAsync("SequentialGuidEndToEndTest");

    public Task DisposeAsync()
    {
        TestStore.Dispose();
        return Task.CompletedTask;
    }
}
