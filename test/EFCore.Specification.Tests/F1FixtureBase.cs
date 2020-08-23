// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class F1FixtureBase : SharedStoreFixtureBase<F1Context>
    {
        protected override string StoreName { get; } = "F1Test";

        protected override bool UsePooling
            => true;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .UseModel(CreateModelExternal())
                .ConfigureWarnings(
                    w => w.Ignore(CoreEventId.SaveChangesStarting, CoreEventId.SaveChangesCompleted));

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Update.Name;

        private IModel CreateModelExternal()
        {
            // Doing this differently here from other tests to have regression coverage for
            // building models externally from the context instance.
            var builder = CreateModelBuilder();

            BuildModelExternal(builder);

            return builder.FinalizeModel();
        }

        public abstract TestHelpers TestHelpers { get; }

        public ModelBuilder CreateModelBuilder()
            => TestHelpers.CreateConventionBuilder();

        protected virtual void BuildModelExternal(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chassis>(b => b.HasKey(c => c.TeamId));

            modelBuilder.Entity<Engine>(
                b =>
                {
                    b.Property(e => e.EngineSupplierId).IsConcurrencyToken();
                    b.Property(e => e.Name).IsConcurrencyToken();
                    b.OwnsOne(
                        e => e.StorageLocation, lb =>
                        {
                            lb.Property(l => l.Latitude).IsConcurrencyToken();
                            lb.Property(l => l.Longitude).IsConcurrencyToken();
                        });
                });

            modelBuilder.Entity<EngineSupplier>();

            modelBuilder.Entity<Gearbox>();

            modelBuilder.Entity<Sponsor>(
                b =>
                {
                    b.Property<int?>(Sponsor.ClientTokenPropertyName)
                        .IsConcurrencyToken();
                });

            modelBuilder.Entity<Team>(
                b =>
                {
                    b.HasOne(e => e.Gearbox).WithOne().HasForeignKey<Team>(e => e.GearboxId);
                    b.HasOne(e => e.Chassis).WithOne(e => e.Team).HasForeignKey<Chassis>(e => e.TeamId);
                });

            modelBuilder.Entity<TestDriver>();
            modelBuilder.Entity<TitleSponsor>()
                .OwnsOne(s => s.Details);

            modelBuilder.Entity<Team>()
                .HasMany(t => t.Sponsors)
                .WithMany(s => s.Teams)
                .UsingEntity<TeamSponsor>(
                    ts => ts
                        .HasOne(t => t.Sponsor)
                        .WithMany(),
                    ts => ts
                        .HasOne(t => t.Team)
                        .WithMany())
                .HasKey(ts => new { ts.SponsorId, ts.TeamId });
        }

        protected override void Seed(F1Context context)
            => F1Context.Seed(context);
    }
}
