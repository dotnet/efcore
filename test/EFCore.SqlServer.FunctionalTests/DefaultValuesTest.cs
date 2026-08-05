// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class DefaultValuesTest : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddEntityFrameworkSqlServer()
        .BuildServiceProvider(validateScopes: true);

    [ConditionalFact]
    public void Can_use_SQL_Server_default_values()
    {
        using (var context = new ChipsContext(_serviceProvider, TestStore.Name))
        {
            context.Database.EnsureCreatedResiliently();

            context.Chippers.Add(
                new Chipper { Id = "Default" });

            context.SaveChanges();

            var honeyDijon = context.Add(
                new KettleChips { Name = "Honey Dijon" }).Entity;
            var buffaloBleu = context.Add(
                new KettleChips { Name = "Buffalo Bleu", BestBuyDate = new DateTime(2111, 1, 11) }).Entity;

            context.SaveChanges();

            Assert.Equal(new DateTime(2035, 9, 25), honeyDijon.BestBuyDate);
            Assert.Equal(new DateTime(2111, 1, 11), buffaloBleu.BestBuyDate);
        }

        using (var context = new ChipsContext(_serviceProvider, TestStore.Name))
        {
            Assert.Equal(new DateTime(2035, 9, 25), context.Chips.Single(c => c.Name == "Honey Dijon").BestBuyDate);
            Assert.Equal(new DateTime(2111, 1, 11), context.Chips.Single(c => c.Name == "Buffalo Bleu").BestBuyDate);
        }
    }

    private class ChipsContext(IServiceProvider serviceProvider, string databaseName) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly string _databaseName = databaseName;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<KettleChips> Chips { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Chipper> Chippers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<KettleChips>(
                b =>
                {
                    b.Property(e => e.BestBuyDate)
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new DateTime(2035, 9, 25));

                    b.Property(e => e.ChipperId)
                        .IsRequired()
                        .HasDefaultValue("Default");
                });
    }

    private class KettleChips
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime BestBuyDate { get; set; }
        public string ChipperId { get; set; }

        public Chipper Manufacturer { get; set; }
    }

    private class Chipper
    {
        public string Id { get; set; }
    }

    protected SqlServerTestStore TestStore { get; private set; }

    public async Task InitializeAsync()
        => TestStore = await SqlServerTestStore.CreateInitializedAsync("DefaultValuesTest");

    public Task DisposeAsync()
    {
        TestStore.Dispose();
        return Task.CompletedTask;
    }
}
