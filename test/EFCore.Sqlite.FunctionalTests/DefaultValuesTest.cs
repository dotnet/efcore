// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class DefaultValuesTest : SharedStoreFixtureBase<DbContext>
{
    [ConditionalFact]
    public void Can_use_SQLite_default_values()
    {
        using (var context = CreateChipsContext())
        {
            var honeyDijon = context.Add(
                new KettleChips { Name = "Honey Dijon" }).Entity;
            var buffaloBleu = context.Add(
                new KettleChips { Name = "Buffalo Bleu", BestBuyDate = new DateTime(2111, 1, 11) }).Entity;

            context.SaveChanges();

            Assert.Equal(new DateTime(2035, 9, 25), honeyDijon.BestBuyDate);
            Assert.Equal(new DateTime(2111, 1, 11), buffaloBleu.BestBuyDate);
        }

        using (var context = CreateChipsContext())
        {
            Assert.Equal(new DateTime(2035, 9, 25), context.Chips.Single(c => c.Name == "Honey Dijon").BestBuyDate);
            Assert.Equal(new DateTime(2111, 1, 11), context.Chips.Single(c => c.Name == "Buffalo Bleu").BestBuyDate);
        }
    }

    protected override string StoreName
        => "DefaultKettleChips";

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override Type ContextType { get; } = typeof(ChipsContext);

    private ChipsContext CreateChipsContext()
        => (ChipsContext)CreateContext();

    private class ChipsContext(DbContextOptions options) : PoolableDbContext(options)
    {

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<KettleChips> Chips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<KettleChips>()
                .Property(e => e.BestBuyDate)
                .HasDefaultValue(new DateTime(2035, 9, 25));
    }

    private class KettleChips
    {
        // ReSharper disable once UnusedMember.Local
        public int Id { get; set; }

        public string Name { get; set; }
        public DateTime BestBuyDate { get; set; }
    }
}
