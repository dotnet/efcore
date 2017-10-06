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
