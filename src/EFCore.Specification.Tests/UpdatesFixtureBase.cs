// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class UpdatesFixtureBase : SharedStoreFixtureBase<UpdatesContext>
    {
        protected override string StoreName { get; } = "UpdateTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Product>().HasOne<Category>().WithMany()
                .HasForeignKey(e => e.DependentId)
                .HasPrincipalKey(e => e.PrincipalId);

            modelBuilder.Entity<Product>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Category>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly>().HasKey(l => new
            {
                l.LoginId,
                l.LoginId1,
                l.LoginId3,
                l.LoginId4,
                l.LoginId5,
                l.LoginId6,
                l.LoginId7,
                l.LoginId8,
                l.LoginId9,
                l.LoginId10,
                l.LoginId11,
                l.LoginId12,
                l.LoginId13,
                l.LoginId14
            });

            modelBuilder.Entity<Profile>().HasKey(l => new
            {
                l.ProfileId,
                l.ProfileId1,
                l.ProfileId3,
                l.ProfileId4,
                l.ProfileId5,
                l.ProfileId6,
                l.ProfileId7,
                l.ProfileId8,
                l.ProfileId9,
                l.ProfileId10,
                l.ProfileId11,
                l.ProfileId12,
                l.ProfileId13,
                l.ProfileId14
            });
        }

        protected override void Seed(UpdatesContext context)
            => UpdatesContext.Seed(context);

        public override UpdatesContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }
    }
}
