// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.Operators;

#nullable disable

public class OperatorsContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OperatorEntityString>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<OperatorEntityInt>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<OperatorEntityNullableInt>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<OperatorEntityLong>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<OperatorEntityBool>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<OperatorEntityNullableBool>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<OperatorEntityDateTimeOffset>().Property(x => x.Id).ValueGeneratedNever();
    }
}
