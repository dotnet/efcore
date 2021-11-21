// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class UpdatesSqlServerFixture : UpdatesRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w =>
            {
                w.Log(SqlServerEventId.DecimalTypeKeyWarning);
            });

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<ProductBase>()
            .Property(p => p.Id).HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<ProductTableWithView>()
            .Property(p => p.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ProductViewTable>()
            .Property(p => p.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ProductTableView>()
            .Property(p => p.Price).HasColumnType("decimal(18,2)");
        modelBuilder
            .Entity<
                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
            >()
            .Property(l => l.ProfileId3).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Profile>()
            .Property(l => l.Id3).HasColumnType("decimal(18,2)");
    }
}
