// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore;

public class SqliteAutoincrementIntegrationTest : IDisposable
{
    private readonly DbContext _context;

    public SqliteAutoincrementIntegrationTest()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .BuildServiceProvider();

        var optionsBuilder = new DbContextOptionsBuilder<TestContext>()
            .UseSqlite("Data Source=:memory:")
            .UseInternalServiceProvider(serviceProvider);

        _context = new TestContext(optionsBuilder.Options);
        _context.Database.EnsureCreated();
    }

    [ConditionalFact]
    public void UseAutoincrement_configures_column_correctly()
    {
        // Verify that the UseAutoincrement method produces the correct SQL
        var sql = _context.Database.GenerateCreateScript();
        
        // Should contain AUTOINCREMENT for the Id column
        Assert.Contains("AUTOINCREMENT", sql);
        Assert.Contains("\"Id\" INTEGER NOT NULL", sql);
    }

    [ConditionalFact]
    public void Autoincrement_works_for_inserts()
    {
        // Insert a record without specifying the ID
        var customer = new Customer { Name = "Test Customer" };
        ((TestContext)_context).Customers.Add(customer);
        _context.SaveChanges();

        // The ID should be automatically generated
        Assert.True(customer.Id > 0);
    }

    [ConditionalFact]
    public void Value_generation_strategy_is_preserved_in_model()
    {
        var entityType = _context.Model.FindEntityType(typeof(Customer))!;
        var idProperty = entityType.FindProperty(nameof(Customer.Id))!;

        Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, idProperty.GetValueGenerationStrategy());
        Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    private class TestContext : DbContext
    {
        public TestContext(DbContextOptions<TestContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(b =>
            {
                b.Property(e => e.Id).UseAutoincrement();
            });
        }
    }

    private class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}