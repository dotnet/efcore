// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;

#nullable disable

public class FunkyDataContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<FunkyCustomer> FunkyCustomers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<FunkyCustomer>().Property(e => e.Id).ValueGeneratedNever();

    public static Task SeedAsync(FunkyDataContext context)
    {
        context.FunkyCustomers.AddRange(FunkyDataData.CreateFunkyCustomers());
        return context.SaveChangesAsync();
    }
}
