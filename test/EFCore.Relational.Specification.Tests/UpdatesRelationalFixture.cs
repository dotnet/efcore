// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class UpdatesRelationalFixture : UpdatesFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

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
}
