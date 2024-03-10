// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqlServerTptIdentityTest(GraphUpdatesSqlServerTptIdentityTest.SqlServerFixture fixture) : GraphUpdatesSqlServerTestBase<GraphUpdatesSqlServerTptIdentityTest.SqlServerFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
    {
        protected override string StoreName
            => "GraphTptIdentityUpdatesTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.UseIdentityColumns();

            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Root>().ToTable(nameof(Root));
            modelBuilder.Entity<Required1>().ToTable(nameof(Required1));
            modelBuilder.Entity<Required1Derived>().ToTable(nameof(Required1Derived));
            modelBuilder.Entity<Required1MoreDerived>().ToTable(nameof(Required1MoreDerived));
            modelBuilder.Entity<Required2Derived>().ToTable(nameof(Required2Derived));
            modelBuilder.Entity<Required2MoreDerived>().ToTable(nameof(Required2MoreDerived));
            modelBuilder.Entity<Optional1>().ToTable(nameof(Optional1));
            modelBuilder.Entity<Optional1Derived>().ToTable(nameof(Optional1Derived));
            modelBuilder.Entity<Optional1MoreDerived>().ToTable(nameof(Optional1MoreDerived));
            modelBuilder.Entity<Optional2Derived>().ToTable(nameof(Optional2Derived));
            modelBuilder.Entity<Optional2MoreDerived>().ToTable(nameof(Optional2MoreDerived));
            modelBuilder.Entity<RequiredSingle1>().ToTable(nameof(RequiredSingle1));
            modelBuilder.Entity<OptionalSingle1>().ToTable(nameof(OptionalSingle1));
            modelBuilder.Entity<OptionalSingle2>().ToTable(nameof(OptionalSingle2));
            modelBuilder.Entity<RequiredNonPkSingle1>().ToTable(nameof(RequiredNonPkSingle1));
            modelBuilder.Entity<RequiredNonPkSingle2Derived>().ToTable(nameof(RequiredNonPkSingle2Derived));
            modelBuilder.Entity<RequiredNonPkSingle2MoreDerived>().ToTable(nameof(RequiredNonPkSingle2MoreDerived));
            modelBuilder.Entity<RequiredAk1>().ToTable(nameof(RequiredAk1));
            modelBuilder.Entity<RequiredAk1Derived>().ToTable(nameof(RequiredAk1Derived));
            modelBuilder.Entity<RequiredAk1MoreDerived>().ToTable(nameof(RequiredAk1MoreDerived));
            modelBuilder.Entity<OptionalAk1>().ToTable(nameof(OptionalAk1));
            modelBuilder.Entity<OptionalAk1Derived>().ToTable(nameof(OptionalAk1Derived));
            modelBuilder.Entity<OptionalAk1MoreDerived>().ToTable(nameof(OptionalAk1MoreDerived));
            modelBuilder.Entity<RequiredSingleAk1>().ToTable(nameof(RequiredSingleAk1));
            modelBuilder.Entity<OptionalSingleAk1>().ToTable(nameof(OptionalSingleAk1));
            modelBuilder.Entity<OptionalSingleAk2Derived>().ToTable(nameof(OptionalSingleAk2Derived));
            modelBuilder.Entity<OptionalSingleAk2MoreDerived>().ToTable(nameof(OptionalSingleAk2MoreDerived));
            modelBuilder.Entity<RequiredNonPkSingleAk1>().ToTable(nameof(RequiredNonPkSingleAk1));
            modelBuilder.Entity<RequiredAk2>().ToTable(nameof(RequiredAk2));
            modelBuilder.Entity<RequiredAk2Derived>().ToTable(nameof(RequiredAk2Derived));
            modelBuilder.Entity<RequiredAk2MoreDerived>().ToTable(nameof(RequiredAk2MoreDerived));
            modelBuilder.Entity<OptionalAk2>().ToTable(nameof(OptionalAk2));
            modelBuilder.Entity<OptionalAk2Derived>().ToTable(nameof(OptionalAk2Derived));
            modelBuilder.Entity<OptionalAk2MoreDerived>().ToTable(nameof(OptionalAk2MoreDerived));
            modelBuilder.Entity<RequiredSingleAk2>().ToTable(nameof(RequiredSingleAk2));
            modelBuilder.Entity<RequiredNonPkSingleAk2>().ToTable(nameof(RequiredNonPkSingleAk2));
            modelBuilder.Entity<RequiredNonPkSingleAk2Derived>().ToTable(nameof(RequiredNonPkSingleAk2Derived));
            modelBuilder.Entity<RequiredNonPkSingleAk2MoreDerived>().ToTable(nameof(RequiredNonPkSingleAk2MoreDerived));
            modelBuilder.Entity<OptionalSingleAk2>().ToTable(nameof(OptionalSingleAk2));
            modelBuilder.Entity<RequiredComposite1>().ToTable(nameof(RequiredComposite1));
            modelBuilder.Entity<OptionalOverlapping2>().ToTable(nameof(OptionalOverlapping2));
            modelBuilder.Entity<BadCustomer>().ToTable(nameof(BadCustomer));
            modelBuilder.Entity<BadOrder>().ToTable(nameof(BadOrder));
            modelBuilder.Entity<TaskChoice>().ToTable(nameof(TaskChoice));
            modelBuilder.Entity<ParentAsAChild>().ToTable(nameof(ParentAsAChild));
            modelBuilder.Entity<ChildAsAParent>().ToTable(nameof(ChildAsAParent));
            modelBuilder.Entity<Poost>().ToTable(nameof(Poost));
            modelBuilder.Entity<Bloog>().ToTable(nameof(Bloog));
            modelBuilder.Entity<Produce>().ToTable(nameof(Produce));
            modelBuilder.Entity<ParentEntity32084>().ToTable(nameof(ParentEntity32084));
            modelBuilder.Entity<ChildBaseEntity32084>().ToTable(nameof(ChildBaseEntity32084));
            modelBuilder.Entity<ChildEntity32084>().ToTable(nameof(ChildEntity32084));
        }
    }
}
