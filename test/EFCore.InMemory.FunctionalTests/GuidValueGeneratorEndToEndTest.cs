// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class GuidValueGeneratorEndToEndTest
{
    [ConditionalFact]
    public async Task Can_use_GUIDs_end_to_end_async()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        var guids = new List<Guid>();
        var guidsHash = new HashSet<Guid>();
        using (var context = new BronieContext(serviceProvider))
        {
            for (var i = 0; i < 10; i++)
            {
                guids.Add(
                    (await context.AddAsync(
                        new Pegasus { Name = "Rainbow Dash " + i })).Entity.Id);
                guidsHash.Add(guids.Last());
            }

            await context.SaveChangesAsync();
        }

        Assert.Equal(10, guidsHash.Count);

        using (var context = new BronieContext(serviceProvider))
        {
            var pegasuses = await context.Pegasuses.OrderBy(e => e.Name).ToListAsync();

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(guids[i], pegasuses[i].Id);
            }
        }
    }

    private class BronieContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(BronieContext))
                .UseInternalServiceProvider(_serviceProvider);

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Pegasus> Pegasuses { get; set; }
    }

    private class Pegasus
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
