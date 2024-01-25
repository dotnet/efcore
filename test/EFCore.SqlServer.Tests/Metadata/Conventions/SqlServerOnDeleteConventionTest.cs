// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class SqlServerOnDeleteConventionTest
{
    [ConditionalFact] // Issue #32732
    public void Convention_does_not_assume_skip_navigations_have_non_null_FK()
    {
        using var context = new SkippyDbContext();
        var model = context.Model;
        Assert.Equal(["ArenaPropensity", "Arena", "Propensity"], model.GetEntityTypes().Select(e => e.ShortName()));
        Assert.Equal(
            [DeleteBehavior.Cascade, DeleteBehavior.ClientCascade],
            model.GetEntityTypes().Single(e => e.ShortName() == "ArenaPropensity").GetForeignKeys().Select(k => k.DeleteBehavior));
    }

    public class SkippyDbContext : DbContext
    {
        public DbSet<Arena> Areas { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer();
    }

    public class Arena : Propensity
    {
        public virtual ICollection<Propensity> AreaProperties { get; set; }
    }

    public abstract class Propensity
    {
        public int Id { get; set; }

        public int? PrimaryYId { get; set; }
        public virtual Propensity PrimaryYProp { get; set; }

        public virtual ICollection<Arena> PropertyAreas { get; set; }
    }
}
