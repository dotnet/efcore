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
            modelBuilder.Entity<Product>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.ProductId);
            modelBuilder.Entity<ProductWithBytes>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.ProductId);

            modelBuilder.Entity<ProductCategory>()
                .HasKey(p => new { p.CategoryId, p.ProductId });

            modelBuilder.Entity<Product>().HasOne<Category>().WithMany()
                .HasForeignKey(e => e.DependentId)
                .HasPrincipalKey(e => e.PrincipalId);

            modelBuilder.Entity<Category>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Category>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<AFewBytes>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder
                .Entity<
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
                >(
                    eb =>
                    {
                        eb.HasKey(
                            l => new
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
                        eb.HasIndex(
                            l => new
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
                                l.ProfileId14,
                                l.ExtraProperty
                            });
                    });

            modelBuilder
                .Entity<
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
                >(
                    eb =>
                    {
                        eb.HasKey(
                            l => new { l.ProfileId });
                        eb.HasOne(d => d.Login).WithOne()
                            .HasForeignKey<
                                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
                            >(
                                l => new
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
                    });

            modelBuilder.Entity<Profile>(
                pb =>
                {
                    pb.HasKey(
                        l => new
                        {
                            l.Id,
                            l.Id1,
                            l.Id3,
                            l.Id4,
                            l.Id5,
                            l.Id6,
                            l.Id7,
                            l.Id8,
                            l.Id9,
                            l.Id10,
                            l.Id11,
                            l.Id12,
                            l.Id13,
                            l.Id14
                        });
                    pb.HasOne(p => p.User)
                        .WithOne(l => l.Profile)
                        .IsRequired();
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
