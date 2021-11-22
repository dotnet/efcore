// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;

public class FunkyDataContext : PoolableDbContext
{
    public FunkyDataContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<FunkyCustomer> FunkyCustomers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<FunkyCustomer>().Property(e => e.Id).ValueGeneratedNever();

    public static void Seed(FunkyDataContext context)
    {
        context.FunkyCustomers.AddRange(FunkyDataData.CreateFunkyCustomers());
        context.SaveChanges();
    }
}
