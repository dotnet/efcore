// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class UnconstrainedForeignKeyFixupTest
{
    [Fact]
    public void Severing_navigation_nulls_a_nullable_unconstrained_FK()
    {
        using var context = CreateContext();

        var principal = new Principal { Id = 1 };
        var dependent = new Dependent { Id = 1, Principal = principal };

        context.Attach(principal);
        context.Attach(dependent);

        dependent.Principal = null;
        context.ChangeTracker.DetectChanges();

        Assert.Null(dependent.PrincipalId);
    }

    [Fact]
    public void Nulling_unconstrained_FK_severs_navigation()
    {
        using var context = CreateContext();

        var principal = new Principal { Id = 1 };
        var dependent = new Dependent { Id = 1, PrincipalId = 1 };

        context.Attach(principal);
        context.Attach(dependent);

        Assert.Same(principal, dependent.Principal);

        dependent.PrincipalId = null;
        context.ChangeTracker.DetectChanges();

        Assert.Null(dependent.Principal);
    }

    [Fact]
    public void Fixup_associates_dependent_to_principal_by_unconstrained_FK_value()
    {
        using var context = CreateContext();

        var principal = new Principal { Id = 7 };
        context.Attach(principal);

        var dependent = new Dependent { Id = 1, PrincipalId = 7 };
        context.Attach(dependent);

        Assert.Same(principal, dependent.Principal);
    }

    [Fact]
    public void Dangling_unconstrained_FK_can_be_tracked_with_no_principal()
    {
        using var context = CreateContext();

        var dependent = new Dependent { Id = 1, PrincipalId = 999 };
        context.Attach(dependent);

        context.ChangeTracker.DetectChanges();

        Assert.Null(dependent.Principal);
        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
    }

    [Fact]
    public void Severing_required_non_nullable_unconstrained_relationship_uses_conceptual_null()
    {
        using var context = CreateRequiredContext();

        var principal = new RequiredPrincipal { Id = 1 };
        var dependent = new RequiredDependent { Id = 1, Principal = principal };

        context.Attach(principal);
        context.Attach(dependent);
        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);

        // The FK is non-nullable, so severing the required relationship produces a conceptual null.
        // The default (Cascade) delete behavior resolves it by deleting the dependent — identical to a
        // constrained required relationship (Decision 2: fixup behaves the same regardless of IsConstrained).
        dependent.Principal = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);
    }

    private static TestContext CreateContext()
        => new(
            new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

    private static RequiredTestContext CreateRequiredContext()
        => new(
            new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

    private class TestContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Dependent>()
                .HasOne(e => e.Principal).WithMany()
                .HasForeignKey(e => e.PrincipalId)
                .IsConstrained(false);
    }

    private class Principal
    {
        public int Id { get; set; }
    }

    private class Dependent
    {
        public int Id { get; set; }
        public int? PrincipalId { get; set; }
        public Principal Principal { get; set; }
    }

    private class RequiredTestContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<RequiredDependent>()
                .HasOne(e => e.Principal).WithMany()
                .HasForeignKey(e => e.PrincipalId)
                .IsConstrained(false);
    }

    private class RequiredPrincipal
    {
        public int Id { get; set; }
    }

    private class RequiredDependent
    {
        public int Id { get; set; }
        public int PrincipalId { get; set; }
        public RequiredPrincipal Principal { get; set; }
    }
}
