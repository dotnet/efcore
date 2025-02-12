// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class LazyLoadProxyRelationalTestBase<TFixture>(TFixture fixture) : LazyLoadProxyTestBase<TFixture>(fixture)
    where TFixture : LazyLoadProxyRelationalTestBase<TFixture>.LoadRelationalFixtureBase
{
    public abstract class LoadRelationalFixtureBase : LoadFixtureBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Called>().ComplexProperty(c => c.Culture, ConfigureCulture);

            modelBuilder.Entity<Quest>().ComplexProperty(q => q.Culture, ConfigureCulture);

            modelBuilder.Entity<Father>(fb =>
            {
                fb.ComplexProperty(f => f.Culture, ConfigureCulture);
                fb.ComplexProperty(f => f.Milk, ConfigureMilk);
            });

            modelBuilder.Entity<Mother>(mb =>
            {
                mb.ComplexProperty(f => f.Culture, ConfigureCulture);
                mb.ComplexProperty(f => f.Milk, ConfigureMilk);
            });

            static void ConfigureCulture(ComplexPropertyBuilder<Culture> cb)
            {
                cb.Property(c => c.Rating).HasColumnName("Culture_Rating");
                cb.Property(c => c.Subspecies).HasColumnName("Culture_Subspecies");
                cb.Property(c => c.Species).HasColumnName("Culture_Species");
                cb.Property(c => c.Validation).HasColumnName("Culture_Validation");
            }

            static void ConfigureMilk(ComplexPropertyBuilder<Milk> mb)
            {
                mb.Property(c => c.Rating).HasColumnName("Milk_Rating");
                mb.Property(c => c.Subspecies).HasColumnName("Milk_Subspecies");
                mb.Property(c => c.Species).HasColumnName("Milk_Species");
                mb.Property(c => c.Validation).HasColumnName("Milk_Validation");
            }
        }
    }
}
