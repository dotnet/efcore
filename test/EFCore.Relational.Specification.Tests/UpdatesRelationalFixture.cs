// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public abstract class UpdatesRelationalFixture : UpdatesFixtureBase
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<ProductViewTable>().HasBaseType((string)null).ToTable("ProductView");
        modelBuilder.Entity<ProductTableWithView>().HasBaseType((string)null).ToView("ProductView").ToTable("ProductTable");
        modelBuilder.Entity<ProductTableView>().HasBaseType((string)null).ToView("ProductTable");

        modelBuilder
            .Entity<
                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
            >(
                eb =>
                {
                    eb.HasKey(
                            l => new { l.ProfileId })
                        .HasName("PK_LoginDetails");

                    eb.HasOne(d => d.Login).WithOne()
                        .HasConstraintName("FK_LoginDetails_Login");
                });
    }
}
