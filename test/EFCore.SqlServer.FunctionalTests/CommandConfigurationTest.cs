// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class CommandConfigurationTest : IClassFixture<CommandConfigurationTest.CommandConfigurationFixture>
{
    public CommandConfigurationTest(CommandConfigurationFixture fixture)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
    }

    protected CommandConfigurationFixture Fixture { get; set; }

    [ConditionalFact]
    public void Constructed_select_query_CommandBuilder_throws_when_negative_CommandTimeout_is_used()
    {
        using var context = CreateContext();
        Assert.Throws<ArgumentException>(() => context.Database.SetCommandTimeout(-5));
    }

    [ConditionalTheory]
    [InlineData(59, 6)]
    [InlineData(50, 5)]
    [InlineData(20, 2)]
    [InlineData(2, 1)]
    public async Task Keys_generated_in_batches(int count, int expected)
    {
        await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext, UseTransaction,
            context =>
            {
                for (var i = 0; i < count; i++)
                {
                    context.Set<KettleChips>().Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos " + i });
                }

                return context.SaveChangesAsync();
            });

        Assert.Equal(expected, CountSqlLinesContaining("SELECT NEXT VALUE FOR", Fixture.TestSqlLoggerFactory.Sql));
    }

    private ChipsContext CreateContext()
        => (ChipsContext)Fixture.CreateContext();

    protected void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public int CountSqlLinesContaining(string searchTerm, string sql)
        => CountLinesContaining(sql, searchTerm);

    public int CountLinesContaining(string source, string searchTerm)
    {
        var text = source.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

        var matchQuery = from word in text
                         where word.Contains(searchTerm)
                         select word;

        return matchQuery.Count();
    }

    private class ChipsContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<KettleChips> Chips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.UseHiLo();
    }

    private class KettleChips
    {
        // ReSharper disable once UnusedMember.Local
        public int Id { get; set; }

        public string Name { get; set; }
        public DateTime BestBuyDate { get; set; }
    }

    public class CommandConfigurationFixture : SharedStoreFixtureBase<DbContext>
    {
        protected override string StoreName
            => "CommandConfiguration";

        protected override Type ContextType { get; } = typeof(ChipsContext);

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
